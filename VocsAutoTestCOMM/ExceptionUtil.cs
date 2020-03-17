using System;

namespace VocsAutoTestCOMM
{
    /// <summary>
    /// 委托
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="msg">异常信息</param>
    public delegate void ExceptionDelegate(string msg);
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

        /// <summary>
        /// 转发分配实现
        /// </summary>
        /// <param name="command"></param>
        public void ExceptionMethod(string msg)
        {
            Console.WriteLine(msg);
            ExceptionEvent(msg);
        }
    }
}
