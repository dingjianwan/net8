using Newtonsoft.Json;

namespace WebApplication1 {
    internal class LocationHelper {
        private static Dictionary<string, Location> locations = null;
        private static Dictionary<string, LocationExt> locationExts = null;

        static LocationHelper() {
            //初始化location加入到字典缓存
            locations = new Dictionary<string, Location>();
            var list = GetAllLocList();
            if (list.Count > 0) {
                list.ForEach(a => {
                    try {
                        locations.Add(a.S_CODE.Trim(), a);
                    }
                    catch (Exception ex) {
                        LogHelper.Error($"LocationHelper-Add站点{a.S_CODE}异常,异常信息={ex.Message}", ex);
                    }
                });
            }
            //初始化locationExt加入到集合缓存
            locationExts = new Dictionary<string, LocationExt>();
            var exts = GetAllLocExtList();
            if (exts.Count > 0) {
                exts.ForEach(a => {
                    locationExts.Add($"{a.S_LOC_CODE.Trim()}_{a.S_EQ_TYPE.Trim()}", a);
                });
            }
        }

        internal static bool CheckExist(string loc) {
            return locations.Keys.Contains(loc);
        }
        internal static Location GetLocation(string loc) {
            if (CheckExist(loc.Trim())) {
                return locations[loc.Trim()];
            }
            return null;
        }

        /// <summary>
        /// 获取货位站点信息
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        internal static int GetAgvSite(string loc) {
            var site = 0;
            if (locations.Keys.Contains(loc.Trim())) {
                var location = locations[loc.Trim()];
                int.TryParse(location.S_AGV_SITE, out site);
            }
            else {
                var location = GetLoc(loc.Trim());
                if (location != null) {
                    locations.Add(loc.Trim(), location);
                    int.TryParse(location.S_AGV_SITE, out site);
                }
            }
            return site;
        }

        internal static int GetAgvSite(string loc, string actionType) {
            var site = 0;
            var key = $"{loc.Trim()}_{actionType.Trim()}";
            if (locationExts.Keys.Contains(loc.Trim())) {
                var location = locationExts[loc.Trim()];
                site = int.Parse(location.S_AGV_SITE);
            }
            return site;
        }

        /// <summary>
        /// 获取所有货位信息
        /// </summary>
        /// <returns></returns>
        internal static List<Location> GetAllLocList() {
            var db = new SqlHelper<object>().GetInstance();
            return db.Queryable<Location>().ToList();
        }
        internal static Location GetLoc(string code) {
            var db = new SqlHelper<object>().GetInstance();
            return db.Queryable<Location>().Where(a => a.S_CODE.Trim() == code).First();
        }


        internal static LocationExt GetLocExtByCode(string locCode) {
            var db = new SqlHelper<object>().GetInstance();
            return db.Queryable<LocationExt>().Where(a => a.S_LOC_CODE.Trim() == locCode).First();
        }
        internal static LocationExt GetLocExtByTrayType(string trayType) {
            var db = new SqlHelper<object>().GetInstance();
            return db.Queryable<LocationExt>().Where(a => a.S_EQ_TYPE.Trim() == trayType).First();
        }
        /// <summary>
        ///获取所有货位扩展信息 
        /// </summary>
        /// <returns></returns>
        internal static List<LocationExt> GetAllLocExtList() {
            var db = new SqlHelper<object>().GetInstance();
            return db.Queryable<LocationExt>().ToList();
        }

        /// <summary>
        /// 判断没有入库锁和出库锁
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        internal static bool CheckLocFree(string code) {
            var result = false;
            var loc = GetLoc(code);
            if (loc != null) {
                result = loc.N_LOCK_STATE == 0;
            }
            return result;
        }

        internal static List<LocCntrRel> GetLocCntrRel(string loc) {
            //1.0 查货位容器表
            var db = new SqlHelper<object>().GetInstance();
            var result = db.Queryable<LocCntrRel>().Where(a => a.S_LOC_CODE == loc.Trim()).ToList();
            return result;
        }


