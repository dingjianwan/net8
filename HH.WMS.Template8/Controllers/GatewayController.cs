using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;


namespace WebApplication1.Controllers {
    /// <summary>
    /// 标准接口类
    /// </summary>
    [Route("api/pei")]
    public class GatewayController : ControllerBase {
        /// <summary>
        /// 替换mobox可编程接口
        /// </summary>
        [HttpPost]
        [Route("add")]
        public MoboxResult Add([FromBody] GatewayMessage model) {

            MoboxResult res = null;
            switch (model.Source) {
                case "GZ-WCS": res = GZWCSApiHandler.Handle(model); break;
            }
            return res;
        }
    }
    public class GatewayMessage {
        public string Name { get; set; }
        public string Source { get; set; }
        // data 用 JToken，兼容对象、数组两种形态
        public JToken Data { get; set; }
    }
    public class MoboxResult {
        public int err_code { get; set; }
        public string err_msg { get; set; }
        public object result { get; set; }
    }
}
