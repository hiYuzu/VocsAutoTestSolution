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
            //接收
            //todo..
        }

        /// <summary>
        /// 设置光谱仪公共参数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetBtn_Click(object sender, RoutedEventArgs e)
        {
            byte[] voltage = BitConverter.GetBytes(Convert.ToSingle(this.ControlVol.Text));
            Command command1 = new Command
            {
                Cmn = "20",
                ExpandCmn = "66",
                Data = "00 " + voltage + " 00 " + BitConverter.GetBytes(100)
            };
            Command command2 = new Command
            {
                Cmn = "21",
                ExpandCmn = "66",
                Data = "00 03 " + BitConverter.GetBytes(Convert.ToSingle(this.AvgTimes.Text))
            };
            Command command3 = new Command
            {
                Cmn = "21",
                ExpandCmn = "66",
                Data = "00 07 " + BitConverter.GetBytes(Convert.ToSingle(this.LightTimes.Text))
            };
            Command command4 = new Command
            {
                Cmn = "21",
                ExpandCmn = "66",
                Data = "00 08 " + BitConverter.GetBytes(Convert.ToSingle(this.IntegrationTime.Text))
            };
            SuperSerialPort.Instance.Send(command1, true);
            SuperSerialPort.Instance.Send(command2, true);
            SuperSerialPort.Instance.Send(command3, true);
            SuperSerialPort.Instance.Send(command4, true);
        }
    }
}
