using Newtonsoft.Json;

namespace WebApplication1 {
    internal class TaskHelper {
        /// <summary>
        /// 自动根据日期生成任务号
        /// </summary>
        /// <returns></returns>
        internal static string GenerateTaskNo() {
            var date = DateTime.Now.ToString("yyMMdd");
            var id = SYSHelper.GetSerialNumber(date, "TN");
            return $"TN{date}{id.ToString().PadLeft(4, '0')}";
        }


        /// <summary>
        /// 更新任务状态
        /// </summary>
        /// <param name="task"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        internal static bool UpdateStatus(WMSTask task, int state) {
            var res = false;
            var db = new SqlHelper<WMSTask>().GetInstance();
            task.N_B_STATE = state;
            task.S_B_STATE = task.GetTaskStateStr();

            db.Updateable(task).UpdateColumns(it => new { it.S_B_STATE, it.N_B_STATE }).ExecuteCommand();

            return res;
        }
        internal static bool UpdateInfo(WMSTask task, string endBit, int state) {
            var res = false;
            var db = new SqlHelper<WMSTask>().GetInstance();
            task.N_B_STATE = state;
            task.S_B_STATE = task.GetTaskStateStr();
            task.S_END_LOC = endBit;
            db.Updateable(task).UpdateColumns(it => new { it.S_B_STATE, it.N_B_STATE, it.S_END_LOC }).ExecuteCommand();
            return res;
        }
        internal static WMSTask GetTask(string no) {
            var db = new SqlHelper<WMSTask>().GetInstance();
            var task = db.Queryable<WMSTask>().Where(a => a.S_CODE.Trim() == no).First();// && a.S_B_STATE != "取消" && a.S_B_STATE != "完成" && a.S_B_STATE != "失败"
            return task;
        }

        internal static List<WMSTask> GetTaskByStart(string bit) {
            var db = new SqlHelper<WMSTask>().GetInstance();
            var task = db.Queryable<WMSTask>().Where(a => a.S_START_LOC.Trim() == bit.Trim()).ToList();
            return task;
        }
        internal static List<WMSTask> GetTaskByEnd(string bit) {
            var db = new SqlHelper<WMSTask>().GetInstance();
            var task = db.Queryable<WMSTask>().Where(a => a.S_END_LOC.Trim() == bit.Trim()).ToList();
            return task;
        }
        internal static List<WMSTask> GetTaskByType(string taskType) {
            var db = new SqlHelper<WMSTask>().GetInstance();
            return db.Queryable<WMSTask>().Where(a => a.S_TYPE.Trim() == taskType).ToList();
        }
        internal static bool CreateTask(string from, string to, string taskType, int pri, string cntrInfo, string bsNo, string bsType, int startLayer = 1, int endLayer = 1) {
            //异常1：此处需要让实施创建数据表时将数据库表字段长度设置长一些-64，不然调用接口打印不出详细信息
            bool result = false;
            try {
                var fromLoc = LocationHelper.GetLocation(from);
                var endLoc = LocationHelper.GetLocation(to);
                if (fromLoc != null && endLoc != null) {
                    WMSTask wmsTask = new WMSTask()
                    {
                        S_CODE = GenerateTaskNo(),
                        S_START_WH = fromLoc.S_WH_CODE,
                        S_END_WH = endLoc.S_WH_CODE,
                        S_START_AREA = fromLoc.S_AREA_CODE,
                        S_END_AREA = endLoc.S_AREA_CODE,
                        S_START_LOC = from,
                        S_END_LOC = to,
                        S_TYPE = taskType,
                        N_PRIORITY = pri,
                        S_SCHEDULE_TYPE = "agv",
                        S_B_STATE = "未执行",
                        N_B_STATE = 0,
                        S_CNTR_CODE = cntrInfo,
                        S_BS_NO = bsNo,
                        S_BS_TYPE = bsType,
                        S_START_SITE_LAYER = startLayer,
                        S_END_SITE_LAYER = endLayer,
                    };
                    LogHelper.Info($"创建任务信息={JsonConvert.SerializeObject(wmsTask)}");
                    result = CreateTask(wmsTask);
                    LogHelper.Info($"创建任务结果 {result}");
                }
                else {
                    LogHelper.Info($"创建任务异常：起点或终点货位数据为空！起点：{fromLoc},终点：{endLoc}");
                }
            }
            catch (Exception ex) {
                LogHelper.Error($"创建任务异常：{ex.Message}", ex);
            }

            return result;
        }

