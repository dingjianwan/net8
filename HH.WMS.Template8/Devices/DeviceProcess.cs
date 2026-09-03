namespace WebApplication1.Devices {
    internal class DeviceProcess {

        /// <summary>
        /// 解析PLC数据
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ip"></param>
        internal static void Analysis(string data, string ip) {

            /*
            data = data.Substring(4);
            var plc = AppConfig.DeviceInfos.Where(a => a.address == ip && a.enable == true).FirstOrDefault();
            if (plc != null)
            {
            }
            else
            {
                LogHelper.Info($"TCP信号处理：未查询到IP为{ip}的数据，请检查deviceInfo配置中心是否存在该IP的数据！");
            }
            */
        }


        private static Dictionary<string, signalInfo> doorStatus = new Dictionary<string, signalInfo>();//普通自动门字典
        private static Dictionary<string, signalInfo> lightStatus = new Dictionary<string, signalInfo>();
        public class signalInfo {
            public string info { get; set; }
            public DateTime modify { get; set; }
        }
       
      

        /// <summary>
        /// RCS-物流门安全交互
        /// </summary>
        /// <param name="deviceName"></param>
        /// <param name="deviceType"></param>
        /// <param name="applyCode"></param>
        internal static void RCSTraffic(string deviceName, string deviceType, string applyCode) {
            LogHelper.Info($"安全门开门请求  设备名称={deviceName} 设备类型:{deviceType} 开门请求={applyCode}");
            
        }
    }
}
