using System.Collections.Concurrent;
using NModbus;
using NModbus.IO;

namespace WebApplication1.Devices {
    internal class ModbusHelper { //static DeviceProcess() {

        private class DeviceState {
            public string Ip { get; set; }
            public int Port { get; set; }
            public System.Net.Sockets.TcpClient Client { get; set; }
            public IModbusMaster Master { get; set; }
            public int[] LatestData { get; set; }
            public DateTime LastReadTime { get; set; }
            public bool Connected { get; set; }
            public int FailCount { get; set; }
            public object LockObj { get; set; } = new object();
            // 每设备独立写队列：只装属于本设备的写请求
            public ConcurrentQueue<WriteRequest> WriteQueue { get; set; } = new ConcurrentQueue<WriteRequest>();
            // 每设备独立轮询线程（生命周期与设备绑定，互不影响）
            public Thread PollThread { get; set; }
            public CancellationTokenSource Cts { get; set; }
        }

        // 写保持寄存器请求
        internal class WriteRequest {
            public string Ip { get; set; }
            public int Port { get; set; }
            public int StartAddress { get; set; }
            public int[] Value { get; set; }
            public int ReTry { get; set; } = 3; // 重试次数`
        }

        // 字段：设备连接/状态字典（key=ip:port）
        private static readonly object _devLock = new object();
        private static readonly Dictionary<string, DeviceState> _devices = new Dictionary<string, DeviceState>();

        // 这些PLC统一读6个保持寄存器
        private const int READ_QTY = 6;
        private const int READ_ADDR = 0;
        private const int FAIL_THRESHOLD = 3;      // 连续失败达到该次数则从设备列表移除（避免返回过期数据）
        private const int POLL_INTERVAL_MS = 100;  // 每设备独立轮询间隔
        private const int CONNECT_TIMEOUT_MS = 3000; // 单设备连接超时，避免坏设备长期占用本设备线程

        private static volatile bool _running = true;

        private static string KeyOf(string ip, int port) => $"{ip}:{port}";

        /// <summary>
        /// 启动后台轮询框架（不再有全局轮询线程；每个设备注册时各自启动独立线程）
        /// </summary>
        internal static void Init() {
            _running = true;
            // 从配置中心预置已启用的设备，让它们在程序启动即开始独立轮询（AppConfig 未初始化时 DeviceInfos 为空，不会报错）
            try {
                RegisterDevice("192.168.1.100", 502);
            }
            catch (Exception ex) { LogHelper.Error("DeviceProcess.Init 预置设备异常:" + ex.Message, ex); }
            LogHelper.Info("Btns 每设备独立轮询框架已启动");
        }

        internal static void Stop() {
            _running = false;
            List<DeviceState> snapshot;
            lock (_devLock) { snapshot = new List<DeviceState>(_devices.Values); }
            foreach (var dev in snapshot) {
                try { dev.Cts?.Cancel(); } catch { }
            }
            foreach (var dev in snapshot) {
                try { dev.PollThread?.Join(3000); } catch { }
            }
            LogHelper.Info("DeviceProcess 已停止所有设备轮询线程");
        }

        /// <summary>
        /// 外部/接口注册新设备（自动加入内存，并启动该设备独立的轮询线程）
        /// </summary>
        internal static void RegisterDevice(string ip, int port) {
            if (string.IsNullOrEmpty(ip)) return;
            var key = KeyOf(ip, port);
            lock (_devLock) {
                if (_devices.ContainsKey(key)) return;
                var dev = new DeviceState
                {
                    Ip = ip,
                    Port = port,
                    Connected = false,
                    Cts = new CancellationTokenSource()
                };
                _devices[key] = dev;
                dev.PollThread = new Thread(() => DevicePollLoop(dev)) { IsBackground = true, Name = $"Poll-{ip}:{port}" };
                dev.PollThread.Start();
            }
        }

        /// <summary>
        /// 接口写入：把请求加入该设备独立的写队列（实际写入由设备独立线程完成）
        /// </summary>
        internal static void EnqueueWrite(WriteRequest wr) {
            if (wr == null) return;
            RegisterDevice(wr.Ip, wr.Port);
            DeviceState dev = null;
            lock (_devLock) { _devices.TryGetValue(KeyOf(wr.Ip, wr.Port), out dev); }
            dev?.WriteQueue.Enqueue(wr);
        }

        /// <summary>
        /// 接口读取：直接从内存取最新结果（按起始地址/长度切片）
        /// </summary>
        internal static int[] GetLatestData(string ip, int port, int startAddress, int length) {
            var key = KeyOf(ip, port);
            DeviceState state = null;
            lock (_devLock) { _devices.TryGetValue(key, out state); }
            if (state == null || state.LatestData == null) return new int[0];
            if (startAddress < 0) startAddress = 0;
            if (startAddress >= state.LatestData.Length) return new int[0];
            int take = Math.Min(length, state.LatestData.Length - startAddress);
            if (take <= 0) return new int[0];
            int[] slice = new int[take];
            Array.Copy(state.LatestData, startAddress, slice, 0, take);
            return slice;
        }