        internal static List<Operation> GetOperationByState(int state) {
            var db = new SqlHelper<object>().GetInstance();
            return db.Queryable<Operation>().Where(x => x.N_B_STATE == state).ToList();

        }

        internal static bool UpdateOperationState(string code, int state, string end = "") {
            var res = false;
            var db = new SqlHelper<object>().GetInstance();
            var operation = db.Queryable<Operation>().Where(a => a.S_CODE.Trim() == code).First();
            if (operation != null) {
                operation.N_B_STATE = state;
                operation.S_B_STATE = operation.GetOpStateStr();
                res = db.Updateable(operation).UpdateColumns(it => new { it.N_B_STATE, it.S_B_STATE }).ExecuteCommand() > 0;
                if (!string.IsNullOrEmpty(end)) {
                    operation.S_END_LOC = end;
                    res = db.Updateable(operation).UpdateColumns(it => new { it.N_B_STATE, it.S_B_STATE, it.S_END_LOC }).ExecuteCommand() > 0;
                }

            }
            return res;
        }
        internal static bool CheckExist(string no) {
            return GetTask(no.Trim()) != null;
        }


        internal static bool UpdateStatus(string no, int state) {
            var res = false;
            var db = new SqlHelper<WMSTask>().GetInstance();
            var task = db.Queryable<WMSTask>().Where(a => a.S_CODE == no).First();
            if (task != null) {
                task.N_B_STATE = state;
                task.S_B_STATE = task.GetTaskStateStr();
                //需要判断任务是否失败或者已完成，不允许再修改
                res = db.Updateable(task).UpdateColumns(it => new { it.S_B_STATE, it.N_B_STATE }).ExecuteCommand() > 0;
            }
            return res;
        }

