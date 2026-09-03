using S7.Net;
namespace WebApplication1.Devices {

    /// <summary>
    /// 西门子plc
    /// </summary>
    public class S7Helper {
        private static bool debug = true;
        private static Plc plc = null;
        static S7Helper() {
            Init();
        }
        private static Dictionary<string, Plc> plcDic = new Dictionary<string, Plc>();
        private static void Init() {
            //配置文件读取所有的plc进行初始化
            try {
                var plc1 = new Plc(CpuType.S71500, "", 0, 1);
                plcDic.Add("plc1", plc1);
                Link(plc1);
            }
            catch (Exception ex) {

                Console.WriteLine("S7Helper Init err=" + ex.Message);
            }
        }
        private static Plc GetPlc(string plc) {
            if (plcDic.ContainsKey(plc)) {
                return plcDic[plc];
            }
            else {
                return null;
            }
        }
        public static Dictionary<string, string> s7TestData = new Dictionary<string, string>();
        private static void Link(Plc plc) {
            try {
                //if (!plc.IsConnected) {
                plc.Close();
                plc.Open();
                if (plc.IsConnected) {
                    Console.WriteLine($"已连接到plc{plc.IP}");
                }
                else {
                    Console.WriteLine($"plc{plc.IP}连接失败");
                    LogHelper.Info($"plc{plc.IP}连接失败", "Plc");
                }

                //}
            }
            catch (Exception ex) {
                Console.WriteLine($"plc{plc.IP}连接失败，err={ex.Message}");
                LogHelper.Info($"plc{plc.IP}连接失败，err={ex.Message}");
                //Init();
            }

        }
        //https://www.ad.siemens.com.cn/productportal/Prods/S7-1200_PLC_EASY_PLUS/SmartSMS/060.html
        //https://www.ad.siemens.com.cn/productportal/Prods/S7-1200_PLC_EASY_PLUS/07-Program/02-basic/01-Data_Type/09-String.html



