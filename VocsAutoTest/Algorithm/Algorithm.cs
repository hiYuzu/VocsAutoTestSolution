using MathWorks.MATLAB.NET.Arrays;
using System;
using System.Reflection;
using VocsAutoTest.Tools;

namespace VocsAutoTest.Algorithm
{
    /// <summary>
    /// 算法调用
    /// </summary>
    public class Algorithm
    {
        MethodInfo method = null;
        Object algorithm = null;
        public Algorithm()
        {
            InitParameter();
        }

        private void InitParameter()
        {
            try
            {
                string assemblyFile = string.Empty;
                assemblyFile = ConstConfig.AppPath + @"\p1.dll";
                Assembly assembly = Assembly.LoadFrom(assemblyFile);
                Type[] types = assembly.GetTypes();
                Type type = types[0];
                method = type.GetMethod("paravector", new Type[] { typeof(MWArray), typeof(MWArray), typeof(MWArray), typeof(MWArray) });
                algorithm = Activator.CreateInstance(type);
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
                return (MWArray)method.Invoke(GetAlgorithm(), new object[] { Conc, Ri, P, T });
        }
    }
}
