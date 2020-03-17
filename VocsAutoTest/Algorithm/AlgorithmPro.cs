using MathWorks.MATLAB.NET.Arrays;
using System;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace VocsAutoTest.Algorithm
{
    class AlgorithmPro
    {
        private Algorithm algorithm = null;
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
        public void Process(out double[,] V, float[,] Conc, float[,] Ri, float P, float T)
        {
            MWArray concMWArray = GetConc(Conc);
            MWArray riMWArray = GetRi(Ri);
            MWArray mwarray = algorithm.Calculate(concMWArray, riMWArray, P, T);
            V = Convert2Array2((MWNumericArray)mwarray);
        }

        //保存测量数据文件
        public string SaveParameter(double[,] V, string machId, string instId, ArrayList selectedGases, string path)
        {
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
                        double[,] gasV = new double[V.GetLength(0), 1];
                        for (int k = 0; k < V.GetLength(0); k++)
                            gasV[k, 0] = V[k, n - 1];
                        FileControl.SaveMatrix(gasV, path + node.name + "_" + machId + "_" + instId + ".txt", 1, "g", null, tail[n - 1]);
                    }
                }
            }
            //FileControl.SaveMatrix(E, path + "拟合误差" + ".txt", 1, "F", head, null);
            return path;
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
