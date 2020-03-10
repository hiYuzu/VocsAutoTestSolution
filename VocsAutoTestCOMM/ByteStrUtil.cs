using System;
using System.Text;

namespace VocsAutoTestCOMM
{
    public class ByteStrUtil
    {
        #region
        /// <summary>
        /// 16进制字符串转16进制数组
        /// </summary>
        /// <param name="msg">16进制字符串</param>
        public static byte[] HexToByte(string msg)
        {
            msg = msg.Replace(" ", "");
            byte[] comBuffer = new byte[msg.Length / 2];
            for (int i = 0; i < msg.Length; i += 2)
                comBuffer[i / 2] = (byte)Convert.ToByte(msg.Substring(i, 2), 16);
            return comBuffer;
        }
        #endregion

        #region
        /// <summary>
        /// 转16进制字符串（不含空格）
        /// </summary>
        /// <param name="comByte">16进制数组</param>
        public static string ByteToHex(byte[] comByte)
        {
            StringBuilder builder = new StringBuilder(comByte.Length * 3);
            foreach (byte data in comByte)
            {
                builder.Append(Convert.ToString(data, 16).PadLeft(2, '0'));
            }
            return builder.ToString().ToUpper();
        }
        #endregion

        #region
        /// <summary>
        /// 转16进制字符串（含空格）
        /// </summary>
        /// <param name="comByte">16进制数组</param>
        public static string ByteToKHex(byte[] comByte)
        {
            StringBuilder builder = new StringBuilder(comByte.Length * 3);
            for (int i = 0; i < comByte.Length; i++)
            {
                builder.Append(Convert.ToString(comByte[i], 16).PadLeft(2, '0'));
                if (i < comByte.Length - 1)
                {
                    builder.Append(" ");
                }
            }
            return builder.ToString().ToUpper();
        }
        #endregion
        /// <summary>
        /// 直接转化显示，效果同方法ByteToKHex，区别是返回字符串第一位为空格
        /// byte[] ==> string
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ByteToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += " " + bytes[i].ToString("x2");
                }
            }
            return returnStr;
        }
    }
}
