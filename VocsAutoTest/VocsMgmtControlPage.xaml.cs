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
        /// <summary>
        /// 设置波长
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetWavelength_Click(object sender, RoutedEventArgs e)
        {

        }
        /// <summary>
        /// 数据保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveData_Click(object sender, RoutedEventArgs e)
        {

        }

        private void StartSave_Click(object sender, RoutedEventArgs e)
        {
            if ("开始保存".Equals(StartSave.Content.ToString()))
            {
                StartSave.Content = "停止保存";
            }
            else
            {
                StartSave.Content = "开始保存";
            }
        }
    }
}