        // 单设备独立轮询线程：持长连接，先处理本设备写队列，再读PLC，循环推进
        private static void DevicePollLoop(DeviceState dev) {
            LogHelper.Info($"设备 {dev.Ip}:{dev.Port} 独立轮询线程已启动");
            var db = new SqlHelper<Object>().GetInstance();
            var httpHelper = new HttpHelper();
            try {
                while (_running && !dev.Cts.IsCancellationRequested) {
                    try {
                        if (!EnsureConnected(dev)) {
                            // 连接失败：清空内存数据，避免向接口返回过期值；连续失败达阈值则移除该设备并结束本线程
                            dev.LatestData = null;
                            if (++dev.FailCount >= FAIL_THRESHOLD) {
                                LogHelper.Info($"设备 {dev.Ip}:{dev.Port} 连续连接失败{FAIL_THRESHOLD}次，移除设备并结束线程");
                                RemoveDevice(dev);
                                return;
                            }
                            Thread.Sleep(1000);
                            continue;
                        }

                        // 1) 处理本设备写队列（只影响自己，慢设备不会拖慢其他设备）
                        while (dev.WriteQueue.TryDequeue(out var w)) {
                            LogHelper.Info($"写保持寄存器 {dev.Ip}:{dev.Port} addr={w.StartAddress} val={string.Join(",", w.Value)} 准备");
                            //var curr = GetLatestData(dev.Ip, dev.Port, w.StartAddress, w.Value.Length);
                            //if (curr != null && curr.SequenceEqual(w.Value)) {
                            //    LogHelper.Info($"写保持寄存器 {dev.Ip}:{dev.Port} addr={w.StartAddress} val={string.Join(",", w.Value)} 取消，值未变化");
                            //}
                            //else {
                            try {
                                if (w.Value != null && w.Value.Length == 1)
                                    dev.Master.WriteSingleRegister(1, (ushort)w.StartAddress, (ushort)w.Value[0]);
                                else
                                    dev.Master.WriteMultipleRegisters(1, (ushort)w.StartAddress,
                                        Array.ConvertAll(w.Value ?? new int[0], x => (ushort)x));
                                LogHelper.Info($"写保持寄存器 {dev.Ip}:{dev.Port} addr={w.StartAddress} val={string.Join(",", w.Value)} 成功");
                                Thread.Sleep(POLL_INTERVAL_MS);
                            }
                            catch (Exception ex) {
                                LogHelper.Info($"写保持寄存器 {dev.Ip}:{dev.Port} addr={w.StartAddress} val={string.Join(",", w.Value)} 失败: {ex.Message}");
                                w.ReTry--;
                                if (w.ReTry > 0) {
                                    //失败后再放入队列，最多重试3次
                                    dev.WriteQueue.Enqueue(w);
                                }
                            }
                            //}


                        }

                        // 2) 读PLC刷新内存
                        var rawData = dev.Master.ReadHoldingRegisters(1, (ushort)READ_ADDR, (ushort)READ_QTY);
                        if (rawData != null && rawData.Length == READ_QTY) {
                            var value = string.Join(",", rawData);
                            Console.WriteLine(DateTime.Now.ToString() + "读到消息" + value);
                            var intData = new int[READ_QTY];
                            for (int i = 0; i < READ_QTY; i++) intData[i] = rawData[i];
                            lock (_devLock) { dev.LatestData = intData; }
                            dev.LastReadTime = DateTime.Now;
                            dev.Connected = true;
                            dev.FailCount = 0;
                            LogHelper.Info($"读保持寄存器 {dev.Ip}:{dev.Port} addr={READ_ADDR} val={value} 成功");
                            //todo 创建任务或者取消任务
                        }
                        else {
                            Console.WriteLine($"Failed to read holding registers {dev.Ip}:{dev.Port}.");
                            dev.LatestData = null;
                            dev.FailCount++;
                            if (dev.FailCount >= FAIL_THRESHOLD) {
                                LogHelper.Info($"设备 {dev.Ip}:{dev.Port} 连续读取失败{FAIL_THRESHOLD}次，移除设备并结束线程");
                                RemoveDevice(dev);
                                return;
                            }
                            LogHelper.Info($"读保持寄存器 {dev.Ip}:{dev.Port} 失败");
                        }
                    }
                    catch (Exception ex) {
                        LogHelper.Error($"设备 {dev.Ip}:{dev.Port} 轮询异常:{ex.Message}", ex);
                        dev.LatestData = null;
                        dev.FailCount++;
                        if (dev.FailCount >= FAIL_THRESHOLD) {
                            LogHelper.Info($"设备 {dev.Ip}:{dev.Port} 连续异常{FAIL_THRESHOLD}次，移除设备并结束线程");
                            RemoveDevice(dev);
                            return;
                        }
                    }
                    Thread.Sleep(POLL_INTERVAL_MS);
                }
            }
            catch (Exception ex) {
                LogHelper.Error($"设备 {dev.Ip}:{dev.Port} 轮询线程异常退出:{ex.Message}", ex);
            }
            finally {
                CloseDevice(dev);
            }
        }

