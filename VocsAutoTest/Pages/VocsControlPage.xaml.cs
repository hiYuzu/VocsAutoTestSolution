using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VocsAutoTestCOMM;

namespace VocsAutoTest.Pages
{
    /// <summary>
    /// VocsControlPage.xaml 的交互逻辑
    /// </summary>
    public partial class VocsControlPage : Page
    {

        private byte[] tempValue;
        private byte[] pressValue;
        private byte[] caliConcValue;
        public VocsControlPage()
        {
            InitializeComponent();
        }

        private void OpenCloseLS_Click(object sender, RoutedEventArgs e)
        {
            SuperSerialPort.Instance.Send(new Command { Cmn = "23", ExpandCmn = "66", Data = lightSourceCtrl.SelectedIndex.ToString() + "" + lightSourceCtrl.SelectedIndex.ToString() });
        }

        private void Zero_Click(object sender, RoutedEventArgs e)
        {
            SuperSerialPort.Instance.Send(new Command { Cmn = "2A", ExpandCmn = "66", Data = lightPath.SelectedIndex.ToString("x2") + orderType.SelectedIndex.ToString("x2") });
        }

        private void Cali_Click(object sender, RoutedEventArgs e)
        {
            if (ValidData())
            {
                SuperSerialPort.Instance.Send(new Command { Cmn = "2B", ExpandCmn = "66", Data = lightPath.SelectedIndex.ToString("x2") + gas.SelectedIndex.ToString("x2") + range.SelectedIndex.ToString("x2") + ByteStrUtil.ByteToHex(tempValue) + ByteStrUtil.ByteToHex(pressValue) + ByteStrUtil.ByteToHex(caliConcValue) + orderType.SelectedIndex.ToString("x2") });
            }
        }

        private bool ValidData()
        {
            try
            {
                tempValue = BitConverter.GetBytes(float.Parse(temp.Text));
                Array.Reverse(tempValue);
                pressValue = BitConverter.GetBytes(float.Parse(press.Text));
                Array.Reverse(pressValue);
                caliConcValue = BitConverter.GetBytes(float.Parse(caliConc.Text));
                Array.Reverse(caliConcValue);
                return true;
            }
            catch
            {
                MessageBox.Show("非法数据!");
                return false;
            }
        }
    }
}