        /// <summary>
        /// 根据货位集合获取有容器的货位
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        internal static List<Location> GetLocList(List<string> loc) {
            //1.0 查货位容器表
            var db = new SqlHelper<object>().GetInstance();
            var list = db.Queryable<Location>().Where(a => loc.Contains(a.S_CODE) && a.N_CURRENT_NUM > 0).ToList();
            return list;
        }

        /// <summary>
        /// 根据货位编码获取货位信息，不管有没有容器
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        internal static List<Location> GetLocListAny(List<string> loc) {
            //1.0 查货位容器表
            var db = new SqlHelper<object>().GetInstance();
            var list = db.Queryable<Location>().Where(a => loc.Contains(a.S_CODE)).ToList();
            return list;

        }

        /// <summary>
        /// 根据货位集合获取有容器 没有锁的货位
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        internal static List<Location> GetLocListFree(List<string> loc) {
            //1.0 查货位容器表
            var db = new SqlHelper<object>().GetInstance();
            var list = db.Queryable<Location>().Where(a => loc.Contains(a.S_CODE) && a.N_CURRENT_NUM > 0 && a.N_LOCK_STATE == 0).ToList();
            return list;

        }

        /// <summary>
        /// 获取固定数量容器 没有锁的货位
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="current"></param>
        /// <returns></returns>
        internal static List<Location> GetLocListFree(List<string> loc, int current) {
            //1.0 查货位容器表
            var db = new SqlHelper<object>().GetInstance();
            var list = db.Queryable<Location>().Where(a => loc.Contains(a.S_CODE) && a.N_CURRENT_NUM == current && a.N_LOCK_STATE == 0).ToList();
            return list;

        }


        /// <summary>
        /// 根据货位集合获取 没有容器 没有锁的货位
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        internal static List<Location> GetLocListEmptyFree(List<string> loc) {
            //1.0 查货位容器表
            var db = new SqlHelper<object>().GetInstance();
            var list = db.Queryable<Location>().Where(a => loc.Contains(a.S_CODE) && a.N_CURRENT_NUM == 0 && a.N_LOCK_STATE == 0).ToList();
            return list;
        }

        /// <summary>
        /// 只是锁定货位
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="lockState">1:入库锁、2:出库锁、3:其它锁</param>
        /// <returns></returns>
        public static bool LockLoc(string loc, int lockState) {
            var res = false;
            var db = new SqlHelper<object>().GetInstance();
            var model = db.Queryable<Location>().Where(a => a.S_CODE == loc).First();
            if (model != null) {

                model.N_LOCK_STATE = lockState;
                model.S_LOCK_STATE = model.GetLockStateStr();
                model.T_MODIFY = DateTime.Now;
                res = db.Updateable(model).UpdateColumns(it => new { it.N_LOCK_STATE, it.S_LOCK_STATE, it.T_MODIFY }).ExecuteCommand() > 0;

            }
            return res;
        }



