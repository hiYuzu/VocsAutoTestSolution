using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using VocsAutoTest.Tools;
using VocsAutoTestBLL;
using VocsAutoTestCOMM;

namespace VocsAutoTest.Pages
{
    /// <summary>
    /// ConcentrationMeasureControlPage.xaml 的交互逻辑
    /// </summary>
    public partial class ConcentrationMeasureControlPage : Page
    {
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
        DataCompute gas1_ConcDataCompute = new DataCompute();
        DataCompute gas2_ConcDataCompute = new DataCompute();
        DataCompute gas3_ConcDataCompute = new DataCompute();
        DataCompute gas4_ConcDataCompute = new DataCompute();
        DataCompute pressDataCompute = new DataCompute();
        DataCompute tempDataCompute = new DataCompute();
        //private ArrayList listHumidity = new ArrayList();//界面要求去掉湿度
        private ArrayList listTime = new ArrayList();
        //三条曲线
        private readonly string[] GasName = new string[4] { "Gas1", "Gas2", "Gas3", "Gas4" };
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
            }
            else {
                MessageBox.Show("无法加载图形显示功能，请重启软件尝试！","错误提示",MessageBoxButton.OK,MessageBoxImage.Error);
            }
            DataForward.Instance.ReadConcMeasure += new DataForwardDelegate(GetConcMeasureData);
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
        }

        /// <summary>
        /// 结束浓度测量
        /// </summary>
        public void Stop_Measure() {
            timer.Dispose();
            DataForward.Instance.ReadConcMeasure -= new DataForwardDelegate(GetConcMeasureData);
        }

        private void GetConcMeasureData(object sender, Command command) {
            if (command != null)
            {
                try
                {
                    byte[] data = ByteStrUtil.HexToByte(command.Data);
                    List<float> concList = new List<float>();
                    byte[] pressData = new byte[4];
                    byte[] tempData = new byte[4];
                    Array.Copy(data, 7, tempData, 0, 4);
                    Array.Copy(data, 11, pressData, 0, 4);
                    byte[] concData = new byte[data.Length - 15];
                    Array.Copy(data, 15, concData, 0, data.Length - 15);
                    for (int i = 0; i < concData.Length / 10; i++)
                    {
                        byte[] conc = new byte[4];
                        Array.Copy(concData, 10 * i + 2, conc, 0, 4);
                        Array.Reverse(conc);
                        concList.Add(BitConverter.ToSingle(conc, 0));
                    }
                    Array.Reverse(pressData);
                    Array.Reverse(tempData);
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        UpadateUI(concList, BitConverter.ToSingle(pressData, 0), BitConverter.ToSingle(tempData, 0));
                    }));
                    concentrationPage.UpdateChart(concList);
                }
                catch(Exception e)
                {
                    ExceptionUtil.ExceptionMethod(e.Message, true);
                }
            }
            else 
            {
            
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
                ExceptionUtil.ExceptionMethod("导入失败：" + ex.Message, true);
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
                    ExceptionUtil.LogMethod("文件已保存：" + savePath);
                }
                if (this.listGas2Conc.Count > 0)
                {
                    FileOperate.SaveConc(savePath + GasName[1], this.listGas2Conc, this.listPress, this.listTemp, this.listTime);
                    ExceptionUtil.LogMethod("文件已保存：" + savePath);
                }
                if (this.listGas3Conc.Count > 0)
                {
                    FileOperate.SaveConc(savePath + GasName[2], this.listGas3Conc, this.listPress, this.listTemp, this.listTime);
                    ExceptionUtil.LogMethod("文件已保存：" + savePath);
                }
                if (this.listGas4Conc.Count > 0)
                {
                    FileOperate.SaveConc(savePath + GasName[3], this.listGas4Conc, this.listPress, this.listTemp, this.listTime);
                    ExceptionUtil.LogMethod("文件已保存：" + savePath);
                }
                listPress.Clear();
                listTemp.Clear();
                listTime.Clear();
            }
            catch (Exception ex)
            {
                ExceptionUtil.ExceptionMethod("保存失败：" + ex.Message, true);
            }
        }

        private void ClearCurve_Click(object sender, RoutedEventArgs e)
        {
            if (concentrationPage != null) {
                concentrationPage.ClearConcChart();
                gas1_ConcDataCompute.Reset();
                gas2_ConcDataCompute.Reset();
                gas3_ConcDataCompute.Reset();
                gas4_ConcDataCompute.Reset();
            }
        }

        private void UpadateUI(List<float> conData, float pressData, float tempData)
        {
            for (int i = 0; i < conData.Count; i++)
            {
                switch (i)
                {
                    case 0:
                        gas1_ConcDataCompute.AddData(conData[i]);
                        text_con_gas1_cur.Text = Convert.ToString(conData[i]);
                        text_con_gas1_avg.Text = Convert.ToString(gas1_ConcDataCompute.GetAvgValue());
                        text_con_gas1_max.Text = Convert.ToString(gas1_ConcDataCompute.GetMaxValue());
                        text_con_gas1_cor.Text = Convert.ToString(gas1_ConcDataCompute.GetCorValue());
                        text_con_gas1_min.Text = Convert.ToString(gas1_ConcDataCompute.GetMinValue());
                        if (checkAutoSave.IsChecked == true)
                        {
                            this.listTime.Add(DateTime.Now);
                            this.listGas1Conc.Add(conData[i]);
                        }
                        break;
                    case 1:
                        gas2_ConcDataCompute.AddData(conData[i]);
                        text_con_gas2_cur.Text = Convert.ToString(conData[i]);
                        text_con_gas2_avg.Text = Convert.ToString(gas2_ConcDataCompute.GetAvgValue());
                        text_con_gas2_max.Text = Convert.ToString(gas2_ConcDataCompute.GetMaxValue());
                        text_con_gas2_cor.Text = Convert.ToString(gas2_ConcDataCompute.GetCorValue());
                        text_con_gas2_min.Text = Convert.ToString(gas2_ConcDataCompute.GetMinValue());
                        if (checkAutoSave.IsChecked == true)
                        {
                            this.listGas2Conc.Add(conData[i]);
                        }
                        break;
                    case 2:
                        gas3_ConcDataCompute.AddData(conData[i]);
                        text_con_gas3_cur.Text = Convert.ToString(conData[i]);
                        text_con_gas3_avg.Text = Convert.ToString(gas3_ConcDataCompute.GetAvgValue());
                        text_con_gas3_max.Text = Convert.ToString(gas3_ConcDataCompute.GetMaxValue());
                        text_con_gas3_cor.Text = Convert.ToString(gas3_ConcDataCompute.GetCorValue());
                        text_con_gas3_min.Text = Convert.ToString(gas3_ConcDataCompute.GetMinValue());
                        if (checkAutoSave.IsChecked == true)
                        {
                            this.listGas3Conc.Add(conData[i]);
                        }
                        break;
                    case 3:
                        gas4_ConcDataCompute.AddData(conData[i]);
                        text_con_gas4_cur.Text = Convert.ToString(conData[i]);
                        text_con_gas4_avg.Text = Convert.ToString(gas4_ConcDataCompute.GetAvgValue());
                        text_con_gas4_max.Text = Convert.ToString(gas4_ConcDataCompute.GetMaxValue());
                        text_con_gas4_cor.Text = Convert.ToString(gas4_ConcDataCompute.GetCorValue());
                        text_con_gas4_min.Text = Convert.ToString(gas4_ConcDataCompute.GetMinValue());
                        if (checkAutoSave.IsChecked == true)
                        {
                            this.listGas4Conc.Add(conData[i]);
                        }
                        break;
                }
            }
            tempDataCompute.AddData(tempData);
            text_temp_gas1_cur.Text = text_temp_gas2_cur.Text = text_temp_gas3_cur.Text = text_temp_gas4_cur.Text = Convert.ToString(tempData);
            text_temp_gas1_avg.Text = text_temp_gas2_avg.Text = text_temp_gas3_avg.Text = text_temp_gas4_avg.Text = Convert.ToString(tempDataCompute.GetAvgValue());
            text_temp_gas1_max.Text = text_temp_gas2_max.Text = text_temp_gas3_max.Text = text_temp_gas4_max.Text = Convert.ToString(tempDataCompute.GetMaxValue());
            text_temp_gas1_min.Text = text_temp_gas2_min.Text = text_temp_gas3_min.Text = text_temp_gas4_min.Text = Convert.ToString(tempDataCompute.GetMinValue());
            pressDataCompute.AddData(pressData);
            text_press_gas1_cur.Text = text_press_gas2_cur.Text = text_press_gas3_cur.Text = text_press_gas4_cur.Text = Convert.ToString(pressData);
            text_press_gas1_avg.Text = text_press_gas2_avg.Text = text_press_gas3_avg.Text = text_press_gas4_avg.Text = Convert.ToString(pressDataCompute.GetAvgValue());
            text_press_gas1_max.Text = text_press_gas2_max.Text = text_press_gas3_max.Text = text_press_gas4_max.Text = Convert.ToString(pressDataCompute.GetMaxValue());
            text_press_gas1_min.Text = text_press_gas2_min.Text = text_press_gas3_min.Text = text_press_gas4_min.Text = Convert.ToString(pressDataCompute.GetMinValue());
            if (checkAutoSave.IsChecked == true)
            {
                this.listPress.Add(pressData);
                this.listTemp.Add(tempData);
            }
        }
    }
}
