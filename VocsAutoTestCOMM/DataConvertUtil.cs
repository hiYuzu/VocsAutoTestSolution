using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VocsAutoTestCOMM
{
    public class DataConvertUtil
    {
        /// <summary>
        /// 大小端转换
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static byte[] ByteReverse(byte[] buffer)
        {
            MemoryStream ms = new MemoryStream();
            for (int i = buffer.Length - 1; i > -1; i--)
            {
                ms.Write(buffer, 0, buffer.Length);
            }
            byte[] res = ms.GetBuffer();
            Array.Reverse(res, 0, buffer.Length * 2);
            return res;
        }
    }
}
