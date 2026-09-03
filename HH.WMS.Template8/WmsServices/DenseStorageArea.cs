using SqlSugar;

namespace WebApplication1 {

    /// <summary>
    /// 密集库
    /// </summary>
    internal class DenseStorageArea {
        /// <summary>
        /// 创建搬运任务
        /// </summary>
        /// <param name="start">起点货位编码</param>
        /// <param name="end">终点货位编码</param>
        /// <param name="taskType">任务类型</param>
        /// <param name="cntrs">托盘码</param>
        /// <param name="startLayer">起点层</param>
        /// <param name="endLayer">终点层</param>
        /// <param name="priority">优先级</param>
        /// <returns></returns>
        public static bool CreateTransport(string start, string end, string taskType, string cntrs, int startLayer, int endLayer, string bsNo, string bsType, int priority = 1) {
            var db = new SqlHelper<object>().GetInstance();
            return TaskHelper.CreateTask(start.Trim(), end.Trim(), taskType, priority, cntrs, bsNo, bsType, startLayer, endLayer);
        }

        /// <summary>
        /// 使用聚合函数排除有锁的排(分组货位不多的直接用聚合函数)
        /// </summary>
        /// <param name="db"></param>
        /// <param name="area"></param>
        /// <param name="item"></param>
        /// <param name="batch"></param>
        /// <param name="needMark"></param>
        /// <returns></returns>
        private static (Location end, bool empty) GetIn(SqlSugarClient db, string area, string item = "", string batch = "") {
            Location end = null;
            var empty = false;
            //1 聚合查询每一排有没有货，有没有放满，排除有锁的货位
            var rowDataList = db.Queryable<Location>()
               .Where(loc => loc.S_AREA_CODE == area)
               .GroupBy(loc => loc.N_ROW)    // 按【排号 N_ROW】分组（同一排）
               .Having(loc => SqlFunc.AggregateSum(SqlFunc.IIF(loc.N_LOCK_STATE != 0, 1, 0)) == 0) // 只要这一排有 任何货位锁定 → 整排排除！
                                                                                                   // 统计：有货最大列、已用列、SKU匹配
                                                                                                   // 新增：层数=1时 → 这一排必须至少有一个货位 S_MARK 有值（不为null且不为空字符串）
                                                                                                   //.Having(loc => isEmptyStack == false || (layer != 1 || SqlFunc.AggregateSum(SqlFunc.IIF(loc.S_MARK != null && loc.S_MARK.Trim() != "", 1, 0)) > 0))
               .Select(loc => new
               {
                   N_ROW = loc.N_ROW,
                   // 1. 只统计【有货】的最大列号（你要的核心）
                   MaxUsedCol = SqlFunc.AggregateMax(SqlFunc.IIF(loc.N_CURRENT_NUM > 0, loc.N_POS, 0)),
                   // 2. 这一排 真正的最大列号（总共有多少列）
                   MaxCol = SqlFunc.AggregateMax(loc.N_COL),
               })
               .OrderBy(loc => loc.N_ROW)
               .ToList();

            //2 判断没有放满的是否有匹配的
            var rowList = rowDataList.Where(x => x.MaxUsedCol > 0 && x.MaxUsedCol < x.MaxCol).ToList();
            if (rowList.Count() > 0) {
                var whereOrList = new List<string>();
                foreach (var row in rowList) {
                    whereOrList.Add($" (N_ROW = {row.N_ROW} AND N_COL = {row.MaxUsedCol}) ");
                }
                string whereSql = "(" + string.Join(" OR ", whereOrList) + ")";
                var query = db.Queryable<Location>()
                    .Where(loc => loc.S_AREA_CODE == area).Where(whereSql);
                query = query.LeftJoin<LocCntrRel>((loc, cntr) => loc.S_CODE == cntr.S_LOC_CODE)
                                .LeftJoin<InvDetail>((loc, cntr, sku) => cntr.S_CNTR_CODE == sku.S_CNTR_CODE)
                                //.Where((loc, cntr, sku) => sku.S_ITEM_CODE == item && sku.S_BATCH_NO == batch)
                                .Where((loc, cntr, sku) =>
                                 (item == "" && sku.S_ITEM_CODE == null) || (item != "" && sku.S_ITEM_CODE == item && sku.S_BATCH_NO == batch));

                var listMatch = query.ToList();
                //查找匹配的货位的，没有放满的货位
                if (listMatch.Count > 0) {
                    //匹配之后循环选择这一排的货位，成功就跳出循环
                    //默认不堆叠，堆叠的还需要判断当前货位
                    foreach (var location in listMatch) {
                        end = db.Queryable<Location>()
                             .Where(loc =>
                                 loc.S_AREA_CODE == area &&
                                 loc.N_ROW == location.N_ROW &&
                                 loc.N_COL > location.N_COL &&
                                 loc.C_ENABLE == "Y")
                             .OrderBy(loc => loc.N_COL)
                             .First();
                        if (end != null) {
                            break;
                        }
                    }
                }


            }
            //3 没有放满的货位或者没有匹配sku的选择空排
            if (end == null) {
                LogHelper.Info($"没有找到未满货位中物料匹配的货位，物料编码{item}，批次号{batch}，开始找空排");
                var emptyRows = rowDataList.Cast<dynamic>().Where(x => x.MaxUsedPos == 0).ToList();
                if (emptyRows.Count > 0) {
                    foreach (var row in emptyRows) {
                        int targetRow = row.N_ROW;
                        end = db.Queryable<Location>()
                            .Where(loc =>
                                loc.S_AREA_CODE == area &&
                                loc.N_ROW == targetRow &&
                                loc.C_ENABLE == "Y")
                            .OrderBy(loc => loc.N_COL)
                            .First();
                        if (end != null) {
                            empty = true;
                            LogHelper.Info($"找到空排，排号{targetRow}，开始选择货位");
                            break;
                        }
                    }

                }

            }

            return (end, empty);
        }

