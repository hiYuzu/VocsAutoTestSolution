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
using System.Windows.Shapes;
using VocsAutoTestCOMM;

namespace VocsAutoTest
{
    /// <summary>
    /// AlgorithmSettingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AlgorithmSettingWindow : Window
    {
        public AlgorithmSettingWindow()
        {
            ExceptionUtil.Instance.ShowLoadingAction(true);
            InitializeComponent();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        override protected void OnClosed(EventArgs e)
        {
            ExceptionUtil.Instance.ShowLoadingAction(false);
        }
    }
}
