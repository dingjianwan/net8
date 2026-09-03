
namespace WebApplication1 {
    public class CPProcess {
        internal static void Process(string ip,string msg) {
            //var db = new SqlHelper<object>().GetInstance();
            //var t = db.Queryable<Location>().First();
            //LogHelper.Info("CPProcess","成品");
            TcpHelper.TcpServerSend(ip, System.Text.Encoding.UTF8.GetBytes("hello world"));
        }
    }
}
