using SqlSugar;

namespace WebApplication1 {

    /// <summary>
    /// 堆垛机库
    /// </summary>
    internal class StackerCraneArea {

        /// <summary>
        /// 单深(不用考虑混不混，调方法前考虑巷道均衡）
        /// </summary>
        /// <param name="area"></param>
        /// <param name="aisle"></param>
        /// <returns></returns>
        public static Location GetInSingle(SqlSugarClient db, string area, int aisle) {
            Location end = null;
            if (db.Queryable<Location>().Any(a => a.S_AREA_CODE == area && a.N_ROADWAY == aisle && a.N_POS == 2)) {
                Console.WriteLine("双深库区请调用GetInDouble方法");
                return end;
            }
            var groupList = db.Queryable<Location>()
               .Where(a => a.S_AREA_CODE == area && a.N_ROADWAY == aisle && a.N_CURRENT_NUM == 0 && a.N_LOCK_STATE == 0 && a.C_ENABLE == "Y")
               .GroupBy(a => a.N_ROW_GROUP)
               .Select(a => new
               {
                   N_ROW_GROUP = a.N_ROW_GROUP,
                   Count = SqlFunc.AggregateCount(1)   // 本组行数
               })
               .ToList();
            var maxGroup = groupList.OrderByDescending(a => a.Count).FirstOrDefault();
            //选择空货位数量多的
            end = db.Queryable<Location>().Where(a => a.S_AREA_CODE == area && a.N_ROADWAY == aisle && a.N_ROW_GROUP == maxGroup.N_ROW_GROUP && a.N_CURRENT_NUM == 0 && a.N_LOCK_STATE == 0 && a.C_ENABLE == "Y").First();
            return end;
        }

        /// <summary>
        /// 双深混箱入（调方法前考虑巷道均衡）
        /// </summary>
        /// <returns></returns>
        public static Location GetInDoubleMix(SqlSugarClient db, string area, int aisle) {
            Location end = null;
            var groupList = db.Queryable<Location>()
              .Where(a => a.S_AREA_CODE == area && a.N_ROADWAY == aisle && a.N_CURRENT_NUM == 0 && a.N_LOCK_STATE == 0 && a.C_ENABLE == "Y")
              .GroupBy(a => a.N_ROW_GROUP)
              .Select(a => new
              {
                  N_ROW_GROUP = a.N_ROW_GROUP,
                  Count = SqlFunc.AggregateCount(1)
              })
              .ToList();
            var maxGroup = groupList.OrderByDescending(a => a.Count).FirstOrDefault();
            var lockedList = db.Queryable<Location>().Where(a => a.S_AREA_CODE == area && a.N_ROADWAY == aisle && a.N_ROW_GROUP == maxGroup.N_ROW_GROUP && a.N_LOCK_STATE > 0).Select(a => new { col = a.N_COL, layer = a.N_LAYER }).Distinct().ToList();
            var list = db.Queryable<Location>().Where(a => a.S_AREA_CODE == area && a.N_ROADWAY == aisle && a.N_ROW_GROUP == maxGroup.N_ROW_GROUP && a.N_CURRENT_NUM == 0 && a.N_LOCK_STATE == 0 && a.C_ENABLE == "Y").OrderBy(a => a.N_POS).OrderBy(a => a.N_COL).OrderBy(a => a.N_LAYER).ToList();
            // (如果内外2个货位都是空闲的，都能查到，但是排序后还是会选择内侧的，不用提前把外侧排除)
            if (list.Count > 0) {
                for (int i = 0; i < list.Count; i++) {
                    if (!lockedList.Any(a => a.col == list[i].N_COL && a.layer == list[i].N_LAYER)) {
                        //内外侧都没有锁
                        if (list[i].N_POS == 1) {
                            //做个校验防止pos=2的货位异常，内侧空闲正常外侧也是空闲
                            var outside = db.Queryable<Location>().Where(a => a.S_AREA_CODE == area && a.N_ROADWAY == aisle && a.N_ROW_GROUP == list[i].N_ROW_GROUP && a.N_COL == list[i].N_COL && a.N_POS == 2 && a.N_CURRENT_NUM == 0).First();
                            if (outside != null) {
                                end = list[i];
                                break;
                            }
                        }
                        else {
                            end = list[i];
                            break;
                        }
                    }

                }
            }
            return end;
        }

