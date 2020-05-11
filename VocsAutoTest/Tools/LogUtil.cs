using System;
using System.Windows.Documents;
using System.Windows.Media;
using VocsAutoTestCOMM;

namespace VocsAutoTest
{
    /// <summary>
    /// 日志管理类
    /// </summary>
    public class LogUtil
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
            main.LogBox.Focus();
            main.LogBox.UpdateLayout();
            main.LogBox.CaretPosition = main.LogBox.Document.ContentEnd;//设置光标的位置到文本尾
        }
        /// <summary>
        /// 细粒度信息
        /// </summary>
        /// <param name="log"></param>
        /// <param name="main"></param>
        public static void Debug(string log, MainWindow main)
        {
            Log4NetUtil.Debug(log);
            LogBoxAppend(Colors.Black, "信息", log, main);
        }
        /// <summary>
        /// 粗粒度信息
        /// </summary>
        /// <param name="log"></param>
        /// <param name="main"></param>
        public static void Info(string log, MainWindow main)
        {
            Log4NetUtil.Info(log);
            LogBoxAppend(Colors.Black, "INFO", log, main);
        }
        /// <summary>
        /// 潜在错误信息
        /// </summary>
        /// <param name="log"></param>
        /// <param name="main"></param>
        public static void Warn(string log, MainWindow main)
        {
            Log4NetUtil.Warn(log);
            LogBoxAppend(Colors.Red, "WARN", log, main);
        }
        /// <summary>
        /// 错误
        /// </summary>
        /// <param name="log"></param>
        /// <param name="main"></param>
        public static void Error(string log, MainWindow main)
        {
            Log4NetUtil.Error(log);
            LogBoxAppend(Colors.Red, "错误", log, main);
        }
        /// <summary>
        /// 严重错误
        /// </summary>
        /// <param name="log"></param>
        /// <param name="main"></param>
        public static void Fatal(string log, MainWindow main)
        {
            Log4NetUtil.Fatal(log);
            LogBoxAppend(Colors.Red, "FATAL", log, main);
        }
    }
}

