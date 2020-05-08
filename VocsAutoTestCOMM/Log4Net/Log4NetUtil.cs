using System;
using log4net;
using log4net.Config;
using log4net.Appender;
using System.Diagnostics;
using System.IO;

namespace VocsAutoTestCOMM
{
    /// <summary>
    /// 日志管理类
    /// </summary>
    public class Log4NetUtil
    {

        /// <summary>
        /// Debug委托
        /// </summary>
        /// <param name="message">日志信息</param>
        public delegate void DDebug(object message);

        /// <summary>
        /// Info委托
        /// </summary>
        /// <param name="message">日志信息</param>
        public delegate void DInfo(object message);

        /// <summary>
        /// Warn委托
        /// </summary>
        /// <param name="message">日志信息</param>
        public delegate void DWarn(object message);

        /// <summary>
        /// Error委托
        /// </summary>
        /// <param name="message">日志信息</param>
        public delegate void DError(object message);

        /// <summary>
        /// Fatal委托
        /// </summary>
        /// <param name="message">日志信息</param>
        public delegate void DFatal(object message);

        /// <summary>
        /// Debug
        /// </summary>
        public static DDebug Debug
        {
            get 
            {
                return LogManager.GetLogger((new StackTrace()).GetFrame(1).GetMethod().DeclaringType).Debug;
            }
        }

        /// <summary>
        /// Info
        /// </summary>
        public static DInfo Info
        {
            get { return LogManager.GetLogger((new StackTrace()).GetFrame(1).GetMethod().DeclaringType).Info; }
        }

        /// <summary>
        /// Warn
        /// </summary>
        public static DWarn Warn
        {
            get { return LogManager.GetLogger((new StackTrace()).GetFrame(1).GetMethod().DeclaringType).Warn; }
        }

        /// <summary>
        /// Error
        /// </summary>
        public static DError Error
        {
            get { return LogManager.GetLogger((new StackTrace()).GetFrame(1).GetMethod().DeclaringType).Error; }
        }

        /// <summary>
        /// Fatal
        /// </summary>
        public static DFatal Fatal
        {
            get { return LogManager.GetLogger((new StackTrace()).GetFrame(1).GetMethod().DeclaringType).Fatal; }
        }

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static Log4NetUtil()
        {
            string path = string.Format("{0}Log4Net\\log4net.config", AppDomain.CurrentDomain.BaseDirectory);
            if (File.Exists(path))
            {
                log4net.Config.XmlConfigurator.Configure(new FileInfo(path));
            }
            else
            {
                RollingFileAppender appender = new RollingFileAppender();
                appender.Name = "root";
                appender.File = "Log/logs.txt";
                appender.AppendToFile = true;
                appender.RollingStyle = RollingFileAppender.RollingMode.Composite;
                appender.DatePattern = "logs_yyyyMMdd\".txt\"";
                appender.MaximumFileSize = "10MB";
                appender.MaxSizeRollBackups = 10;
                log4net.Layout.PatternLayout layout = new log4net.Layout.PatternLayout("%d{yyyy-MM-dd HH:mm:ss,fff}[%t] %-5p [%c] : %m%n");
                appender.Layout = layout;
                BasicConfigurator.Configure(appender);
                appender.ActivateOptions();
            }
        }
    }
}

