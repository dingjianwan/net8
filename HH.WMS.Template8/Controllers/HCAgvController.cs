using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static WebApplication1.ApiModels;


namespace WebApplication1.Controllers {
  
    [Route("api")]
    public class HcAgvController : ControllerBase {
        /// <summary>
        /// agv上报状态
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AGVCallbackState")]
        [ProducesResponseType(typeof(ReturnResult), 200)]   // Swagger 上能同时看到两种响应
        [ProducesResponseType(typeof(ReturnResult), 409)]
        public ActionResult<ReturnResult> AGVCallbackState([FromBody] AgvTaskState model) {
            LogHelper.Info("AGVCallbackState Request：" + JsonConvert.SerializeObject(model), "HosttoagvTask");

            bool ok = ApiHelper.OperateTaskStatus(model);

            // 成功/失败都用同一个 ReturnResult 结构，方便 AGV 侧统一解析
            var result = ok
                ? new ReturnResult()
                : new ReturnResult { ResultCode = 409, ResultMsg = "处理失败，可重试" };

            LogHelper.Info("AGVCallbackState Return：" + JsonConvert.SerializeObject(result), "HosttoagvTask");

            // 等价于老版本的 ResponseMessage(Request.CreateResponse(...))，不抛异常
            return StatusCode(ok ? 200 : 409, result);
            // 语义化写法（仅 409 场景）：return ok ? (ActionResult<ReturnResult>)Ok(result) : Conflict(result);
        }
    }
}