        private static bool EnsureConnected(DeviceState dev) {
            try {
                if (dev.Client != null && IsSocketAlive(dev.Client.Client)) {
                    return true;
                }

                CloseDevice(dev);

                var client = new System.Net.Sockets.TcpClient();
                client.NoDelay = true;
                EnableTcpKeepAlive(client.Client, 5000, 1000);

                var task = client.ConnectAsync(dev.Ip, dev.Port);
                if (!task.Wait(TimeSpan.FromMilliseconds(CONNECT_TIMEOUT_MS))) {
                    LogHelper.Info($"连接设备超时 {dev.Ip}:{dev.Port}");
                    client.Close();
                    return false;
                }

                dev.Client = client;
                dev.Master = new ModbusFactory().CreateIpMaster(new TcpClientAdapter(client));
                dev.Master.Transport.Retries = 1;
                dev.Master.Transport.RetryOnOldResponseThreshold = 10;
                dev.Connected = true;
                dev.FailCount = 0;
                LogHelper.Info($"设备连接成功 {dev.Ip}:{dev.Port}");
                return true;
            }
            catch (Exception ex) {
                LogHelper.Info($"EnsureConnected {dev.Ip}:{dev.Port} 失败:{ex.Message}");
                dev.Connected = false;
                CloseDevice(dev);
                return false;
            }
        }

        private static bool IsSocketAlive(System.Net.Sockets.Socket socket) {
            try {
                if (!socket.Connected) return false;
                return !socket.Poll(100, System.Net.Sockets.SelectMode.SelectRead) || socket.Available > 0;
            }
            catch {
                return false;
            }
        }

        private static void EnableTcpKeepAlive(System.Net.Sockets.Socket socket, int keepAliveTimeMs, int keepAliveIntervalMs) {
            try {
                socket.SetSocketOption(System.Net.Sockets.SocketOptionLevel.Socket, System.Net.Sockets.SocketOptionName.KeepAlive, true);
                byte[] inOptionValues = new byte[12];
                BitConverter.GetBytes((uint)1).CopyTo(inOptionValues, 0);
                BitConverter.GetBytes((uint)keepAliveTimeMs).CopyTo(inOptionValues, 4);
                BitConverter.GetBytes((uint)keepAliveIntervalMs).CopyTo(inOptionValues, 8);
                socket.IOControl(System.Net.Sockets.IOControlCode.KeepAliveValues, inOptionValues, null);
            }
            catch {
            }
        }

        private static void CloseDevice(DeviceState dev) {
            try { dev.Master?.Dispose(); } catch { }
            dev.Master = null;
            try { dev.Client?.Close(); } catch { }
            dev.Client = null;
        }

        /// <summary>
        /// 从注册设备列表移除（连不上的PLC，如错误IP/端口）；清理连接、清空本设备写队列、并通知其线程退出
        /// </summary>
        private static void RemoveDevice(DeviceState dev) {
            CloseDevice(dev);
            var key = KeyOf(dev.Ip, dev.Port);
            lock (_devLock) { _devices.Remove(key); }
            // 清空本设备残留写请求（不再有全局队列）
            while (dev.WriteQueue.TryDequeue(out _)) { }
            try { dev.Cts?.Cancel(); } catch { }
            LogHelper.Info($"已移除设备 {dev.Ip}:{dev.Port}");
        }

        //-------------------------------------上面是轮询读plc代码，下面是单个测试-------------------------------------

        public static void TestRead() {

            try {
                var client = new System.Net.Sockets.TcpClient();
                client.NoDelay = true;
                EnableTcpKeepAlive(client.Client, 5000, 1000);

                var task = client.ConnectAsync("127.0.0.1", 502);
                if (!task.Wait(TimeSpan.FromMilliseconds(CONNECT_TIMEOUT_MS))) {
                    client.Close();
                    return;
                }
                var master = new ModbusFactory().CreateIpMaster(new TcpClientAdapter(client));
                //读保持寄存器（从站地址=单元号、起始地址、数量）——从站地址必须与从站实际单元号一致(与正式轮询一致用1)
                var data = master.ReadHoldingRegisters(1, 0, 5);
                LogHelper.Info($"读取数据 {string.Join(",", data)}");
            }
            catch (Exception ex) {

                Console.WriteLine(ex.Message);
            }
        }

    }


}
