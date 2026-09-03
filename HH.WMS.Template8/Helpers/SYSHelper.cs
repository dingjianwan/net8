using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication1 {
    internal class SYSHelper
    {
        private static object locker = new object();
        internal static int GetSerialNumber(string snType, string prefix)
        {
            int result = 0;
            lock (locker)
            {
                var db = new SqlHelper<object>().GetInstance();
                var sId = db.Queryable<OI_SYS_MAXID>().Where(a => a.CN_S_TYPE.Trim() == snType && a.CN_S_PRE.Trim() == prefix).First();
                if (sId != null)
                {
                    sId.CN_N_MAX++;
                    if (db.Ado.ExecuteCommand($"update OI_SYS_MAXID set CN_N_MAX={sId.CN_N_MAX} where CN_S_TYPE= '{snType}' and CN_S_PRE='{prefix}' ") > 0)
                    {
                        result = sId.CN_N_MAX;
                    }
                }
                else
                {
                    //插入表
                    sId = new OI_SYS_MAXID { CN_S_TYPE = snType, CN_S_PRE = prefix, CN_N_MAX = 1 };
                    result = db.Insertable<OI_SYS_MAXID>(sId).ExecuteCommand() > 0 ? 1 : 0;
                }
            }
            return result;
        }
        /// <summary>
        /// 生成特定位数的唯一字符串
        /// </summary>
        /// <param name="num">特定位数</param>
        /// <returns></returns>
        public static string GenerateUniqueText(int num)
        {
            string randomResult = string.Empty;
            string readyStr = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            char[] rtn = new char[num];
            Guid gid = Guid.NewGuid();
            var ba = gid.ToByteArray();
            for (var i = 0; i < num; i++)
            {
                rtn[i] = readyStr[((ba[i] + ba[num + i]) % 35)];
            }
            foreach (char r in rtn)
            {
                randomResult += r;
            }
            return randomResult;
        }
        [SugarTable("dbo.OI_SYS_MAXID")]
        public class OI_SYS_MAXID
        {
            public string CN_S_TYPE { get; set; }
            public string CN_S_PRE { get; set; }
            public int CN_N_MAX { get; set; }
        }
    }
}
