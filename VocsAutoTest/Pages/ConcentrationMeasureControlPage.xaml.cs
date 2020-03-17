using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using VocsAutoTest.Tools;
using VocsAutoTestBLL;
using VocsAutoTestBLL.Impl;
using VocsAutoTestBLL.Interface;
using VocsAutoTestBLL.Model;
using VocsAutoTestCOMM;

namespace VocsAutoTest.Pages
{
    /// <summary>
    /// ConcentrationMeasureControlPage.xaml 的交互逻辑
    /// </summary>
    public partial class ConcentrationMeasureControlPage : Page
    {
        private ISpecMeasure iSpecMeasure = SpecMeasureImpl.GetInstance();
        private ConcentrationMeasurePage concentrationPage;
        //声明时间控件
        private Timer timer;
        private delegate void UpdateTimer();
        private bool timerFlag = true;
        //气体浓度数据保存列表
        private ArrayList listGas1Conc = new ArrayList();
        private ArrayList listGas2Conc = new ArrayList();
        private ArrayList listGas3Conc = new ArrayList();
        private ArrayList listGas4Conc = new ArrayList();
        private ArrayList listTemp = new ArrayList();
        private ArrayList listPress = new ArrayList();
        DataCompute concDataCompute = new DataCompute();
        DataCompute pressDataCompute = new DataCompute();
        DataCompute tempDataCompute = new DataCompute();
        //private ArrayList listHumidity = new ArrayList();//界面要求去掉湿度
        private ArrayList listTime = new ArrayList();
        private float[] curConc = null;
        //三条曲线
        private string[] GasName = new string[4] { "Gas1", "Gas2", "Gas3", "Gas4" };
        //文件保存路径
        private string savePath = string.Empty;
        private string saveName = "ConcFiles";
        private string importPath = string.Empty;
        public ConcentrationMeasureControlPage()
        {
            InitializeComponent();
            Init_Load();
        }

        public ConcentrationMeasureControlPage(ConcentrationMeasurePage concentrationPage) {
            InitializeComponent();
            Init_Load();
            if (concentrationPage != null)
            {
                this.concentrationPage = concentrationPage;
                this.concentrationPage.initPage();
            }
            else {
                MessageBox.Show("无法加载图形显示功能，请重启软件尝试！","错误提示",MessageBoxButton.OK,MessageBoxImage.Error);
            }
         
        }

        private void Init_Load() {
            string path = ConstConfig.GetValue(saveName);
            if (string.IsNullOrEmpty(path))
            {
                path = ConstConfig.AppPath + @"\ConcFiles";
            }
            textSelectSaveUrl.Text = savePath = importPath = path;
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
        }

        /// <summary>
        /// 开始浓度测量
        /// </summary>
        public void Start_Measure()
        {
            timer = new Timer(new TimerCallback(TimerDelegate));
            int period = int.Parse(this.textSaveInterval.Text);
            timer.Change(0, period * 60 * 1000);
            DataForward.Instance.ReadConcMeasure += new DataForwardDelegate(GetSpecMeasureData);
        }

        /// <summary>
        /// 结束浓度测量
        /// </summary>
        public void Stop_Measure() {
            timer.Dispose();
            DataForward.Instance.ReadConcMeasure -= new DataForwardDelegate(GetSpecMeasureData);
        }

        private void GetSpecMeasureData(object sender, Command command) {
            if (command != null)
            {
                if (command.Data.Length > 0)
                {
                    byte[] data = ByteStrUtil.HexToByte(command.Data);
                    float[] conData = null;
                    float[] pressData = null;
                    float[] tempData = null;
                    if (conData.Length>=6)
                    {
                        Array.Copy(data, 2, conData, 0, 4);
                    }
                    if (conData.Length >= 10)
                    {
                        Array.Copy(data, 6, pressData, 0, 4);
                    }
                    if (conData.Length >= 14)
                    {
                        Array.Copy(data, 8, tempData, 0, 4);
                    }
                    UpadateUI(conData, pressData, tempData);
                }
                else {
                }
            }
            else { 
            
            }
        }