        /// <summary>
        /// 双深不混箱入，相同物料一个内一个外（调方法前考虑巷道均衡）参考四向车库写法
        /// </summary>
        /// <param name="db"></param>
        /// <param name="area"></param>
        /// <param name="aisle"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static Location GetInDoubleNotMix(SqlSugarClient db, string area, int aisle, string item = "") {
            Location end = null;
            var groupList = db.Queryable<Location>()
             .Where(a => a.S_AREA_CODE == area && a.N_ROADWAY == aisle && a.N_CURRENT_NUM == 0 && a.N_LOCK_STATE == 0 && a.C_ENABLE == "Y")
             .GroupBy(a => a.N_ROW_GROUP)
             .Select(a => new
             {
                 N_ROW_GROUP = a.N_ROW_GROUP,
                 Count = SqlFunc.AggregateCount(1)
             })
             .OrderByDescending(a => a.Count)
             .ToList();
            //巷道侧均衡
            bool outside = false;
            for (int g = 0; g < groupList.Count; g++) {
                var rowgroup = groupList[g].N_ROW_GROUP;
                var colLayerDataList = db.Queryable<Location>()
                  .Where(loc => loc.S_AREA_CODE == area && loc.N_ROADWAY == aisle && loc.N_ROW_GROUP == rowgroup)
                  .GroupBy(loc => new { loc.N_COL, loc.N_LAYER })    // 按列和层分组
                  .Having(loc => SqlFunc.AggregateSum(SqlFunc.IIF(loc.N_LOCK_STATE != 0, 1, 0)) == 0)
                  .Select(loc => new
                  {
                      N_COL = loc.N_COL,
                      N_LAYER = loc.N_LAYER,
                      MaxUsedPos = SqlFunc.AggregateMax(SqlFunc.IIF(loc.N_CURRENT_NUM > 0, loc.N_POS, 0)),
                      MaxPos = SqlFunc.AggregateMax(loc.N_POS),
                  })
                  .OrderBy(loc => loc.N_COL)
                  .ToList();
                ISugarQueryable<Location> query = null;
                var notfullList = colLayerDataList.Where(a => a.MaxUsedPos == 1).ToList();
                var emptyList = colLayerDataList.Where(a => a.MaxUsedPos == 0).ToList();
                if (notfullList.Count > 0) {
                    //没放满，判断是否匹配
                    var whereOrList = new List<string>();
                    foreach (var row in notfullList) {
                        whereOrList.Add($" (N_COL = {row.N_COL} AND N_LAYER = {row.N_LAYER} AND N_POS = {row.MaxUsedPos}) ");
                    }
                    string whereSql = "(" + string.Join(" OR ", whereOrList) + ")";
                    query = db.Queryable<Location>()
                        .Where(loc => loc.S_AREA_CODE == area
                            && loc.N_ROADWAY == aisle
                            && loc.N_ROW_GROUP == groupList[g].N_ROW_GROUP);

                    query = query.Where(whereSql);
                    //query 是共用的
                    var finalQuery = query.LeftJoin<LocCntrRel>((loc, cntr) => loc.S_CODE == cntr.S_LOC_CODE)
                                    .LeftJoin<InvDetail>((loc, cntr, sku) => cntr.S_CNTR_CODE == sku.S_CNTR_CODE)
                                    .Where((loc, cntr, sku) => (item == "" && sku.S_ITEM_CODE == null) || sku.S_ITEM_CODE == item);

                    var listMatch = finalQuery.ToList();
                    //查找匹配的货位的，没有放满的货位
                    if (listMatch.Count > 0) {
                        //匹配之后循环选择这一排的货位，成功就跳出循环
                        foreach (var location in listMatch) {
                            end = db.Queryable<Location>()
                                 .Where(loc =>
                                     loc.S_AREA_CODE == area &&
                                     loc.N_ROADWAY == aisle &&
                                     loc.N_ROW_GROUP == rowgroup &&
                                     loc.N_LAYER == location.N_LAYER &&
                                     loc.N_COL == location.N_COL &&
                                     loc.N_POS == 2 &&
                                     loc.N_CURRENT_NUM == 0 &&      //第一次查询已经判断当前没有货
                                     loc.N_LOCK_STATE == 0 &&       //第一次查询已经判断当前没有锁，因为是2次查询再校验一次
                                     loc.C_ENABLE == "Y")
                                 .First();

                            if (end != null) {
                                outside = true;
                                break;
                            }
                        }
                    }
                }
                if (end == null && emptyList.Count > 0) {
                    foreach (var row in emptyList) {
                        end = db.Queryable<Location>()
                            .Where(loc =>
                                loc.S_AREA_CODE == area &&
                                     loc.N_ROADWAY == aisle &&
                                     loc.N_ROW_GROUP == rowgroup &&
                                     loc.N_LAYER == row.N_LAYER &&
                                     loc.N_COL == row.N_COL &&
                                     loc.N_POS == 1 &&
                                     loc.N_CURRENT_NUM == 0 &&      //第一次查询已经判断当前没有货
                                     loc.N_LOCK_STATE == 0 &&       //第一次查询已经判断当前没有锁，因为是2次查询再校验一次
                                     loc.C_ENABLE == "Y")
                            .First();
                        if (end != null) {
                            break;
                        }
                    }
                }
                if (end != null && outside) {
                    break;
                }
            }
            return end;
        }

