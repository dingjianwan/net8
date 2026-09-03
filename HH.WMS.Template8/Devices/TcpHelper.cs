using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace WebApplication1 {
    public class TcpHelper {
        /// <summary>已接入的客户端连接（键为客户端 IP），供 TcpServerSend 按 IP 下发数据</summary>
        private static readonly ConcurrentDictionary<string, ClientEntry> _clientEntries = new();

        /// <summary>单个客户端连接信息</summary>
        private sealed class ClientEntry {
            public TcpClient Client = null!;
            public NetworkStream Stream = null!;
            /// <summary>串行化对同一连接的写入，防止并发写导致数据帧交错</summary>
            public readonly SemaphoreSlim SendGate = new(1, 1);
        }

        /// <summary>TCP 监听主逻辑</summary>
        public static async Task RunTcpServerAsync(IPAddress listenIp, int port, IServiceProvider sp, CancellationToken stopToken) {
            var log = sp.GetRequiredService<ILoggerFactory>().CreateLogger("TcpServer");
            TcpListener listener = new TcpListener(listenIp, port);
            try {
                listener.Start();
                log.LogInformation("TCP服务启动成功，监听端口:{Port}", port);

                while (!stopToken.IsCancellationRequested) {
                    // 异步等待客户端连接
                    var client = await listener.AcceptTcpClientAsync(stopToken);
                    // 每个客户端单独开任务处理，不阻塞新连接接入
                    _ = HandleClientAsync(client, sp, stopToken);
                }
            }
            catch (OperationCanceledException) {
                log.LogInformation("TCP服务收到停止信号，关闭监听");
            }
            catch (Exception ex) {
                log.LogError(ex, "TCP监听异常，5秒后重启");
                await Task.Delay(5000, stopToken);
                // 异常后重启监听
                _ = RunTcpServerAsync(listenIp, port, sp, stopToken);
            }
            finally {
                listener.Stop();
                // 停止监听时断开所有已接入的客户端
                foreach (var kv in _clientEntries) {
                    try { kv.Value.Client.Close(); } catch { /* 忽略 */ }
                }
                _clientEntries.Clear();
                log.LogInformation("TCP监听已关闭");
            }
        }

        /// <summary>向已接入的指定 IP 客户端发送数据</summary>
        internal static bool TcpServerSend(string ip, byte[] bytes) {
            if (!_clientEntries.TryGetValue(ip, out var entry)) {
                LogHelper.Info($"TCP发送失败，未找到已接入的客户端:IP={ip}", "TcpServer");
                return false;
            }
            try {
                entry.SendGate.Wait();
                try {
                    entry.Stream.Write(bytes, 0, bytes.Length);
                    entry.Stream.Flush();
                }
                finally {
                    entry.SendGate.Release();
                }
                return true;
            }
            catch (Exception ex) {
                // 发送失败说明连接已不可用，清理该连接，下次可重连后再发
                _clientEntries.TryRemove(new KeyValuePair<string, ClientEntry>(ip, entry));
                try { entry.Client.Close(); } catch { /* 忽略 */ }
                LogHelper.Info($"TCP发送失败，连接已清理:IP={ip},Err={ex.Message}", "TcpServer");
                return false;
            }
        }

        /// <summary>单个TCP客户端消息处理</summary>
        static async Task HandleClientAsync(TcpClient client, IServiceProvider sp, CancellationToken globalStopToken) {
            using var clientScope = sp.CreateScope();
            var scopedSp = clientScope.ServiceProvider;
            var log = scopedSp.GetRequiredService<ILoggerFactory>().CreateLogger("TcpClient");
            var ct = CancellationTokenSource.CreateLinkedTokenSource(globalStopToken);
            ct.CancelAfter(TimeSpan.FromSeconds(30)); // 空闲30秒断开

            // 将连接信息登记到内存，供 TcpServerSend 按 IP 下发数据
            var remoteIp = (client.Client.RemoteEndPoint as IPEndPoint)?.Address.ToString() ?? "unknown";
            var entry = new ClientEntry { Client = client, Stream = client.GetStream() };
            _clientEntries[remoteIp] = entry;
            log.LogInformation("TCP客户端接入:{Ip}", remoteIp);

            try {
                byte[] buffer = new byte[1024];
                while (!ct.Token.IsCancellationRequested) {
                    int readLen = await entry.Stream.ReadAsync(buffer, 0, buffer.Length, ct.Token);
                    if (readLen == 0) break; // 客户端主动断开

                    byte[] recvData = buffer.Take(readLen).ToArray();
                    string msg = System.Text.Encoding.UTF8.GetString(recvData);
                    //log.LogDebug("收到TCP客户端消息:{Msg}", msg);
                    LogHelper.Info($"收到TCP客户端消息:{msg}", "TcpClient");
                    CPProcess.Process(remoteIp, msg);

                    // 回复客户端（与 TcpServerSend 共用发送门，避免同连接并发写）
                    /*
                    byte[] response = System.Text.Encoding.UTF8.GetBytes($"服务已收到:{msg}");
                    await entry.SendGate.WaitAsync(ct.Token);
                    try {
                        await entry.Stream.WriteAsync(response, 0, response.Length, ct.Token);
                        await entry.Stream.FlushAsync(ct.Token);
                    }
                    finally {
                        entry.SendGate.Release();
                    }
                    */
                }
            }
            catch (Exception ex) {
                log.LogError(ex, "TCP客户端连接异常");
            }
            finally {
                // 仅当仍是本次连接实例时才移除，避免误删同 IP 重连后的新连接
                _clientEntries.TryRemove(new KeyValuePair<string, ClientEntry>(remoteIp, entry));
                client.Close();
                ct.Dispose();
                log.LogInformation("TCP客户端连接断开:{Ip}", remoteIp);
            }
        }
    }
}
