using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using VocsAutoTestBLL;
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
            DataForward.Instance.ReadCommParam += new DataForwardDelegate(SetCommParam);
            DataForward.Instance.ReadVocsParam += new DataForwardDelegate(SetVocsParam);
            SuperSerialPort.Instance.SendAll(commands, true);
        }
        /// <summary>
        /// 设置控制电压参数
        /// 设备 -> 软件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="command"></param>
        private void SetCommParam(object sender, Command command)
        {
            byte[] data = new byte[4];
            Array.Copy(ByteStrUtil.HexToByte(command.Data), 1, data, 0, 4);
            Array.Reverse(data, 0, data.Length);
            Dispatcher.Invoke(new Action(() =>
            {
                ControlVol.Text = BitConverter.ToSingle(data, 0).ToString("f1");
            }));
        }
        /// <summary>
        /// 设置光谱仪参数
        /// 设备 -> 软件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="command"></param>
        private void SetVocsParam(object sender, Command command)
        {
            byte[] data = ByteStrUtil.HexToByte(command.Data);
            switch (data[1])
            {
                case 03://光谱平均次数
                    Dispatcher.Invoke(new Action(() =>
                    {
                        AvgTimes.Text = data[2].ToString();
                    }));
                    break;
                case 07://打灯次数
                    Dispatcher.Invoke(new Action(() =>
                    {
                        LightTimes.Text = data[2].ToString();
                    }));
                    break;
                case 08://积分时间
                    Dispatcher.Invoke(new Action(() =>
                    {
                        byte[] time = new byte[2];
                        Array.Copy(data, 2, time, 0, 2);
                        Array.Reverse(time, 0, time.Length);
                        IntegrationTime.Text = BitConverter.ToUInt16(time, 0).ToString();
                    }));
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 设置光谱仪公共参数
        /// 软件 -> 设备
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckData()) {
                MessageBox.Show("请检查测量参数！");
                return;
            }
            byte[] voltage = BitConverter.GetBytes(this.voltage);
            Array.Reverse(voltage);
            byte[] zeroTimes = BitConverter.GetBytes((ushort)100);
            Array.Reverse(zeroTimes);
            byte[] integtationTime = BitConverter.GetBytes(this.integtationTime);
            Array.Reverse(integtationTime);
            List<Command> commands = new List<Command>()
            {
                //光谱仪平均次数
                new Command
                {
                    Cmn = "21",
                    ExpandCmn = "66",
                    Data = "00 03 " + avgTimes.ToString("x2")
                },
                //氙灯控制电压
                new Command
                {
                    Cmn = "20",
                    ExpandCmn = "66",
                    Data = "00" + ByteStrUtil.ByteToHexStr(voltage) + " 00" + ByteStrUtil.ByteToHexStr(zeroTimes)
                },
                //打灯次数
                new Command
                {
                    Cmn = "21",
                    ExpandCmn = "66",
                    Data = "00 07 " + lightTimes.ToString("x2")
                },
                //积分时间
                new Command
                {
                    Cmn = "21",
                    ExpandCmn = "66",
                    Data = "00 08" + ByteStrUtil.ByteToHexStr(integtationTime)
                }
            };
            SuperSerialPort.Instance.SendAll(commands, true);
        }
        /// <summary>
        /// 检查数据是否规范正确输入
        /// </summary>
        /// <returns></returns>
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
