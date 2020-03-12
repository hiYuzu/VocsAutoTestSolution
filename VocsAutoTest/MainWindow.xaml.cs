using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using VocsAutoTest.Pages;
using VocsAutoTest.Log4Net;
using VocsAutoTestBLL.Interface;
using VocsAutoTestBLL.Impl;
using System.Windows.Threading;
using VocsAutoTestCOMM;
using VocsAutoTestBLL;
using System.Threading;

namespace VocsAutoTest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //测量结果数据
        private float[] specData = null;
        //页面*6
        private VocsMgmtPage VocsPage;
        private VocsMgmtControlPage VocsControlPage;
        private ConcentrationMeasurePage ConcentrationPage;
        private ConcentrationMeasureControlPage ConcentrationControlPage;
        private AlgoGeneraControlPage AlgoControlPage;
        private LeftControlPage LeftPage;
        //日志栏折叠
        private bool isLogBoxOpen = true;
        //日志栏高度
        private double oldBottomHeight = 0;
        private DispatcherTimer showTimer;
        //当前页面标识 1：光谱采集 2：浓度测量 3：算法生成
        private ushort pageFlag;
        //光谱采集数据包
        private ushort dataPackage;
        public MainWindow()
        {
            InitializeComponent();
            DataForward.Instance.StartService();
            InitBottomInfo();
            InitLeftPage();
            VocsCollectBtn_Click(null, null);
            DataForward.Instance.WriteResult += WriteRes;
            DataForward.Instance.ReadVocsData += new DataForwardDelegate(SetVocsData);
            PassPortImpl.GetInstance().PassValueEvent += new PassPortDelegate(ReceievedValues);
        }
        private void WriteRes(bool res)
        {
            if (res)
                MessageBox.Show("设置成功");
            else
                MessageBox.Show("设置失败");
        }
        public void SetReadInterval(object sender, Command command)
        {
            byte[] data = ByteStrUtil.HexToByte(command.Data);
            if(data[1] == 09)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    ReadInterval.Text = BitConverter.ToUInt16(data, 2).ToString();
                }));  
            }
        }
        /// <summary>
        /// 初始化左侧控制页
        /// </summary>
        private void InitLeftPage()
        {
            LeftPage = new LeftControlPage(this);
            LeftControlPage.Content = new Frame()
            {
                Content = LeftPage
            };
        }
        /// <summary>
        /// 初始化底部信息
        /// 版本号/时间
        /// </summary>
        private void InitBottomInfo()
        {
            //版本号
            this.versionNum.Content = "V1.0";
            //时间
            showTimer = new DispatcherTimer();
            showTimer.Tick += ShowTimer_Tick;
            showTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            showTimer.Start();
        }

        private void ShowTimer_Tick(object sender, EventArgs e)
        {
            this.currentTime.Content = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        }

        /// <summary>
        /// 拖动窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        /// <summary>
        /// 串口设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PortBtn_Click(object sender, RoutedEventArgs e)
        {
            PortSettingWindow portSettingWindow = new PortSettingWindow
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            portSettingWindow.Show();
        }

        /// <summary>
        /// 串口接收
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReceievedValues(object sender, PassPortEventArgs e)
        {
            Log4NetUtil.Info("修改串口信息为：串口号:" + e.portModel.Port + "，波特率:" + e.portModel.Baud + "，校检:" + e.portModel.Parity + "，数据位:" + e.portModel.Data + "，停止位:" + e.portModel.Stop, this);
            SuperSerialPort.Instance.SetPortInfo(e.portModel.Port, Convert.ToInt32(e.portModel.Baud), e.portModel.Parity, Convert.ToInt32(e.portModel.Data), Convert.ToInt32(e.portModel.Stop));
        }
        /// <summary>
        /// 关于系统
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AboutSysBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Copyright © 天津七一二通信广播股份有限公司", "关于系统", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        /// <summary>
        /// 退出系统
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("是否确定退出系统？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                this.Close();
            }
        }
        /// <summary>
        /// 最大化与正常化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MaxButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized;
            MaxButton.Visibility = Visibility.Collapsed;
            NormalButton.Visibility = Visibility.Visible;
        }
        private void NormalButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Normal;
            NormalButton.Visibility = Visibility.Collapsed;
            MaxButton.Visibility = Visibility.Visible;
        }
        /// <summary>
        /// 最小化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MiniButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        /// <summary>
        /// 连续测量
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MTsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            MTsTextBox.IsEnabled = true;
        }
        private void MTsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            MTsTextBox.IsEnabled = false;
        }
        /// <summary>
        /// 光谱采集
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VocsCollectBtn_Click(object sender, RoutedEventArgs e)
        {
            if ("停止测量".Equals(MultiMeasure.Content.ToString()))
            {
                MessageBox.Show("请停止测量再切换页面!");
                return;
            }
            pageFlag = 1;
            if (VocsPage == null && VocsControlPage == null)
            {
                VocsPage = new VocsMgmtPage();
                VocsControlPage = new VocsMgmtControlPage(VocsPage);
            }
            ChartPage.Content = new Frame()
            {
                Content = VocsPage
            };
            ControlPage.Content = new Frame()
            {
                Content = VocsControlPage
            };
            this.tempTextBox.Visibility = Visibility.Hidden;
            this.pressTextBox.Visibility = Visibility.Hidden;
        }
        /// <summary>
        /// 浓度测量
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConcentrationMeasureBtn_Click(object sender, RoutedEventArgs e)
        {
            if ("停止测量".Equals(MultiMeasure.Content.ToString()))
            {
                MessageBox.Show("请停止测量再切换页面!");
                return;
            }
            pageFlag = 2;
            if (ConcentrationPage == null && ConcentrationControlPage == null)
            {
                ConcentrationPage = new ConcentrationMeasurePage();
                ConcentrationControlPage = new ConcentrationMeasureControlPage(ConcentrationPage);
                ConcentrationControlPage.Start_Measure();
            }
            ChartPage.Content = new Frame()
            {
                Content = ConcentrationPage
            };
            ControlPage.Content = new Frame()
            {
                Content = ConcentrationControlPage
            };
            this.tempTextBox.Visibility = Visibility.Visible;
            this.pressTextBox.Visibility = Visibility.Visible;
        }
        /// <summary>
        /// 算法生成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AlgoGeneraBtn_Click(object sender, RoutedEventArgs e)
        {
            if ("停止测量".Equals(MultiMeasure.Content.ToString()))
            {
                MessageBox.Show("请停止测量再切换页面!");
                return;
            }
            pageFlag = 3;
            if (VocsPage == null)
            {
                VocsPage = new VocsMgmtPage();
            }
            if(AlgoControlPage == null)
            {
                AlgoControlPage = new AlgoGeneraControlPage();
            }
            ChartPage.Content = new Frame()
            {
                Content = VocsPage
            };
            ControlPage.Content = new Frame()
            {
                Content = AlgoControlPage
            };
            this.tempTextBox.Visibility = Visibility.Hidden;
            this.pressTextBox.Visibility = Visibility.Hidden;
        }
        /// <summary>
        /// 隐藏日志信息栏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HideLogBoxBtn_Click(object sender, RoutedEventArgs e)
        {
            if (isLogBoxOpen)
            {
                oldBottomHeight = grid.RowDefinitions[4].Height.Value;
                grid.RowDefinitions[4].Height = new GridLength(0);
                isLogBoxOpen = false;
                Image img = (Image)HideLogBoxBtn.Template.FindName("LogBoxImg", HideLogBoxBtn);//查找btn中模板图片对象
                Thickness thickness = new Thickness
                {
                    Bottom = 0,
                    Left = 0,
                    Right = 0,
                    Top = 0
                };
                img.Margin = thickness;
            }
            else
            {
                grid.RowDefinitions[4].Height = new GridLength(218);
                isLogBoxOpen = true;
                Image img = (Image)HideLogBoxBtn.Template.FindName("LogBoxImg", HideLogBoxBtn);
                Thickness thickness = new Thickness
                {
                    Bottom = 0,
                    Left = -30,
                    Right = 0,
                    Top = 0
                };
                img.Margin = thickness;
            }
        }
        /// <summary>
        /// 清除日志显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearLogBoxBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("是否清除所有日志信息？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                this.LogBox.Document.Blocks.Clear();
            }
        }
        /// <summary>
        /// 单次测量
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SingleMeasure_Click(object sender, RoutedEventArgs e)
        {
            string dataType = LeftPage.DataType.SelectedIndex.ToString();
            if (pageFlag == 1)
            {
                dataType += dataType;
                string cmn = "24";
                string expandCmn = "55";
                string data = "00 " + dataType + " 01";
                SingleDataMeasure(1,cmn, expandCmn, data);
            } else if (pageFlag == 3) {
                //算法生成
                //dataType += dataType;
                //string cmn = "24";
                //string expandCmn = "55";
                //string data = "00 " + dataType + " 01";
                //SingleDataMeasure(1, cmn, expandCmn, data);
            }
            else if (pageFlag == 2)
            {
                //浓度测量
                byte[] tempValues;
                string temp = tempTextBox.Text.Trim();
                if (string.IsNullOrEmpty(temp))
                {
                    MessageBox.Show("请输入温度值！");
                    tempTextBox.Focus();
                    return;
                }
                else
                {
                    tempValues = BitConverter.GetBytes(float.Parse(temp));
                }
                byte[] pressValues;
                string press = pressTextBox.Text.Trim();
                if (string.IsNullOrEmpty(press))
                {
                    MessageBox.Show("请输入压力值！");
                    pressTextBox.Focus();
                    return;
                }
                else
                {
                    pressValues = BitConverter.GetBytes(float.Parse(press));
                }
                //浓度测量
                dataType += dataType;
                string cmn = "29";
                string expandCmn = "55";
                string data = "00";
                for (int i =0;i<4;i++) {
                    data += " "+ Convert.ToString(tempValues[i],16); 
                }
                for (int i = 0; i < 4; i++)
                {
                    data += " " + Convert.ToString(pressValues[i], 16);
                }
                SingleDataMeasure(2, cmn, expandCmn, data);
            }
            else
            {
                MessageBox.Show("操作页面不明确！");
            }
        }
        /// <summary>
        /// 多次测量
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MultiMeasure_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ("连续测量".Equals(MultiMeasure.Content.ToString()))
                {
                    string dataType = LeftPage.DataType.SelectedIndex.ToString();
                    uint measureTimes = 1;
                    ushort readInterval = Convert.ToUInt16(ReadInterval.Text);
                    if (MTsTextBox.IsEnabled)
                    {
                        measureTimes = Convert.ToUInt32(MTsTextBox.Text);
                    }
                    MultiMeasure.Content = "停止测量";
                    new Thread(new ThreadStart(() =>
                    {
                        if (pageFlag == 1)
                        {
                            dataType += dataType;
                            string cmn = "24";
                            string expandCmn = "55";
                            string data = "00 " + dataType + " 01";
                            SingleDataMeasure(1, cmn, expandCmn, data);
                        }
                        else if (pageFlag == 3)
                        {
                            //算法生成
                            //dataType += dataType;
                            //string cmn = "24";
                            //string expandCmn = "55";
                            //string data = "00 " + dataType + " 01";
                            //SingleDataMeasure(1, cmn, expandCmn, data);
                        }
                        else if (pageFlag == 2)
                        {
                            //浓度测量
                            //浓度测量
                            byte[] tempValues;
                            string temp = tempTextBox.Text.Trim();
                            if (string.IsNullOrEmpty(temp))
                            {
                                MessageBox.Show("请输入温度值！");
                                tempTextBox.Focus();
                                return;
                            }
                            else
                            {
                                tempValues = BitConverter.GetBytes(float.Parse(temp));
                            }
                            byte[] pressValues;
                            string press = pressTextBox.Text.Trim();
                            if (string.IsNullOrEmpty(press))
                            {
                                MessageBox.Show("请输入压力值！");
                                pressTextBox.Focus();
                                return;
                            }
                            else
                            {
                                pressValues = BitConverter.GetBytes(float.Parse(press));
                            }
                            //浓度测量
                            dataType += dataType;
                            string cmn = "29";
                            string expandCmn = "55";
                            string data = "00";
                            for (int i = 0; i < 4; i++)
                            {
                                data += " " + Convert.ToString(tempValues[i], 16);
                            }
                            for (int i = 0; i < 4; i++)
                            {
                                data += " " + Convert.ToString(pressValues[i], 16);
                            }
                            SingleDataMeasure(2, cmn, expandCmn, data);
                        }
                        Thread.Sleep(readInterval * 1000);
                    }));
                }
                else
                {
                    MultiMeasure.Content = "连续测量";
                }
            }
            catch
            {
                MessageBox.Show("连续测量参数错误！");
            }
            
        }
        /// <summary>
        /// 发送数据采集命令
        /// </summary>
        /// <param name="dataType"></param>
        private void SingleDataMeasure(uint measureTimes,string cmn,string expandCmn,string data)
        {
            while (measureTimes > 0)
            {
                Command command = new Command
                {
                    Cmn = cmn,//"24"
                    ExpandCmn = expandCmn,//"55"
                    Data = data//"00 " + dataType + " 01"
                };
                SuperSerialPort.Instance.Send(command, true);
                measureTimes -= 1;
            }
        }
        /// <summary>
        /// 得到光谱采集数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="command"></param>
        private void SetVocsData(object sender, Command command)
        {
            byte[] data = ByteStrUtil.HexToByte(command.Data);
            dataPackage = Convert.ToUInt16(data[3]);
            if (dataPackage < Convert.ToUInt16(data[4])) {
                //TODO..
            }
            Console.WriteLine(data[3].ToString());
            Console.WriteLine(data[4].ToString());
        }
    }
}
