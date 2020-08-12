using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using VocsAutoTest.Algorithm;
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
        //private readonly SpecMeasurePage specPage;
        private readonly SpecComOne specPage;
        private readonly SpecDataSave specDataSave;
        private List<string> historyData = null;
        public SpecMeasureControlPage(SpecComOne page)
        {
            InitializeComponent();
            if (page != null)
            {
                this.specPage = page;
            }
            specDataSave = SpecDataSave.Instance;
            specPage.SetWave(3, pixelNumber, 185);
            FolderPath.Text = specDataSave.SpecDataSavePath;
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
            System.Windows.Forms.FolderBrowserDialog openFolderDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (openFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
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
            if (specPage != null)
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
            if (specPage != null)
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
            specPage.IsPixel = true;
            specPage.XAxisTitle = "像素";
            if (!ChangeAxis())
            {
                specPage.IsPixel = false;
                specPage.XAxisTitle = "波长";
                wavelength.IsChecked = true;
            }
        }
        private void Pixel_Unchecked(object sender, RoutedEventArgs e)
        {
            InitTitleAndTag();
            //波长，像素=>波长
            specPage.IsPixel = false;
            specPage.XAxisTitle = "波长";
            if (!ChangeAxis())
            {
                specPage.IsPixel = true;
                specPage.XAxisTitle = "像素";
                pixel.IsChecked = true;
            }
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
            if (!ChangeAxis())
            {
                specPage.YAxisTitle = "积分值";
                specPage.IsVoltage = false;
                integralValue.IsChecked = true;
            }
        }
        private void VoltageValue_Unchecked(object sender, RoutedEventArgs e)
        {
            InitTitleAndTag();
            //积分值，电压=>积分
            specPage.YAxisTitle = "积分值";
            specPage.IsVoltage = false;
            if (!ChangeAxis())
            {
                specPage.YAxisTitle = "电压值(V)";
                specPage.IsVoltage = true;
                voltageValue.IsChecked = true;
            }
        }
        private bool ChangeAxis()
        {
            try
            {
                specPage.CreateCurrentChart();
                specPage.CreateHistoricalChart();
                return true;
            }
            catch
            {
                MessageBox.Show("无图表数据");
                return false;
            }
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
            try
            {
                ExceptionUtil.Instance.ShowLoadingAction(true);
                OpenFileDialog op = new OpenFileDialog();
                if (FolderPath.Text != null && !"".Equals(FolderPath.Text))
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
                    ModifyImportCurveBox();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("导入历史数据失败");
                ExceptionUtil.Instance.ExceptionMethod(ex.Message, true);
            }
            finally
            {
                ExceptionUtil.Instance.ShowLoadingAction(false);
            }
        }
        /// <summary>
        /// 修改“选择导入曲线”下拉框数据
        /// </summary>
        private void ModifyImportCurveBox()
        {
            importCurve.Items.Clear();
            for (int i = 0; i < specPage.YListCollect.Count; i++)
            {
                ComboBoxItem item = new ComboBoxItem
                {
                    Content = "IMP_" + i
                };
                importCurve.Items.Add(item);
            }
        }
        /// <summary>
        /// 所选历史数据发生变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImportCurve_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(specPage.YListCollect.Count > 0)
            {
                historyData = specPage.YListCollect[importCurve.SelectedIndex];
            }
            else
            {
                historyData = null;
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
                ExceptionUtil.Instance.LogMethod("传感器类型：" + sensorType.SelectedItem.ToString() + "\t第一参量：" + fstParam.ToString());
            }
            catch(Exception ex)
            {
                MessageBox.Show("设置失败");
                ExceptionUtil.Instance.ExceptionMethod("设置波长操作失败：" + ex.Message, false);
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
                int xValue = int.Parse(xPixel.Text);
                MESY.Text = "";
                IMPY.Text = "";
                if (specPage.CurrentData != null && specPage.CurrentData.Length >= xValue)
                {
                    string value = string.Empty;
                    if(specPage.IsVoltage)
                    {
                        value = (Convert.ToDouble(specPage.CurrentData[xValue - 1]) * specPage.FACTOR_VOL_TO_INTEG).ToString();
                    }
                    else
                    {
                        value = (Convert.ToDouble(specPage.CurrentData[xValue - 1])).ToString();
                    }
                    MESY.Text = value;
                }
                if(historyData != null && historyData.Count > 0)
                {
                    string value = string.Empty;
                    if (specPage.IsVoltage)
                    {
                        value = (Convert.ToDouble(historyData[xValue - 1]) * specPage.FACTOR_VOL_TO_INTEG).ToString();
                    }
                    else
                    {
                        value = (Convert.ToDouble(historyData[xValue - 1])).ToString();
                    }
                    IMPY.Text = value;
                }
            }
            catch
            {
                MessageBox.Show("参数错误！");
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
            historyData = null;
            specPage.ClearAllSeries();
            importCurve.Items.Clear();
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
            historyData = null;
            specPage.ClearHistoricalSeries();
            importCurve.Items.Clear();
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
            StringBuilder sb = new StringBuilder();
            if(specPage.YListCollect.Count > 0)
            {
                foreach (List<string> data in specPage.YListCollect)
                {
                    string[] datas = data.ToArray();
                    double[] doubledata = new double[datas.Length];
                    for (int i = 0; i < datas.Length; i++)
                    {
                        doubledata[i] = Convert.ToDouble(data[i]);
                    }

                    sb.Append(CalPos(doubledata).ToString()).Append("\r\n");
                }
                MessageBox.Show(sb.ToString());
            }
            else
            {
                MessageBox.Show("当前无历史数据！");
            }
        }
        private float CalPos(double[] specData)
        {
            double[] newdata = OMAAlgorithm.SplineFunc(specData);
            double[] movedata = OMAAlgorithm.AjustMove(newdata, -3);
            double[] finaldata = new double[512];
            for (int i = 0; i < finaldata.Length; i++)
            {
                finaldata[i] = movedata[i * 4];
            }
            int index = (int)((160 - 1) * 4);
            int indexstart = index - 10 * 4;
            if (indexstart < 0) indexstart = 0;
            int indexend = index + 10 * 4;
            if (indexend >= newdata.Length) indexend = newdata.Length - 1;
            double maxvalue = double.MinValue;
            int maxIndex = index;
            for (int i = indexstart; i <= indexend; i++)
            {
                if (newdata[i] > maxvalue)
                {
                    maxIndex = i;
                    maxvalue = newdata[i];
                }
            }
            return maxIndex / 4.0f + 1;
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
                MessageBox.Show("没有可以保存的数据");
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
                MessageBox.Show("请先开启连续测量");
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
                MessageBox.Show("参数错误！");
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
