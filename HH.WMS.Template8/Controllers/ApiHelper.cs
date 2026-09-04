using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using SqlSugar;
using System.Collections.Concurrent;
using static WebApplication1.ApiModels;

namespace WebApplication1 {
    public static class ApiHelper {
        static ApiHelper() {
        }
        #region   HostToAgv接口实现类
        /// <summary>
        /// AGV信号处理
        /// </summary>
        /// <param name="model"></param>
        public static bool OperateTaskStatus(AgvTaskState model) {
            bool result = true;
            string key = string.IsNullOrEmpty(model.task_no) ? $"no_task_{model.state}_{model.lock_no}" : $"{model.task_no}_{model.state}";
            try {
                if (string.IsNullOrEmpty(model.task_no)) {
                    //安全请求等
                    TaskProcess.OperateTraffic(key, model.state, model.forklift_no, model.ext_data);
                }
                else {
                    var wmsTask = TaskHelper.GetTask(model.task_no);
                    if (wmsTask != null) {
                        //1开始2完成3开始取货4取货完成5开始卸货6卸货完成7异常取消
                        if (model.forklift_no != "0") {
                            if (model.state <= 7) {

                                //有任务号请求
                                switch (model.state) {
                                    //开始
                                    case 1:
                                        //ndc-hosttoagv 的1我们转成2
                                        TaskHelper.Begin(wmsTask, model.forklift_no);
                                        break;
                                    #region MyRegion
                                    //开始取货
                                    case 3:
                                        TaskHelper.UpdateStatus(wmsTask, 3);
                                        break;
                                    //取货完成
                                    case 4:
                                        TaskProcess.OperateStatus(wmsTask, 4);
                                        break;
                                    //开始卸货
                                    case 5:
                                        TaskProcess.OperateStatus(wmsTask, 5);
                                        break;
                                    //卸货完成
                                    case 6:
                                        TaskProcess.OperateStatus(wmsTask, 6);
                                        break;
                                    #endregion

                                    //完成
                                    case 2:
                                        //ndc-hosttoagv 的2我们转成8
                                        if (!TaskHelper.CheckActionRecordExist(model.task_no, 4)) {
                                            TaskProcess.OperateStatus(wmsTask, 4);
                                        }
                                        if (!TaskHelper.CheckActionRecordExist(model.task_no, 6)) {
                                            TaskProcess.OperateStatus(wmsTask, 6);
                                        }
                                        TaskHelper.Finish(wmsTask);
                                        break;
                                    //异常取消
                                    case 7:
                                        TaskProcess.OperateStatus(wmsTask, 7);
                                        break;
                                }
                                TaskHelper.AddActionRecord(model.task_no, model.state, model.forklift_no, model.ext_data);

                            }
                            else {
                                //安全请求等
                                TaskProcess.OperateReq(key, wmsTask, model.state, model.forklift_no, model.ext_data);
                            }
                        }
                    }
                }

            }
            catch (Exception ex) {
                LogHelper.Error($"处理任务状态异常 task:{model.task_no} state:{model.state}", ex);
                result=false;
            }
            return result;

        }

        /// <summary>
        /// AGV状态处理
        /// </summary>
        /// <param name="forkliftNo"></param>
        /// <param name="battery"></param>
        /// <param name="agvCurrTaskInfo"></param>
        /// <param name="errCode"></param>
        /// <param name="errCode2"></param>
        /// <param name="faildCode"></param>
        internal static void AGVDeviceReceiveSet(string forkliftNo, string battery, string agvCurrTaskInfo, string errCode, string errCode2, string faildCode) {
            var db = new SqlHelper<HangChaAGV>().GetInstance();
            var agvDeviceInfo = db.Queryable<HangChaAGV>().Where(a => a.agvNo == forkliftNo).First();
            if (agvDeviceInfo == null) {
                var agvInfo = new HangChaAGV()
                {
                    agvNo = forkliftNo,
                    agvBattery = battery,
                    agvCurrTaskInfo = agvCurrTaskInfo,
                    agvErrCode = errCode,
                    errCode2 = errCode2,
                    faildCode = faildCode
                };
                db.Insertable(agvInfo).ExecuteCommand();
            }
            else {
                agvDeviceInfo.agvBattery = battery;
                agvDeviceInfo.agvCurrTaskInfo = agvCurrTaskInfo;
                agvDeviceInfo.agvErrCode = errCode;
                agvDeviceInfo.errCode2 = errCode2;
                agvDeviceInfo.faildCode = faildCode;
                db.Updateable(agvDeviceInfo).UpdateColumns(a => new
                { a.agvBattery, a.agvCurrTaskInfo, a.agvErrCode, a.errCode2, a.faildCode }).ExecuteCommand();
            }
        }