        /// <summary>
        /// 任务未推送或者推送未执行时更新表数据
        /// </summary>
        /// <param name="no"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        internal static bool UpdatePriority(string no, int priority) {
            var res = false;
            var db = new SqlHelper<WMSTask>().GetInstance();
            var task = db.Queryable<WMSTask>().Where(a => a.S_CODE == no).First();
            if (task != null) {
                task.N_PRIORITY = priority;
                //需要判断任务是否失败或者已完成，不允许再修改
                res = db.Updateable(task).UpdateColumns(it => new { it.N_PRIORITY }).ExecuteCommand() > 0;
            }
            return res;
        }
        internal static void Begin(WMSTask task, string agvNo) {
            var db = new SqlHelper<WMSTask>().GetInstance();
            if (task != null) {
                if (task.N_B_STATE == 1) {
                    task.N_B_STATE = 2;
                    task.S_B_STATE = task.GetTaskStateStr();
                    task.T_START_TIME = DateTime.Now;
                    task.S_EQ_NO = agvNo;
                    db.Updateable(task).UpdateColumns(it => new { it.S_B_STATE, it.N_B_STATE, it.T_START_TIME, it.S_EQ_NO }).ExecuteCommand();
                }
            }
        }
        /// <summary>
        /// 任务完成状态更新
        /// </summary>
        /// <param name="task"></param>
        internal static void Finish(WMSTask task) {
            var db = new SqlHelper<WMSTask>().GetInstance();
            if (task != null) {
                task.N_B_STATE = 8;
                task.S_B_STATE = task.GetTaskStateStr();
                task.T_END_TIME = DateTime.Now;
                db.Updateable(task).UpdateColumns(it => new { it.S_B_STATE, it.N_B_STATE, it.T_END_TIME }).ExecuteCommand();
                var op = db.Queryable<Operation>().Where(a => a.S_CODE == task.S_OP_CODE).First();
                if (op != null) {
                    op.N_B_STATE = 2;
                    op.S_B_STATE = op.GetOpStateStr();
                    db.Updateable(op).UpdateColumns(it => new { it.N_B_STATE, it.S_B_STATE }).ExecuteCommand();
                }
            }


        }
        internal static void Fail(WMSTask task) {
            var db = new SqlHelper<WMSTask>().GetInstance();
            if (task != null) {
                //判断有没有取货完成，没有就变成失败。有取货完成默认完成了（跟据项目而定，有些项目人工拉走了也没有放到终点）。
                task.N_B_STATE = 8;
                db.Updateable(task).UpdateColumns(it => new { it.S_B_STATE, it.N_B_STATE }).ExecuteCommand();
                var op = db.Queryable<Operation>().Where(a => a.S_CODE == task.S_OP_CODE).First();
                if (op != null) {
                    op.N_B_STATE = 3;
                    op.S_B_STATE = "失败";
                    db.Updateable(op).UpdateColumns(it => new { it.N_B_STATE, it.S_B_STATE }).ExecuteCommand();
                }
            }
        }
        internal static bool CreateTask(WMSTask wmsTask) {
            var res = false;
            var db = new SqlHelper<WMSTask>().GetInstance();
            try {
                db.BeginTran();
                res = db.Insertable(wmsTask).ExecuteCommand() > 0;
                if (res) {
                    var start = db.Queryable<Location>().Where(x => x.S_CODE == wmsTask.S_START_LOC).First();
                    var end = db.Queryable<Location>().Where(x => x.S_CODE == wmsTask.S_END_LOC).First();
                    var res1 = true; var res2 = true;
                    if (start.N_TYPE == 0) {
                        res1 = db.Updateable<Location>().SetColumns(x => new Location { N_LOCK_STATE = 2, S_LOCK_STATE = "出库锁", T_MODIFY = DateTime.Now, S_LOCK_OP = wmsTask.S_CODE }).Where(x => x.S_CODE == wmsTask.S_START_LOC && x.N_LOCK_STATE == 0).ExecuteCommand() > 0;
                    }
                    if (end.N_TYPE == 0) {
                        res2 = db.Updateable<Location>().SetColumns(x => new Location { N_LOCK_STATE = 1, S_LOCK_STATE = "入库锁", T_MODIFY = DateTime.Now, S_LOCK_OP = wmsTask.S_CODE }).Where(x => x.S_CODE == wmsTask.S_END_LOC && x.N_LOCK_STATE == 0).ExecuteCommand() > 0;
                    }
                    if (res1 && res2) {
                        db.CommitTran();
                        LogHelper.Info($"创建任务成功，任务号：{wmsTask.S_CODE}，起点货位：{wmsTask.S_START_LOC}，终点货位：{wmsTask.S_END_LOC}");
                    }
                    else {
                        db.RollbackTran();
                        LogHelper.Info($"创建任务失败，任务号：{wmsTask.S_CODE}，起点货位：{wmsTask.S_START_LOC}，终点货位：{wmsTask.S_END_LOC}，锁定货位失败");
                        res = false;
                    }
                }
            }
            catch (Exception ex) {
                db.RollbackTran();
                LogHelper.Error($"创建任务异常，任务号：{wmsTask.S_CODE}，起点货位：{wmsTask.S_START_LOC}，终点货位：{wmsTask.S_END_LOC}，异常信息：{ex.Message}", ex);
            }

            return res;
        }

        /// <summary>
        /// 根据任务状态获取任务
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        internal static List<WMSTask> GetTaskListByState(int state) {
            var db = new SqlHelper<object>().GetInstance();
            return db.Queryable<WMSTask>().Where(a => a.N_B_STATE == state).ToList();
        }

        internal static bool AddActionRecord(string no, int state, string forkliftNo, string extData) {
            var db = new SqlHelper<WmsTaskAction>().GetInstance();
            var action = new WmsTaskAction()
            {
                N_ACTION_CODE = state,
                S_TASK_CODE = no,
                S_EQ_CODE = forkliftNo,
                S_EQ_TYPE = "agv",
                S_DATA = extData
            };
            return db.Insertable(action).ExecuteCommand() > 0;
        }

        internal static bool CheckActionRecordExist(string no, int code) {
            var db = new SqlHelper<WmsTaskAction>().GetInstance();
            return db.Queryable<WmsTaskAction>().Count(a => a.S_TASK_CODE.Trim() == no.Trim() && a.N_ACTION_CODE == code) > 0;
        }




    }
}
