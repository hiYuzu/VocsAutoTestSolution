using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using VocsAutoTestBLL.Interface;
using VocsAutoTestBLL.Impl;
using VocsAutoTestBLL.Model;
using VocsAutoTestCOMM;
using System;

namespace VocsAutoTest
{
    /// <summary>
    /// PortSettingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PortSettingWindow : Window
    {
        public PortSettingWindow()
        {
            ExceptionUtil.Instance.ShowLoadingAction(true);
            InitializeComponent();
            InitSerialPort();
            SetCurrentData();
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
        /// 窗口显示为当前所选参数
        /// </summary>
        private void SetCurrentData()
        {
            //TODO..
        }

        /// <summary>
        /// 确认按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (CheckData())
            {
                PassPortImpl.GetInstance().GetPort(new PortModel(portCombo.Text, baudCombo.Text, parityCombo.Text, dataCombo.Text, stopCombo.Text));
                Close();
            }
            else
            {
                MessageBox.Show("参数不得为空！");
            }
                
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

        /// <summary>
        /// 取消按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        override protected void OnClosed(EventArgs e)
        {
            ExceptionUtil.Instance.ShowLoadingAction(false);
        }
    }
}
