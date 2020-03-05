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
using VocsAutoTestBLL.Impl;
using VocsAutoTestBLL.Interface;

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
        private System.Threading.Timer timer;
        private delegate void UpdateTimer();
        private bool timerFlag = true;
        //气体浓度数据保存列表
        private ArrayList listGas1Conc = new ArrayList();
        private ArrayList listGas2Conc = new ArrayList();
        private ArrayList listGas3Conc = new ArrayList();
        private ArrayList listGas4Conc = new ArrayList();
        private ArrayList listTemp = new ArrayList();
        private ArrayList listPress = new ArrayList();
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
            iSpecMeasure.QuerySpecMeasureCompleted += new QuerySpecMeasureDelegate(GetSpecMeasureData);
        }

        /// <summary>
        /// 结束浓度测量
        /// </summary>
        public void Stop_Measure() {
            timer.Dispose();
            iSpecMeasure.QuerySpecMeasureCompleted -= new QuerySpecMeasureDelegate(GetSpecMeasureData);
        }

        private void GetSpecMeasureData(object sender, QuerySpecMeasureEventArgs e) {
            if (e.argSpecMeasureState.Result)
            {
                if (e.argSpecMeasureState.ResultsT.Count > 0)
                {
                    float[] conData = new float[4] { 1,2,3,4};
                    float[] pressData = new float[4] { 1, 2, 3, 4 };
                    float[] tempData = new float[4] { 1, 2, 3, 4 };
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
            if (conData != null)// && !ConcCompute.GetInstance().compute
            {
                curConc = conData;
                for (int i = 0; i < 4; i++)
                {
                    //concGraph.AddLinePoint(GasName[i] + CustomResource.ConcCuve, DateTime.Now, conData[i], GetColor(i));
                    switch (i)
                    {
                        case 0:
                            text_con_gas1.Text = Convert.ToString(conData[i]);
                            if (checkAutoSave.IsChecked == true)
                            {
                                this.listTime.Add(DateTime.Now);
                                this.listGas1Conc.Add(conData[i]);
                            }
                            break;
                        case 1:
                            text_con_gas2.Text = Convert.ToString(conData[i]);
                            if (checkAutoSave.IsChecked == true)
                            {
                                this.listGas2Conc.Add(conData[i]);
                            }
                            break;
                        case 2:
                            text_con_gas3.Text = Convert.ToString(conData[i]);
                            if (checkAutoSave.IsChecked == true)
                            {
                                this.listGas3Conc.Add(conData[i]);
                            }
                            break;
                        case 3:
                            text_con_gas4.Text = Convert.ToString(conData[i]);
                            if (checkAutoSave.IsChecked == true)
                            {
                                this.listGas4Conc.Add(conData[i]);
                            }
                            break;
                        default:
                            break;
                    }
                }
                //uC_ZedGraphConc.Refresh();
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
                            text_temp_gas1.Text = Convert.ToString(tempData[i]);
                            break;
                        case 1:
                            text_temp_gas2.Text = Convert.ToString(tempData[i]);
                            break;
                        case 2:
                            text_temp_gas3.Text = Convert.ToString(tempData[i]);
                            break;
                        case 3:
                            text_temp_gas4.Text = Convert.ToString(tempData[i]);
                            break;
                        default:
                            break;
                    }
                }
                if (checkAutoSave.IsChecked == true)
                {
                    this.listTemp.Add(tempData[0]);
                }
                //uC_ZedGraphTemp.Refresh();
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
                            text_press_gas1.Text = Convert.ToString(pressData[i]);
                            break;
                        case 1:
                            text_press_gas2.Text = Convert.ToString(pressData[i]);
                            break;
                        case 2:
                            text_press_gas3.Text = Convert.ToString(pressData[i]);
                            break;
                        case 3:
                            text_press_gas4.Text = Convert.ToString(pressData[i]);
                            break;
                        default:
                            break;
                    }
                }
                if (checkAutoSave.IsChecked == true)
                {
                    this.listPress.Add(pressData[0]);
                }
                //uC_ZedGraphTemp.Refresh();
            }
            //if (graphRefresh)
            //{
            //    uC_ZedGraphTemp.Refresh();
            //}

        }

    }
}
