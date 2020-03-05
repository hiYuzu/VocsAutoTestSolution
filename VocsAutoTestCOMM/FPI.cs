using System;
using System.Text;

namespace VocsAutoTestCOMM
{
    class FPI
    {
        private const string head = "7D 7B";
        private const string end = "7D 7D";

        #region
        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="command">命令数据</param>
        /// <param name="isForward">是否转发</param>
        public static byte[] Encoder(Command command, bool isForward)
        {
            string data = command.Pack();
            StringBuilder encoderData = new StringBuilder();
            encoderData.Append(head);
            string address = "01 12 01 F2";
            if (isForward)
            {
                address = "02 10 12 01 F2";
            }
            data = address + data;
            data += Crc(data);
            data = Repeat(data);
            encoderData.Append(data);
            encoderData.Append(end);
            return Tools.HexToByte(encoderData.ToString());
        }
        #endregion

        #region
        /// <summary>
        /// 解码
        /// </summary>
        /// <param name="data">帧数据</param>
        public static Command Decoder(byte[] data)
        {
            string msg = Tools.ByteToKHex(data);
            Console.WriteLine("接收命令: " + msg);

            if (data != null && data.Length > 13)
            {
                if (msg.StartsWith(head) && msg.EndsWith(end))
                {
                    msg = msg.Replace("7D 82", "7D");
                    msg = msg.Replace(" ", "");
                    msg = msg.Substring(4, msg.Length - 12);
                    byte var1 = (byte)Convert.ToByte(msg.Substring(0, 2), 16);
                    byte var2 = (byte)Convert.ToByte(msg.Substring((var1 + 1) * 2, 2), 16);
                    int startIndex = (var1 + var2 + 2) * 2;
                    msg = msg.Substring(startIndex);
                    Command command;
                    if (msg.Length > 8)
                    {
                        command = new Command
                        {
                            Cmn = msg.Substring(0, 2),
                            ExpandCmn = msg.Substring(2, 2),
                            Data = msg.Substring(8)
                        };
                    }
                    else
                    {
                        command = new Command
                        {
                            Cmn = msg.Substring(0, 2),
                            ExpandCmn = msg.Substring(2, 2)
                        };
                    }
                    return command;
                }
            }
            return null;

        }
        #endregion
        private static string Crc(string data)
        {
            string crcData = CRC.CRC16(data);
            return crcData;
        }

        private static string Repeat(string data)
        {
            StringBuilder encoderData = new StringBuilder();
            data = data.ToUpper();
            for (int i = 0; i < data.Length; i += 2)
            {
                encoderData.Append(data.Substring(i, 2));
                if (data.Substring(i, 2).Equals("7D"))
                {
                    encoderData.Append("82");
                }
            }
            return encoderData.ToString();
        }
    }
}