        /// <summary>
        /// 简单出库，当作密集库
        /// </summary>
        /// <param name="db"></param>
        /// <param name="area"></param>
        /// <param name="roadway">一般会遍历巷道</param>
        /// <param name="item">空默认空托盘出库</param>
        /// <returns></returns>
        public static Location GetOutNotMix(SqlSugarClient db, string area, int roadway, string item = "") {
            Location start = null;
            for (int i = 1; i <= 2; i++) {
                var rowDataList = db.Queryable<Location>()
                                 .Where(loc => loc.S_AREA_CODE == area && loc.N_ROADWAY == roadway && loc.N_ROW_GROUP == i)
                                 .GroupBy(loc => new { loc.N_COL, loc.N_LAYER })
                                 .Having(loc => SqlFunc.AggregateSum(SqlFunc.IIF(loc.N_LOCK_STATE != 0, 1, 0)) == 0)
                                 .Select(loc => new
                                 {
                                     N_COL = loc.N_COL,
                                     N_LAYER = loc.N_LAYER,
                                     MaxUsedPos = SqlFunc.AggregateMax(SqlFunc.IIF(loc.N_CURRENT_NUM > 0, loc.N_POS, 0)),
                                     MaxPos = SqlFunc.AggregateMax(loc.N_POS),

                                 })
                                 .OrderByDescending(loc => loc.MaxUsedPos)
                                 .ToList();
                var rowList = rowDataList.Where(x => x.MaxUsedPos > 0).ToList();
                if (rowList.Count > 0) {
                    ISugarQueryable<Location> query = null;
                    var whereOrList = new List<string>();
                    foreach (var row in rowList) {
                        whereOrList.Add($" (N_COL = {row.N_COL} AND N_LAYER = {row.N_LAYER} AND N_POS = {row.MaxUsedPos}) ");
                    }
                    string whereSql = "(" + string.Join(" OR ", whereOrList) + ")";
                    query = db.Queryable<Location>()
                        .Where(loc => loc.S_AREA_CODE == area
                            && loc.N_ROADWAY == roadway
                            && loc.N_ROW_GROUP == i);

                    query = query.Where(whereSql);
                    var finalQuery = query.LeftJoin<LocCntrRel>((loc, lcr) => loc.S_CODE == lcr.S_LOC_CODE)
                                          .LeftJoin<InvDetail>((loc, lcr, inv) => lcr.S_CNTR_CODE == inv.S_CNTR_CODE && ((item == "" && inv.S_ITEM_CODE == null) || (inv.S_ITEM_CODE == item)));
                    var listMatch = finalQuery.OrderBy(loc => loc.N_POS).OrderBy(loc => loc.N_COL).ToList();
                    if (listMatch.Count > 0) {
                        foreach (var loc in listMatch) {
                            start = loc; break;
                        }
                    }
                }
                if (start != null) {
                    break;
                }
            }

            return start;
        }
        /// <summary>
        /// 整托出，查找时间早的(是否要移库再作业启动或者任务推送时候判断)
        /// </summary>
        /// <param name="db"></param>
        /// <param name="area"></param>
        /// <param name="item"></param>
        /// <returns>如果需要移库move就不是null</returns>
        public static (Location start, string cntr) GetOutFIFO(SqlSugarClient db, string area, string item = "") {
            Location start = null;
            string cntr = null;
            var res = db.Queryable<Location>().Where(a => a.S_AREA_CODE == area && a.N_CURRENT_NUM == 1 && a.N_LOCK_STATE == 0)
                .InnerJoin<LocCntrRel>((a, b) => a.S_CODE == b.S_LOC_CODE)
                .InnerJoin<InvDetail>((a, b, c) => b.S_CNTR_CODE == c.S_CNTR_CODE && c.S_ITEM_CODE == item)
                .OrderBy((a, b, c) => c.S_BATCH_NO).OrderByDescending((a, b, c) => a.N_POS)
                .Select((a, b, c) => new { Loc = a, Rel = b, Inv = c })
                .First();
            if (res != null) {
                start = res.Loc;
                cntr = res.Rel.S_CNTR_CODE;
            }
            //有优化空间，如果最早批次几个都是内深位，那么优先级是外侧空无锁优先级最高可以不用移库，如果外侧有货或者入库锁要等入库完成移库
            return (start, cntr);
        }
        public static (Location start, string cntr) GetOutFIFOPlus(SqlSugarClient db, string area, string item = "") {
            Location start = null;
            string cntr = null;
            // 第一步：子查询，拿到【最早的批次号】
            var minBatchQry = db.Queryable<Location>()
                .Where(a => a.S_AREA_CODE == area && a.N_CURRENT_NUM == 1 && a.N_LOCK_STATE == 0)
                .InnerJoin<LocCntrRel>((a, b) => a.S_CODE == b.S_LOC_CODE)
                .InnerJoin<InvDetail>((a, b, c) => b.S_CNTR_CODE == c.S_CNTR_CODE && c.S_ITEM_CODE == item)
                .Min((a, b, c) => c.S_BATCH_NO);

            if (minBatchQry != null) {
                //第二步：查询【该最早批次下全部满足的数据】，再内部按库位N_POS倒序排序
                var list = db.Queryable<Location>()
                    .Where(a => a.S_AREA_CODE == area && a.N_CURRENT_NUM == 1 && a.N_LOCK_STATE == 0)
                    .InnerJoin<LocCntrRel>((a, b) => a.S_CODE == b.S_LOC_CODE)
                    .InnerJoin<InvDetail>((a, b, c) => b.S_CNTR_CODE == c.S_CNTR_CODE && c.S_ITEM_CODE == item)
                    .Where((a, b, c) => c.S_BATCH_NO == minBatchQry) //限定只取最早这一个批次
                    .OrderByDescending((a, b, c) => a.N_POS) //同批次内部，库位N_POS降序
                    .Select((a, b, c) => new { Loc = a, Rel = b, Inv = c })
                    .ToList();
                int pri = 3;
                foreach (var x in list) {
                    if (x.Loc.N_POS == 2) {
                        start = x.Loc;
                        cntr = x.Rel.S_CNTR_CODE;
                        break;
                    }
                    else {
                        //内深的排个优先级
                        var outside = db.Queryable<Location>().Where(a => a.S_AREA_CODE == area && a.N_POS == 2 && a.N_ROW_GROUP == x.Loc.N_ROW_GROUP).First();
                        if (outside != null) {
                            if (outside.N_CURRENT_NUM == 0) {
                                if (outside.N_LOCK_STATE == 0) {
                                    start = x.Loc;
                                    cntr = x.Rel.S_CNTR_CODE;
                                    break;
                                }
                                else {
                                    //外侧空有锁
                                    if (pri > 2) {
                                        start = x.Loc;
                                        cntr = x.Rel.S_CNTR_CODE;
                                        pri = 2;
                                    }
                                }

                            }
                            else {
                                if (outside.N_LOCK_STATE == 1) {
                                    //外侧满有锁
                                    if (pri > 0) {
                                        start = x.Loc;
                                        cntr = x.Rel.S_CNTR_CODE;
                                        pri = 0;
                                    }
                                }
                                else {
                                    //外侧满无锁
                                    if (pri > 1) {
                                        start = x.Loc;
                                        cntr = x.Rel.S_CNTR_CODE;
                                        pri = 1;
                                    }
                                }
                            }
                        }
                    }
                }

            }
            return (start, cntr);
        }
        /// <summary>
        /// 按数量出，严格要求先进先出，正在搬运中的也要计算到
        /// </summary>
        /// <param name="db"></param>
        /// <param name="area"></param>
        /// <param name="item"></param>
        /// <param name="qty"></param>
        /// <param name="checkQtyComplete">如果出多个托盘判断数量累计是否满足</param>
        /// <returns></returns>
        public static (List<Location> locs, List<string> cntrs, List<float> qtys) GetOutMixFIFOWithQty(SqlSugarClient db, string area, string item, float qty, bool checkQtyComplete) {
            var locs = new List<Location>();
            var cntrs = new List<string>();
            var qtys = new List<float>();
            if (!checkQtyComplete || db.Queryable<InvDetail>().Where(a => a.S_AREA_CODE == area && a.S_ITEM_CODE == item).Sum(a => a.F_QTY_VALID) > qty) {
                var list = db.Queryable<InvDetail>().Where(a => a.S_AREA_CODE == area && a.S_ITEM_CODE == item && a.F_QTY_VALID > 0)
               .LeftJoin<LocCntrRel>((a, b) => a.S_CNTR_CODE == b.S_CNTR_CODE)
               .LeftJoin<Location>((a, b, c) => c.S_CODE == b.S_LOC_CODE)
               .OrderBy(a => a.S_BATCH_NO).OrderBy(a => a.F_QTY_VALID).OrderByDescending((a, b, c) => c.N_POS)
               .Select((a, b, c) => new { Loc = c, Rel = b, Inv = a }).ToList();
                if (list.Count > 0) {
                    float sum = 0;
                    for (int i = 0; i < list.Count; i++) {
                        locs.Add(list[i].Loc);
                        cntrs.Add(list[i].Rel.S_CNTR_CODE);
                        if (list[i].Inv.F_QTY_VALID >= qty - sum) {
                            qtys.Add(qty - sum);
                            sum += qty - sum;
                        }
                        else {
                            qtys.Add(list[i].Inv.F_QTY_VALID);
                            sum += list[i].Inv.F_QTY_VALID;
                        }
                        if (sum >= qty) { break; }
                    }
                }
            }
            return (locs, cntrs, qtys);
        }
        /// <summary>
        /// 混箱的不能直接找最大使用pos，要查所有的，如果满足条件的在内侧，需要返回移库货位
        /// </summary>
        /// <param name="db"></param>
        /// <param name="area"></param>
        /// <param name="roadway"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static Location GetOutMix(SqlSugarClient db, string area, int roadway, string item = "") {
            Location start = null;
            return start;
        }

        internal static void Init() {
            var db = new SqlHelper<object>().GetInstance();
            //TKQ
            List<Location> list = new List<Location>();
            //巷道侧
            for (int i = 1; i <= 2; i++) {
                //排
                for (int j = 1; j <= 2; j++) {
                    //层
                    for (int k = 1; k <= 5; k++) {
                        //列
                        for (int l = 1; l <= 10; l++) {
                        }
                    }
                }
            }
            db.Insertable(list).ExecuteCommand();

        }
        internal static void Test() {
            var db = new SqlHelper<object>().GetInstance();
            Location loc = null;
            //LocationHelper.BindingLoc("TKQ-1-1-1-1", "AAA", "");
            //LocationHelper.BindingLoc("TKQ-1-2-1-1", "AAA", "");
            //LocationHelper.BindingLoc("TKQ-1-1-1-3", "AAA", "");

            //LocationHelper.BindingLoc("TKQ-2-4-2-3", "AAA", "");
            //LocationHelper.BindingLoc("TKQ-2-4-4-4", "BBB", "");
            //LocationHelper.BindingLoc("TKQ-2-4-4-10", "AAA", "");
            //LocationHelper.BindingLoc("TKQ-2-4-4-1", "AAA", "");

            //loc = GetIn(db, "TKQ", 1);
            //loc = GetInDoubleMix(db, "TKQ", 1);
            Random r = new Random();
            for (int i = 1; i <= 10; i++) {
                //loc = GetInDoubleMix(db, "TKQ", 1);
                var item = r.Next(1, 100) < 50 ? "AAA" : "BBB";
                loc = GetInDoubleNotMix(db, "TKQ", 1, item);
                if (loc != null) {
                    LocationHelper.BindingCntrItem(loc.S_CODE, item, "");
                }
            }
            for (int i = 1; i <= 20; i++) {
                //loc = GetInDoubleMix(db, "TKQ", 1);
                var item = r.Next(1, 100) < 50 ? "AAA" : "BBB";
                loc = GetOutNotMix(db, "TKQ", 1, item);
                if (loc != null) {
                    LocationHelper.UnBindTest(loc.S_CODE);
                }
            }
        }
    }
}
