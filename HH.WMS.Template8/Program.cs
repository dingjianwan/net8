using NLog;
using NLog.Web;
using System.Net;
using WebApplication1;
using static HH.WMS.Template8.Background.DailyScheduleBackgroundService;
using static WebApplication1.Background.BaseBackgroundService;

// 最顶部初始化NLog
//var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
//logger.Info("====静态NLog测试，这条不经过AspNet日志管线====");
try {
    var builder = WebApplication.CreateBuilder(args);
    // 接入 NLog：清掉默认控制台等 Provider，统一由 nlog.config 的 Target 输出
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();
    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    //初始化配置文件
    ConfigHelper.Init();
    // 批量注册所有后台任务，框架自动并行启动
    builder.Services.AddHostedService<MessageConsumeTask>();
    builder.Services.AddHostedService<DataClearTask>();
    builder.Services.AddHostedService<StatReportTask>();
    builder.Services.AddHostedService<ModbusTask>();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    //if (app.Environment.IsDevelopment()) {
        app.UseSwagger();
        app.UseSwaggerUI();
    //}

    app.UseAuthorization();

    app.MapControllers();
  
    #region 启动TCP监听 放在app.Run之前
    // 单独开异步后台任务运行TCP服务，不阻塞HTTP
    _ = TcpHelper.RunTcpServerAsync(IPAddress.Any, 8899, app.Services, app.Lifetime.ApplicationStopping);
    #endregion
    app.Run();
}
catch (Exception ex) {
    LogHelper.Error("程序启动崩溃", ex);
}
finally {
    NLog.LogManager.Shutdown();
}