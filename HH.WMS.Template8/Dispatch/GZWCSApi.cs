using Newtonsoft.Json;

namespace WebApplication1 {
    /// <summary>
    /// 国自wcs
    /// </summary>
    internal class GZWCSApi {
        private static readonly HttpHelper apiHelper = new HttpHelper();
        private static readonly string baseUrl = "";//改成config文件获取
        private static string logName = "GZWCSApi";

        /// <summary>
        /// AGV请求取货、取货完成、请求放货、放货完成时调用，请求取货、请求放货状态时需要返回是否可取\放货
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static bool LocState(locModel model) {
            bool result = false;
            var reqStr = JsonConvert.SerializeObject(model);
            var res = apiHelper.WebPost(baseUrl + "start", reqStr);
            if (res != "") {
                try {
                    var dataResult = JsonConvert.DeserializeObject<wcsResponse>(res);
                    result = dataResult.result_flag == 0;
                }
                catch (Exception ex) {
                    LogHelper.Error("LocState 接口调用失败，返回报文：" + res + "，异常信息：" + ex.Message, ex, logName);
                }
            }
            return result;
        }
        /// <summary>
        /// 任务下发 todo:组织参数
        /// </summary>
        /// <returns></returns>
        public static bool Start(wcsTaskInfo task) {
            bool result = false;
            var list = new List<wcsTaskInfo>() { task };
            var reqStr = JsonConvert.SerializeObject(list);
            var res = apiHelper.WebPost(baseUrl + "start", reqStr);
            if (res != "") {
                try {
                    var dataResult = JsonConvert.DeserializeObject<wcsResponse>(res);
                    result = dataResult.result_flag == 0;
                }
                catch (Exception ex) {
                    LogHelper.Error("Start 接口调用失败，返回报文：" + res + "，异常信息：" + ex.Message, ex, logName);
                }
            }
            return result;
        }
        /// <summary>
        /// 换料时，WMS向WCS下发机器人切换的规格
        /// </summary>
        /// <param name="spec"></param>
        /// <returns></returns>
        public static bool ChangeSpecifications(string spec) {
            bool result = false;
            var reqStr = JsonConvert.SerializeObject(new { req_no = Guid.NewGuid().ToString(), palletizing = spec });
            var res = apiHelper.WebPost(baseUrl + "start", reqStr);
            if (res != "") {
                try {
                    var dataResult = JsonConvert.DeserializeObject<wcsResponse>(res);
                    result = dataResult.result_flag == 0;
                }
                catch (Exception ex) {
                    LogHelper.Error("ChangeSpecifications 接口调用失败，返回报文：" + res + "，异常信息：" + ex.Message, ex, logName);
                }
            }
            return result;
        }
        /// <summary>
        /// 修改优先级
        /// </summary>
        /// <param name="taskNo"></param>
        /// <param name="pri"></param>
        /// <returns></returns>
        public static bool PriorityChange(string taskNo, int pri) {
            bool result = false;
            var reqStr = JsonConvert.SerializeObject(new { req_no = Guid.NewGuid().ToString(), task_no = taskNo, task_pri = pri });
            var res = apiHelper.WebPost(baseUrl + "PriorityChange", reqStr);
            if (res != "") {
                try {
                    var dataResult = JsonConvert.DeserializeObject<wcsResponse>(res);
                    result = dataResult.result_flag == 0;
                }
                catch (Exception ex) {
                    LogHelper.Error("PriorityChange 接口调用失败，返回报文：" + res + "，异常信息：" + ex.Message, ex, logName);
                }
            }
            return result;
        }
        /// <summary>
        /// 任务充分
        /// </summary>
        /// <param name="task_no"></param>
        /// <param name="tunnel_no"></param>
        /// <param name="mat_code"></param>
        /// <param name="from_pos"></param>
        /// <param name="pre_task_no"></param>
        /// <returns></returns>
        public static bool ChangeTask(string task_no, string tunnel_no, string mat_code, string from_pos, string pre_task_no) {
            bool result = false;
            var reqStr = JsonConvert.SerializeObject(new { req_no = Guid.NewGuid().ToString(), task_no = task_no, tunnel_no = tunnel_no, mat_code = mat_code, from_pos = from_pos, pre_task_no = pre_task_no });
            var res = apiHelper.WebPost(baseUrl + "ChangeTask", reqStr);
            if (res != "") {
                try {
                    var dataResult = JsonConvert.DeserializeObject<wcsResponse>(res);
                    result = dataResult.result_flag == 0;
                }
                catch (Exception ex) {
                    LogHelper.Error("ChangeTask 接口调用失败，返回报文：" + res + "，异常信息：" + ex.Message, ex, logName);
                }
            }
            return result;
        }
        /// <summary>
        /// 获取堆垛机状态
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static stackerStateData StackerState(List<stackerStateModel> models) {
            stackerStateData result = null;
            var reqStr = JsonConvert.SerializeObject(models);
            var res = apiHelper.WebPost(baseUrl + "start", reqStr);
            if (res != "") {
                try {
                    result = JsonConvert.DeserializeObject<stackerStateData>(res);
                }
                catch (Exception ex) {
                    LogHelper.Error("StackerState 接口调用失败，返回报文：" + res + "，异常信息：" + ex.Message, ex, logName);
                }
            }
            return result;
        }
        public class wcsResponse {
            /// <summary>
            /// 返回值；0：正常，其它值为异常错误码
            /// </summary>
            public int result_flag { get; set; }
            public string err_msg { get; set; }
        }
        public class wcsTaskInfo {
            /// <summary>
            /// 唯一码，用于接口校验，默认生成GUID
            /// </summary>
            public string req_no { get; set; }