        /// <summary>
        /// 不用分组聚合函数，平面库太大了效率会高一点
        /// </summary>
        /// <param name="db"></param>
        /// <param name="area"></param>
        /// <param name="layer"></param>
        /// <param name="roadway"></param>
        /// <param name="group"></param>
        /// <param name="item"></param>
        /// <param name="batch"></param>
        /// <returns></returns>
        private static (Location end, bool empty) GetInNew(SqlSugarClient db, string area, string item = "", string batch = "") {
            Location end = null;
            var empty = false;
            // 比groupby然后having查询效率高，having需要把一排所有的货位查完
            var lockedRowList = db.Queryable<Location>()
                .Where(loc => loc.S_AREA_CODE == area && loc.N_LOCK_STATE != 0)
                .Select(loc => loc.N_ROW)
                .Distinct()
                .ToList();

            // 第2次查询：排除掉锁定的排，再分组统计每排的最大列信息
            var rowDataList = db.Queryable<Location>()
                .Where(loc => loc.S_AREA_CODE == area)
                // 排除有锁的排；lockedRowList为空时，这个条件自动不生效
                .WhereIF(lockedRowList.Any(), loc => !lockedRowList.Contains(loc.N_ROW))
                .GroupBy(loc => loc.N_ROW)
                .Select(loc => new
                {
                    N_ROW = loc.N_ROW,
                    MaxUsedCol = SqlFunc.AggregateMax(SqlFunc.IIF(loc.N_CURRENT_NUM > 0, loc.N_COL, 0)),
                    MaxCol = SqlFunc.AggregateMax(loc.N_COL)
                })
                .OrderBy(loc => loc.N_ROW)
                .ToList();

            //2 判断没有放满的是否有匹配的
            var rowList = rowDataList.Where(x => x.MaxUsedCol > 0 && x.MaxUsedCol < x.MaxCol).ToList();
            if (rowList.Count() > 0) {
                var whereOrList = new List<string>();
                foreach (var row in rowList) {
                    whereOrList.Add($" (N_ROW = {row.N_ROW} AND N_COL = {row.MaxUsedCol}) ");
                }
                string whereSql = "(" + string.Join(" OR ", whereOrList) + ")";
                var query = db.Queryable<Location>()
                    .Where(loc => loc.S_AREA_CODE == area).Where(whereSql);
                query = query.LeftJoin<LocCntrRel>((loc, cntr) => loc.S_CODE == cntr.S_LOC_CODE)
                                .LeftJoin<InvDetail>((loc, cntr, sku) => cntr.S_CNTR_CODE == sku.S_CNTR_CODE)
                                //.Where((loc, cntr, sku) => sku.S_ITEM_CODE == item && sku.S_BATCH_NO == batch)
                                .Where((loc, cntr, sku) =>
                                 (item == "" && sku.S_ITEM_CODE == null) || (item != "" && sku.S_ITEM_CODE == item && sku.S_BATCH_NO == batch));

                var listMatch = query.ToList();
                //查找匹配的货位的，没有放满的货位
                if (listMatch.Count > 0) {
                    //匹配之后循环选择这一排的货位，成功就跳出循环
                    //默认不堆叠，堆叠的还需要判断当前货位
                    foreach (var location in listMatch) {
                        end = db.Queryable<Location>()
                                 .Where(loc =>
                                     loc.S_AREA_CODE == area &&
                                     loc.N_ROW == location.N_ROW &&
                                     loc.N_COL > location.N_COL &&
                                     loc.C_ENABLE == "Y")
                                 .OrderBy(loc => loc.N_COL)
                                 .First();
                        if (end != null) {
                            break;
                        }
                    }
                }


            }
            //3 没有放满的货位或者没有匹配sku的选择空排
            if (end == null) {
                LogHelper.Info($"没有找到未满货位中物料匹配的货位，物料编码{item}，批次号{batch}，开始找空排");
                var emptyRows = rowDataList.Cast<dynamic>().Where(x => x.MaxUsedPos == 0).ToList();
                if (emptyRows.Count > 0) {
                    foreach (var row in emptyRows) {
                        int targetRow = row.N_ROW;
                        end = db.Queryable<Location>()
                            .Where(loc =>
                                loc.S_AREA_CODE == area &&
                                loc.N_ROW == targetRow &&
                                loc.C_ENABLE == "Y")
                            .OrderBy(loc => loc.N_POS)
                            .First();
                        if (end != null) {
                            empty = true;
                            LogHelper.Info($"找到空排，排号{targetRow}，开始选择货位");
                            break;
                        }
                    }
                }

            }

            return (end, empty);
        }