        #endregion

        #region RestAPI-RCS

        internal static RCSReturnResult OrderStatusNotify(OrderStatusModel model) {
            RCSReturnResult result = new RCSReturnResult() { code = 0, msg = "success" };
            try {
                var task_no = model.orderName;//model.orderID.ToString();
                var wmsTask = TaskHelper.GetTask(task_no);
                int state = 0;
                if (wmsTask != null) {
                    switch (model.orderStatus) {
                        //任务开始
                        case "active":
                            TaskHelper.Begin(wmsTask, model.agvIDList);
                            state = 1;
                            break;
                        //取货完成
                        case "source_finish":
                            TaskProcess.OperateStatus(wmsTask, 4);
                            TaskHelper.UpdateStatus(wmsTask, 5);
                            state = 4;
                            break;
                        //卸货完成
                        case "dest_finish":
                            TaskProcess.OperateStatus(wmsTask, 6);
                            TaskHelper.UpdateStatus(wmsTask, 7);
                            state = 6;
                            break;
                        //任务完成
                        case "finish":
                            if (!TaskHelper.CheckActionRecordExist(task_no, 4)) {
                                TaskProcess.OperateStatus(wmsTask, 4);
                            }
                            if (!TaskHelper.CheckActionRecordExist(task_no, 6)) {
                                TaskProcess.OperateStatus(wmsTask, 6);
                            }
                            TaskHelper.Finish(wmsTask);
                            state = 2;
                            break;
                        //任务取消完成
                        case "cancel_finish"
                            :
                            TaskProcess.OperateStatus(wmsTask, 7);
                            state = 7;
                            break;
                    }
                    TaskHelper.AddActionRecord(task_no, state, model.agvIDList, model.extraInfo1);
                }
                return result;
            }
            catch (Exception ex) {
                result.code = 1;
                result.msg = ex.Message;
                LogHelper.Error($"订单状态推送异常：{ex.Message}", ex, "RCSTask");
                return result;
            }
        }
        private static ConcurrentDictionary<string, byte> dic = new ConcurrentDictionary<string, byte>();

        /// <summary>
        /// 国自接口是同步的，要直接返回成功或者失败，不像hosttoagv可以主动改参数（如果是主动去查国自交互表的需要开启轮询GZRobot.QueryInteractInfo）
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        internal static RCSReturnResult SafetyInteraction(SafetyInteractionModel model) {
            RCSReturnResult result = new RCSReturnResult() { code = 0, msg = "success" };
            try {
                var wmsTask = TaskHelper.GetTask(model.Order_id.ToString());
                //根据传入的设备类型判断是什么的安全交互
                //"Station" -机台，输送线
                if (model.device_type == "Station") {
                    if (wmsTask != null) {
                    }
                }
                //"Area" -物流门（含普通卷帘门、风淋门）
                else if (model.device_type == "Area") {
                }
                //"Lift" - 电梯
                else if (model.device_type == "Lift") {
                }
                return result;
            }
            catch (Exception ex) {
                result.code = 1;
                LogHelper.Error($"安全交互异常：{ex.Message}", ex, "RCSTask");
                return result;
            }
        }

        #endregion


    }
}
