using MathWorks.MATLAB.NET.Arrays;
using p1;
using System;

namespace VocsAutoTest.Algorithm
{
    public class Algorithm1
    {
        Class1 algorithm = null;
        public Algorithm1()
        {
            InitParameter();
        }

        private void InitParameter()
        {
            try
            {
                algorithm = new Class1();
            }
            catch (Exception ex)
            {
                Console.WriteLine("加载p1.dll失败,是否已经安装matlab？", ex);
            }
        }

        private Object GetAlgorithm()
        {
            while (algorithm == null)
            {
                System.Threading.Thread.Sleep(200);
            }
            return algorithm;
        }
        public MWArray Calculate(MWArray Conc, MWArray Ri, MWArray P, MWArray T)
        {
                return (MWArray)algorithm.paravector(Conc, Ri, P, T);
        }
    }
}
