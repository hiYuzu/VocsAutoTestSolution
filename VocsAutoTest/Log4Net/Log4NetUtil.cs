using System;
using log4net;
using log4net.Config;
using log4net.Appender;
using System.Diagnostics;
using System.IO;
using System.Windows.Documents;
using System.Windows.Media;

namespace VocsAutoTest.Log4Net
{
    /// <summary>
    /// 日志管理类
    /// </summary>
    public class Log4NetUtil
    {
        private static Run run;
        private static Paragraph paragraph;

        /// <summary>
        /// 日志显示
        /// </summary>
        /// <param name="color">文字颜色</param>
        /// <param name="level">日志等级</param>
        /// <param name="log">日志内容</param>
        /// <param name="main">主窗口对象</param>
        private static void LogBoxAppend(Color color, string level, string log, MainWindow main)
        {
            run = new Run()
            {
                Text = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]  [") + level + "] " + log,
                Foreground = new SolidColorBrush(color)
            };
            paragraph = new Paragraph();
            paragraph.Inlines.Add(run);
            main.LogBox.Document.Blocks.Add(paragraph);
            main.LogBox.UpdateLayout();
        }
        /// <summary>
        /// 细粒度信息
        /// </summary>
        /// <param name="log"></param>
        /// <param name="main"></param>
        public static void Debug(string log, MainWindow main)
        {
            _Debug(log);
            LogBoxAppend(Colors.Black, "DEBUG", log, main);
        }
        /// <summary>
        /// 粗粒度信息
        /// </summary>
        /// <param name="log"></param>
        /// <param name="main"></param>
        public static void Info(string log, MainWindow main)
        {
            _Info(log);
            LogBoxAppend(Colors.Black, "INFO", log, main);
        }
        /// <summary>
        /// 潜在错误信息
        /// </summary>
        /// <param name="log"></param>
        /// <param name="main"></param>
        public static void Warn(string log, MainWindow main)
        {
            _Warn(log);
            LogBoxAppend(Colors.Red, "WARN", log, main);
        }
        /// <summary>
        /// 错误
        /// </summary>
        /// <param name="log"></param>
        /// <param name="main"></param>
        public static void Error(string log, MainWindow main)
        {
            _Error(log);
            LogBoxAppend(Colors.Red, "ERROR", log, main);
        }
        /// <summary>
        /// 严重错误
        /// </summary>
        /// <param name="log"></param>
        /// <param name="main"></param>
        public static void Fatal(string log, MainWindow main)
        {
            _Fatal(log);
            LogBoxAppend(Colors.Red, "FATAL", log, main);
        }

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
        private static DDebug _Debug
        {
            get { return LogManager.GetLogger((new StackTrace()).GetFrame(1).GetMethod().DeclaringType).Debug; }
        }

        /// <summary>
        /// Info
        /// </summary>
        private static DInfo _Info
        {
            get { return LogManager.GetLogger((new StackTrace()).GetFrame(1).GetMethod().DeclaringType).Info; }
        }

        /// <summary>
        /// Warn
        /// </summary>
        private static DWarn _Warn
        {
            get { return LogManager.GetLogger((new StackTrace()).GetFrame(1).GetMethod().DeclaringType).Warn; }
        }

        /// <summary>
        /// Error
        /// </summary>
        private static DError _Error
        {
            get { return LogManager.GetLogger((new StackTrace()).GetFrame(1).GetMethod().DeclaringType).Error; }
        }

        /// <summary>
        /// Fatal
        /// </summary>
        private static DFatal _Fatal
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
                appender.File = "log.txt";
                appender.AppendToFile = true;
                appender.RollingStyle = RollingFileAppender.RollingMode.Composite;
                appender.DatePattern = "yyyyMMdd-HHmm\".txt\"";
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

