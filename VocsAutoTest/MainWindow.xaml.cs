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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading;

namespace VocsAutoTest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //页面*6
        private VocsMgmtPage VocsPage;
        private VocsMgmtControlPage VocsControlPage;
        private ConcentrationMeasurePage ConcentrationPage;
        private ConcentrationMeasureControlPage ConcentrationControlPage;
        private AlgoGeneraControlPage AlgoControlPage;
        private LeftControlPage LeftPage;
        //日志栏折叠
        private bool isLogBoxOpen = true;
        //日志栏原始高度
        private double oldBottomHeight = 0;
        private DispatcherTimer showTimer;
        public MainWindow()
        {
            InitializeComponent();
            InitBottomInfo();
            InitReadInterval();
            InitLeftPage();
            VocsCollectBtn_Click(null, null);
        }
        /// <summary>
        /// 初始化左侧控制页
        /// </summary>
        private void InitLeftPage()
        {
            LeftPage = new LeftControlPage();
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
        /// 初始化读数间隔
        /// </summary>
        private void InitReadInterval()
        {
            Command command = new Command
            {
                Cmn = "21",
                ExpandCmn = "55",
                Data = "00 09"
            };
            SuperSerialPort.Instance.Send(command, true);
            Thread.Sleep(DefaultArgument.INTERVAL_TIME);
            //接收TODO..
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
            PassPortImpl.GetInstance().PassValueEvent += new PassPortDelegate(ReceievedValues);
            portSettingWindow.Show();
        }

        /// <summary>
        /// 串口接收
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReceievedValues(object sender, PassPortEventArgs e)
        {
            SuperSerialPort.Instance.Close();
            Log4NetUtil.Info("修改串口信息为：串口号:" + e.portModel.Port + "，波特率:" + e.portModel.Baud + "，校检:" + e.portModel.Parity + "，数据位:" + e.portModel.Data + "，停止位:" + e.portModel.Stop, this);
            SuperSerialPort.Instance.SetPortInfo(e.portModel.Port, Convert.ToInt32(e.portModel.Baud), e.portModel.Parity, Convert.ToInt32(e.portModel.Data), Convert.ToInt32(e.portModel.Stop));
            SuperSerialPort.Instance.Open();
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
            if (ConcentrationPage == null && ConcentrationControlPage == null)
            {
                ConcentrationPage = new ConcentrationMeasurePage();
                ConcentrationControlPage = new ConcentrationMeasureControlPage(ConcentrationPage);
            }
            ChartPage.Content = new Frame()
            {
                Content = ConcentrationPage
            };
            ControlPage.Content = new Frame()
            {
                Content = ConcentrationControlPage
            };
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
            //
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
            Log4NetUtil.Info("单次测量：" + LeftPage.DataType.Text + "\n" + "光谱仪平均次数:" + LeftPage.AvgTimes.Text +"，氙灯控制电压:" + LeftPage.ControlVol.Text + "V，氙灯打灯次数:" + LeftPage.LightTimes.Text + "，积分时间:" + LeftPage.IntegrationTime.Text + "ms", this);
        }
        /// <summary>
        /// 多次测量
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MultiMeasure_Click(object sender, RoutedEventArgs e)
        {
            
            if ("连续测量".Equals(MultiMeasure.Content.ToString()))
            {
                MultiMeasure.Content = "停止测量";
                Log4NetUtil.Info("间隔" + this.ReadInterval.Text + "ms连续测量：" + LeftPage.DataType.Text + "\n" + "光谱仪平均次数:" + LeftPage.AvgTimes.Text + "，氙灯控制电压:" + LeftPage.ControlVol.Text + "V，氙灯打灯次数:" + LeftPage.LightTimes.Text + "，积分时间:" + LeftPage.IntegrationTime.Text + "ms", this);
                //TODO..
            }
            else
            {
                MultiMeasure.Content = "连续测量";
            }
        }
    }
}