        /// <summary>
        /// 只是解锁货位
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        public static bool UnLockLoc(string loc, string taskNo) {
            LogHelper.Info("UnLockLoc:" + loc);
            var res = false;
            var db = new SqlHelper<object>().GetInstance();
            var model = db.Queryable<Location>().Where(a => a.S_CODE == loc).First();
            if (model != null && (String.IsNullOrEmpty(model.S_LOCK_OP) || model.S_LOCK_OP.Trim() == taskNo)) {
                model.N_LOCK_STATE = 0;
                model.S_LOCK_STATE = model.GetLockStateStr();
                model.T_MODIFY = DateTime.Now;
                res = db.Updateable(model).UpdateColumns(it => new { it.N_LOCK_STATE, it.S_LOCK_STATE, it.T_MODIFY }).ExecuteCommand() > 0;
            }
            else {
                LogHelper.Info("UnLockLoc 失败");
            }
            return res;
        }
        /// <summary>
        /// 解绑货位，必须没有业务锁定才可以解绑，主要用于测试
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        public static bool UnBindTest(string loc) {
            var res = false;
            var db = new SqlHelper<object>().GetInstance();
            var location = db.Queryable<Location>().Where(a => a.S_CODE == loc).First();
            if (location.N_LOCK_STATE == 0 && string.IsNullOrEmpty(location.S_LOCK_OP)) {
                LogHelper.Info($"{loc}解绑托盘：全部托盘码");
                db.BeginTran();
                try {
                    db.Deleteable<LocCntrRel>().Where(it => it.S_LOC_CODE == loc).ExecuteCommand();
                    location.N_CURRENT_NUM = 0;
                    location.T_MODIFY = DateTime.Now;
                    db.Updateable(location).UpdateColumns(it => new { it.N_CURRENT_NUM, it.T_MODIFY }).ExecuteCommand();
                    db.Ado.CommitTran();
                    res = true;
                }
                catch (Exception ex) {
                    db.Ado.RollbackTran();
                    LogHelper.Error($"{loc}解绑异常={ex.Message}", ex);
                }
            }
            return res;
        }
        /// <summary>
        /// 解绑已经存在容器
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="cntrs"></param>
        /// <returns></returns>
        public static bool UnBindingLoc(string loc, List<string> cntrs, string taskNo) {
            var res = false;
            var db = new SqlHelper<object>().GetInstance();
            var location = db.Queryable<Location>().Where(a => a.S_CODE == loc).First();
            if (location.N_TYPE == 0) {
                LogHelper.Info($"{loc}取货完成解绑托盘：全部托盘码：{JsonConvert.SerializeObject(cntrs)}");
                db.BeginTran();
                try {
                    cntrs.ForEach(a => {
                        if (!string.IsNullOrEmpty(a)) {
                            string trayCode = a.Trim();
                            db.Deleteable<LocCntrRel>().Where(it => it.S_CNTR_CODE == trayCode && it.S_LOC_CODE == loc).ExecuteCommand();
                            var invList = db.Queryable<InvDetail>().Where(i => i.S_CNTR_CODE == a).ToList();
                            if (invList.Count > 0) {
                                invList.ForEach(b => {
                                    b.S_LOC_CODE = "";
                                    b.T_MODIFY = DateTime.Now;
                                    db.Updateable<InvDetail>(b).UpdateColumns(x => new { x.S_LOC_CODE, x.T_MODIFY }).ExecuteCommand();
                                });
                            }

                        }
                    });
                    location.N_CURRENT_NUM = db.Queryable<LocCntrRel>().Count(a => a.S_LOC_CODE == loc);
                    location.T_MODIFY = DateTime.Now;
                    if (string.IsNullOrEmpty(location.S_LOCK_OP) || location.S_LOCK_OP.Trim() == taskNo) {
                        location.N_LOCK_STATE = 0;
                        location.S_LOCK_STATE = location.GetLockStateStr();
                        db.Updateable(location).UpdateColumns(it => new { it.N_LOCK_STATE, it.S_LOCK_STATE, it.N_CURRENT_NUM, it.T_MODIFY }).ExecuteCommand();
                    }
                    else {
                        //特殊情况，如果货位被其它任务锁住了，不可以解绑，还是锁定状态，只能锁定的任务才能解锁，或者人工介入
                        db.Updateable(location).UpdateColumns(it => new { it.N_CURRENT_NUM, it.T_MODIFY }).ExecuteCommand();
                    }

                    db.Ado.CommitTran();

                    res = true;
                }
                catch (Exception ex) {
                    db.Ado.RollbackTran();
                    LogHelper.Error($"任务{taskNo}起点{loc}解绑异常={ex.Message}", ex);
                }
            }
            return res;
        }
        /// <summary>
        /// 生成容器 库存 并且绑定
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="item"></param>
        /// <param name="batch"></param>
        /// <returns></returns>
        public static bool BindingCntrItem(string loc, string item, string batch) {
            var res = false;
            var db = new SqlHelper<object>().GetInstance();
            db.BeginTran();
            var res1 = 0;
            var res2 = 0;
            var cntrCode = ContainerHelper.GenerateCntrNo();
            try {
                var cntr = new Container { S_CODE = cntrCode, N_DETAIL_COUNT = 1 };
                var inv = new InvDetail { S_CNTR_CODE = cntrCode, S_ITEM_CODE = item, S_BATCH_NO = batch };
                res1 = db.Insertable(cntr).ExecuteCommand();
                res2 = db.Insertable(inv).ExecuteCommand();
                db.CommitTran();
            }
            catch (Exception ex) {
                db.Ado.RollbackTran();
            }
            if (res1 == 1 && res2 == 1) {
                res = BindingLoc(loc, new List<string> { cntrCode }, "");
            }
            return res;
        }
        /// <summary>
        /// 货位绑定已经存在容器
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="cntrs"></param>
        /// <returns></returns>
        public static bool BindingLoc(string loc, List<string> cntrs, string taskNo) {
            LogHelper.Info($"任务{taskNo} 解锁终点货位 绑定终点货位{loc} 托盘 {JsonConvert.SerializeObject(cntrs)}");
            var res = false;
            var db = new SqlHelper<object>().GetInstance();
            var location = db.Queryable<Location>().Where(a => a.S_CODE == loc).First();

            if (location.N_TYPE == 0) {
                db.BeginTran();
                try {
                    var lcrList = db.Queryable<LocCntrRel>().Where(a => a.S_LOC_CODE == loc).ToList();
                    cntrs.ForEach(a => {
                        if (lcrList.Count(b => b.S_CNTR_CODE.Trim() == a) == 0 && !string.IsNullOrEmpty(a)) {
                            db.Insertable<LocCntrRel>(new LocCntrRel { S_LOC_CODE = loc, S_CNTR_CODE = a }).ExecuteCommand();
                        }
                        var invList = db.Queryable<InvDetail>().Where(i => i.S_CNTR_CODE == a).ToList();
                        if (invList.Count > 0) {
                            invList.ForEach(b => {
                                b.S_LOC_CODE = loc;
                                b.T_MODIFY = DateTime.Now;
                                db.Updateable<InvDetail>(b).UpdateColumns(x => new { x.S_LOC_CODE, x.T_MODIFY }).ExecuteCommand();
                            });
                        }
                    });
                    location.N_CURRENT_NUM = db.Queryable<LocCntrRel>().Count(a => a.S_LOC_CODE == loc);
                    location.T_MODIFY = DateTime.Now;
                    if (string.IsNullOrEmpty(location.S_LOCK_OP) || location.S_LOCK_OP.Trim() == taskNo) {
                        location.N_LOCK_STATE = 0;
                        location.S_LOCK_STATE = location.GetLockStateStr();
                        db.Updateable(location).UpdateColumns(it => new { it.N_LOCK_STATE, it.S_LOCK_STATE, it.N_CURRENT_NUM, it.T_MODIFY }).ExecuteCommand();
                    }
                    else {
                        //特殊情况，如果货位被其它任务锁住了，不可以解绑，还是锁定状态，只能锁定的任务才能解锁，或者人工介入
                        db.Updateable(location).UpdateColumns(it => new { it.N_CURRENT_NUM, it.T_MODIFY }).ExecuteCommand();
                    }

                    db.Ado.CommitTran();
                    res = true;
                }
                catch (Exception ex) {
                    db.Ado.RollbackTran();
                    LogHelper.Error($"任务{taskNo}终点{loc}绑定异常={ex.Message}", ex);
                }

            }

            return res;
        }


    }
}
