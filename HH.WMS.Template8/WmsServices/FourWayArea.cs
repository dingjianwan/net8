using Newtonsoft.Json;
using SqlSugar;

namespace WebApplication1 {

    /// <summary>
    /// 四向车库，一层层的查找
    /// </summary>
    internal class FourWayArea {
        internal static Location GetFullEnd(SqlSugarClient db, int[] layers, InvDetail cir, string areaCode) {
            Location end = null;
            bool isBreak = false;
            int priority = 0;
            for (int i = 0; i < layers.Length && !isBreak; i++) {
                var res = GetFullEndByLayer(db, cir, areaCode, layers[i]);
                if (res.end != null) {
                    if (end == null) {
                        end = res.end;
                        priority = res.priority;
                    }
                    else if (res.priority < priority) {
                        end = res.end;
                        priority = res.priority;
                    }
                    if (priority == 0) {
                        isBreak = true;
                        break;
                    }
                }
            }
            return end;
        }
        /// <summary>
        /// 一层有2个巷道，等于4个小库区
        /// </summary>
        /// <param name="db"></param>
        /// <param name="cir"></param>
        /// <param name="areaCode"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static (Location end, int priority) GetFullEndByLayer(SqlSugarClient db, InvDetail cir, string areaCode, int layer) {
            Location end = null; int priority = 0;
            bool isBreak = false;
            for (int j = 1; j <= 2 && !isBreak; j++) {
                for (int k = 1; k <= 2 && !isBreak; k++) {
                    var res = GetFullIn(db, areaCode, layer, j, k, cir.S_ITEM_CODE, cir.S_BATCH_NO);
                    if (res.end != null) {
                        LogHelper.Info($"找到货位{res.end.S_CODE}深度{res.end.N_POS} 优先级{res.priority}");
                        if (end == null) {
                            end = res.end;
                            priority = res.priority;
                        }
                        else if (res.priority < priority) {
                            end = res.end;
                            priority = res.priority;

                        }
                        if (priority == 0) {
                            isBreak = true;
                            break;
                        }
                    }
                }
            }
            return (end, priority);
        }
        /// <summary>
        /// 满托查找货位，优先找同物料同批次，然后空排，最后同物料
        /// </summary>
        /// <param name="db"></param>
        /// <param name="area"></param>
        /// <param name="layer"></param>
        /// <param name="roadway"></param>
        /// <param name="side"></param>
        /// <param name="item"></param>
        /// <param name="batch"></param>
        /// <returns></returns>
        private static (Location end, int priority) GetFullIn(SqlSugarClient db, string area, int layer, int roadway, int side, string item, string batch) {
            Location end = null;
            var priority = 0;//0同物料同批次 1混放 2空排 3同物料不同批次
            //1 聚合查询每一排有没有货，有没有放满，排除有锁的货位
            var rowDataList = db.Queryable<Location>()
               .Where(loc => loc.S_AREA_CODE == area && loc.N_LAYER == layer && loc.N_ROADWAY == roadway && loc.N_ROW_GROUP == side)
               .GroupBy(loc => loc.N_ROW)    // 按【排号 N_ROW】分组（同一排）
               .Having(loc => SqlFunc.AggregateSum(SqlFunc.IIF(loc.N_LOCK_STATE != 0, 1, 0)) == 0)
               .Select(loc => new
               {
                   N_ROW = loc.N_ROW,
                   MaxUsedPos = SqlFunc.AggregateMax(SqlFunc.IIF(loc.N_CURRENT_NUM > 0, loc.N_POS, 0)),
                   MaxPos = SqlFunc.AggregateMax(loc.N_POS),
               })
               .OrderBy(loc => loc.N_ROW)
               .ToList();

            //2 判断没有放满的是否有匹配的
            ISugarQueryable<Location> query = null;
            var rowList = rowDataList.Where(x => x.MaxUsedPos > 0 && x.MaxUsedPos < x.MaxPos).ToList();
            if (rowList.Count() > 0) {
                var whereOrList = new List<string>();
                foreach (var row in rowList) {
                    whereOrList.Add($" (N_ROW = {row.N_ROW} AND N_POS = {row.MaxUsedPos}) ");
                }
                string whereSql = "(" + string.Join(" OR ", whereOrList) + ")";
                query = db.Queryable<Location>()
                    .Where(loc => loc.S_AREA_CODE == area
                        && loc.N_LAYER == layer
                        && loc.N_ROADWAY == roadway
                        && loc.N_ROW_GROUP == side);

                query = query.Where(whereSql);
                //query 是共用的
                var finalQuery = query.LeftJoin<LocCntrRel>((loc, cntr) => loc.S_CODE == cntr.S_LOC_CODE)
                                .LeftJoin<InvDetail>((loc, cntr, sku) => cntr.S_CNTR_CODE == sku.S_CNTR_CODE)
                                .Where((loc, cntr, sku) => sku.S_ITEM_CODE == item && sku.S_BATCH_NO == batch);

                var listMatch = finalQuery.ToList();
                //查找匹配的货位的，没有放满的货位
                if (listMatch.Count > 0) {
                    //匹配之后循环选择这一排的货位，成功就跳出循环
                    foreach (var location in listMatch) {
                        end = db.Queryable<Location>()
                             .Where(loc =>
                                 loc.S_AREA_CODE == area &&
                                 loc.N_LAYER == layer &&
                                 loc.N_ROADWAY == roadway &&
                                 loc.N_ROW_GROUP == side &&
                                 loc.N_ROW == location.N_ROW &&
                                 loc.N_POS > location.N_POS &&
                                 loc.C_ENABLE == "Y")
                             .OrderBy(loc => loc.N_POS)
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
                                loc.N_LAYER == layer &&
                                loc.N_ROADWAY == roadway &&
                                loc.N_ROW_GROUP == side &&
                                loc.N_ROW == targetRow &&
                                loc.C_ENABLE == "Y")
                            .OrderBy(loc => loc.N_POS)
                            .First();
                        if (end != null) {
                            priority = 2;
                            LogHelper.Info($"找到空排，排号{targetRow}，开始选择货位");
                            break;
                        }
                    }
                }
            }

