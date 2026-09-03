using WebApplication1;
using WebApplication1.Background;

namespace HH.WMS.Template8.Background {
    /// <summary>
    /// 每天固定时间点触发的定时任务基类（如 08:00 / 14:30 / 20:00）
    /// </summary>
    public abstract class DailyScheduleBackgroundService : BaseBackgroundService {
        private readonly TimeOnly[] _triggerTimes;
        private readonly SemaphoreSlim _runGate = new(1, 1);

        protected DailyScheduleBackgroundService(ILogger logger, params TimeOnly[] triggerTimes) : base(logger) {
            if (triggerTimes == null || triggerTimes.Length == 0)
                throw new ArgumentException("至少配置一个触发时间点", nameof(triggerTimes));

            _triggerTimes = triggerTimes.Distinct().OrderBy(t => t).ToArray();
        }

        protected override async Task RunWorkLoop(CancellationToken stoppingToken) {
            while (!stoppingToken.IsCancellationRequested) {
                DateTime next = GetNextRunTime(DateTime.Now);
                _logger.LogInformation("【{TaskName}】下次执行时间:{Next}", TaskName, next.ToString("yyyy-MM-dd HH:mm:ss"));

                try {
                    await Task.Delay(next - DateTime.Now, stoppingToken);
                }
                catch (OperationCanceledException) { break; }   // 停止信号，正常退出

                if (stoppingToken.IsCancellationRequested) break;

                // 上一轮还没跑完就跳过本次，避免重叠执行
                if (!await _runGate.WaitAsync(0, stoppingToken)) {
                    _logger.LogWarning("【{TaskName}】上一轮尚未结束，本次跳过", TaskName);
                    continue;
                }

                try {
                    _logger.LogInformation("【{TaskName}】开始执行", TaskName);
                    await ExecuteJobAsync(stoppingToken);
                    _logger.LogInformation("【{TaskName}】执行完成", TaskName);
                }
                catch (Exception ex) {
                    // 单轮失败只记日志，绝不能让整个调度循环挂掉
                    _logger.LogError(ex, "【{TaskName}】执行失败", TaskName);
                    LogHelper.Error($"【{TaskName}】执行失败", ex, "后台");
                }
                finally {
                    _runGate.Release();
                }
            }
        }

        /// <summary>下一个触发时刻：今天剩下的点里最近的；今天都过了就取明天第一个</summary>
        private DateTime GetNextRunTime(DateTime now) {
            DateOnly today = DateOnly.FromDateTime(now);
            TimeOnly current = TimeOnly.FromDateTime(now);

            foreach (var t in _triggerTimes) {
                if (t > current) return today.ToDateTime(t);
            }
            return today.AddDays(1).ToDateTime(_triggerTimes[0]);
        }

        /// <summary>业务实现，子类重写</summary>
        protected abstract Task ExecuteJobAsync(CancellationToken stoppingToken);

        #region 后台定时任务示例
        // 定时数据清理：每天 02:00 / 08:30 / 20:00
        public class DataClearTask : DailyScheduleBackgroundService {
            public DataClearTask(ILogger<DataClearTask> logger)
                : base(logger,
                    new TimeOnly(2, 0),     // 02:00
                    new TimeOnly(8, 30),    // 08:30
                    new TimeOnly(20, 0))    // 20:00
            { }

            protected override async Task ExecuteJobAsync(CancellationToken stoppingToken) {
                LogHelper.Info("开始清理数据", "后台");
                // TODO: 清理逻辑
                await Task.CompletedTask;
            }
        }

        // 定时统计上报：每天 09:00 / 18:00
        public class StatReportTask : DailyScheduleBackgroundService {
            public StatReportTask(ILogger<StatReportTask> logger)
                : base(logger, new TimeOnly(9, 0), new TimeOnly(18, 0)) { }

            protected override async Task ExecuteJobAsync(CancellationToken stoppingToken) {
                LogHelper.Info("开始统计上报", "后台");
                await Task.CompletedTask;
            }
        }
        #endregion
    }


}
