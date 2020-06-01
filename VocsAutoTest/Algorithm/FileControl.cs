using System;
using System.Collections;
using System.IO;
using System.Threading;
using VocsAutoTestCOMM;

namespace VocsAutoTest.Algorithm
{
    class FileControl
    {
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
                        if (j==data.GetLength(1)-1)
                        {
                            line = line + (data[i, j] * d).ToString(format);
                        }
                        else
                        {
                            line = line + (data[i, j] * d).ToString(format) + "\t";
                        }
                    }
                    textWriter.WriteLine(line);
                }
                if (tail != null)
                    textWriter.WriteLine(tail);
            }
            catch (Exception)
            {
            }
            finally
            {
                if (textWriter != null)
                    textWriter.Close();
            }
        }
        /// <summary>
        /// 保存光谱数据
        /// </summary>
        /// <param name="name">文件名</param>
        /// <param name="list">光谱数据列表</param>
        public static void SaveRawFile(string name, ArrayList list)
        {
            if (list.Count < 1) 
                return;
            string filename;
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
                StreamWriter sw = new StreamWriter(filename);
                if (list.Count > 0)
                {
                    string[] f = (string[])list[0];
                    int hang = f.GetLength(0);
                    int lie = list.Count + 1;
                    string[,] arr = new string[hang, lie];
                    for (int i = 0; i < list.Count; i++)
                    {
                        string[] data = (string[])list[i];
                        for (int j = 0; j < data.GetLength(0); j++)
                        {
                            if (0 == i)
                            {
                                arr[j, i] = (j + 1).ToString();
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
                ExceptionUtil.Instance.LogMethod("光谱文件已保存：" + name);
            }
            catch (Exception ex)
            {
                ExceptionUtil.Instance.ExceptionMethod("保存失败:" + ex.Message, true);
            }
        }
        public static void SaveRawFile(string name, ArrayList fblist, DateTime date)
        {
            if (fblist.Count < 1) 
                return;
            string fbfilename;
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
                    fbfilename = name + @"\" + time + "-副本.txt";
                }
                StreamWriter swr = new StreamWriter(fbfilename);
                if (fblist.Count > 0)
                {
                    string[] f = (string[])fblist[0];
                    int hang = f.GetLength(0);
                    int lie = fblist.Count + 1;
                    string[,] arr = new string[hang, lie];

                    for (int i = 0; i < fblist.Count; i++)
                    {
                        string[] data = (string[])fblist[i];
                        for (int j = 0; j < data.GetLength(0); j++)
                        {
                            if (0 == i)
                            {
                                arr[j, i] = (j + 1).ToString();
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
                ExceptionUtil.Instance.LogMethod("光谱文件已保存：" + name + @"\");
            }
            catch (Exception ex)
            {
                ExceptionUtil.Instance.ExceptionMethod("保存失败:" + ex.Message, true);
            }
        }
    }
}
