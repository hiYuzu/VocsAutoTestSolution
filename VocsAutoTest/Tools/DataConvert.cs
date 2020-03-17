namespace VocsAutoTest.Tools
{
    public class DataConvert
    {
        private int pixelNumber = 0;
        private float[] waveLength = null; //像素对应的波长
        private double FACTOR_VOL_TO_INTEG = 2.5 / 65536.0;//电压积分转换系数

        #region 单子模式

        private DataConvert()
        {
        }
        private static object syncObj = new object();
        private static DataConvert instance = null;

        public static DataConvert GetInstance()
        {
            lock (syncObj)
            {
                if (instance == null)
                {
                    instance = new DataConvert();
                }
            }
            return instance;
        }

        #endregion

        #region public method

        /// <summary>
        /// 设置波长
        /// </summary>
        /// <param name="index">传感器类型</param>
        /// <param name="pixels">象素数</param>
        /// <param name="wavepara">第一参数</param>
        public void SetWave(int index, int pixels, float wavepara)
        {
            pixelNumber = pixels;
            if (pixelNumber == 2048)
            {
                FACTOR_VOL_TO_INTEG = 2.5 / 65536.0;
            }
            else
            {
                FACTOR_VOL_TO_INTEG = 4.096 / 65536.0;
            }
            waveLength = new float[pixels];
            for (int i = 0; i < pixels; i++)
            {
                switch (index)
                {
                    case 0://2048
                        waveLength[i] = (float)(wavepara + 0.1792 * i - 2.72E-05 * i * i + 2.25E-09 * i * i * i);
                        break;
                    case 1://1024
                        waveLength[i] = (float)(wavepara + 0.28 * i - 2.25E-5 * i * i - 2E-9 * i * i * i);

                        break;
                    case 2://长512
                        waveLength[i] = (float)(wavepara + 0.56 * i - 9E-5 * i * i + 1.6E-8 * i * i * i);

                        break;
                    case 3://短512
                        waveLength[i] = (float)(wavepara + 0.28 * i - 2.25E-5 * i * i - 2E-9 * i * i * i);
                        break;
                    case 4://256

                        break;
                }
            }
        }
        /// <summary>
        /// 获得像素数组
        /// </summary>
        /// <returns></returns>
        public float[] GetPixelData(int pixelNum)
        {
            if (pixelNum == 0) return null;
            float[] datas = new float[pixelNum];
            for (int i = 0; i < pixelNum; i++)
            {
                datas[i] = i + 1;
            }
            return datas;
        }
        /// <summary>
        /// 获得波长数组
        /// </summary>
        /// <returns></returns>
        public float[] GetWaveData()
        {
            return waveLength;
        }


        /// <summary>
        /// 电压值转积分值
        /// </summary>
        /// <param name="vol"></param>
        /// <returns></returns>
        public float VolToInteg(float vol)
        {
            return (float)(vol / FACTOR_VOL_TO_INTEG);
        }

        /// <summary>
        /// 电压值转积分值
        /// </summary>
        /// <param name="vols"></param>
        /// <returns></returns>
        public float[] VolToInteg(float[] vols)
        {
            float[] integs = new float[vols.Length];
            for (int i = 0; i < vols.Length; i++)
            {
                integs[i] = (float)(vols[i] / FACTOR_VOL_TO_INTEG);
            }
            return integs;
        }
        /// <summary>
        /// 积分值转电压值
        /// </summary>
        /// <param name="integ"></param>
        /// <returns></returns>
        public float IntegToVol(float integ)
        {
            return (float)(integ * FACTOR_VOL_TO_INTEG);
        }

        /// <summary>
        /// 积分值转电压值
        /// </summary>
        /// <param name="integs"></param>
        /// <returns></returns>
        public float[] IntegToVol(float[] integs)
        {
            float[] vols = new float[integs.Length];
            for (int i = 0; i < integs.Length; i++)
            {
                vols[i] = (float)(integs[i] * FACTOR_VOL_TO_INTEG);
            }
            return vols;
        }
        /// <summary>
        /// 得到像素点对应的波长
        /// </summary>
        /// <param name="pixel">像素</param>
        /// <returns></returns>
        public float GetWaveByPixel(int pixel)
        {
            if (waveLength == null)
            {
                //throw new Exception(CustomResource.NoWavelengh);
            }
            if (pixel >= waveLength.Length)
            {
                return float.NaN;
            }
            return waveLength[pixel];
        }


        /// <summary>
        /// 得到像素点对应的波长
        /// </summary>
        /// <param name="pixel">像素</param>
        /// <returns></returns>
        public float GetWaveByPixel(ref float pixel)
        {
            if (waveLength == null)
            {
                //throw new Exception(CustomResource.NoWavelengh);
            }
            if (pixel >= waveLength.Length)
            {
                return float.NaN;
            }

            for (int i = 0; i < waveLength.Length; i++)
            {
                float xcord1 = i;
                float xcord2 = i + 1;

                if (pixel >= xcord1 && pixel < xcord2)
                {
                    if ((xcord2 - pixel) < (pixel - xcord1))
                    {
                        pixel = xcord2;
                    }
                    else
                    {
                        pixel = xcord1;
                    }
                }
            }
            return waveLength[(int)pixel - 1];
        }

        /// <summary>
        /// 得到波长对应的像素点
        /// </summary>
        /// <param name="wave">波长</param>
        /// <returns></returns>
        public int GetPixelByWave(ref float wave)
        {
            if (waveLength == null)
            {
                //throw new Exception(CustomResource.NoWavelengh);
            }
            int index = -1;
            for (int i = 0; i < waveLength.Length; i++)
            {
                float xcord1 = 0;
                float xcord2 = 0;

                xcord1 = waveLength[i];
                if (waveLength.Length - 1 == i)
                {
                    xcord2 = waveLength[i];
                }
                else
                {
                    xcord2 = waveLength[i + 1];
                }

                if (wave >= xcord1 && wave < xcord2)
                {
                    if ((xcord2 - wave) < (wave - xcord1))
                    {
                        wave = xcord2;
                    }
                    else
                    {
                        wave = xcord1;
                    }
                    index = i;
                }
            }
            return index;
        }
        
        #endregion
    }
}
