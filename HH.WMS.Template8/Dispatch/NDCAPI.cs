using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication1 {
    public class NDCApi
    {
        public static string NDCApiUrl { set; get; }
        static NDCApi()
        {
            NDCApiUrl = ConfigHelper.ThirdScheduleApiUrl;//Settings.HostToAgvServerUrl
        }


        public static AgvApiResult AddOrderNew(int ts, int pri, string taskNo, List<param> param)
        {
            var httpH = new HttpHelper();
            var agvApiResult = new AgvApiResult();
            var model = new AddOrderNewModel();
            model.ts_no = ts;
            model.pri = pri;
            model.task_no = taskNo;
            model.param = param;
            try
            {
                string jsonInfo = JsonConvert.SerializeObject(model);
                var AppKey = "";
                var AppSecret = "";
                var ReqTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                //LogHelper.Info($"加密前 AppKey={AppKey} AppSecret={AppSecret} ReqTime={ReqTime}");
                var ReqVerify = GetMd5FromString(AppKey + AppSecret + ReqTime);
                //LogHelper.Info($"加密后 AppKey={AppKey} AppSecret={AppSecret} ReqTime={ReqTime} ReqVerify={ReqVerify}");
                LogHelper.Info($"任务{taskNo}下发，{jsonInfo}", "NDC");
                var result = httpH.WebPost(NDCApiUrl + "Add", jsonInfo, "application/json", AppKey, ReqTime, false);
                LogHelper.Info($"任务下发结果res={result}", "NDC");
                if (result != "")
                {
                    agvApiResult = JsonConvert.DeserializeObject<AgvApiResult>(result);
                }
                else
                {
                    if (ConfigHelper.TestMode == "Debug") {
                        agvApiResult.err_code = 0;
                        agvApiResult.err_msg = "测试模式，模拟成功返回！";
                    }
                    else {
                        agvApiResult.err_code = -1;
                        agvApiResult.err_msg = "任务下发结果为空！";
                    }
                }
                return agvApiResult;
            }
            catch (Exception e)
            {
                LogHelper.Error($"任务下发失败 res={e.Message}", e, "NDC");
                agvApiResult.err_code = -1;
                agvApiResult.err_msg = e.Message;
                return agvApiResult;
            }
        }

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="sInput"></param>
        /// <returns></returns>
        public static string GetMd5FromString(string sInput)
        {
            var lstData = Encoding.GetEncoding("utf-8").GetBytes(sInput);
            var lstHash = new MD5CryptoServiceProvider().ComputeHash(lstData);
            var result = new StringBuilder(32);
            for (int i = 0; i < lstHash.Length; i++)
            {
                result.Append(lstHash[i].ToString("x2").ToUpper());
            }
            return result.ToString();
        }

        public static AgvApiResult AddOrderNew(AddOrderNewModel model)
        {
            var httpH = new HttpHelper();
            var agvApiResult = new AgvApiResult();

            try
            {
                string jsonInfo = JsonConvert.SerializeObject(model);
                LogHelper.Info($"任务{model.task_no}下发，{jsonInfo}", "NDC");

                var result = httpH.WebPost(NDCApiUrl + "Add", jsonInfo);
                LogHelper.Info($"任务下发结果res={result}", "NDC");

                agvApiResult = JsonConvert.DeserializeObject<AgvApiResult>(result);
                return agvApiResult;
            }
            catch (Exception e)
            {
                LogHelper.Error($"任务下发失败 res={e.Message}", e, "NDC");
                agvApiResult.err_code = -1;
                agvApiResult.err_msg = e.Message;
                return agvApiResult;
            }
        }

        public static AgvApiResult CancelOrder(string tsNo, bool is_force = true)
        {
            var httpH = new HttpHelper();
            var agvApiResult = new AgvApiResult();
            var model = new CancelOrderModel();
            model.task_no = tsNo;
            model.is_force = is_force;
            try
            {
                string jsonInfo = JsonConvert.SerializeObject(model);
                LogHelper.Info($"任务{model.task_no}取消，{jsonInfo}", "NDC");

                var result = httpH.WebPost(NDCApiUrl + "Cancel", jsonInfo);
                LogHelper.Info($"任务{model.task_no}取消结果={result}", "NDC");
                agvApiResult = JsonConvert.DeserializeObject<AgvApiResult>(result);
                return agvApiResult;
            }
            catch (Exception e)
            {
                LogHelper.Error($"CancelOrder  res={e.Message}", e, "NDC");
                agvApiResult.err_code = -1;
                agvApiResult.err_msg = e.Message;
                return agvApiResult;
            }
        }

        public static AgvApiResult CancelOrder(CancelOrderModel model)
        {
            var httpH = new HttpHelper();
            var agvApiResult = new AgvApiResult();
            try
            {
                string jsonInfo = JsonConvert.SerializeObject(model);
                LogHelper.Info($"任务{model.task_no}取消，{jsonInfo}", "NDC");

                var result = httpH.WebPost(NDCApiUrl + "Cancel", jsonInfo);
                LogHelper.Info($"任务{model.task_no}取消结果={result}", "NDC");

                agvApiResult = JsonConvert.DeserializeObject<AgvApiResult>(result);
                return agvApiResult;
            }
            catch (Exception e)
            {
                LogHelper.Error($"CancelOrder  res={e.Message}", e, "NDC");
                agvApiResult.err_code = -1;
                agvApiResult.err_msg = e.Message;
                return agvApiResult;
            }

        }

        public static AgvApiResult ChangeOrderPri(string taskNo, int newPri)
        {
            var httpH = new HttpHelper();
            var agvApiResult = new AgvApiResult();
            var model = new ChangePriModel();
            model.task_no = taskNo;
            model.pri = newPri;
            try
            {
                string jsonInfo = JsonConvert.SerializeObject(model);
                LogHelper.Info($"任务{model.task_no}优先级更改，{jsonInfo}", "NDC");

                var result = httpH.WebPost(NDCApiUrl + "ChangePri", jsonInfo);
                LogHelper.Info($"任务{model.task_no}优先级更改结果={result}", "NDC");

                agvApiResult = JsonConvert.DeserializeObject<AgvApiResult>(result);
                return agvApiResult;
            }
            catch (Exception e)
            {
                LogHelper.Error($"ChangeOrderPri res={e.Message}", e, "NDC");
                agvApiResult.err_code = -1;
                agvApiResult.err_msg = e.Message;
                return agvApiResult;
            }
        }

        public static AgvApiResult ChangeOrderParam(string taskNo, int paramNo, string paramStr)
        {
            var httpH = new HttpHelper();
            var agvApiResult = new AgvApiResult();
            var model = new ChangeParamModel();
            model.task_no = taskNo;
            model.param_no = paramNo;
            model.param = paramStr;
            try
            {
                string jsonInfo = JsonConvert.SerializeObject(model);
                LogHelper.Info($"任务{model.task_no}参数更改，{jsonInfo}", "NDC");

                var result = httpH.WebPost(NDCApiUrl + "ChangeParam", jsonInfo);
                LogHelper.Info($"任务{model.task_no}参数更改结果={result}", "NDC");

                agvApiResult = JsonConvert.DeserializeObject<AgvApiResult>(result);
                return agvApiResult;
            }
            catch (Exception e)
            {
                LogHelper.Error($"ChangeOrderParam res={e.Message}", e, "NDC");
                agvApiResult.err_code = -1;
                agvApiResult.err_msg = e.Message;
                return agvApiResult;
            }
        }

        public static AgvApiResult ChangeOrderParam(ChangeParamModel model)
        {
            var httpH = new HttpHelper();
            var agvApiResult = new AgvApiResult();
            try
            {
                string jsonInfo = JsonConvert.SerializeObject(model);
                LogHelper.Info($"任务{model.task_no}参数更改，{jsonInfo}", "NDC");

                var result = httpH.WebPost(NDCApiUrl + "ChangeParam", jsonInfo);
                LogHelper.Info($"任务{model.task_no}参数更改结果={result}", "NDC");

                agvApiResult = JsonConvert.DeserializeObject<AgvApiResult>(result);
                return agvApiResult;
            }
            catch (Exception e)
            {
                LogHelper.Error($"ChangeOrderParam res={e.Message}", e, "NDC");
                agvApiResult.err_code = -1;
                agvApiResult.err_msg = e.Message;
                return agvApiResult;
            }
        }


    }

    /// <summary>
    /// 返回信息Model
    /// </summary>
    public class AgvApiResult
    {
        public int err_code { set; get; }//异常码：0 - 正常，其它值为异常错误码
        public string err_msg { set; get; }//返回的错误描述，在 err_code <> 0 时返回
        public object result { set; get; }//正确返回的结果内容，在 err_code = 0 且有返回内容时
    }

    public class AddOrderNewModel
    {
        public int ts_no { set; get; }//TS 号，必须有值	
        public int pri { set; get; }//优先级
        public string task_no { set; get; }//上游任务编码，如果 no_feedback = 1 时，可以为空
        public List<param> param { set; get; } = new List<param>();//参数列表
    }


    public class param
    {
        public string name { set; get; }//参数名
        public string value { set; get; }//参数值
    }

    public class CancelOrderModel
    {
        public string task_no { set; get; }//上游任务编码
        public bool is_force { set; get; } = true;//是否强制取消，1 – 强制
    }

    public class ChangeParamModel
    {
        public string task_no { set; get; }//上游任务编码
        public int param_no { set; get; }//参数号
        public string param { set; get; }//参数内容，多个参数以英文分号(;)分隔
    }

    public class ChangePriModel
    {
        public string task_no { set; get; }//上游任务编码
        public int pri { set; get; }//新优先级
    }
}