            /// <summary>
            /// 任务类型：1=货物入库；2=货物出库；3=托盘组入库；4=托盘组出库；5=移动（不过库位）；6=移库；7=不同巷道移库
            /// </summary>
            public int task_type { get; set; }

            /// <summary>
            /// 任务号，单托盘唯一任务号
            /// </summary>
            public string task_no { get; set; }

            /// <summary>
            /// 前置任务号，外伸出库时，内伸移库任务WMS自动下发，且用该字段标记对应的移库任务号
            /// </summary>
            public string pre_task_no { get; set; }

            /// <summary>
            /// 巷道号，例：1，2，3...
            /// </summary>
            public string tunnel_no { get; set; }

            /// <summary>
            /// 起点位置：入库时起点位置为站台号，终点位置为库位号
            /// </summary>
            public string from_pos { get; set; }

            /// <summary>
            /// 终点位置：出库时起点位置为库位号，终点位置为站台号
            /// </summary>
            public string to_pos { get; set; }

            /// <summary>
            /// 货物RFID
            /// </summary>
            public string mat_code { get; set; }

            /// <summary>
            /// 货物类型（注意：原字段拼写mat_tyoe）
            /// </summary>
            public string mat_tyoe { get; set; }

            /// <summary>
            /// 货物描述
            /// </summary>
            public string mat_memo { get; set; }

            /// <summary>
            /// 创建时间
            /// </summary>
            public DateTime req_time { get; set; }

            /// <summary>
            /// 数量，托盘组出库时，传该数量
            /// </summary>
            public int? qty { get; set; }

            /// <summary>
            /// 高度规格，例：1，2，3 空托胶料为1
            /// </summary>
            public string mat_size { get; set; }

            /// <summary>
            /// 任务优先级，预留
            /// </summary>
            public int? task_pri { get; set; }
        }

      
        public class stackerStateData : wcsResponse {
            public List<stackerStateInfo> data { get; set; }
            public class stackerStateInfo {
                public string roadway { get; set; }
                /// <summary>
                /// 堆垛机状态 1=空闲，2=执行中，3=报警，0=不可用
                /// </summary>
                public string roadway_state { get; set; }
            }
        }
        public class stackerStateModel {
            public string req_no { get; set; }
            public string roadway { get; set; }
        }
       


        public class locModel {
            /// <summary>
            /// 货位号
            /// </summary>
            public string loc_code { get; set; }

            /// <summary>
            /// 请求类型 1 请求取货 2 请求放货 3 取货完成 4 放货完成
            /// </summary>
            public string type { get; set; }

            /// <summary>
            /// 唯一码
            /// </summary>
            public string req_no { get; set; }

            /// <summary>
            /// 任务号
            /// </summary>
            public string task_no { get; set; }
        }

    }
}
