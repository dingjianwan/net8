using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication1 {
    /// <summary>
    /// 国自调度辅助类
    /// </summary>
    public class GZRobot
    {
        private static readonly HttpHelper apiHelper = new HttpHelper();
        private static readonly string baseUrl = "http://192.168.1.99:2000/";
        //private static readonly string logName = "guozi";
        public static List<IOState> GetIO()
        {
            var result = apiHelper.Get(baseUrl + "api/engine/view/iostates/");
            var data = JsonConvert.DeserializeObject<gzResult<IOState>>(result);
            return data.data;
        }
        public static void UpdateIOState()
        {
            var data = new { data = new List<IOSateInfo>() };
            var result = apiHelper.Post(baseUrl + "api/engine/tasks/iostates/", JsonConvert.SerializeObject(data));
            var dataResult = JsonConvert.DeserializeObject<gzResult<IOStatesInfoResult>>(result);
        }
        /// <summary>
        /// 创建任务
        /// </summary>
        /// <param name="taskNo">任务编号</param>
        /// <param name="priority">优先级</param>
        /// <param name="param">参数</param>
        /// <param name="ts">时间戳(由车辆类型决定 p2p,roller_p)</param>
        /// <returns></returns>
        public static int CreateOrder(string taskNo, int priority, string param, string ts = "p2p")
        {
            var msg = "";
            var orderId = 0;
            var data = new OrderInfo() { order_name = taskNo, priority = priority, dead_line = DateTime.Now, ts_name = ts, parameters = param };
            var request = JsonConvert.SerializeObject(data);
            var response = apiHelper.Post(baseUrl + "api/om/order/", request);
            msg = $"[guozi-CreateOrder] request={request} response={response}";
            //LogHelper.Info(msg);
            if (response != "")
            {
                try
                {
                    var dataResult = JsonConvert.DeserializeObject<gzResult<OrderInfoResult>>(response);
                    if (dataResult.code == 0)
                    {
                        orderId = dataResult.data[0].in_order_id;
                    }
                }
                catch (Exception ex)
                {

                }
            }
            else
            {
                msg = "[guozi-CreateOrder]创建订单失败";
                //LogHelper.Info(msg);
            }

            return orderId;
        }
        /// <summary>
        /// 取消任务
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public static bool CancelOrder(int orderId)
        {
            bool result = false;
            string msg = "";
            var request = new CancelOrderInfo() { order_list = new List<int>(orderId) };
            var response = apiHelper.Post(baseUrl + "api/om/order/cancel/", JsonConvert.SerializeObject(request));
            msg = $"[guozi-CancelOrder] request={request};response={response}";
            //LogHelper.Info(msg);
            if (response != "")
            {
                var dataResult = JsonConvert.DeserializeObject<gzResult<CancelOrderInfoResult>>(response);
                if (dataResult.code == 0)
                {
                    result = true;
                }
            }
            else
            {
                msg = "[guozi-CancelOrder]取消订单失败";
                //LogHelper.Info(msg);
            }
            return result;
        }
        public static QueryOrderResult QueryOrder(int id)
        {
            //
            var result = new QueryOrderResult() { agv_list = new List<int> { 0 } };
            string msg = "";
            var response = apiHelper.Get(baseUrl + $"/api/om/order/{id}/");
            msg = $"[guozi-QueryOrderResult] request={id};response={response}";
            //LogHelper.Info(msg);
            if (response != "")
            {
                var data = JsonConvert.DeserializeObject<gzResult<QueryOrderResult>>(response);
                result = data.data[0];
            }
            else
            {
                msg = "[guozi-QueryOrderResult]查询订单信息失败";
                //LogHelper.Info(msg);
            }
            return result;
        }
        /// <summary>
        /// 获取交互信息
        /// </summary>
        /// <param name="typeId">1任务状态  2开门或交管  3目的点</param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static List<InteractInfoResult> QueryInteractInfo(int typeId, string status = "active")
        {

            //string aaa = "{\"app_name\": \"Gouzi client\", \"version\": \"1.0.0\", \"code\": 0, \"msg\": \"success\", \"data\": [{\"interaction_info_id\": 233, \"interaction_info_name\": \"TN2012030001\", \"interaction_info_desp\": null, \"interaction_info_type_id\": 3, \"value_json\": {\"state\": \"4\"}, \"info_status\": \"active\", \"return_value\": null}]}";
            //var data = JsonConvert.DeserializeObject<gzResult<InteractInfoResult>>(aaa);
            var list = new List<InteractInfoResult>();
            string msg = "";
            var result = apiHelper.Get(baseUrl + $"api/om/interaction_info/find_by_type/?type_id={typeId}&info_status={status}");
            if (!string.IsNullOrEmpty(result))
            {
                //LogHelper.Info(result);
                // {"app_name": "Gouzi client", "version": "1.0.0", "code": 0, "msg": "success", "data": [{"interaction_info_id": 230, "interaction_info_name": "2", "interaction_info_desp": null, "interaction_info_type_id": 3, "value_json": {"state": "4"}, "info_status": "active", "return_value": null}, {"interaction_info_id": 231, "interaction_info_name": "2", "interaction_info_desp": null, "interaction_info_type_id": 3, "value_json": {"state": "6"}, "info_status": "active", "return_value": null}, {"interaction_info_id": 232, "interaction_info_name": "2", "interaction_info_desp": null, "interaction_info_type_id": 3, "value_json": {"state": "2"}, "info_status": "active", "return_value": null}, {"interaction_info_id": 233, "interaction_info_name": "TN2012030001", "interaction_info_desp": null, "interaction_info_type_id": 3, "value_json": {"state": "4"}, "info_status": "active", "return_value": null}]}
                try
                {
                    var data = JsonConvert.DeserializeObject<gzResult<InteractInfoResult>>(result);
                    if (data.data != null)
                    {
                        list = data.data;
                    }
                }
                catch (Exception ex)
                {
                    //LogHelper.Info(ex.Message);
                }
            }
            else
            {
                msg = "[guozi-QueryInteractInfo]读取交互信息失败";
                //LogHelper.Info(msg);
            }
            return list;
        }
        public static bool UpdateInteractInfo(UpdateInteractInfo interactInfo)
        {
            string msg = "";
            var result = false;
            var request = JsonConvert.SerializeObject(interactInfo);
            var response = apiHelper.Post(baseUrl + "api/om/interaction_info/update/", request);
            msg = $"[mes-UpdateInteractInfo] request={request};response={response}";
            if (response != "")
            {
                var dataResult = JsonConvert.DeserializeObject<gzResult<object>>(response);
                result = dataResult.code == 0;
            }
            else
            {
                msg = "[guozi-UpdateInteractInfo]更新交互信息失败";
                //LogHelper.Info(msg);
            }
            return result;
        }

    }
    public class gzResult<T>
    {
        public string app_name { get; set; }
        public string version { get; set; }
        public int code { get; set; }
        public string msg { get; set; }
        public List<T> data { get; set; }
    }
    public class IOState
    {
        public int io_id { get; set; }
        public string io_name { get; set; }
        public string io_type_id { get; set; }
        public string io_type { get; set; }
        public int io_status_id { get; set; }
        public string io_status_type { get; set; }
        public string parameter_definition_int4_1 { get; set; }
        public int io_value_int4_1 { get; set; }
        public string parameter_definition_int4_2 { get; set; }
        public int io_value_int4_2 { get; set; }
        public string parameter_definition_int4_3 { get; set; }
        public int io_value_int4_3 { get; set; }
        public string parameter_definition_int4_4 { get; set; }
        public int io_value_int4_4 { get; set; }
        public string parameter_definition_json { get; set; }
        public string io_value_json { get; set; }
    }
    public class IOSateInfo
    {
        public int io_id { get; set; }
        public int io_status_id { get; set; }
        public int io_value_int4_1 { get; set; }
        public int io_value_int4_2 { get; set; }
        public int io_value_int4_3 { get; set; }
        public int io_value_int4_4 { get; set; }
        public string io_value_json { get; set; }
    }
    public class IOStatesInfoResult
    {
        public List<ResultInfo> success_list { get; set; }
        public List<ResultInfo> error_list { get; set; }
        public class ResultInfo { public int io_id { get; set; } }
    }
    /// <summary>
    /// 创建任务信息
    /// </summary>
    public class OrderInfo
    {
        public string order_name { get; set; }
        public int priority { get; set; }
        public DateTime dead_line { get; set; }
        public string ts_name { get; set; }
        public string parameters { get; set; }//{\"dock\":1}
    }
    public class OrderInfoResult
    {
        public int in_order_id { get; set; }
    }
    public class CancelOrderInfo
    {
        public List<int> order_list { get; set; }
    }
    public class CancelOrderInfoResult
    {
        public List<ResultInfo> success_list { get; set; }
        public List<ResultInfo> error_list { get; set; }
        public class ResultInfo { public int order_id { get; set; } }
    }
    public class QueryOrderResult
    {
        public int order_id { get; set; }
        public string order_name { get; set; }
        public int priority { get; set; }
        public DateTime dead_line { get; set; }
        public string ts_id { get; set; }
        public string parameters { get; set; }//{"LocationName":"3-2-B", "PalletType":1}",
        public string trigger { get; set; }
        public string command { get; set; }
        public string status { get; set; }
        public List<int> agv_list { get; set; }
        public string current_task { get; set; }
        public string current_dest { get; set; }
        public string current_opt { get; set; }
        public string current_omi { get; set; }
        public DateTime create_time { get; set; }
        public DateTime? active_time { get; set; }
        public DateTime? finished_time { get; set; }
        public DateTime? cancel_time { get; set; }
        public string response_timespan { get; set; }
        public string execute_timespa { get; set; }
        public string total_timespan { get; set; }
    }
    public class InteractInfoResult
    {
        public int interaction_info_id { get; set; }
        public string interaction_info_name { get; set; }
        public string interaction_info_desp { get; set; }
        public int interaction_info_type_id { get; set; }
        public object value_json { get; set; }//{"dock": 1},
        public string info_status { get; set; }
        public string return_value { get; set; }
    }
    public class UpdateInteractInfo
    {
        public int interaction_info_id { get; set; }
        public string info_status { get; set; }
        public string return_value { get; set; }
    }
    public class interaction_state
    {
        public string state { get; set; }
    }
    public class interaction_door
    {
        public string door { get; set; }
    }
    public class interaction_bit
    {
        public string order { get; set; }
    }
}
