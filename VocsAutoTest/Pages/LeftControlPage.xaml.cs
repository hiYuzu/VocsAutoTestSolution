using System;
using System.Windows;
using System.Windows.Controls;
using VocsAutoTestCOMM;

namespace VocsAutoTest.Pages
{
    /// <summary>
    /// LeftControlPage.xaml 的交互逻辑
    /// </summary>
    public partial class LeftControlPage : Page
    {
        float voltage;
        int avgTimes;
        int lightTimes;
        int integtationTime;
        public LeftControlPage()
        {
            InitializeComponent();
            ReadBtn_Click(null, null);
        }

        /// <summary>
        ///  读取光谱仪公共参数
        /// </summary>
        private void ReadBtn_Click(object sender, RoutedEventArgs e)
        {
            //光谱仪公共参数
            Command command1 = new Command
            {
                Cmn = "20",
                ExpandCmn = "55",
                Data = ""
            };
            //光谱仪平均次数
            Command command2 = new Command
            {
                Cmn = "21",
                ExpandCmn = "55",
                Data = "00 03"
            };
            //打灯次数
            Command command3 = new Command
            {
                Cmn = "21",
                ExpandCmn = "55",
                Data = "00 07"
            };
            //积分时间
            Command command4 = new Command
            {
                Cmn = "21",
                ExpandCmn = "55",
                Data = "00 08"
            };
            SuperSerialPort.Instance.Send(command1, true);
            SuperSerialPort.Instance.Send(command2, true);
            SuperSerialPort.Instance.Send(command3, true);
            SuperSerialPort.Instance.Send(command4, true);
            //接收TODO..
        }

        /// <summary>
        /// 设置光谱仪公共参数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetBtn_Click(object sender, RoutedEventArgs e)
        {
            if(!CheckData()) {
                MessageBox.Show("请检查测量参数！");
                return;
            }
            Command command1 = new Command
            {
                Cmn = "20",
                ExpandCmn = "66",
                Data = "00 " + VocsAutoTestCOMM.Tools.ByteToHexStr(BitConverter.GetBytes(voltage)) + " 00 " + VocsAutoTestCOMM.Tools.ByteToHexStr(BitConverter.GetBytes(100))
            };
            Console.WriteLine(command1.Data);
            Command command2 = new Command
            {
                Cmn = "21",
                ExpandCmn = "66",
                Data = "00 03 " + VocsAutoTestCOMM.Tools.ByteToHexStr(BitConverter.GetBytes(avgTimes))
            };
            Command command3 = new Command
            {
                Cmn = "21",
                ExpandCmn = "66",
                Data = "00 07 " + VocsAutoTestCOMM.Tools.ByteToHexStr(BitConverter.GetBytes(lightTimes))
            };
            Command command4 = new Command
            {
                Cmn = "21",
                ExpandCmn = "66",
                Data = "00 08 " + VocsAutoTestCOMM.Tools.ByteToHexStr(BitConverter.GetBytes(integtationTime))
            };
            SuperSerialPort.Instance.Send(command1, true);
            SuperSerialPort.Instance.Send(command2, true);
            SuperSerialPort.Instance.Send(command3, true);
            SuperSerialPort.Instance.Send(command4, true);
        }

        private bool CheckData()
        {
            try
            {
                voltage = Convert.ToSingle(ControlVol.Text);
                avgTimes = Convert.ToInt32(AvgTimes.Text);
                lightTimes = Convert.ToInt32(LightTimes.Text);
                integtationTime = Convert.ToInt32(IntegrationTime.Text);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
