namespace VocsAutoTestCOMM
{
    class CRC
    {
        #region
        /// <summary>
        /// CRC16校验
        /// </summary>
        /// <param name="hexByte">待校验数据</param>
        public static string CRC16(string hexByte)
        {
            byte[] data = ByteStrUtil.HexToByte(hexByte);
            int len = data.Length;
            byte[] result = new byte[] { 0, 0 };
            if (len > 0)
            {
                ushort crc = 0xFFFF;

                for (int i = 0; i < len; i++)
                {
                    crc = (ushort)(crc ^ (data[i]));
                    for (int j = 0; j < 8; j++)
                    {
                        crc = (crc & 1) != 0 ? (ushort)((crc >> 1) ^ 0xA001) : (ushort)(crc >> 1);
                    }
                }
                byte hi = (byte)((crc & 0xFF00) >> 8);  //高位置
                byte lo = (byte)(crc & 0x00FF);         //低位置

                result = new byte[] { lo, hi };
            }
            return ByteStrUtil.ByteToHex(result);
        }
        #endregion
    }
}

