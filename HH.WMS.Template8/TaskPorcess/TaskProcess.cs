using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace WebApplication1 {
    internal static class TaskProcess {
        #region 标准任务流程
        //--------------------------------------------------任务相关--------------------------------------------------

        /// <summary>
        /// 任务分发
        /// </summary>
        internal static void Dispatch() {
            //获取所有未执行的任务
            var list = TaskHelper.GetTaskListByState(0);
            if (list.Count > 0) {
                list.ForEach(task => {
                    if (!Intercept(task)) {
                        //使用自定义任务推送
                        if (task.N_SCHEDULE_TYPE == 0) {
                            TaskProcess.SendNdcTask(task);
                        }
                        #region wcs任务推送
                        else if (task.N_SCHEDULE_TYPE == 1) {
                            SendGzTask(task);
                        }
                        #endregion

                    }
                });
            }

        }

        /// <summary>
        /// 任务拦截
        /// </summary>
        /// <param name="mst"></param>
        /// <returns> true:拦截  false:不拦截</returns>
        internal static bool Intercept(WMSTask mst) {
            var result = false;
            if (!string.IsNullOrEmpty(mst.S_PRE_TASK_NO)) {
                var db = new SqlHelper<object>().GetInstance();
                var finishedTask = db.Queryable<WMSTask>()
                    .First(t => t.S_CODE.Trim() == mst.S_PRE_TASK_NO.Trim());
                //前置任务没有完成
                if (finishedTask != null) {
                    LogHelper.Info($"任务：{mst.S_CODE}对应的前置任务:{finishedTask.S_CODE}存在，且前置任务的状态为:{finishedTask.N_B_STATE},{finishedTask.S_B_STATE}");
                    if (finishedTask.N_B_STATE < 8) {
                        result = true;
                    }
                }
                else {
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// 推送任务-NDC参数标准版
        /// </summary>
        /// <param name="mst">任务数据</param>
        /// <returns></returns>
        internal static bool SendNdcTask(WMSTask mst) {
            var result = false;
            try {
                var startLoc = LocationHelper.GetLoc(mst.S_START_LOC);
                var endLoc = LocationHelper.GetLoc(mst.S_END_LOC);
                if (startLoc != null && endLoc != null) {
                    int TsNo = 1;//下发任务类型：默认(坯盖)-1，成品任务-5
                    string func = "";//功能码      16进制转10进制
                    string data = "";
                    var from = LocationHelper.GetAgvSite(mst.S_START_LOC);
                    var to = LocationHelper.GetAgvSite(mst.S_END_LOC);

                    //创建NDC参数字典
                    var dic = new List<param>();
                    dic.Add(new param() { name = "From", value = from.ToString() });
                    dic.Add(new param() { name = "To", value = to.ToString() });
                    dic.Add(new param() { name = "Func", value = func });
                    dic.Add(new param() { name = "Data", value = data });

                    LogHelper.Info($"sendtask taskno={mst.S_CODE}");

                    var res = new AgvApiResult();
                    res = NDCApi.AddOrderNew(TsNo, mst.N_PRIORITY, mst.S_CODE, dic);

                    if ((res != null && (res.err_code == 0 || res.err_code == 50009))) {
                        TaskHelper.UpdateStatus(mst, 1);
                        result = true;
                    }
                }

            }
            catch (Exception ex) {
                LogHelper.Error($"SendNdcTask Error:{ex.Message}", ex);
            }

            return result;
        }

        /// <summary>
        /// 推送任务-GZrobot参数标准版
        /// </summary>
        /// <param name="mst">任务数据</param>
        /// <returns></returns>
        internal static bool SendGzTask(WMSTask mst) {
            var result = false;
            try {
                if (mst.N_B_STATE == 0) {
                    var startLoc = LocationHelper.GetLoc(mst.S_START_LOC);
                    var endLoc = LocationHelper.GetLoc(mst.S_END_LOC);
                    if (startLoc != null && endLoc != null) {
                        #region 通用数据赋值

                        var db = new SqlHelper<object>().GetInstance();
                        // 获取起终点的AGV站点 查询 扩展货位表 S_PICKUP_POINT-点位层数 S_LOC_CODE-货位编码 GetAgvSite-标准获取扩展货位表数据的方法
                        var param1 = LocationHelper.GetAgvSite(mst.S_START_LOC);
                        var param2 = LocationHelper.GetAgvSite(mst.S_END_LOC);

                        #endregion

                        //创建NDC参数字典
                        var dic = new List<param>();

                        dic.Add(new param() { name = "src", value = param1.ToString() });
                        dic.Add(new param() { name = "dst", value = param2.ToString() });

                        LogHelper.Info($"sendtask taskno={mst.S_CODE}");
                        int order_id = GZRobotApi.CreateOrder(mst.S_CODE, mst.N_PRIORITY, JsonConvert.SerializeObject(dic), "p2p");
                        if (order_id != 0) {
                            TaskHelper.UpdateStatus(mst, 1);
                            result = true;
                        }

                    }

                }
            }
            catch (Exception ex) {
                LogHelper.Error($"SendTaskStandard Error:{ex.Message}", ex);
            }

            return result;
        }

        /// <summary>
        /// 任务状态更新处理
        /// </summary>
        /// <param name="mst"></param>
        /// <param name="state"></param>
        internal static bool OperateStatus(WMSTask mst, int state) {
            bool result = true;
            if (state == 4) {
                result = CacheBitUpdate(mst, true);
            }
            else if (state == 6)//卸货完成
            {
                result = CacheBitUpdate(mst, false);
            }
            else if (state == 7) {
                if (mst.N_B_STATE < 7) {
                    result = CacheBitCancelUpdate(mst, true);
                }
            }
            return result;
        }

        /// <summary>
        /// 取货卸货完成，缓存位状态更新
        /// </summary>
        /// <param name="mst"></param>
        /// <param name="load">true:取货完成 false:卸货完成</param>
        internal static bool CacheBitUpdate(WMSTask mst, bool load) {
            bool result = true;
            if (load) {
                LogHelper.Info($"任务{mst.S_CODE} 货位{mst.S_START_LOC}取货完成，起点解绑容器{mst.S_CNTR_CODE}");
                result = LocationHelper.UnBindingLoc(mst.S_START_LOC.Trim(), mst.S_CNTR_CODE.Split(',').ToList(), mst.S_CODE);
            }
            else {
                LogHelper.Info($"任务{mst.S_CODE} 货位{mst.S_END_LOC}卸货完成，终点绑定容器{mst.S_CNTR_CODE}");
                result = LocationHelper.BindingLoc(mst.S_END_LOC, mst.S_CNTR_CODE.Split(',').ToList(), mst.S_CODE);
            }
            return result;
        }

        /// <summary>
        /// 任务取消，缓存位状态更新
        /// </summary>
        /// <param name="mst"></param>
        internal static bool CacheBitCancelUpdate(WMSTask mst, bool updateStatus = false) {
            bool result = true;
            //任务取消，取货完成前的，起点的loadingCount和终点unLoadingCount都清除，取货完成的只处理终点
            if (TaskHelper.CheckActionRecordExist(mst.S_CODE, 4)) {
                result = CacheBitUpdate(mst, false);
                if (result) {
                    TaskHelper.UpdateStatus(mst, 9);
                }
            }
            else {
                //起点终点解锁
                result = LocationHelper.UnLockLoc(mst.S_START_LOC, mst.S_CODE) && LocationHelper.UnLockLoc(mst.S_END_LOC, mst.S_CODE);
                if (result) {
                    TaskHelper.UpdateStatus(mst, 7);
                }
            }
            return result;
        }
        private static ConcurrentDictionary<string, byte> dic = new ConcurrentDictionary<string, byte>();
        /// <summary>
        /// 安全交互、变更参数终点等
        /// </summary>
        /// <param name="wmsTask"></param>
        /// <param name="state"></param>
        /// <param name="forklift_no"></param>
        /// <param name="ext_data"></param>
        internal static void OperateReq(string key, WMSTask wmsTask, int state, string forklift_no, string ext_data) {
            if (dic.TryAdd(key, 1)) {
                Task.Run(() => {
                    //处理业务
                    dic.TryRemove(key, out _);
                });
            }

        }
        /// <summary>
        /// 交管相关（自动门）
        /// </summary>
        /// <param name="state"></param>
        /// <param name="forklift_no"></param>
        /// <param name="ext_data"></param>
        internal static void OperateTraffic(string key, int state, string forklift_no, string ext_data) {
            if (dic.TryAdd(key, 1)) {
                Task.Run(() => {
                    //处理业务
                    dic.TryRemove(key, out _);
                });
            }
        }

        /// <summary>
        /// 启动作业
        /// </summary>
        internal static void StartOperation() {
            var list = TaskHelper.GetOperationByState(0);
            if (list.Count > 0) {
                list.ForEach(op => {
                    //启动作业创建任务，有些项目不创建作业只创建任务
                });
            }
        }


        #endregion
    }
}
