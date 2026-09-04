using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebApplication1.Controllers;

namespace WebApplication1 {
    internal class GZWCSApiHandler {
        private static string logName = "GZWCS";
        internal static MoboxResult Handle(GatewayMessage model) {
            LogHelper.Info("Request：" + JsonConvert.SerializeObject(model), logName);
            var dt = DateTime.Now;
            MoboxResult res = null;
            switch (model.Name) {
                case "ApplyDest": res = ApplyDest(model.Data); break;
                case "GetMaterialHeight": res = GetMaterialHeight(model.Data); break;
                case "GZTaskStatus": res = GZTaskStatus(model.Data); break;
                case "LocStateFeedBack": res = LocStateFeedBack(model.Data); break;
                case "notifyDeviceSignal": res = notifyDeviceSignal(model.Data); break;
                case "RecalculateLoc": res = RecalculateLoc(model.Data); break;
                case "WCSCallback": res = WCSCallback(model.Data); break;

            }
            LogHelper.Info($"{DateTime.Now.Subtract(dt).TotalSeconds} Result：" + JsonConvert.SerializeObject(res), logName);
            return res;
        }
        /// <summary>
        /// 001-任务请求
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private static MoboxResult WCSCallback(JToken data) {
            MoboxResult res = null;
            // todo 解析参数处理任务状态
            res = new MoboxResult { err_code = 0, err_msg = "成功" };
            return res;
        }
        public class WCSCallbackData {
            /// <summary>
            /// 托盘号 FRID
            /// </summary>
            public string cntr_code { get; set; }

            /// <summary>
            /// 工位，参照站台编码表
            /// </summary>
            public string station_no { get; set; }

            /// <summary>
            /// 唯一码
            /// </summary>
            public string req_no { get; set; }

            /// <summary>
            /// 请求类型 1=货物入库/退库；2=托盘组入库；3=RFID托盘查询 4 = 呼叫空托
            /// </summary>
            public int req_type { get; set; }

            /// <summary>
            /// 数量，机械臂抓胶入库，一托的散装胶块数/码盘机的容器数量
            /// </summary>
            public int qty { get; set; }

            /// <summary>
            /// 称重的重量
            /// </summary>
            public decimal weight { get; set; }

            /// <summary>
            /// 流程节点，参照流程节点表
            /// </summary>
            public string process_node { get; set; }

            /// <summary>
            /// 请求时间
            /// </summary>
            public DateTime req_time { get; set; }
        }
        /// <summary>
        /// 重写申请货位
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private static MoboxResult RecalculateLoc(JToken data) {
            MoboxResult res = null;
            // todo 解析参数处
            res = new MoboxResult { err_code = 0, err_msg = "成功", result = new { roadway = "", task_no = "", end_loc_code = "" } };
            return res;
        }
        public class RecalculateLocData {
            /// <summary>
            /// 任务号，单托盘唯一任务号
            /// </summary>
            public string task_no { get; set; }

            /// <summary>
            /// 唯一码
            /// </summary>
            public string req_no { get; set; }

            /// <summary>
            /// 异常类型
            /// 1 取货无货（锁一组货位）
            /// 2 放货有货（锁一组货位，重新申请货位）
            /// 3 取深浅有（锁一组货位）
            /// 4 放深浅有（锁一组货位，重新申请货位）
            /// </summary>
            public int type { get; set; }

            /// <summary>
            /// 托盘号 FRID
            /// </summary>
            public string cntr_code { get; set; }
        }
        /// <summary>
        /// 设备信号反馈
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private static MoboxResult notifyDeviceSignal(JToken data) {
            throw new NotImplementedException();
        }
        public class notifyDeviceSignalData {
            /// <summary>
            /// 唯一码
            /// </summary>
            public string req_no { get; set; }

            /// <summary>
            /// 上料点、下料点、读码位、称重位，参考站台编码表
            /// </summary>
            public string loc { get; set; }

            /// <summary>
            /// 托盘号 FRID
            /// </summary>
            public string cntr_code { get; set; }

            /// <summary>
            /// 请求类型 1:下线请求，2：叫料请求 3：读码请求，4:下线记录，5：异常申请（固定传3）
            /// </summary>
            public int signalType { get; set; }

            /// <summary>
            /// 扩展参数 {flag:0/1}  0正常 1异常
            /// </summary>
            public int extData { get; set; }

