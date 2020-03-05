using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using VocsAutoTest.Log4Net;

namespace VocsAutoTest.Pages
{
    /// <summary>
    /// VocsMgmtControlPage.xaml 的交互逻辑
    /// </summary>
    public partial class VocsMgmtControlPage : Page
    {
        public VocsMgmtControlPage()
        {
            InitializeComponent();
        }
        private void IntervalSaveData_Checked(object sender, RoutedEventArgs e)
        {
            IntervalTime.IsEnabled = true;
        }
        private void UnIntervalSaveData_Checked(object sender, RoutedEventArgs e)
        {
            IntervalTime.IsEnabled = false;
        }

        private void SetFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();
            if(openFolderDialog.ShowDialog() == DialogResult.OK)
            {
                FolderPath.Text = openFolderDialog.SelectedPath;
                Log4NetUtil.Info("光谱文件目录已设置为：" + FolderPath.Text, Window.GetWindow(this) as MainWindow);
            }
        }

        private void StartSave_Click(object sender, RoutedEventArgs e)
        {
            if ("开始保存".Equals(startSave.Content.ToString()))
            {
                startSave.Content = "停止保存";
            }
            else
            {
                startSave.Content = "开始保存";

            }
        }
    }
}
