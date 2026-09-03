using NLog;

namespace WebApplication1 {
    public static class LogHelper {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        public static void Info(string message, string name = "") {
            if (name != "") {
                var log = LogManager.GetLogger(name);
                log.Info(message);
            }
            else {
                _log.Info(message);
            }
        }

        public static void Error(string message, Exception ex, string name = "") {
            if (name != "") {
                var log = LogManager.GetLogger(name);
                log.Error(ex, message);
            }
            else {
                _log.Error(ex, message);
            }
        }
    }
}
