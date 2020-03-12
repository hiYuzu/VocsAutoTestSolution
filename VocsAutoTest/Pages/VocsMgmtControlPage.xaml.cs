using System;
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
        private int pixelNumber = 512;
        private readonly VocsMgmtPage vocsMgmtPage;
        public VocsMgmtControlPage(VocsMgmtPage page)
        {
            InitializeComponent();
            if (page != null)
            {
                this.vocsMgmtPage = page;
            }
            vocsMgmtPage.SetWave(3, this.pixelNumber, 185);
        }
        /// <summary>
        /// 间隔保存UI控制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IntervalSaveData_Checked(object sender, RoutedEventArgs e)
        {
            IntervalTime.IsEnabled = true;
        }
        private void UnIntervalSaveData_Checked(object sender, RoutedEventArgs e)
        {
            IntervalTime.IsEnabled = false;
        }
        /// <summary>
        /// 设置光谱文件目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// 开始保存（连续保存）TODO..
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// 是否显示标题
        /// 显示：1
        /// 不显示：0
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowTitle_Checked(object sender, RoutedEventArgs e)
        {
            if(vocsMgmtPage != null)
            {
                vocsMgmtPage.IsShow(1);
            }
        }
        private void ShowTitle_Unchecked(object sender, RoutedEventArgs e)
        {
            vocsMgmtPage.IsShow(0);
        }
        /// <summary>
        /// 是否显示标签
        /// 显示：3
        /// 不显示：2
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowTag_Checked(object sender, RoutedEventArgs e)
        {
            if(vocsMgmtPage != null)
            {
                vocsMgmtPage.IsShow(3);
            }
        }
        private void ShowTag_Unchecked(object sender, RoutedEventArgs e)
        {
            vocsMgmtPage.IsShow(2);
        }
        /// <summary>
        /// X轴单位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Wavelength_Unchecked(object sender, RoutedEventArgs e)
        {
            InitTitleAndTag();
            //像素，波长=>像素
            xAxisName.Text = "X(像素)";
            vocsMgmtPage.IsPixel = true;
            vocsMgmtPage.XAxisTitle = "像素";
            vocsMgmtPage.CreateChartSpline();
        }
        private void Pixel_Unchecked(object sender, RoutedEventArgs e)
        {
            InitTitleAndTag();
            //波长，像素=>波长
            xAxisName.Text = "X(波长)";
            vocsMgmtPage.IsPixel = false;
            vocsMgmtPage.XAxisTitle = "波长";
            vocsMgmtPage.CreateChartSpline();
        }
        /// <summary>
        /// Y轴单位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IntegralValue_Unchecked(object sender, RoutedEventArgs e)
        {
            InitTitleAndTag();
            //电压值，积分=>电压
            vocsMgmtPage.YAxisTitle = "电压值(V)";
            vocsMgmtPage.IsVoltage = true;
            vocsMgmtPage.CreateChartSpline();
        }
        private void VoltageValue_Unchecked(object sender, RoutedEventArgs e)
        {
            InitTitleAndTag();
            //积分值，电压=>积分
            vocsMgmtPage.YAxisTitle = "积分值";
            vocsMgmtPage.IsVoltage = false;
            vocsMgmtPage.CreateChartSpline();
        }
        /// <summary>
        /// 初始化标题与标签UI
        /// </summary>
        private void InitTitleAndTag()
        {
            ShowTitle.IsChecked = true;
            ShowTag.IsChecked = true;
        }
        /// <summary>
        /// 导入历史数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImportHistory_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog op = new Microsoft.Win32.OpenFileDialog();
            if(FolderPath.Text != null && !"".Equals(FolderPath.Text))
            {
                op.InitialDirectory = FolderPath.Text;
            }
            else
            {
                op.InitialDirectory = System.Windows.Forms.Application.StartupPath + "\\ParameterGen\\"; ;//默认的打开路径
            }
            op.RestoreDirectory = true;
            op.Filter = " 文本文件(*.txt)|*.txt|所有文件(*.*)|*.* ";
            if (op.ShowDialog() == true)
            {
                vocsMgmtPage.ImportHistoricalData(op.FileName);
            }
        }
        /// <summary>
        /// 设置波长
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetWavelength_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uint fstParam = Convert.ToUInt32(firstParam.Text);
                vocsMgmtPage.SetWave(sensorType.SelectedIndex, pixelNumber, fstParam);
            }
            catch
            {
                //TODO..
            }
        }
        /// <summary>
        /// 当前测量单次保存UI变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SingleSave_Checked(object sender, RoutedEventArgs e)
        {
            if ("停止保存".Equals(startSave.Content.ToString()))
            {
                if(MessageBoxResult.Yes == System.Windows.MessageBox.Show("当前正在连续保存，是否停止？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question))
                {
                    StartSave_Click(null, null);
                    save.IsEnabled = true;
                    startSave.IsEnabled = false;
                    intervalSave.IsEnabled = false;
                    noIntervalSave.IsEnabled = false;
                    IntervalTime.IsEnabled = false;
                    saveNum.IsEnabled = false;
                }
                else
                {
                    ContinSave.IsChecked = true;
                }
            }
            if ("开始保存".Equals(startSave.Content.ToString()))
            {
                save.IsEnabled = true;
                startSave.IsEnabled = false;
                intervalSave.IsEnabled = false;
                noIntervalSave.IsEnabled = false;
                IntervalTime.IsEnabled = false;
                saveNum.IsEnabled = false;
            }
        }
        /// <summary>
        /// 连续保存UI变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SingleSave_Unchecked(object sender, RoutedEventArgs e)
        {
            save.IsEnabled = false;
            startSave.IsEnabled = true;
            intervalSave.IsEnabled = true;
            noIntervalSave.IsEnabled = true;
            IntervalTime.IsEnabled = true;
            saveNum.IsEnabled = true;
        }
        /// <summary>
        /// 是否显示具体值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowSpecific_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                xPixel.IsEnabled = false;
                showSpecific.IsChecked = false;
            }
            catch
            {
                System.Windows.MessageBox.Show("请输入有效数据！");
                showSpecific.IsChecked = false;
            }
            
        }
        private void ShowSpecific_Unchecked(object sender, RoutedEventArgs e)
        {
            xPixel.IsEnabled = true;
        }

        private void ClearAllSeries_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(vocsMgmtPage.SpectrumChart.Children.Count);
            vocsMgmtPage.SpectrumChart.Children.Clear();
        }
    }
}
