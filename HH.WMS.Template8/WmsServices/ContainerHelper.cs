using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication1 {
    internal class ContainerHelper {
        internal static string GenerateCntrNo() {
            var date = DateTime.Now.ToString("yyMMdd");
            var id = SYSHelper.GetSerialNumber(date, "TP");
            return $"TP{date}{id.ToString().PadLeft(4, '0')}";
        }
    }
}