        public static void Test() {
            var db = new SqlHelper<Object>().GetInstance();
            var itemBatch = "20260720";
            var item = "AAA";
            var test = db.Queryable<Location>()
                .Where(loc => loc.S_AREA_CODE == "CPK_X").LeftJoin<LocCntrRel>((loc, cntr) => loc.S_CODE == cntr.S_LOC_CODE)
                                   .LeftJoin<InvDetail>((loc, cntr, sku) => cntr.S_CNTR_CODE == sku.S_CNTR_CODE)
                                   .Where((loc, cntr, sku) => sku.S_ITEM_CODE == item && sku.S_BATCH_NO.Length == itemBatch.Length && sku.S_BATCH_NO.Trim().EndsWith(itemBatch.Substring(itemBatch.Length - 2))).ToList();
            // 第1次查询：拿到【有任意货位被锁】的排号集合，排号去重
            var lockedRowList = db.Queryable<Location>()
                .Where(loc => loc.S_AREA_CODE == "CPK_X" && loc.N_LOCK_STATE != 0)
                .Select(loc => loc.N_ROW)
                .Distinct()
                .ToList();

            // 第2次查询：排除掉锁定的排，再分组统计每排的最大列信息
            var result = db.Queryable<Location>()
                .Where(loc => loc.S_AREA_CODE == "CPK_X")
                // 排除有锁的排；lockedRowList为空时，这个条件自动不生效
                .WhereIF(lockedRowList.Any(), loc => !lockedRowList.Contains(loc.N_ROW))
                .GroupBy(loc => loc.N_ROW)
                .Select(loc => new
                {
                    N_ROW = loc.N_ROW,
                    MaxUsedPos = SqlFunc.AggregateMax(SqlFunc.IIF(loc.N_CURRENT_NUM > 0, loc.N_COL, 0)),
                    MaxPos = SqlFunc.AggregateMax(loc.N_COL)
                })
                .OrderBy(loc => loc.N_ROW)
                .ToList();


        }
    }


}
