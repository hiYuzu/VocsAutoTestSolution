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
        public static event ExceptionDelegate ExceptionEvent;
        public static event ExceptionDelegate LogEvent;
        public static Action<bool> ShowLoadingAction;

        /// <summary>
        /// 异常信息
        /// </summary>
        /// <param name="msg">信息</param>
        /// <param name="isShow">是否显示在主界面</param>
        public static void ExceptionMethod(string msg, bool isShow)
        {
            ExceptionEvent(msg, isShow);
        }
        /// <summary>
        /// 日志信息（显示在主界面）
        /// </summary>
        /// <param name="msg">信息</param>
        public static void LogMethod(string msg)
        {
            LogEvent(msg, true);
        }
    }
}
