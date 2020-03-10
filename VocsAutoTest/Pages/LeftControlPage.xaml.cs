using System;
using System.Collections.Generic;
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
        byte avgTimes;
        byte lightTimes;
        ushort integtationTime;
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
            List<Command> commands = new List<Command>()
            {
                //光谱仪公共参数
                new Command
                {
                    Cmn = "20",
                    ExpandCmn = "55",
                    Data = ""
                },
                //光谱仪平均次数
                new Command
                {
                    Cmn = "21",
                    ExpandCmn = "55",
                    Data = "00 03"
                },
                //打灯次数
                new Command
                {
                    Cmn = "21",
                    ExpandCmn = "55",
                    Data = "00 07"
                },
                //积分时间
                new Command
                {
                    Cmn = "21",
                    ExpandCmn = "55",
                    Data = "00 08"
                }
            };
            SuperSerialPort.Instance.SendAll(commands, true);
            DataForward.Instance.ReadCommParam += new DataForwardDelegate(SetCommParam);
            DataForward.Instance.ReadVocsParam += new DataForwardDelegate(SetVocsParam);
        }
        private void SetCommParam(object sender, DataForwardEventArgs e)
        {
            byte[] data = ByteStrUtil.HexToByte(e.command.Data);
            ControlVol.Text = Convert.ToString(BitConverter.ToSingle(data, 1));
        }
        private void SetVocsParam(object sender, DataForwardEventArgs e)
        {
            byte[] data = ByteStrUtil.HexToByte(e.command.Data);
            switch (data[1])
            {
                case 03://光谱平均次数
                    break;
                case 07://打灯次数
                    break;
                case 08://积分时间
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 设置光谱仪公共参数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckData()) {
                MessageBox.Show("请检查测量参数！");
                return;
            }
            List<Command> commands = new List<Command>()
            {
                //光谱仪公共参数
                new Command
                {
                    Cmn = "20",
                    ExpandCmn = "66",
                    Data = "00" + ByteStrUtil.ByteToHexStr(BitConverter.GetBytes(voltage)) + " 00" + ByteStrUtil.ByteToHexStr(BitConverter.GetBytes((ushort)100))
                },
                //光谱仪平均次数
                new Command
                {
                    Cmn = "21",
                    ExpandCmn = "66",
                    Data = "00 03 " + avgTimes.ToString("x2")
                },
                //打灯次数
                new Command
                {
                    Cmn = "21",
                    ExpandCmn = "66",
                    Data = "00 07" + lightTimes.ToString("x2")
                },
                //积分时间
                new Command
                {
                    Cmn = "21",
                    ExpandCmn = "66",
                    Data = "00 08" + ByteStrUtil.ByteToHexStr(BitConverter.GetBytes(integtationTime))
                }
            };
            SuperSerialPort.Instance.SendAll(commands, true);
        }

        private bool CheckData()
        {
            try
            {
                voltage = Convert.ToSingle(ControlVol.Text);
                avgTimes = Convert.ToByte(AvgTimes.Text);
                lightTimes = Convert.ToByte(LightTimes.Text);
                integtationTime = Convert.ToUInt16(IntegrationTime.Text);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
