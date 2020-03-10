using System;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using VocsAutoTest.Log4Net;
using VocsAutoTestCOMM;

namespace VocsAutoTest
{
    /// <summary>
    /// StartWindow.xaml 的交互逻辑
    /// </summary>
    public partial class StartWindow : Window
    {
        public StartWindow()
        {
            InitializeComponent();
            InitSerialPort();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (CheckData())
            {
                SuperSerialPort.Instance.Close();
                SuperSerialPort.Instance.SetPortInfo(portCombo.Text, Convert.ToInt32(baudCombo.Text), parityCombo.Text, Convert.ToInt32(dataCombo.Text), Convert.ToInt32(stopCombo.Text));
                SuperSerialPort.Instance.Open();
                MainWindow main = new MainWindow();               
                Log4NetUtil.Info("当前串口信息：串口号:" + portCombo.Text + "，波特率:" + baudCombo.Text + "，校检:" + parityCombo.Text + "，数据位:" + dataCombo.Text + "，停止位:" + stopCombo.Text, main);                
                main.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("参数不得为空！");
            }
        }

        /// <summary>
        /// 扫描设备端口号
        /// </summary>
        private void InitSerialPort()
        {
            portCombo.Items.Clear();
            foreach (string port in SerialPort.GetPortNames())
            {
                ComboBoxItem item = new ComboBoxItem
                {
                    Content = port
                };
                portCombo.Items.Add(item);
            }
            portCombo.SelectedIndex = 0;
        }

        /// <summary>
        /// 验证所有数据正确输入
        /// </summary>
        /// <returns></returns>
        private bool CheckData()
        {
            bool havaPort = portCombo.SelectedIndex != -1;
            bool havaBaud = baudCombo.SelectedIndex != -1;
            bool havaParity = parityCombo.SelectedIndex != -1;
            bool havaData = dataCombo.SelectedIndex != -1;
            bool havaStop = stopCombo.SelectedIndex != -1;
            return havaPort && havaBaud && havaParity && havaData && havaStop;
        }
    }
}
