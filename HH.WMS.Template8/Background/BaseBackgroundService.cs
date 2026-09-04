using Dm.util;
using WebApplication1.Devices;

namespace WebApplication1.Background {
    /// <summary>
    /// 通用后台任务基类，所有常驻任务继承它
    /// </summary>
    public abstract class BaseBackgroundService : BackgroundService {
        protected readonly ILogger _logger;
        protected string TaskName => GetType().Name;

        protected BaseBackgroundService(ILogger logger) {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            _logger.LogInformation("【{TaskName}】后台任务已启动", TaskName);

            try {
                await RunWorkLoop(stoppingToken);
            }
            catch (OperationCanceledException) {
                _logger.LogInformation("【{TaskName}】收到停止信号，正常退出", TaskName);
            }
            catch (Exception ex) {
                _logger.LogCritical(ex, "【{TaskName}】任务发生致命异常", TaskName);
                // 可在这里加自动重启逻辑
            }
        }

        /// <summary>
        /// 业务循环由子类实现
        /// </summary>
        protected abstract Task RunWorkLoop(CancellationToken stoppingToken);

        #region 后台轮询示例
        // 任务1：消息消费
        public class MessageConsumeTask : BaseBackgroundService {
            public MessageConsumeTask(ILogger<MessageConsumeTask> logger) : base(logger) { }

            protected override async Task RunWorkLoop(CancellationToken stoppingToken) {

                while (!stoppingToken.IsCancellationRequested) {
                    //_logger.LogDebug("消息消费执行一轮");
                    //_logger.LogInformation("【{TaskName}】后台任务正在执行", TaskName);
                    //LogHelper.Info($"后台任务正在执行", "后台");
                    //var a=new SqlHelper<object>().GetInstance().Queryable<Location>().First();
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }
     
        // 任务4：Modbus通信任务
        public class ModbusTask : BaseBackgroundService {
            public ModbusTask(ILogger<ModbusTask> logger) : base(logger) { }

            protected override async Task RunWorkLoop(CancellationToken stoppingToken) {
                while (!stoppingToken.IsCancellationRequested) {

                    //ModbusHelper.RegisterDevice("127.0.0.1", 502);
                    ModbusHelper.TestRead();
                    await Task.Delay(5000, stoppingToken);
                }
            }
        } 
        #endregion

    }
    
}
