using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using VocsAutoTest.Tools;
using VocsAutoTestBLL.Impl;
using VocsAutoTestCOMM;

namespace VocsAutoTest.Pages
{
    /// <summary>
    /// VocsMgmtControlPage.xaml 的交互逻辑
    /// </summary>
    public partial class SpecMeasureControlPage : Page
    {
        private int pixelNumber = 512;
        private readonly SpecMeasurePage specPage;
        private SpecDataSave specDataSave;
        public SpecMeasureControlPage(SpecMeasurePage page)
        {
            InitializeComponent();
            if (page != null)
            {
                this.specPage = page;
            }
            specDataSave = SpecDataSave.Instance;
            FolderPath.Text = specDataSave.SpecDataSavePath;
            specPage.SetWave(3, pixelNumber, 185);
        }
        /// <summary>
        /// 间隔保存UI控制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IntervalSaveData_Checked(object sender, RoutedEventArgs e)
        {
            intervalTime.IsEnabled = true;
        }
        private void UnIntervalSaveData_Checked(object sender, RoutedEventArgs e)
        {
            intervalTime.IsEnabled = false;
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
                specDataSave.SpecDataSavePath = openFolderDialog.SelectedPath;
                ExceptionUtil.Instance.LogMethod("光谱文件目录已设置为：" + FolderPath.Text);
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
            if(specPage != null)
            {
                specPage.IsShow(1);
            }
        }
        private void ShowTitle_Unchecked(object sender, RoutedEventArgs e)
        {
            specPage.IsShow(0);
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
            if(specPage != null)
            {
                specPage.IsShow(3);
            }
        }
        private void ShowTag_Unchecked(object sender, RoutedEventArgs e)
        {
            specPage.IsShow(2);
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
            specPage.IsPixel = true;
            specPage.XAxisTitle = "像素";
            specPage.CreateCurrentChart();
            specPage.CreateHistoricalChart();
        }
        private void Pixel_Unchecked(object sender, RoutedEventArgs e)
        {
            InitTitleAndTag();
            //波长，像素=>波长
            xAxisName.Text = "X(波长)";
            specPage.IsPixel = false;
            specPage.XAxisTitle = "波长";
            specPage.CreateCurrentChart();
            specPage.CreateHistoricalChart();
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
            specPage.YAxisTitle = "电压值(V)";
            specPage.IsVoltage = true;
            specPage.CreateCurrentChart();
            specPage.CreateHistoricalChart();
        }
        private void VoltageValue_Unchecked(object sender, RoutedEventArgs e)
        {
            InitTitleAndTag();
            //积分值，电压=>积分
            specPage.YAxisTitle = "积分值";
            specPage.IsVoltage = false;
            specPage.CreateCurrentChart();
            specPage.CreateHistoricalChart();
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
                specPage.ImportHistoricalData(op.FileName);
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
                specPage.SetWave(sensorType.SelectedIndex, pixelNumber, fstParam);
            }
            catch
            {
                //TODO..
            }
        }
        /// <summary>
        /// 当前测量保存UI变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SingleSave_Checked(object sender, RoutedEventArgs e)
        {
            singleSave.IsEnabled = true;
            startSave.IsEnabled = false;
            intervalSave.IsEnabled = false;
            noIntervalSave.IsEnabled = false;
            intervalTime.IsEnabled = false;
            saveNum.IsEnabled = false;
        }
        /// <summary>
        /// 连续保存UI变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SingleSave_Unchecked(object sender, RoutedEventArgs e)
        {
            singleSave.IsEnabled = false;
            startSave.IsEnabled = true;
            intervalSave.IsEnabled = true;
            noIntervalSave.IsEnabled = true;
            intervalTime.IsEnabled = true;
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
        /// <summary>
        /// 清除全部曲线
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearAllSeries_Click(object sender, RoutedEventArgs e)
        {
            specPage.ClearAllSeries();
        }
        /// <summary>
        /// 清除当次测量曲线
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearCurrentSeries_Click(object sender, RoutedEventArgs e)
        {
            specPage.ClearCurrentSeries();
        }
        /// <summary>
        /// 清除历史测量曲线
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearHistoricalSeries_Click(object sender, RoutedEventArgs e)
        {
            specPage.ClearHistoricalSeries();
        }
        /// <summary>
        /// 设置光谱限值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetSpecLimit_Click(object sender, RoutedEventArgs e)
        {

        }
        /// <summary>
        /// 计算顶点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComputeTopPoint_Click(object sender, RoutedEventArgs e)
        {

        }
        /// <summary>
        /// 单次保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SingleSave_Click(object sender, RoutedEventArgs e)
        {
            string[] data = specPage.CurrentData;
            if (data == null)
            {
                System.Windows.MessageBox.Show("没有可以保存的数据");
                return;
            }
            specDataSave.SaveCurrentData(data);
        }
        /// <summary>
        /// 开始连续保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartSave_Click(object sender, RoutedEventArgs e)
        {
            if (!specDataSave.StartSave && !MeasureMgrImpl.Instance.StartMeasure)
            {
                System.Windows.MessageBox.Show("请先开启连续测量");
                return;
            }
            int saveCount;
            int intervalTime = 0;
            bool isIntervalSave = false;
            try
            {
                saveCount = int.Parse(saveNum.Text);
                if ((bool)intervalSave.IsChecked)
                {
                    isIntervalSave = true;
                    intervalTime = int.Parse(this.intervalTime.Text) * 1000;
                }
            }
            catch
            {
                System.Windows.MessageBox.Show("参数错误！");
                return;
            }
            specDataSave.StartSave = !specDataSave.StartSave;
            if (specDataSave.StartSave)
            {
                specDataSave.saveCount = saveCount;
                specDataSave.isIntervalSave = isIntervalSave;
                specDataSave.intervalTime = intervalTime;
                startSave.Content = "停止保存";
                SingleSave.IsEnabled = false;
                ContinSave.IsEnabled = false;
                intervalSave.IsEnabled = false;
                noIntervalSave.IsEnabled = false;
                this.intervalTime.IsEnabled = false;
                saveNum.IsEnabled = false;
            }
            else
            {
                startSave.Content = "开始保存";
                SingleSave.IsEnabled = true;
                ContinSave.IsEnabled = true;
                intervalSave.IsEnabled = true;
                noIntervalSave.IsEnabled = true;
                this.intervalTime.IsEnabled = true;
                saveNum.IsEnabled = true;
                specDataSave.StartSave = false;
            } 
        }
    }
}