            /// <summary>
            /// 设备编码
            /// </summary>
            public string deviceNo { get; set; }
        }
        /// <summary>
        /// WCS反馈货位是否可用
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static MoboxResult LocStateFeedBack(JToken data) {
            MoboxResult res = null;
            // todo 解析参数处理
            res = new MoboxResult { err_code = 0, err_msg = "成功" };
            return res;
        }
        public class LocStateFeedBackData {
            /// <summary>
            /// 货位号
            /// </summary>
            public string loc_code { get; set; }

            /// <summary>
            /// 请求类型 1 允许取货 2 允许放货 3 取货完成允许离开 4 放货完成允许离开
            /// </summary>
            public string type { get; set; }

            /// <summary>
            /// 任务号 AGV传过来的任务号
            /// </summary>
            public string task_no { get; set; }

            /// <summary>
            /// 唯一码
            /// </summary>
            public string req_no { get; set; }
        }
        /// <summary>
        /// 任务状态反馈
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private static MoboxResult GZTaskStatus(JToken data) {
            MoboxResult res = null;
            // todo 解析参数处理任务状态
            res = new MoboxResult { err_code = 0, err_msg = "成功" };
            return res;
        }
        public class GZTaskStatusData {
            /// <summary>
            /// 唯一码
            /// </summary>
            public string req_no { get; set; }

            /// <summary>
            /// 反馈类型 1=任务开始；2=任务完成；3=任务异常；4=任务取消；5=任务更改（待定）;6=货物节点;8 = 入库重量上报;9 = 货物节点任务异常上报
            /// </summary>
            public int feed_type { get; set; }

            /// <summary>
            /// 任务类型 1=货物入库；2=货物出库；3=托盘组入库；4=托盘组出库；5=移动（不过库位）；6=移库； 7=不同巷道移库
            /// </summary>
            public int task_type { get; set; }

            /// <summary>
            /// 任务号，单托盘唯一任务号
            /// </summary>
            public string task_no { get; set; }

            /// <summary>
            /// 托盘号 FRID
            /// </summary>
            public string cntr_code { get; set; }

            /// <summary>
            /// 当前位置，参照站台编码表
            /// </summary>
            public string cur_station_no { get; set; }

            /// <summary>
            /// 称重的重量
            /// </summary>
            public decimal weight { get; set; }

            /// <summary>
            /// 反馈时间
            /// </summary>
            public DateTime feed_time { get; set; }

            /// <summary>
            /// 异常信息，只针对反馈类型 3和9
            /// </summary>
            public string err_msg { get; set; }
        }
        /// <summary>
        /// 获取物料高度
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private static MoboxResult GetMaterialHeight(JToken data) {
            MoboxResult res = null;
            // todo 解析参数获取高度
            res = new MoboxResult { err_code = 0, err_msg = "成功", result = new { height = 100 } };
            return res;
        }

        /// <summary>
        /// 1：堆垛机放货异常申请新终点；2：输送线到达接驳位申请终点；
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static MoboxResult ApplyDest(JToken data) {
            MoboxResult res = null;
            var list = new List<ApplyDestData>();
            if (data is JArray arr) {
                list = arr.Select(x => x.ToObject<ApplyDestData>()).ToList();
            }
            else {
                list.Add(data.ToObject<ApplyDestData>());
            }
            //todo 获取终点 
            var end = "";
            var aisle = 1;
            res = new MoboxResult
            {
                err_code = 0,
                err_msg = "成功",
                result = new
                {
                    end = end,
                    aisle = aisle
                }
            };
            return res;
        }
        public class ApplyDestData {
            /// <summary>
            /// 唯一码
            /// </summary>
            public string req_no { get; set; }

            /// <summary>
            /// 任务号
            /// </summary>
            public string task_no { get; set; }

            /// <summary>
            /// 当前位置
            /// </summary>
            public string loc { get; set; }

            /// <summary>
            /// 申请类型
            /// </summary>
            public int applyType { get; set; }

            /// <summary>
            /// 可用巷道号
            /// </summary>
            public string tunnel_no { get; set; }
        }
        public class GetMaterialHeightData {
            /// <summary>
            /// 唯一码
            /// </summary>
            public string req_no { get; set; }

            /// <summary>
            /// 托盘号
            /// </summary>
            public string cntr_code { get; set; }

            /// <summary>
            /// 工位
            /// </summary>
            public string station_no { get; set; }

            /// <summary>
            /// 类型
            /// </summary>
            public string type { get; set; }
        }

    }
}