            //4 判断没有放满的sku 匹配的
            if (end == null) {
                if (rowList.Count() > 0) {
                    var finalQuery = query.LeftJoin<LocCntrRel>((loc, cntr) => loc.S_CODE == cntr.S_LOC_CODE)
                                    .LeftJoin<InvDetail>((loc, cntr, sku) => cntr.S_CNTR_CODE == sku.S_CNTR_CODE)
                                    .Where((loc, cntr, sku) => sku.S_ITEM_CODE == item);

                    var listMatch = finalQuery.ToList();
                    //查找匹配的货位的，没有放满的货位
                    if (listMatch.Count > 0) {
                        //匹配之后循环选择这一排的货位，成功就跳出循环
                        foreach (var location in listMatch) {
                            end = db.Queryable<Location>()
                                 .Where(loc =>
                                     loc.S_AREA_CODE == area &&
                                     loc.N_LAYER == layer &&
                                     loc.N_ROADWAY == roadway &&
                                     loc.N_ROW_GROUP == side &&
                                     loc.N_ROW == location.N_ROW &&
                                     loc.N_POS > location.N_POS &&
                                     loc.C_ENABLE == "Y")
                                 .OrderBy(loc => loc.N_POS)
                                 .First();

                            if (end != null) {
                                priority = 3;
                                break;
                            }
                        }
                    }
                }
            }

