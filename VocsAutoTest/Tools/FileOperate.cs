using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VocsAutoTest.Tools
{
    public class FileOperate
    {
        /// <summary>
        /// 获得文件中的测量次数
        /// </summary>
        /// <param name="filename">文件名</param>
        /// <returns>测量次数</returns>
        public static int GetCurveNumber(string filename)
        {
            System.IO.StreamReader srtest = new
                System.IO.StreamReader(filename);
            string c = srtest.ReadLine();
            srtest.Close();
            string[] arry = c.Split('\t');
            int number = arry.Length - 1;
            if (arry[number].Length < 1)
            {
                number--;
            }
            return number;
        }

        private static float GetMaxOfData(float[] data)
        {
            float max = data[0];
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] > max)
                {
                    max = data[i];
                }
            }
            return max;
        }

        /// <summary>
        /// 获得光谱数据
        /// </summary>
        /// <param name="filename">文件名</param>
        /// <param name="pixelNumber">象素数</param>
        /// <param name="measnumber">测量次数</param>
        /// <param name="IsInteg">是否积分值</param>
        /// <returns>光谱数据</returns>
        public static float[][] GetSpecData(string filename, int pixelNumber, int measnumber, ref bool IsInteg)
        {
            float[][] spec = new float[measnumber][];
            for (int i = 0; i < measnumber; i++)
            {
                spec[i] = new float[pixelNumber];
            }
            System.IO.StreamReader sr = new System.IO.StreamReader(filename);
            int index = 0;
            while (sr.Peek() >= 0)
            {
                try
                {
                    string read = sr.ReadLine();
                    int startindex = 0;
                    int pos = read.IndexOf("\t", startindex);
                    for (int i = 0; i < measnumber; i++)
                    {
                        startindex = (pos + 1);
                        pos = read.IndexOf("\t", startindex);
                        float s = float.NaN;
                        if (pos < 0)
                        {
                            s = float.Parse(read.Substring(startindex));
                        }
                        else
                        {
                            s = float.Parse(read.Substring(startindex, pos - startindex));
                        }
                        if (index >= pixelNumber) break;
                        spec[i][index] = s;
                    }
                    index++;
                }
                catch
                {
                }
            }
            float max = GetMaxOfData(spec[0]);
            IsInteg = true;
            if (max < 6)
            {
                IsInteg = false;
            }
            return spec;
        }


        /// <summary>
        /// 保存光谱数据
        /// </summary>
        /// <param name="name">文件名</param>
        /// <param name="list">光谱数据列表</param>
        public static void SaveRawFile(string name, ArrayList list)
        {
            if (list.Count < 1) return;
            if (list[0] is float[])
            {
                string filename = string.Empty;
                try
                {
                    if (null == list || list.Count <= 0)
                    {
                        return;
                    }

                    DateTime dt = DateTime.Now;
                    string time = dt.Year + "-" + dt.Month + "-" + dt.Day + "-" + dt.Hour + "-" + dt.Minute + "-" + dt.Second;
                    if (name.IndexOf(@"\") > 0)
                    {
                        string path = name.Substring(0, name.LastIndexOf(@"\") + 1);
                        filename = path + time + "-" + name.Substring(name.LastIndexOf(@"\") + 1) + ".txt"; ;
                    }
                    else
                    {
                        filename = time + "-" + name + ".txt";
                    }
                    System.IO.StreamWriter sw = new System.IO.StreamWriter(filename);

                    if (list.Count > 0)
                    {
                        float[] f = (float[])list[0];

                        int hang = f.GetLength(0);
                        int lie = list.Count + 1;
                        float[,] arr = new float[hang, lie];

                        for (int i = 0; i < list.Count; i++)
                        {
                            float[] data = (float[])list[i];
                            for (int j = 0; j < data.GetLength(0); j++)
                            {

                                if (0 == i)
                                {
                                    arr[j, i] = j + 1;
                                }
                                arr[j, i + 1] = data[j];

                            }

                        }

                        for (int i = 0; i < hang; i++)
                        {
                            string str = "";
                            for (int j = 0; j < lie; j++)
                            {
                                str += string.Format("{0, -16}", arr[i, j]) + "\t";

                            }
                            sw.WriteLine(str);

                        }
                    }
                    sw.Close();
                    //LogUtil.ShowInfo(string.Format(CustomResource.SaveFileSuc, filename));


                }
                catch (Exception ex)
                {
                    //LogUtil.ShowInfo(string.Format(CustomResource.SaveFileErr, filename) + ex.Message);
                }
            }
            else
            {
                string filename = string.Empty;
                try
                {
                    if (null == list || list.Count <= 0)
                    {
                        return;
                    }

                    DateTime dt = DateTime.Now;
                    string time = dt.Year + "-" + dt.Month + "-" + dt.Day + "-" + dt.Hour + "-" + dt.Minute + "-" + dt.Second;
                    if (name.IndexOf(@"\") > 0)
                    {
                        string path = name.Substring(0, name.LastIndexOf(@"\") + 1);
                        filename = path + time + "-" + name.Substring(name.LastIndexOf(@"\") + 1) + ".txt"; ;
                    }
                    else
                    {
                        filename = time + "-" + name + ".txt";
                    }
                    System.IO.StreamWriter sw = new System.IO.StreamWriter(filename);

                    if (list.Count > 0)
                    {
                        double[] f = (double[])list[0];

                        int hang = f.GetLength(0);
                        int lie = list.Count + 1;
                        double[,] arr = new double[hang, lie];

                        for (int i = 0; i < list.Count; i++)
                        {
                            double[] data = (double[])list[i];
                            for (int j = 0; j < data.GetLength(0); j++)
                            {

                                if (0 == i)
                                {
                                    arr[j, i] = j + 1;
                                }
                                arr[j, i + 1] = data[j];

                            }

                        }

                        for (int i = 0; i < hang; i++)
                        {
                            string str = "";
                            for (int j = 0; j < lie; j++)
                            {
                                str += string.Format("{0, -16}", arr[i, j]) + "\t";

                            }
                            sw.WriteLine(str);

                        }
                    }
                    sw.Close();
                    //LogUtil.ShowInfo(string.Format(CustomResource.SaveFileSuc, filename));


                }
                catch (Exception ex)
                {
                    //LogUtil.ShowInfo(string.Format(CustomResource.SaveFileErr, filename) + ex.Message);
                }
            }
        }

        public static void SaveRawFile(string name, ArrayList list, ArrayList fblist, DateTime date)
        {
            if (fblist.Count < 1) return;
            if (fblist[0] is float[])
            {
                //string filename = string.Empty;
                string fbfilename = string.Empty;
                try
                {
                    if (null == fblist || fblist.Count <= 0)
                    {
                        return;
                    }

                    DateTime dt = date;
                    string time = dt.Year + "-" + dt.Month + "-" + dt.Day + "-" + dt.Hour + "-" + dt.Minute + "-" + dt.Second;
                    if (name.EndsWith(@"\"))
                    {
                        string path = name;
                        fbfilename = path + time + "-副本.txt";
                    }
                    else
                    {
                        //filename = time + "-" + name + ".txt";
                        fbfilename = name + @"\" + time + "-副本.txt";
                    }
                    #region 包含温度压力
                    System.IO.StreamWriter swr = new System.IO.StreamWriter(fbfilename);

                    if (fblist.Count > 0)
                    {
                        float[] f = (float[])fblist[0];

                        int hang = f.GetLength(0);
                        int lie = fblist.Count + 1;
                        float[,] arr = new float[hang, lie];

                        for (int i = 0; i < fblist.Count; i++)
                        {
                            float[] data = (float[])fblist[i];
                            for (int j = 0; j < data.GetLength(0); j++)
                            {

                                if (0 == i)
                                {
                                    arr[j, i] = j + 1;
                                }
                                arr[j, i + 1] = data[j];

                            }

                        }

                        for (int i = 0; i < hang; i++)
                        {
                            string str = "";
                            for (int j = 0; j < lie; j++)
                            {
                                str += string.Format("{0, -16}", arr[i, j]) + "\t";

                            }
                            swr.WriteLine(str);

                        }
                    }
                    swr.Close();
                    #endregion


                    //LogUtil.ShowInfo(string.Format(CustomResource.SaveFileSuc, fbfilename));
                }
                catch (Exception ex)
                {
                    //LogUtil.ShowInfo(string.Format(CustomResource.SaveFileErr, fbfilename) + ex.Message);
                }
            }
            else
            {
                string fbfilename = string.Empty;
                try
                {
                    if (null == fblist || fblist.Count <= 0)
                    {
                        return;
                    }

                    DateTime dt = date;
                    string time = dt.Year + "-" + dt.Month + "-" + dt.Day + "-" + dt.Hour + "-" + dt.Minute + "-" + dt.Second;
                    if (name.EndsWith(@"\"))
                    {
                        string path = name;
                        fbfilename = path + time + "-副本.txt";
                    }
                    else
                    {
                        //filename = time + "-" + name + ".txt";
                        fbfilename = name + @"\" + time + "-副本.txt";
                    }
                    System.IO.StreamWriter sw = new System.IO.StreamWriter(fbfilename);

                    if (fblist.Count > 0)
                    {
                        double[] f = (double[])fblist[0];

                        int hang = f.GetLength(0);
                        int lie = fblist.Count + 1;
                        double[,] arr = new double[hang, lie];

                        for (int i = 0; i < fblist.Count; i++)
                        {
                            double[] data = (double[])fblist[i];
                            for (int j = 0; j < data.GetLength(0); j++)
                            {

                                if (0 == i)
                                {
                                    arr[j, i] = j + 1;
                                }
                                arr[j, i + 1] = data[j];

                            }

                        }

                        for (int i = 0; i < hang; i++)
                        {
                            string str = "";
                            for (int j = 0; j < lie; j++)
                            {
                                str += string.Format("{0, -16}", arr[i, j]) + "\t";

                            }
                            sw.WriteLine(str);

                        }
                    }
                    sw.Close();
                    //LogUtil.ShowInfo(string.Format(CustomResource.SaveFileSuc, fbfilename));


                }
                catch (Exception ex)
                {
                    //LogUtil.ShowInfo(string.Format(CustomResource.SaveFileErr, fbfilename) + ex.Message);
                }
            }
        }
        /// <summary>
        /// 保存浓度数据
        /// </summary>
        /// <param name="name"></param>
        /// <param name="list"></param>
        public static void SaveConc(string name, ArrayList list)
        {
            string filename = string.Empty;
            try
            {
                lock (list)
                {
                    DateTime dt = DateTime.Now;
                    string time = dt.Year + "-" + dt.Month + "-" + dt.Day;


                    if (name.IndexOf(@"\") > 0)
                    {
                        string path = name.Substring(0, name.LastIndexOf(@"\") + 1);
                        filename = path + time + "-" + name.Substring(name.LastIndexOf(@"\") + 1) + "-Conc.txt"; ;
                    }
                    else
                    {
                        filename = time + "-" + name + "-Conc.txt";
                    }

                    //filename = time + "-" + name + "-Conc.txt";
                    System.IO.StreamWriter sw = new System.IO.StreamWriter(filename, true);

                    for (int i = 0; i < list.Count; i++)
                    {
                        if (0 == i)
                        {
                            float f = (float)list[i];
                            sw.WriteLine(f.ToString() + "\t" + dt);
                        }
                        else
                        {
                            float f = (float)list[i];
                            sw.WriteLine(f.ToString());
                        }
                    }
                    sw.Close();
                    list.Clear();
                    //LogUtil.ShowInfo(string.Format(CustomResource.SaveFileSuc, filename));
                }

            }
            catch (Exception ex)
            {
                //LogUtil.ShowInfo(string.Format(CustomResource.SaveFileErr, filename) + ex.Message);
            }
        }

        /// <summary>
        /// 保存光谱文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        /// <param name="savesource">保存积分</param>
        /// <param name="savewave">保存波长</param>
        public static void SaveSpecFile(string fileName, float[] data, bool savesource, bool savewave)
        {
            StreamWriter sw = new StreamWriter(fileName);
            for (int i = 0; i < data.Length; i++)
            {
                string temp;
                if (savewave)
                {
                    temp = string.Format("{0}", DataConvert.GetInstance().GetWaveByPixel(i));
                }
                else
                {
                    temp = string.Format("{0}", i);
                }
                if (savesource)
                {
                    temp += ("\t" + data[i].ToString("g"));
                }
                else
                {
                    temp += ("\t" + DataConvert.GetInstance().IntegToVol(data[i]).ToString("g"));
                }
                sw.WriteLine(temp);
            }
            sw.Close();
            //LogUtil.ShowInfo(string.Format(CustomResource.SaveFileSuc, fileName));
        }
        /// <summary>
        /// 保存光谱文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        /// <param name="savesource">保存积分</param>
        /// <param name="savewave">保存波长</param>
        public static void SaveSpecFile(string fileName, double[] data, bool savesource, bool savewave)
        {
            float[] floatData = new float[data.Length];
            for (int i = 0; i < floatData.Length; i++)
            {
                floatData[i] = Convert.ToSingle(data[i]);
            }

            SaveSpecFile(fileName, floatData, savesource, savewave);
        }

        public static void SaveMatrix(double[,] data, string fileName, int d, string format, string head, string tail)
        {
            TextWriter textWriter = null;
            try
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
                textWriter = File.CreateText(fileName);
                if (head != null)
                    textWriter.WriteLine(head);

                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-us");

                for (int i = 0; i < data.GetLength(0); i++)
                {
                    string line = "";
                    for (int j = 0; j < data.GetLength(1); j++)
                    {
                        line = line + (data[i, j] * d).ToString(format) + "\t";
                    }
                    textWriter.WriteLine(line);
                }
                if (tail != null)
                    textWriter.WriteLine(tail);
            }
            catch (Exception e)
            {
                //LogUtil.ShowError(e.Message);
            }
            finally
            {
                if (textWriter != null)
                    textWriter.Close();
            }
        }

        internal static void SaveConc(string name, ArrayList listConc, ArrayList listPress, ArrayList listTemp, ArrayList listTime, ArrayList listHumidity = null)
        {
            string filename = string.Empty;
            try
            {
                lock (listConc)
                {
                    DateTime dt = DateTime.Now;
                    string time = dt.Year + "-" + dt.Month + "-" + dt.Day;


                    if (name.IndexOf(@"\") > 0)
                    {
                        string path = name.Substring(0, name.LastIndexOf(@"\") + 1);
                        filename = path + time + "-" + name.Substring(name.LastIndexOf(@"\") + 1) + "-Conc.txt"; ;
                    }
                    else
                    {
                        filename = time + "-" + name + "-Conc.txt";
                    }

                    //filename = time + "-" + name + "-Conc.txt";
                    System.IO.StreamWriter sw = new System.IO.StreamWriter(filename, true);

                    for (int i = 0; i < listConc.Count; i++)
                    {
                        DateTime dtRev = (DateTime)listTime[i];
                        float f = (float)listConc[i];
                        string line = dtRev.ToString("yyyy-MM-dd hh:mm:ss") + "\t" + f.ToString();
                        if (listTemp != null && i < listTemp.Count)
                        {
                            float temp = (float)listTemp[i];
                            line += "\t" + temp.ToString();
                        }
                        if (listPress != null && i < listPress.Count)
                        {
                            float press = (float)listPress[i];
                            line += "\t" + press.ToString();
                        }
                        //取消湿度
                        //if (listHumidity != null && i < listHumidity.Count)
                        //{
                        //    float humidity = (float)listHumidity[i];
                        //    line += "\t" + humidity.ToString();
                        //}
                        sw.WriteLine(line);

                    }
                    sw.Close();
                    listConc.Clear();

                    //LogUtil.ShowInfo(string.Format(CustomResource.SaveFileSuc, filename));
                }

            }
            catch (Exception ex)
            {
                //LogUtil.ShowInfo(string.Format(CustomResource.SaveFileErr, filename) + ex.Message);
            }
        }
    }
}