        void TimerDelegate(object state)
        {
            this.Dispatcher.BeginInvoke(new UpdateTimer(TimerEventFunc));
        }

        void TimerEventFunc()
        {
            if (timerFlag)
            {
                SaveConc();
            }
        }

        private void BtnSelectSave_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.InitialDirectory = "C:\\";//默认的打开路径
            if (savePath != null) {
                op.InitialDirectory = savePath;
            }
            op.RestoreDirectory = true;
            op.Filter = " 文本文件(*.txt)|*.txt|所有文件(*.*)|*.* ";
            if (op.ShowDialog() == true)
            {
                textSelectSaveUrl.Text = op.FileName;
                savePath = op.FileName.Substring(0, op.FileName.LastIndexOf('\\'));
            }
            
        }

        private void ImportConcentration_Click(object sender, RoutedEventArgs e)
        {
            try {
                OpenFileDialog op = new OpenFileDialog();
                op.InitialDirectory = "C:\\";//默认的打开路径
                if (importPath != null)
                {
                    op.InitialDirectory = importPath;
                }
                op.RestoreDirectory = true;
                op.Filter = " 文本文件(*.txt)|*.txt|所有文件(*.*)|*.* ";
                if (op.ShowDialog() == true)
                {
                    importPath = op.FileName.Substring(0, op.FileName.LastIndexOf('\\'));
                    MessageBox.Show(op.FileName, "选择文件", MessageBoxButton.OK, MessageBoxImage.Information);
                    string filenames = op.FileName;
                    int piexNumber = 512;//Global.ConnectInst.Pixels
                    float[] xVer = new float[piexNumber];
                    for (int i = 0; i < piexNumber; i++)
                    {
                        xVer[i] = i + 1;
                    }
                    if (File.Exists(filenames))//for (int files = 0; files < filenames.Length; files++)
                    {
                        int number = FileOperate.GetCurveNumber(filenames);

                        bool isInteg = true;
                        float[][] spec = FileOperate.GetSpecData(filenames, piexNumber, number, ref isInteg);
                        for (int i = 0; i < spec.Length; i++)
                        {
                            //DataCacheControl.GetInstance().SetValue(spec[i], null, null, null, null, null);
                        }
                    }
                }
            }
            catch (Exception ex) { 
                
            }

        }

        private void CheckAutoSave_Click(object sender, RoutedEventArgs e)
        {
            if (checkAutoSave.IsChecked == true)
            {
                textSaveInterval.IsEnabled = true;
                int period = int.Parse(this.textSaveInterval.Text);
                timer.Change(0, period*1000*60);
                timerFlag = true;

            }
            else
            {
                textSaveInterval.IsEnabled = false;
                timerFlag = false;
            }
        }

        private void AlgorithmSetting_Click(object sender, RoutedEventArgs e)
        {
            AlgorithmSettingWindow algorithmSettingWindow = new AlgorithmSettingWindow()
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            algorithmSettingWindow.Show();
        }

        private void ManualSave_Click(object sender, RoutedEventArgs e)
        {
            SaveConc();
        }

