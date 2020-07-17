using MathWorks.MATLAB.NET.Arrays;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace VocsAutoTest.Algorithm
{
    class AlgorithmPro
    {
        private Algorithm algorithm = null;
        private const int GAS_NUMBER = 4;//最多选择气体种类
        private const string HEAD_GAS = "GAS";
        private const string HEAD_FLOW = "FLOW";
        private const string HEAD_SPEC = "SPEC";
        //光谱像元数
        public int PixelSize = 512;

        private AlgorithmPro()
        {
            Thread tempThread = new Thread(new ThreadStart(InitConcent));
            tempThread.Priority = ThreadPriority.Normal;
            tempThread.Start();
        }

        private void InitConcent()
        {
            algorithm = new Algorithm();
        }

        private static AlgorithmPro instance = null;
        [MethodImpl(MethodImplOptions.Synchronized)]

        public static AlgorithmPro GetInstance()
        {
            if (instance == null)
            {
                instance = new AlgorithmPro();
            }
            return instance;
        }
        //算法执行
        public void Process(out double[,] V, out double[,] E, float[,] Conc, float[,] Ri, float P, float T)
        {
            MWArray concMWArray = GetConc(Conc);
            MWArray riMWArray = GetRi(Ri);
            MWArray[] mwarray = algorithm.Calculate(2,concMWArray, riMWArray, P, T);
            V = Convert2Array2((MWNumericArray)mwarray[0]);
            E = Convert2Array2((MWNumericArray)mwarray[1]);
        }

        //保存测量数据文件
        public string SaveParameter(double[,] V, double[,] E, string machId, string instId, ArrayList selectedGases, Dictionary<string, List<string>> map)
        {
            string path = System.Windows.Forms.Application.StartupPath + "\\ParameterGen\\";
            if (!string.IsNullOrEmpty(machId) && !string.IsNullOrEmpty(instId))
                path = path + machId + "@" + instId + "\\";

            path = path + DateTime.Now.ToString("s").Replace(':', '：').Replace('/', '-') + "\\";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string head = GetMatrixHead(selectedGases);
            string[] tail = new string[4];

            tail[0] = "a1 * x1 + a2 * x2 + a3 * x1 * x1 + a4 * x1 * x2 + a5 * x2 * x2";
            tail[1] = "a1 * x1 + a2 * x2 + a3 * x3 + a4 * x1 * x1 + a5 * x1 * x2 + a6 * x1 * x3 + a7 * x2 * x2 + a8 * x2 * x3 + a9 * x3 * x3";

            //FileControl.SaveMatrix(Cs, path + "吸收截面" + ".txt", 1, "e", head, null);

            //保存每种气体,0表示N2，不用保存
            for (int n = 1; n < selectedGases.Count; n++)
            {
                for (int j = 0; j < selectedGases.Count; j++)
                {
                    GasNode node = (GasNode)selectedGases[j];
                    if (node.index == n)
                    {
                        string thingName = node.name;
                        List<string> lists = map[thingName];
                        int start = V.GetLength(0);
                        int end = 0;
                        int py=0;
                        for (int k = 0; k < V.GetLength(0); k++)
                        {
                            if (V[k, n - 1] != 0)
                            {
                                start = k;
                                break;
                            }
                        }
                        if(start< V.GetLength(0))
                        {
                            for (int k = V.GetLength(0) - 6; k >= 0; k--)
                            {
                                if (V[k, n - 1] !=0)
                                {
                                    end = k;
                                    break;
                                }
                            }
                            py = end - start + 1;
                        }
                        int count = py + 1;
                        lists.Add(count.ToString());
                        lists.Add("5");

                        int length = V.GetLength(0) + 13;
                        double[,] gasV = new double[length, 1];
                        string fist = lists[0];
                        for(int i = 1; i < lists.Count; i++)
                        {
                            gasV[i-1,0]= Convert.ToDouble(lists[i]); 
                        }
                        int listCount = lists.Count;
                        for (int k = 0; k < V.GetLength(0); k++)
                            gasV[listCount+k-1, 0] = V[k, n - 1];
                        FileControl.SaveMatrix(gasV, path + node.name + "_" + machId + "_" + instId + ".txt", 1, "g", fist, null);
                    }
                }
            }
            FileControl.SaveMatrix(E, path + "拟合误差.txt", 1, "F", head, null);
            return path;
        }

        private string specNo = string.Empty;
        private double[] specArry = new double[512];
        //保存光谱数据
        public void SaveSpecData(string path, string[] gasName, string[] gasValue, ObservableCollection<string[]> _obervableCollection, Dictionary<int, float[]> riDataMap)
        {
            string fileName = path + "光谱.txt";
            TextWriter textWriter = null;
            try
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
                textWriter = File.CreateText(fileName);
                string line;

                //保存气体和标气浓度
                textWriter.WriteLine(HEAD_GAS);
                for (int i = 0; i < gasName.Length; i++)
                {
                    line = gasName[i] + "\t" + gasValue[i];
                    textWriter.WriteLine(line);
                }
                textWriter.WriteLine();

                //保存测量数据和浓度误差
                textWriter.WriteLine(HEAD_FLOW);
                for (int i = 0; i < _obervableCollection.Count; i++)
                {
                    line = "";
                    for (int j = 0; j < _obervableCollection[i].Length; j++)
                    {
                        line = line + _obervableCollection[i][j] + "\t"; ;
                    }
                    textWriter.WriteLine(line);
                }
                textWriter.WriteLine();

                //保存光谱数据
                textWriter.WriteLine(HEAD_SPEC);
                //第一行，写编号
                line = "";
                for (int i = 0; i < _obervableCollection.Count; i++)
                {
                    line = line + _obervableCollection[i][0] + "\t";
                }
                textWriter.WriteLine(line);

                //写光谱数据,每列对应1次测量,每行对应1个象素
                for (int i = 0; i < 512; i++)
                {
                    line = "";
                    for (int j = 1; j <= riDataMap.Count; j++)
                    {
                        riDataMap.TryGetValue(j, out float[] values);
                        line = line + values[i] + "\t";
                    }
                    textWriter.WriteLine(line);
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.ToString());
            }
            finally
            {
                if (textWriter != null)
                    textWriter.Close();
            }

        }

        private string GetMatrixHead(ArrayList selectedGases)
        {
            string head = "";
            for (int n = 1; n < selectedGases.Count; n++)
            {
                for (int j = 0; j < selectedGases.Count; j++)
                {
                    GasNode node = (GasNode)selectedGases[j];
                    if (node.index == n)
                        head = head + node.name + "\t";
                }
            }
            return head;
        }
        private MWArray GetConc(float[,] data)
        {
            double[,] doubleData = new double[data.GetLength(0), data.GetLength(1)];
            for (int i = 0; i < data.GetLength(0); i++)
                for (int j = 0; j < data.GetLength(1); j++)
                    doubleData[i, j] = data[i, j];
            MWNumericArray Conc = doubleData;
            return Conc;
        }

        private MWArray GetRi(float[,] data)
        {
            double[,] doubleData = new double[data.GetLength(0), data.GetLength(1)];
            for (int i = 0; i < data.GetLength(0); i++)
                for (int j = 0; j < data.GetLength(1); j++)
                    doubleData[i, j] = data[i, j];
            MWNumericArray Ri = doubleData;
            return Ri;
        }

        private double[] Convert2Array1(MWNumericArray mwarray)
        {
            Array array = mwarray.ToArray(MWArrayComponent.Real);
            double[] returnData = new double[array.Length];
            long[] index = { 0L };
            for (int i = 0; i < returnData.Length; i++)
            {
                returnData[i] = (double)array.GetValue(index);
            }
            return returnData;
        }

        private double[,] Convert2Array2(MWNumericArray mwarray)
        {
            Array array = mwarray.ToArray(MWArrayComponent.Real);
            double[,] returnData = new double[array.GetLength(0), array.GetLength(1)];
            long[] index = { 0L, 0L };
            for (int i = 0; i < returnData.GetLength(0); i++)
            {
                index[0] = i;
                for (int j = 0; j < returnData.GetLength(1); j++)
                {
                    index[1] = j;
                    returnData[i, j] = (double)array.GetValue(index);
                }
            }
            return returnData;
        }

        private float[,] Trans(float[,] data)
        {
            float[,] tData = new float[data.GetLength(1), data.GetLength(0)];
            for (int i = 0; i < tData.GetLength(0); i++)
            {
                for (int j = 0; j < tData.GetLength(1); j++)
                {
                    tData[i, j] = data[j, i];
                }
            }
            return tData;

        }
        private void Print(double[,] arr)
        {
            for (int i = 0; i < arr.GetLength(0); i++)
            {
                for (int j = 0; j < arr.GetLength(1); j++)
                {
                    Console.Write("{0}  ", arr[i, j]);
                }
                Console.Write("\n");
            }
        }
    }
}