        internal static short[] ReadInt(string device, int db, int byteAddr, int count) {
            short[] result = null;
            try {
                if (debug) {
                    var s7Key = $"int_{db}_{byteAddr}_{count}";
                    if (s7TestData.ContainsKey(s7Key)) {
                        var data = s7TestData[s7Key].Split(',');
                        if (data.Length == count) {
                            result = Array.ConvertAll(data, s => short.Parse(s));
                        }
                        else {
                            result = new short[count];
                            s7TestData[s7Key] = string.Join(",", result);
                        }
                        Console.WriteLine($"读取plc {device}信息成功，  addr={byteAddr} data={string.Join(",", result)}");
                    }
                }
                else {
                    var plc = GetPlc(device);
                    if (plc != null) {
                        if (plc.IsConnected) {
                            result = (short[])plc.Read(DataType.DataBlock, db, byteAddr, VarType.Int, count, 0);
                            Console.WriteLine($"读取plc {device}信息成功，ip={plc.IP}  addr={byteAddr} data={string.Join(",", result)}");
                            if (result.Length == 0) {
                                Console.WriteLine($"plc {device}准备重新连接");
                                Link(plc);
                            }
                        }
                        else {
                            Console.WriteLine($"准备连接plc {device}");
                            Link(plc);
                        }
                    }
                    else {
                        Console.WriteLine($"plc {device}不存在");
                    }

                }

            }
            catch (Exception ex) {
                Console.WriteLine($"ReadInt，device={device}  addr={byteAddr} count={count} err={ex.Message}");
                LogHelper.Error($"ReadInt，device={device}  addr={byteAddr}  count={count} err={ex.Message}", ex);
            }
            return result;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="byteAddr"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static bool WriteInt(int db, int byteAddr, short data) {
            var result = false;
            try {
                if (plc.IsConnected) {
                    plc.Write(DataType.DataBlock, db, byteAddr, data);
                    Console.WriteLine($"写入plc信息，ip={plc.IP} addr={byteAddr} data={data} ");
                    LogHelper.Info($"写入plc信息，ip={plc.IP} addr={byteAddr} data={data} ");
                    if (result) {
                        //写完再读一次确认
                        var readData = (short)plc.Read(DataType.DataBlock, db, byteAddr, VarType.Int, 1, 0);
                        Console.WriteLine($"读取plc信息，ip={plc.IP} addr={byteAddr} data={data} res={string.Join(", ", readData)}");
                        LogHelper.Info($"读取plc信息，ip={plc.IP} addr={byteAddr} data={data} res={string.Join(", ", readData)}", "PLC");
                        result = readData == data;
                    }


                }
                else {
                    Console.WriteLine("准备连接plc1");
                    Link(plc);
                }
            }
            catch (Exception ex) {
                LogHelper.Error($"写入plc1信息失败，ip={plc.IP} addr={byteAddr} data={data} err={ex.Message}", ex);
            }
            return result;
        }
        public static object ReadBit(string device, int db, int byteAddr, byte bitAddr) {
            object result = null;
            try {
                if (debug) {
                    var s7Key = $"bit_{db}_{byteAddr}_{bitAddr}";
                    if (s7TestData.ContainsKey(s7Key)) {
                        var data = s7TestData[s7Key];
                        if (data == "1") {
                            result = true;
                        }
                        else { result = false; }
                        Console.WriteLine($"读取plc {device}信息成功，  addr={byteAddr} data={result.ToString()}");
                    }
                }
                else {
                    var plc = GetPlc(device);
                    if (plc != null) {
                        if (plc.IsConnected) {
                            result = plc.Read(DataType.DataBlock, db, byteAddr, VarType.Int, 1, bitAddr);
                            Console.WriteLine($"读取plc {device}信息成功，ip={plc.IP}  addr={byteAddr} data={result.ToString()}");
                        }
                        else {
                            Console.WriteLine($"准备连接plc {device}");
                            Link(plc);
                        }
                    }
                    else {
                        Console.WriteLine($"plc {device}不存在");
                    }
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"ReadBit，device={device}  addr={byteAddr} bit={bitAddr} err={ex.Message}");
                LogHelper.Error($"ReadBit，device={device}  addr={byteAddr} bit={bitAddr} err={ex.Message}", ex);
            }
            return result;
        }
        public static string ReadStr(string device, int db, int byteAddr, int count) {
            string result = string.Empty;
            try {
                if (debug) {
                    var s7Key = $"str_{db}_{byteAddr}_{count}";
                    if (s7TestData.ContainsKey(s7Key)) {
                        var data = s7TestData[s7Key];
                        if (data.Length == count) {
                            result = data;
                            Console.WriteLine($"ReadStr 成功， addr={byteAddr}  res={result}");
                        }
                    }
                }
                else {
                    if (plc.IsConnected) {
                        result = plc.Read(DataType.DataBlock, 100, byteAddr, VarType.String, count, 0).ToString();
                        Console.WriteLine($"ReadStr 成功，ip={plc.IP} addr={byteAddr}  res={result}");
                        if (result.Length == 0) {
                            Link(plc);
                        }
                    }
                    else {
                        Console.WriteLine("准备连接plc");
                        Link(plc);
                    }
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"ReadStr，device={device}  addr={byteAddr}  count={count} err={ex.Message}");
                LogHelper.Error($"ReadStr，device={device}  addr={byteAddr}  count={count} err={ex.Message}", ex);
            }
            return result;
        }


        #region 用于模拟测试
        /// <summary>
        /// short类型，一个占2个byte
        /// </summary>
        public class DBWModel {
            public int db { get; set; }
            public int byteAddr { get; set; }
            /// <summary>
            /// int类型需要用逗号分开，string不需要
            /// </summary>
            public string value { get; set; }
        }

    }
    #endregion
}
