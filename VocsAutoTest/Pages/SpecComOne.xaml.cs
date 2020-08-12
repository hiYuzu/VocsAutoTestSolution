using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using VocsAutoTest.Tools;
using VocsAutoTestBLL.Impl;
using VocsAutoTestBLL.Interface;
using C1.WPF.C1Chart;
using System.Windows.Input;

namespace VocsAutoTest
{
    /// <summary>
    /// SpecComOne.xaml 的交互逻辑
    /// </summary>
    public partial class SpecComOne : UserControl
    {
        //默认像素
        public static int pixelNumber = 512;
        //波长
        private float[] waveLength = null;
        private int lineNum = 0;
        //测量数据线
        private DataSeries currentDataSeries = null;
        private readonly ChartData chartData = new ChartData();
        public bool IsPixel { get; set; }
        public bool IsVoltage { get; set; }
        public string XAxisTitle { get; set; }
        public string YAxisTitle { get; set; }
        public bool TitleEnabled { get; set; }
        //电压积分转换系数
        public double FACTOR_VOL_TO_INTEG = (4.096 / 65536.0);
        public List<List<string>> YListCollect { get; } = new List<List<string>>();
        public string[] CurrentData { get; private set; }

        public SpecComOne()
        {
            InitializeComponent();
            SpecChart.ActionEnter += new EventHandler(Actions_Enter);
            SpecChart.ActionLeave += new EventHandler(Actions_Leave);
            SpecOperatorImpl.Instance.SpecDataEvent += new SpecDataDelegate(ImportCurrentData);
            UpdateScrollbars();
            InitParam();
        }
        /// <summary>
        /// 初始化参数
        /// </summary>
        private void InitParam()
        {
            IsPixel = true;
            IsVoltage = false;
            XAxisTitle = "像素";
            YAxisTitle = "积分值";
            SpecChart.BeginUpdate();
            SpecChart.ChartType = ChartType.Line;
            UpdateData();
            SpecChart.EndUpdate();
        }
        private void UpdateData()
        {
            SpecChart.View.AxisX.Title = XAxisTitle;
            SpecChart.View.AxisY.Title = YAxisTitle;
            SpecChart.View.AxisX.Min = 1;
            foreach (DataSeries ds in SpecChart.Data.Children)
            {
                ds.SymbolStyle = FindResource("sstyle") as Style;
                ds.ConnectionStyle = FindResource("sstyle") as Style;
                ds.PointTooltipTemplate = FindResource("lbl") as DataTemplate;
            }
        }
        void Actions_Enter(object sender, EventArgs e)
        {
            if (sender is ScaleAction)
            {
                SpecChart.Cursor = Cursors.SizeNS;
            }
            else if (sender is TranslateAction)
            {
                SpecChart.Cursor = Cursors.SizeAll;
            }
            else
            {
                SpecChart.Cursor = Cursors.Hand;
            }
        }
        void Actions_Leave(object sender, EventArgs e)
        {
            SpecChart.Cursor = Cursors.Arrow;
            UpdateScrollbars();
        }
        private void UpdateScrollbars()
        {
            double sx = SpecChart.View.AxisX.Scale;
            AxisScrollBar sbx = (AxisScrollBar)SpecChart.View.AxisX.ScrollBar;
            if (sx >= 1.0)
            {
                sbx.Visibility = Visibility.Collapsed;
            }
            else
            {
                sbx.Visibility = Visibility.Visible;
            }
            double sy = SpecChart.View.AxisY.Scale;
            AxisScrollBar sby = (AxisScrollBar)SpecChart.View.AxisY.ScrollBar;
            if (sy >= 1.0)
            {
                sby.Visibility = Visibility.Collapsed;
            }
            else
            {
                sby.Visibility = Visibility.Visible;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SpecChart.BeginUpdate();
            SpecChart.View.AxisX.Scale = 1;
            SpecChart.View.AxisX.Value = 0.5;
            SpecChart.View.AxisY.Scale = 1;
            SpecChart.View.AxisY.Value = 0.5;
            UpdateScrollbars();
            SpecChart.EndUpdate();
        }

        /// <summary>
        /// 事件响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="specData"></param>
        public void ImportCurrentData(object sender, ushort[] specData)
        {
            CurrentData = Array.ConvertAll<ushort, string>(specData, new Converter<ushort, string>(UshortToString));
            if (SpecDataSave.Instance.StartSave)
            {
                SpecDataSave.Instance.SaveSpecDataContin(CurrentData);
            }
            Dispatcher.BeginInvoke(new System.Action(() =>
            {
                CreateCurrentChart();
            }));
        }
        private string UshortToString(ushort us)
        {
            return us.ToString();
        }
        /// <summary>
        /// 绘制当前数据线
        /// </summary>
        public void CreateCurrentChart()
        {
            if(CurrentData != null && CurrentData.Length > 0)
            {
                SpecChart.BeginUpdate();
                if (currentDataSeries != null)
                {
                    SpecChart.Data.Children.Remove(currentDataSeries);
                }
                currentDataSeries = SetDataSeries(new List<string>(CurrentData), -1);
                currentDataSeries.ConnectionFill = new SolidColorBrush(Colors.Red);
                SpecChart.Data.Children.Add(currentDataSeries);
                UpdateData();
                SpecChart.EndUpdate();
            }
        }
        /// <summary>
        /// 设置波长
        /// </summary>
        /// <param name="index">传感器类型</param>
        /// <param name="pixels">象素数</param>
        /// <param name="wavepara">第一参数</param>
        public void SetWave(int index, int pixels, float wavepara)
        {
            pixelNumber = pixels;
            if (pixelNumber == 2048)
            {
                FACTOR_VOL_TO_INTEG = 2.5 / 65536.0;
            }
            else
            {
                FACTOR_VOL_TO_INTEG = 4.096 / 65536.0;
            }
            waveLength = new float[pixels];
            for (int i = 0; i < pixels; i++)
            {
                switch (index)
                {
                    case 0://2048
                        waveLength[i] = (float)(wavepara + 0.1792 * i - 2.72E-05 * i * i + 2.25E-09 * i * i * i);
                        break;
                    case 1://1024
                        waveLength[i] = (float)(wavepara + 0.28 * i - 2.25E-5 * i * i - 2E-9 * i * i * i);

                        break;
                    case 2://长512
                        waveLength[i] = (float)(wavepara + 0.56 * i - 9E-5 * i * i + 1.6E-8 * i * i * i);

                        break;
                    case 3://短512
                        waveLength[i] = (float)(wavepara + 0.28 * i - 2.25E-5 * i * i - 2E-9 * i * i * i);
                        break;
                    case 4://256
                        break;
                }
            }
        }
        /// <summary>
        /// 导入历史数据
        /// </summary>
        /// <param name="fileName"></param>
        public void ImportHistoricalData(string fileName)
        {
            FileInfo file = new FileInfo(fileName);
            TextReader textReader = file.OpenText();
            string line;
            List<string[]> vocsCollectData = new List<string[]>();
            while ((line = textReader.ReadLine()) != null)
            {
                string[] lineData = ParseLine(line);
                List<string> temp = new List<string>(lineData);
                temp.RemoveRange(0, 1);
                lineData = temp.ToArray();
                vocsCollectData.Add(lineData);
            }
            ParseVocsCollectData(vocsCollectData);
        }
        /// <summary>
        /// 将字符串解析为字符数组
        /// 按'\t'解析：
        /// "a    b    c" => {"a","b","c"}
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private string[] ParseLine(string line)
        {
            if (line == null)
                return new string[0];
            ArrayList list = new ArrayList();
            line = line.Trim();
            while (line.Length > 0)
            {
                int index = line.IndexOf('\t');
                if (index > 0)
                {
                    list.Add(line.Substring(0, index).Trim());
                    line = line.Substring(index + 1).Trim();
                }
                else
                {
                    list.Add(line);
                    break;
                }
            }

            string[] returnArray = new string[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                returnArray[i] = (string)list[i];
            }
            return returnArray;
        }
        /// <summary>
        /// 解析光谱数据
        /// </summary>
        /// <param name="vocsCollectData"></param>
        private void ParseVocsCollectData(List<string[]> vocsCollectData)
        {
            if (vocsCollectData.Count > 0)
            {
                int lineNum = vocsCollectData[0].Length;
                //y轴，[数据线数量][每条线数据量]
                for (int i = 0; i < lineNum; i++)
                {
                    YListCollect.Add(new List<string>());
                }
                for (int i = 0; i < vocsCollectData.Count; i++)
                {
                    for (int j = this.lineNum, k = 0; j < this.lineNum + lineNum; j++,k++)
                    {
                        YListCollect[j].Add(vocsCollectData[i][k]);
                    }
                }
                this.lineNum += lineNum;
                CreateHistoricalChart();
            }
        }
        /// <summary>
        /// 绘制历史数据线
        /// </summary>
        public void CreateHistoricalChart()
        {
            if (lineNum != 0)
            {
                SpecChart.BeginUpdate();
                SpecChart.Data.Children.Clear();
                if (currentDataSeries != null)
                {
                    SpecChart.Data.Children.Add(currentDataSeries);
                }
                for (int i = 0; i < lineNum; i++)
                {
                    SpecChart.Data.Children.Add(SetDataSeries(YListCollect[i], i));
                }
                UpdateData();
                SpecChart.EndUpdate();
            }
        }
        /// <summary>
        /// 设置并返回数据线
        /// </summary>
        /// <param name="i">lineNum</param>
        /// <returns>数据线</returns>
        private DataSeries SetDataSeries(List<string> yListCollect, int i)
        {
            XYDataSeries dataSeries = new XYDataSeries
            {
                Label = "MES",
                ConnectionStrokeThickness = 1
            };
            if (i > -1)
            {
                dataSeries.Label = "IMP_" + i;
            }
            double[] valueY = new double[pixelNumber];
            double[] valueX = new double[pixelNumber];
            //像素-电压
            if (IsPixel && IsVoltage)
            {
                for (int j = 0; j < yListCollect.Count; j++)
                {
                    valueX[j] = j + 1;
                    valueY[j] = double.Parse(yListCollect[j]) * FACTOR_VOL_TO_INTEG;
                };
                dataSeries.XValuesSource = valueX;
                dataSeries.ValuesSource = valueY;
            }
            //像素-积分
            else if (IsPixel && !IsVoltage)
            {
                for (int j = 0; j < yListCollect.Count; j++)
                {
                    valueX[j] = j + 1;
                    valueY[j] = double.Parse(yListCollect[j]);
                };
                dataSeries.XValuesSource = valueX;
                dataSeries.ValuesSource = valueY;
            }
            //波长-电压
            else if (!IsPixel && IsVoltage)
            {
                for (int j = 0; j < yListCollect.Count; j++)
                {
                    valueX[j] = GetWaveByPixel(j + 1);
                    valueY[j] = double.Parse(yListCollect[j]) * FACTOR_VOL_TO_INTEG;
                };
                dataSeries.XValuesSource = valueX;
                dataSeries.ValuesSource = valueY;
            }
            //波长-积分
            else if (!IsPixel && !IsVoltage)
            {
                for (int j = 0; j < yListCollect.Count; j++)
                {
                    valueX[j] = GetWaveByPixel(j + 1);
                    valueY[j] = double.Parse(yListCollect[j]);
                };
                dataSeries.XValuesSource = valueX;
                dataSeries.ValuesSource = valueY;
            }
            return dataSeries;
        }
        /// <summary>
        /// 是否显示图像标题/标签
        /// </summary>
        /// <param name="isShow"></param>
        public void IsShow(int isShow)
        {
            if (title != null)
            {
                switch (isShow)
                {
                    case 0:
                        title.Visibility = Visibility.Hidden;
                        break;
                    case 1:
                        title.Visibility = Visibility.Visible;
                        break;
                    case 2:
                        c1legend.Visibility = Visibility.Collapsed;
                        break;
                    case 3:
                        c1legend.Visibility = Visibility.Visible;
                        break;
                    default:
                        break;
                }
            }
        }
        /// <summary>
        /// 得到像素点对应的波长
        /// </summary>
        /// <param name="pixel">像素</param>
        /// <returns></returns>
        private float GetWaveByPixel(int pixel)
        {
            if (waveLength == null)
            {
                MessageBox.Show("x轴数据无法转换为波长！");
            }
            if (pixel >= waveLength.Length)
            {
                return float.NaN;
            }
            return waveLength[pixel];
        }
        /// <summary>
        /// 清除当前测量数据线
        /// </summary>
        public void ClearCurrentSeries()
        {
            SpecChart.BeginUpdate();
            SpecChart.Data.Children.Remove(currentDataSeries);
            SpecChart.EndUpdate();
            CurrentData = null;
            currentDataSeries = null;
        }
        /// <summary>
        /// 清除历史数据线
        /// </summary>
        public void ClearHistoricalSeries()
        {
            lineNum = 0;
            YListCollect.Clear();
            SpecChart.BeginUpdate();
            SpecChart.Data.Children.Clear();
            if (currentDataSeries != null)
            {
                SpecChart.Data.Children.Add(currentDataSeries);
                UpdateData();
            }
            SpecChart.EndUpdate();
        }
        /// <summary>
        /// 清除全部曲线
        /// </summary>
        public void ClearAllSeries()
        {
            CurrentData = null;
            currentDataSeries = null;
            lineNum = 0;
            YListCollect.Clear();
            SpecChart.BeginUpdate();
            SpecChart.Data.Children.Clear();
            SpecChart.EndUpdate();
        }
        /// <summary>
        /// 标签左键隐藏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Label_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            LegendItem cli = (sender as Label).DataContext as LegendItem;
            if (cli != null)
            {
                XYDataSeries ds = cli.Item as XYDataSeries;
                if(ds != null)
                {
                    if(ds.Visibility != Visibility.Hidden)
                    {
                        ds.Visibility = Visibility.Hidden;
                        ds.SymbolFill = new SolidColorBrush(Colors.Gray);
                    }
                    else
                    {
                        ds.Visibility = Visibility.Visible;
                        ds.SymbolFill = ds.ConnectionFill;
                    }
                }
            }
        }
    }
}
