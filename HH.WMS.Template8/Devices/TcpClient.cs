using System.Net.Sockets;
using System.Text;

namespace WebApplication1.Devices {
    internal class TcpClient
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip">127.0.0.1</param>
        /// <param name="port">8888</param>
        /// <param name="hex">01 02 00 00 00 0C 78 0F</param>
        /// <returns></returns>
        private static string SendHexOnce(string ip, int port, string hex) {
            var res = string.Empty;
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.Connect(ip, port);
            client.ReceiveTimeout = 2000;
            if (client.Connected) {
                try {
                    client.Send(Hex2Bytes(hex));
                    byte[] buffer = new byte[1024];
                    var length = client.Receive(buffer, SocketFlags.None);
                    byte[] data = new byte[length];
                    Array.Copy(buffer, data, length);
                    res = BitConverter.ToString(data).Replace("-", "");
                }
                catch (Exception ex) {
                    LogHelper.Error(ex.Message, ex);
                }
                client.Disconnect(true);
                client.Dispose();
            }
            client = null;
            return res;
        }

        /// <summary>
        /// 读保持寄存器，modbus rtu的封装
        /// </summary>
        /// <param name="address"></param>
        /// <param name="qty"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        internal int[] ReadHoldingRegistersRtu(int address, int qty, string ip, int port = 502) {
            int[] res = new int[qty];
            var hex = $"0103{address.ToString("X4")}{qty.ToString("X4")}";
            hex = hex + BitConverter.ToString(CRC16LH(Hex2Bytes(hex))).Replace("-", "").Replace(" ", "");
            var data = SendHexOnce(ip, port, hex);
            if (!string.IsNullOrEmpty(data)) {
                if (CheckCRC(data)) {
                    byte[] buffer = Hex2Bytes(data);
                    int regNum = buffer[2] / 2;
                    if (regNum == qty) {
                        for (int i = 0; i < regNum; i++) {
                            // buffer[3]是第一个寄存器高字节，buffer[4]低字节
                            ushort val = (ushort)(buffer[3 + i * 2] << 8 | buffer[4 + i * 2]);
                            res[i] = val;
                        }
                    }
                   
                }
            }
            return res.ToArray();
        }
        /// <summary>
        /// 写单个寄存器
        /// </summary>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        internal bool WriteSingleRegisterRtu(int address, int value, string ip, int port = 502) {
            var res = false;
            var hex = $"0106{address.ToString("X4")}{value.ToString("X4")}";
            hex = hex + BitConverter.ToString(CRC16LH(Hex2Bytes(hex))).Replace("-", "").Replace(" ", "");
            var data = SendHexOnce(ip, port, hex);
            if (!string.IsNullOrEmpty(data)) {
                res = true;
            }
            return res;
        }
        internal static byte[] Hex2Bytes(string hexString) {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);

            return returnBytes;
        }
        internal static string Hex2Ascii(string hexString) {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);

            return Encoding.ASCII.GetString(returnBytes);
        }

        #region 进制转换+CRC
        internal static bool CheckCRC(string hex) {
            var result = false;
            var data = hex.Replace(" ", "");
            if (data.Length % 2 == 0) {
                var code1 = data.Substring(data.Length - 4, 4).ToLower();
                var code2 = BitConverter.ToString(CRC16LH(Hex2Bytes(data.Substring(0, data.Length - 4)))).Replace("-", "").Replace(" ", "").ToLower();
                result = code1 == code2;
            }
            return result;
        }
        internal static byte[] CRC16LH(byte[] pDataBytes) {
            ushort crc = 0xffff;
            ushort polynom = 0xA001;

            for (int i = 0; i < pDataBytes.Length; i++) {
                crc ^= pDataBytes[i];
                for (int j = 0; j < 8; j++) {
                    if ((crc & 0x01) == 0x01) {
                        crc >>= 1;
                        crc ^= polynom;
                    }
                    else {
                        crc >>= 1;
                    }
                }
            }

            byte[] result = BitConverter.GetBytes(crc);
            return result;
        }
        internal static byte[] CRC16HL(byte[] pDataBytes) {
            ushort crc = 0xffff;
            ushort polynom = 0xA001;

            for (int i = 0; i < pDataBytes.Length; i++) {
                crc ^= pDataBytes[i];
                for (int j = 0; j < 8; j++) {
                    if ((crc & 0x01) == 0x01) {
                        crc >>= 1;
                        crc ^= polynom;
                    }
                    else {
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
