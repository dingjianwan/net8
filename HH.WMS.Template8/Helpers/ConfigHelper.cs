using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication1 {

    /// <summary>
    /// 全局配置
    /// </summary>
    public class ConfigHelper {
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public static string DefaultConnectionString { get; set; }

        /// <summary>
        /// 程序接口地址
        /// </summary>
        public static string ProgramApiUrl { get; set; }
        /// <summary>
        /// 第三方接口地址
        /// </summary>
        public static string ThirdScheduleApiUrl { get; set; }

        public static List<DeviceInfo> DeviceInfos { get; set; } = new List<DeviceInfo>();
        public static List<TrafficZoneInfo> TrafficZoneInfos { get; set; } = new List<TrafficZoneInfo>();

        public static string TestMode { get; set; }
        /// <summary>
        /// 程序配置初始化
        /// </summary>
        public static void Init() {
            var jsonFile = System.AppDomain.CurrentDomain.BaseDirectory + "/config.json";
            using (System.IO.StreamReader file = System.IO.File.OpenText(jsonFile)) {
                using (JsonTextReader reader = new JsonTextReader(file)) {
                    JObject o = (JObject)JToken.ReadFrom(reader);
                    foreach (Newtonsoft.Json.Linq.JProperty keyValue in o.Properties()) {
                        // 数据库连接
                        if (keyValue.Name == "SqlServer") {
                            DefaultConnectionString = keyValue.Value.ToString();
                        }
                        if (keyValue.Name == "TestMode") {
                            TestMode = keyValue.Value.ToString();
                        }


                    }
                }
            }

        }
    }

    /// <summary>
    /// 重量区间对应层数
    /// </summary>
    public class WeightLayerItem {
        /// <summary>
        /// 最小重量
        /// </summary>
        public float MinWeight { get; set; }
        /// <summary>
        /// 最大重量
        /// </summary>
        public float MaxWeight { get; set; }
        /// <summary>
        /// 可放层规则
        /// </summary>
        public string LayerRule { get; set; }
    }

    public class DeviceInfo {
        public string address { get; set; }
        public string deviceName { get; set; }
        public string deviceNo { get; set; }
        public int deviceType { get; set; }
        public string[] location { get; set; }
        public bool enable { get; set; }
        public string[] virtualDoor { get; set; }
    }

    public class TrafficZoneInfo {
        public string unit { get; set; }
        public string lockNo { get; set; }
        public string door { get; set; }
        /// <summary>
        /// 1表示1期，2表示2期，3表示3期
        /// </summary>
        public int first { get; set; }
        public string[] taskType { get; set; }
        public string camera { get; set; }

    }
}