            return (end, priority);
        }
        internal static (Location start, int moveCount) GetFullOut(SqlSugarClient db, string areaCode, string item, string batch) {
            Location start = null;
            int moveCount = 0;
            //指定物料批次找同物料同批次的，如果能出直接出，出不来就要移库
            bool isBreak = false;
            for (int i = 1; i <= 3 && !isBreak; i++) {
                for (int j = 1; j <= 2 && !isBreak; j++) {
                    for (int k = 1; k <= 2 && !isBreak; k++) {
                        var res = GetOut(db, areaCode, i, j, k, item, batch);
                        if (res.start != null) {
                            if (start == null || res.moveCount < moveCount) {
                                LogHelper.Info($"找到货位{res.start.S_CODE}深度{res.start.N_POS} 可以出库,需要移库数量{res.moveCount}");
                                start = res.start;
                                moveCount = res.moveCount;
                            }
                            if (moveCount == 0) {
                                isBreak = true;
                                break;
                            }
                        }
                    }
                }
            }
            return (start, moveCount);
        }
        private static (Location start, int moveCount) GetOut(SqlSugarClient db, string area, int layer, int roadway, int group, string item, string batch) {
            LogHelper.Info($"四向车满托出库申请开始查找{layer}层{roadway}巷道{group}组,物料{item}批次{batch}", "四向车");
            Location start = null;
            int movecount = 0;
            var rowDataList = db.Queryable<Location>()
               .Where(loc => loc.S_AREA_CODE == area && loc.N_LAYER == layer && loc.N_ROADWAY == roadway && loc.N_ROW_GROUP == group)
               .GroupBy(loc => loc.N_ROW)    // 按【排号 N_ROW】分组（同一排）
               .Having(loc => SqlFunc.AggregateSum(SqlFunc.IIF(loc.N_LOCK_STATE != 0, 1, 0)) == 0)
               .Select(loc => new
               {
                   N_ROW = loc.N_ROW,
                   MaxUsedPos = SqlFunc.AggregateMax(SqlFunc.IIF(loc.N_CURRENT_NUM > 0, loc.N_POS, 0)),
                   MaxPos = SqlFunc.AggregateMax(loc.N_POS),

               })
               .OrderBy(loc => loc.N_ROW)//已经限定了层、巷道、巷道侧，所以这里只需要按排分组（具体还看货位设计）
               .ToList();
            var rowList = rowDataList.Where(x => x.MaxUsedPos > 0).ToList();
            if (rowList.Count() > 0) {
                var rowNums = rowList.Select(c => c.N_ROW).ToList();
                //有货的排查一下物料和批次匹配的有多少，按排分组。
                var matchList = db.Queryable<Location>()
                 .Where(loc => rowNums.Contains(loc.N_ROW)//Contains语法支持，Any不支持要用 Expressionable 拼接 OR
                     && loc.S_AREA_CODE == area
                     && loc.N_LAYER == layer
                     && loc.N_ROADWAY == roadway
                     && loc.N_ROW_GROUP == group)
                 .LeftJoin<LocCntrRel>((loc, lcr) => loc.S_CODE == lcr.S_LOC_CODE)
                 .LeftJoin<InvDetail>((loc, lcr, inv) => lcr.S_CNTR_CODE == inv.S_CNTR_CODE
                                                      && inv.S_ITEM_CODE == item
                                                      && inv.S_BATCH_NO == batch)
                 //.Where((loc, lcr, inv) => inv.S_ITEM_CODE == item
                 //    && inv.S_BATCH_NO == batch)
                 .Select((loc, lcr, inv) => new { loc.N_ROW, loc.N_POS })
                 .ToList().GroupBy(c => c.N_ROW)//已经限定了层、巷道、巷道侧，所以这里只需要按排分组（具体还看货位设计）
                 .Select(g => new
                 {
                     N_ROW = g.Key,
                     MatchCount = g.Count(),
                     MaxPos = g.Max(c => c.N_POS)
                 }).ToList();
                if (matchList.Count() > 0) {
                    LogHelper.Info($"四向车满托出库 {layer}层{roadway}巷道{group}组,物料{item}批次{batch} 找到匹配的托盘", "四向车");
                    //匹配的组里面如果最大pos和有货的最大使用pos一样，说明不是混排的，否则里面的pos匹配，外面的不匹配要移库，优先出不移库并且数量少的
                    var matchesWithDiff = matchList.Join(rowList, match => match.N_ROW, row => row.N_ROW,
                                                        (match, row) => new
                                                        {
                                                            OriginalMatch = match,
                                                            diff = row.MaxUsedPos - match.MaxPos
                                                        }).OrderBy(c => c.diff).ThenBy(c => c.OriginalMatch.MatchCount).FirstOrDefault();
                    LogHelper.Info($"{JsonConvert.SerializeObject(matchesWithDiff)}", "四向车");
                    start = db.Queryable<Location>().Where(loc => loc.S_AREA_CODE == area
                                                            && loc.N_LAYER == layer
                                                            && loc.N_ROADWAY == roadway
                                                            && loc.N_ROW_GROUP == group
                                                            && loc.N_ROW == matchesWithDiff.OriginalMatch.N_ROW
                                                            && loc.N_POS == matchesWithDiff.OriginalMatch.MaxPos).First();
                    movecount = matchesWithDiff.diff;



                }
            }
            return (start, movecount);
        }

        internal static List<Location> GetInMove(SqlSugarClient db, string areaCode, string item, string batch, int moveCount, Location start) {
            List<Location> moveList = new List<Location>();
            int pri = 0;
            bool isBreak = false;
            var layers = new int[] { 1, 2, 3 };
            if (start.N_LAYER == 2) {
                layers = new int[] { 2, 1, 3 };
            }
            else if (start.N_LAYER == 3) {
                layers = new int[] { 3, 1, 2 };
            }
            for (int i = 0; i < 3; i++) {
                for (int j = 1; j <= 2 && !isBreak; j++) {
                    for (int k = 1; k <= 2 && !isBreak; k++) {
                        //移库遍历时候不要移到起点区域了
                        LogHelper.Info($"四向车满托查找移库的终点{layers[i]}层{j}巷道{k}组,物料{item}批次{batch}", "四向车");
                        var res = GetMoveIn(db, areaCode, layers[i], j, k, item, batch, moveCount, start);
                        if (res.list.Count > 0) {
                            if (moveList.Count == 0 || res.pri < pri) {
                                moveList = res.list;
                                pri = res.pri;
                            }
                            if (pri == 0) {
                                //找到空托移库的终点直接退出
                                break;
                            }
                        }

                    }
                }
            }
            return moveList;
        }
        /// <summary>
        /// 理论上可以往多排移库，太复杂不考虑，目前只支持一次性移动到一排，移到多排要人工来整理
        /// </summary>
        /// <param name="db"></param>
        /// <param name="area"></param>
        /// <param name="layer"></param>
        /// <param name="roadway"></param>
        /// <param name="group"></param>
        /// <param name="item"></param>
        /// <param name="batch"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        private static (List<Location> list, int pri) GetMoveIn(SqlSugarClient db, string area, int layer, int roadway, int group, string item, string batch, int moveCount, Location start) {
            List<Location> moveList = new List<Location>();
            int pri = 0;
            //1 聚合查询每一排有没有货，有没有放满，排除有锁的货位
            var rowDataList = db.Queryable<Location>()
               .Where(loc => loc.S_AREA_CODE == area && loc.N_LAYER == layer && loc.N_ROADWAY == roadway && loc.N_ROW_GROUP == group)
               .GroupBy(loc => loc.N_ROW)    // 按【排号 N_ROW】分组（同一排）
               .Having(loc => SqlFunc.AggregateSum(SqlFunc.IIF(loc.N_LOCK_STATE != 0, 1, 0)) == 0)
               .Select(loc => new
               {
                   N_ROW = loc.N_ROW,
                   MaxUsedPos = SqlFunc.AggregateMax(SqlFunc.IIF(loc.N_CURRENT_NUM > 0, loc.N_POS, 0)),
                   MaxPos = SqlFunc.AggregateMax(loc.N_POS),
               })
               .OrderBy(loc => loc.N_ROW)
               .ToList();
            //2 移库优先找空排移，没有空排找同物料同批次的移库然后找同物料不同批次
            var emptyRows = rowDataList.Where(x => x.MaxUsedPos == 0 && x.MaxPos >= moveCount).ToList();
            if (emptyRows.Count > 0) {
                LogHelper.Info($"四向车满托查找移库的终点空排{layer}巷道{roadway}组{group}空排，继续校验", "四向车");
                for (int i = 0; i < emptyRows.Count; i++) {
                    //防止有货位禁用了
                    var list = db.Queryable<Location>().Where(loc => loc.S_AREA_CODE == area &&
                                                                    loc.N_LAYER == layer &&
                                                                    loc.N_ROADWAY == roadway &&
                                                                    loc.N_ROW_GROUP == group &&
                                                                    loc.N_ROW == emptyRows[i].N_ROW &&
                                                                    loc.C_ENABLE == "Y").OrderBy(loc => loc.N_POS).ToList();
                    if (list.Count >= moveCount) {
                        moveList.AddRange(list.Take(moveCount));
                        break;
                    }
                }
            }
            if (moveList.Count == 0) {
                //3 查找同物料同批次的移库,要求剩余空位超过moveCount
                var rowList = rowDataList.Where(x => x.MaxUsedPos > 0 && x.MaxUsedPos < x.MaxPos && (x.MaxPos - x.MaxUsedPos) >= moveCount).ToList();
                if (layer == start.N_LAYER && roadway == start.N_ROADWAY && group == start.N_ROW_GROUP) {
                    //和起点同区域，把起点排给排除了
                    rowList.Remove(rowDataList.Where(x => x.N_ROW == start.N_ROW).FirstOrDefault());
                }
                if (rowList.Count() > 0) {
                    ISugarQueryable<Location> query = null;
                    var whereOrList = new List<string>();
                    foreach (var row in rowList) {
                        whereOrList.Add($" (N_ROW = {row.N_ROW} AND N_POS = {row.MaxUsedPos}) ");
                    }
                    string whereSql = "(" + string.Join(" OR ", whereOrList) + ")";
                    query = db.Queryable<Location>()
                        .Where(loc => loc.S_AREA_CODE == area
                            && loc.N_LAYER == layer
                            && loc.N_ROADWAY == roadway
                            && loc.N_ROW_GROUP == group);

                    query = query.Where(whereSql);
                    //查找物料和批次匹配的货位的，没有放满的货位
                    var finalQuery = query.LeftJoin<LocCntrRel>((loc, cntr) => loc.S_CODE == cntr.S_LOC_CODE)
                                    .LeftJoin<InvDetail>((loc, cntr, sku) => cntr.S_CNTR_CODE == sku.S_CNTR_CODE)
                                    .Where((loc, cntr, sku) => sku.S_ITEM_CODE == item && sku.S_BATCH_NO == batch);

                    var listMatch = finalQuery.ToList();
                    if (listMatch.Count > 0) {
                        LogHelper.Info($"四向车满托查找移库的终点同物料同批次排{layer}巷道{roadway}组{group}空排，继续校验", "四向车");
                        //匹配之后循环选择这一排的货位，成功就跳出循环
                        foreach (var location in listMatch) {
                            var list = db.Queryable<Location>()
                                 .Where(loc =>
                                     loc.S_AREA_CODE == area &&
                                     loc.N_LAYER == layer &&
                                     loc.N_ROADWAY == roadway &&
                                     loc.N_ROW_GROUP == group &&
                                     loc.N_ROW == location.N_ROW &&
                                     loc.N_POS > location.N_POS &&
                                     loc.C_ENABLE == "Y")
                                 .OrderBy(loc => loc.N_POS)
                                 .ToList();

                            if (list.Count >= moveCount) {
                                moveList.AddRange(list.Take(moveCount));
                                pri = 1;
                                break;
                            }
                        }
                    }
                    if (moveList.Count == 0) {
                        //4 查找同物料不同批次的移库,要求剩余空位超过moveCount

                        //查找物料匹配的货位的，没有放满的货位
                        finalQuery = query.LeftJoin<LocCntrRel>((loc, cntr) => loc.S_CODE == cntr.S_LOC_CODE)
                                    .LeftJoin<InvDetail>((loc, cntr, sku) => cntr.S_CNTR_CODE == sku.S_CNTR_CODE)
                                    .Where((loc, cntr, sku) => sku.S_ITEM_CODE == item && sku.S_BATCH_NO != batch);

                        listMatch = finalQuery.ToList();
                        if (listMatch.Count > 0) {
                            LogHelper.Info($"四向车满托查找移库的终点同物料不同批次排{layer}巷道{roadway}组{group}空排，继续校验", "四向车");
                            //匹配之后循环选择这一排的货位，成功就跳出循环
                            foreach (var location in listMatch) {
                                var list = db.Queryable<Location>()
                                     .Where(loc =>
                                         loc.S_AREA_CODE == area &&
                                         loc.N_LAYER == layer &&
                                         loc.N_ROADWAY == roadway &&
                                         loc.N_ROW_GROUP == group &&
                                         loc.N_ROW == location.N_ROW &&
                                         loc.N_POS > location.N_POS &&
                                         loc.C_ENABLE == "Y")
                                     .OrderBy(loc => loc.N_POS)
                                     .ToList();

                                if (list.Count >= moveCount) {
                                    moveList.AddRange(list.Take(moveCount));
                                    pri = 2;
                                    break;
                                }
                            }
                        }
                    }


                }
            }


            return (moveList, pri);
        }
    }
}
