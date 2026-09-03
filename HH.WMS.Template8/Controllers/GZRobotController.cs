using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static WebApplication1.ApiModels;

namespace WebApplication1.Controllers {
    /// <summary>
    /// GZRobot接口类
    /// </summary>
    [Route("gz")]
    public class GZRobotController : ControllerBase {
        [HttpPost]
        [Route("orderStatusReport")]
        public RCSReturnResult orderStatusReport([FromBody] OrderStatusModel model)
        {
            LogHelper.Info("orderStatusReport Request：" + JsonConvert.SerializeObject(model), "RCSTask");
            RCSReturnResult result = ApiHelper.OrderStatusNotify(model);
            LogHelper.Info("orderStatusReport Return：" + JsonConvert.SerializeObject(result), "RCSTask");
            return result;
        }

        [HttpPost]
        [Route("safetyInteraction")]
        public RCSReturnResult safetyInteraction([FromBody] SafetyInteractionModel model)
        {
            LogHelper.Info("safetyInteraction Request：" + JsonConvert.SerializeObject(model), "RCSTask");
            RCSReturnResult result = ApiHelper.SafetyInteraction(model);
            LogHelper.Info("safetyInteraction Return：" + JsonConvert.SerializeObject(result), "RCSTask");
            return result;
        }

    }
}
