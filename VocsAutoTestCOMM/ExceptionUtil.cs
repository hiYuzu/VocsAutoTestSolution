using System;

namespace VocsAutoTestCOMM
{
    /// <summary>
    /// 委托
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="msg">异常信息</param>
    public delegate void ExceptionDelegate(object sender, string msg);
    /// <summary>
    /// 异常通知类
    /// </summary>
    public class ExceptionUtil
{
        private static volatile ExceptionUtil instance;
        private static readonly object _obj = new object();
        public static ExceptionUtil Instance
        {
            get
            {
                if(instance == null)
                {
                    lock (_obj)
                    {
                        instance = new ExceptionUtil();
                    }
                }
                return instance;
            }
        }

        public event ExceptionDelegate ExceptionEvent;
        public event ExceptionDelegate LogEvent;

        /// <summary>
        /// 异常信息（不显示在主界面）
        /// </summary>
        /// <param name="command"></param>
        public void ExceptionMethod(string msg)
        {
            Console.WriteLine(msg);
            ExceptionEvent(this, msg);
        }
        /// <summary>
        /// 日志信息（显示在主界面）
        /// </summary>
        /// <param name="msg"></param>
        public void LogMethod(string msg)
        {
            Console.WriteLine(msg);
            LogEvent(this, msg);
        }
    }
}
