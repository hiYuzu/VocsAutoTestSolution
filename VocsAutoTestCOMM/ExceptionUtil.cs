using System;

namespace VocsAutoTestCOMM
{
    /// <summary>
    /// 委托
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="msg">异常信息</param>
    public delegate void ExceptionDelegate(string msg, bool isShow);
    /// <summary>
    /// 异常通知类
    /// </summary>
    public class ExceptionUtil
    {
        public event ExceptionDelegate ExceptionEvent;
        public event ExceptionDelegate LogEvent;
        public Action<bool> ShowLoadingAction;
        private static volatile ExceptionUtil instance = null;
        private static readonly object obj = new object();

        public static ExceptionUtil Instance
        {
            get
            {
                if(instance == null)
                {
                    lock (obj)
                    {
                        instance = new ExceptionUtil();
                    }
                }
                return instance;
            }
        }
        /// <summary>
        /// 异常信息
        /// </summary>
        /// <param name="msg">信息</param>
        /// <param name="isShow">是否显示在主界面</param>
        public void ExceptionMethod(string msg, bool isShow)
        {
            ExceptionEvent?.Invoke(msg, isShow);
        }
        /// <summary>
        /// 日志信息（显示在主界面）
        /// </summary>
        /// <param name="msg">信息</param>
        public void LogMethod(string msg)
        {
            LogEvent?.Invoke(msg, true);
        }
    }
}
