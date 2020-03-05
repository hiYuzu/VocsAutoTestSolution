using System;

namespace VocsAutoTestCOMM
{
    public class Command
    {
        public string Cmn { set; get; }
        public string ExpandCmn { set; get; }
        public string Data { set; get; }

        #region
        /// <summary>
        /// 数据封装
        /// </summary>
        public string Pack()
        {
            if (Data == null || Data.Equals(""))
            {
                return Cmn + ExpandCmn + "0000";
            }
            else
            {
                Data = Data.Replace(" ", "");
                short length = (short)(Data.Length / 2);
                byte[] len = BitConverter.GetBytes(length);
                return Cmn + ExpandCmn + Convert.ToString(len[1], 16).PadLeft(2, '0') + Convert.ToString(len[0], 16).PadLeft(2, '0') + Data;
            }
        }
        #endregion
    }
}
