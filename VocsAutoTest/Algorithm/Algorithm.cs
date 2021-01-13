using System;
using System.Reflection;
using VocsAutoTest.Tools;
using MathWorks.MATLAB.NET.Arrays;
using VocsAutoTestCOMM;

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
                assemblyFile = ConstConfig.AppPath + @"\paravector.dll";
                Assembly assembly = Assembly.LoadFrom(assemblyFile);
                Type[] types = assembly.GetTypes();
                Type type = types[0];
                method = type.GetMethod("paravector", new Type[] { typeof(int), typeof(MWArray), typeof(MWArray), typeof(MWArray), typeof(MWArray) });
                algorithm = Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {
                Console.WriteLine("加载paravector.dll失败,是否已经安装matlab？", ex);
                ExceptionUtil.Instance.ExceptionMethod("加载paravector.dll失败,是否已经安装matlab？", true);
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
        public MWArray[] Calculate(int numArgsOut, MWArray Conc, MWArray Ri, MWArray P, MWArray T)
        {
            return (MWArray[])method.Invoke(GetAlgorithm(), new object[] { numArgsOut, Conc, Ri, P, T });
        }
    }
}
