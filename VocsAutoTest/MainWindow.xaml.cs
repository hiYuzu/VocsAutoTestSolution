﻿using System;
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
        //页面*7
        private AlgoGeneraPage algoPage;
        private AlgoGeneraControlPage algoControlPage;
        private SpecMeasurePage specPage;
        private SpecMeasureControlPage specControlPage;
        private ConcentrationMeasurePage concentrationPage;
        private ConcentrationMeasureControlPage concentrationControlPage;
        private LeftControlPage leftPage;
        //日志栏折叠
        private bool isLogBoxOpen = true;
        //日志栏高度
        private double bottomHeight;
        private DispatcherTimer showTimer;
        //当前页面标识 1：光谱采集 2：浓度测量 3：算法生成
        private ushort pageFlag = 1;
        //连续测量
        private MeasureMgrImpl measureMgr;
        public MainWindow()
        {
            InitializeComponent();
            DataForward.Instance.StartService();
            InitBottomInfo();
            InitLeftPage();
            measureMgr = MeasureMgrImpl.Instance;
            VocsCollectBtn_Click(null, null);
            DataForward.Instance.WriteResult += WriteRes;
            PassPortImpl.GetInstance().PassValueEvent += new PassPortDelegate(ReceievedValues);
            ExceptionUtil.Instance.ExceptionEvent += new ExceptionDelegate(ShowExceptionMsg);
        }
        /// <summary>
        /// 设置通用订阅
        /// </summary>
        /// <param name="res"></param>
        private void WriteRes(bool res)
        {
            if (res)
                Log4NetUtil.Debug("设置成功", this);
            else
                Log4NetUtil.Warn("设置失败", this);
        }
        /// <summary>
        /// 异常订阅
        /// </summary>
        /// <param name="msg"></param>
        private void ShowExceptionMsg(string msg)
        {
            Dispatcher.BeginInvoke(new Action(() => 
            {
                Log4NetUtil.Warn(msg, this);
            }));
            
        }
        /// <summary>
        /// 读取间隔订阅
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="command"></param>
        public void SetReadInterval(object sender, Command command)
        {
            byte[] data = ByteStrUtil.HexToByte(command.Data);
            if(data[1] == 09)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    ReadInterval.Text = BitConverter.ToUInt16(DataConvertUtil.ByteReverse(data), 2).ToString();
                }));  
            }
        }
        /// <summary>
        /// 初始化左侧控制页
        /// </summary>
        private void InitLeftPage()
        {
            leftPage = new LeftControlPage(this);
            LeftControlPage.Content = new Frame()
            {
                Content = leftPage
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
            
            SuperSerialPort.Instance.Close();
            SuperSerialPort.Instance.SetPortInfo(e.portModel.Port, Convert.ToInt32(e.portModel.Baud), e.portModel.Parity, Convert.ToInt32(e.portModel.Data), Convert.ToInt32(e.portModel.Stop));
            if (SuperSerialPort.Instance.Open())
            {
                Log4NetUtil.Debug("修改串口信息为：串口号:" + e.portModel.Port + "，波特率:" + e.portModel.Baud + "，校检:" + e.portModel.Parity + "，数据位:" + e.portModel.Data + "，停止位:" + e.portModel.Stop, this);
            }
            else
            {
                Log4NetUtil.Warn("修改串口信息失败！", this);
            }
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
        /// 连续测量次数可用控制
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
            if (measureMgr.StartMeasure)
            {
                MessageBox.Show("请停止测量再切换页面!");
                return;
            }
            pageFlag = 1;
            if (specPage == null && specControlPage == null)
            {
                specPage = new SpecMeasurePage();
                specControlPage = new SpecMeasureControlPage(specPage);
            }
            ChartPage.Content = new Frame()
            {
                Content = specPage
            };
            ControlPage.Content = new Frame()
            {
                Content = specControlPage
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
            if (measureMgr.StartMeasure)
            {
                MessageBox.Show("请停止测量再切换页面!");
                return;
            }
            pageFlag = 2;
            if (concentrationPage == null && concentrationControlPage == null)
            {
                concentrationPage = new ConcentrationMeasurePage();
                concentrationControlPage = new ConcentrationMeasureControlPage(concentrationPage);
                concentrationControlPage.Start_Measure();
            }
            ChartPage.Content = new Frame()
            {
                Content = concentrationPage
            };
            ControlPage.Content = new Frame()
            {
                Content = concentrationControlPage
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
            if (measureMgr.StartMeasure)
            {
                MessageBox.Show("请停止测量再切换页面!");
                return;
            }
            pageFlag = 3;
            if (algoPage == null)
            {
                algoPage = new AlgoGeneraPage();
            }
            if(algoControlPage == null)
            {
                algoControlPage = new AlgoGeneraControlPage(algoPage);
            }
            ChartPage.Content = new Frame()
            {
                Content = algoPage
            };
            ControlPage.Content = new Frame()
            {
                Content = algoControlPage
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
                bottomHeight = grid.RowDefinitions[4].Height.Value;
                grid.RowDefinitions[4].Height = new GridLength(0);
                isLogBoxOpen = false;
                Image img = (Image)HideLogBoxBtn.Template.FindName("LogBoxImg", HideLogBoxBtn);
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
                grid.RowDefinitions[4].Height = new GridLength(bottomHeight);
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
            try
            {
                MTsCheckBox.IsChecked = false;
                measureMgr.measureTimes = 1;
                BeginMeasure();
            }
            catch
            {
                MessageBox.Show("参数错误！");
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
                if (measureMgr.StartMeasure)
                {
                    measureMgr.StartMeasure = false;
                    SingleMeasure.IsEnabled = true;
                    MultiMeasure.Content = "连续测量";
                }
                else
                {
                    measureMgr.StartMeasure = true;
                    SingleMeasure.IsEnabled = false;
                    MultiMeasure.Content = "停止测量";
                    measureMgr.pageFlag = pageFlag;
                    BeginMeasure();
                }
            }
            catch
            {
                measureMgr.StartMeasure = false;
                SingleMeasure.IsEnabled = true;
                MessageBox.Show("参数错误！");
                MultiMeasure.Content = "连续测量";
            }
        }
        private void BeginMeasure()
        {
            measureMgr.endAction += EndMeasure;
            measureMgr.pageFlag = pageFlag;
            if (MTsTextBox.IsEnabled)
            {
                measureMgr.measureTimes = Convert.ToInt32(MTsTextBox.Text);
            }
            measureMgr.timeInterval = int.Parse(ReadInterval.Text);
            switch (pageFlag)
            {
                case 1:
                    measureMgr.specType = leftPage.DataType.SelectedIndex.ToString();
                    break;
                case 2:
                    measureMgr.tempValues = BitConverter.GetBytes(float.Parse(tempTextBox.Text));
                    measureMgr.pressValues = BitConverter.GetBytes(float.Parse(pressTextBox.Text));
                    break;
                case 3:
                    break;
            }
            measureMgr.StartMultiMeasure();
        }
        private void EndMeasure(bool beEnd)
        {
            if (beEnd)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    SingleMeasure.IsEnabled = true;
                    MultiMeasure.Content = "连续测量";
                }));
            }
        }
    }
}
