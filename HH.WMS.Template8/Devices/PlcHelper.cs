using System.Text;

namespace WebApplication1.Devices {
    internal class PlcHelper
    {
        internal static void Receive(string ip, string msg)
        {
            //处理设备信号
            ////LogHelper.Info($"处理PLC信息：IP：{ip},MSG:{msg}");
            LogHelper.Info($"处理PLC信息：IP：{ip},MSG:{msg}", "Plc");
            DeviceProcess.Analysis(msg, ip);
        }
        internal static bool SendHex(string ip, string msg)
        {
            ////LogHelper.Info($"发送PLC信息：IP：{ip},MSG:{msg}");       
            var res = TcpHelper.TcpServerSend(ip, Hex2Bytes(msg));
            LogHelper.Info($"发送PLC信息：IP：{ip},MSG:{msg} res={res}", "Plc");
            return res;
        }
        internal static void SendAscii(string ip, string msg)
        {
            TcpHelper.TcpServerSend(ip, Encoding.ASCII.GetBytes(msg));
        }

        internal static byte[] Hex2Bytes(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }
        internal static string Hex2Ascii(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);

            return Encoding.ASCII.GetString(returnBytes);
        }

        #region 进制转换+CRC
        internal static byte[] CRC16LH(byte[] pDataBytes)
        {
            ushort crc = 0xffff;
            ushort polynom = 0xA001;

            for (int i = 0; i < pDataBytes.Length; i++)
            {
                crc ^= pDataBytes[i];
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 0x01) == 0x01)
                    {
                        crc >>= 1;
                        crc ^= polynom;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }

            byte[] result = BitConverter.GetBytes(crc);
            return result;
        }
        internal static byte[] CRC16HL(byte[] pDataBytes)
        {
            ushort crc = 0xffff;
            ushort polynom = 0xA001;

            for (int i = 0; i < pDataBytes.Length; i++)
            {
                crc ^= pDataBytes[i];
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 0x01) == 0x01)
                    {
                        crc >>= 1;
                        crc ^= polynom;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }

            byte[] result = BitConverter.GetBytes(crc).Reverse().ToArray();
            return result;
        }

        #endregion
    }
}