        private void SaveConc()
        {
            try
            {
                if (!savePath.EndsWith(@"\"))
                {
                    savePath = savePath + @"\";
                }

                if (this.listGas1Conc.Count > 0)
                {
                    FileOperate.SaveConc(savePath + GasName[0], this.listGas1Conc, this.listPress, this.listTemp, this.listTime);
                }
                if (this.listGas2Conc.Count > 0)
                {
                    FileOperate.SaveConc(savePath + GasName[1], this.listGas2Conc, this.listPress, this.listTemp, this.listTime);
                }
                if (this.listGas3Conc.Count > 0)
                {
                    FileOperate.SaveConc(savePath + GasName[2], this.listGas3Conc, this.listPress, this.listTemp, this.listTime);
                }
                if (this.listGas4Conc.Count > 0)
                {
                    FileOperate.SaveConc(savePath + GasName[3], this.listGas4Conc, this.listPress, this.listTemp, this.listTime);
                }
                listPress.Clear();
                listTemp.Clear();
                listTime.Clear();
                //listHumidity.Clear();
            }
            catch (Exception ex)
            {
                //Log.LogUtil.ShowError(CustomResource.SaveConcErr + ex.Message);
            }
        }

        private void ClearCurve_Click(object sender, RoutedEventArgs e)
        {
            if (this.concentrationPage != null) {
                this.concentrationPage.MeasureChart.Children.Clear();
            }
        }

        private void UpadateUI(float[] conData, float[] pressData, float[] tempData)
        {
            if (conData != null)
            {
                curConc = conData;
                for (int i = 0; i < 4; i++)
                {
                    //concGraph.AddLinePoint(GasName[i] + CustomResource.ConcCuve, DateTime.Now, conData[i], GetColor(i));
                    switch (i)
                    {
                        case 0:
                            concDataCompute.AddData(conData[i]);
                            text_con_gas1_cur.Text = Convert.ToString(conData[i]);
                            text_con_gas1_avg.Text = Convert.ToString(concDataCompute.GetAvgValue());
                            text_con_gas1_max.Text = Convert.ToString(concDataCompute.GetMaxValue());
                            text_con_gas1_cor.Text = Convert.ToString(concDataCompute.GetCorValue());
                            text_con_gas1_min.Text = Convert.ToString(concDataCompute.GetMinValue());
                            if (checkAutoSave.IsChecked == true)
                            {
                                this.listTime.Add(DateTime.Now);
                                this.listGas1Conc.Add(conData[i]);
                            }
                            break;
                        case 1:
                            concDataCompute.AddData(conData[i]);
                            text_con_gas2_cur.Text = Convert.ToString(conData[i]);
                            text_con_gas2_avg.Text = Convert.ToString(concDataCompute.GetAvgValue());
                            text_con_gas2_max.Text = Convert.ToString(concDataCompute.GetMaxValue());
                            text_con_gas2_cor.Text = Convert.ToString(concDataCompute.GetCorValue());
                            text_con_gas2_min.Text = Convert.ToString(concDataCompute.GetMinValue());
                            if (checkAutoSave.IsChecked == true)
                            {
                                this.listGas2Conc.Add(conData[i]);
                            }
                            break;
                        case 2:
                            concDataCompute.AddData(conData[i]);
                            text_con_gas3_cur.Text = Convert.ToString(conData[i]);
                            text_con_gas3_avg.Text = Convert.ToString(concDataCompute.GetAvgValue());
                            text_con_gas3_max.Text = Convert.ToString(concDataCompute.GetMaxValue());
                            text_con_gas3_cor.Text = Convert.ToString(concDataCompute.GetCorValue());
                            text_con_gas3_min.Text = Convert.ToString(concDataCompute.GetMinValue());
                            if (checkAutoSave.IsChecked == true)
                            {
                                this.listGas3Conc.Add(conData[i]);
                            }
                            break;
                        case 3:
                            concDataCompute.AddData(conData[i]);
                            text_con_gas4_cur.Text = Convert.ToString(conData[i]);
                            text_con_gas4_avg.Text = Convert.ToString(concDataCompute.GetAvgValue());
                            text_con_gas4_max.Text = Convert.ToString(concDataCompute.GetMaxValue());
                            text_con_gas4_cor.Text = Convert.ToString(concDataCompute.GetCorValue());
                            text_con_gas4_min.Text = Convert.ToString(concDataCompute.GetMinValue());
                            if (checkAutoSave.IsChecked == true)
                            {
                                this.listGas4Conc.Add(conData[i]);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            bool graphRefresh = false;
            if (tempData != null)
            {
                graphRefresh = true;
                //tempGraph.AddLinePoint(CustomResource.TempCuve, DateTime.Now, tempData[0], tempColor);
                for (int i = 0; i < 4; i++)
                {
                    switch (i)
                    {
                        case 0:
                            tempDataCompute.AddData(tempData[i]);
                            text_temp_gas1_cur.Text = Convert.ToString(tempData[i]);
                            text_temp_gas1_avg.Text = Convert.ToString(tempDataCompute.GetAvgValue());
                            text_temp_gas1_max.Text = Convert.ToString(tempDataCompute.GetMaxValue());
                            text_temp_gas1_min.Text = Convert.ToString(tempDataCompute.GetMinValue());
                            break;
                        case 1:
                            tempDataCompute.AddData(tempData[i]);
                            text_temp_gas2_cur.Text = Convert.ToString(tempData[i]);
                            text_temp_gas2_avg.Text = Convert.ToString(tempDataCompute.GetAvgValue());
                            text_temp_gas2_max.Text = Convert.ToString(tempDataCompute.GetMaxValue());
                            text_temp_gas2_min.Text = Convert.ToString(tempDataCompute.GetMinValue());
                            break;
                        case 2:
                            tempDataCompute.AddData(tempData[i]);
                            text_temp_gas3_cur.Text = Convert.ToString(tempData[i]);
                            text_temp_gas3_avg.Text = Convert.ToString(tempDataCompute.GetAvgValue());
                            text_temp_gas3_max.Text = Convert.ToString(tempDataCompute.GetMaxValue());
                            text_temp_gas3_min.Text = Convert.ToString(tempDataCompute.GetMinValue());
                            break;
                        case 3:
                            tempDataCompute.AddData(tempData[i]);
                            text_temp_gas4_cur.Text = Convert.ToString(tempData[i]);
                            text_temp_gas4_avg.Text = Convert.ToString(tempDataCompute.GetAvgValue());
                            text_temp_gas4_max.Text = Convert.ToString(tempDataCompute.GetMaxValue());
                            text_temp_gas4_min.Text = Convert.ToString(tempDataCompute.GetMinValue());
                            break;
                        default:
                            break;
                    }
                }
                if (checkAutoSave.IsChecked == true)
                {
                    this.listTemp.Add(tempData[0]);
                }
            }

            if (pressData != null)
            {
                graphRefresh = true;
                //tempGraph.AddLinePoint(CustomResource.PressCuve, DateTime.Now, pressData[0], pressColor);
                for (int i = 0; i < 4; i++)
                {
                    switch (i)
                    {
                        case 0:
                            pressDataCompute.AddData(tempData[i]);
                            text_press_gas1_cur.Text = Convert.ToString(pressData[i]);
                            text_press_gas1_avg.Text = Convert.ToString(pressDataCompute.GetAvgValue());
                            text_press_gas1_max.Text = Convert.ToString(pressDataCompute.GetMaxValue());
                            text_press_gas1_min.Text = Convert.ToString(pressDataCompute.GetMinValue());
                            break;
                        case 1:
                            pressDataCompute.AddData(tempData[i]);
                            text_press_gas2_cur.Text = Convert.ToString(pressData[i]);
                            text_press_gas2_avg.Text = Convert.ToString(pressDataCompute.GetAvgValue());
                            text_press_gas2_max.Text = Convert.ToString(pressDataCompute.GetMaxValue());
                            text_press_gas2_min.Text = Convert.ToString(pressDataCompute.GetMinValue());
                            break;
                        case 2:
                            pressDataCompute.AddData(tempData[i]);
                            text_press_gas3_cur.Text = Convert.ToString(pressData[i]);
                            text_press_gas3_avg.Text = Convert.ToString(pressDataCompute.GetAvgValue());
                            text_press_gas3_max.Text = Convert.ToString(pressDataCompute.GetMaxValue());
                            text_press_gas3_min.Text = Convert.ToString(pressDataCompute.GetMinValue());
                            break;
                        case 3:
                            pressDataCompute.AddData(tempData[i]);
                            text_press_gas4_cur.Text = Convert.ToString(pressData[i]);
                            text_press_gas4_avg.Text = Convert.ToString(pressDataCompute.GetAvgValue());
                            text_press_gas4_max.Text = Convert.ToString(pressDataCompute.GetMaxValue());
                            text_press_gas4_min.Text = Convert.ToString(pressDataCompute.GetMinValue());
                            break;
                        default:
                            break;
                    }
                }
                if (checkAutoSave.IsChecked == true)
                {
                    this.listPress.Add(pressData[0]);
                }
            }
            //if (graphRefresh)
            //{
            //    uC_ZedGraphTemp.Refresh();
            //}

        }

    }
}
