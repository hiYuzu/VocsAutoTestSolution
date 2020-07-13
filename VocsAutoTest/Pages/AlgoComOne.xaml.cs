using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections;

using C1.WPF.C1Chart;

namespace VocsAutoTest.Pages
{
    class ColorRandom
    {
        public static Color ColorSelect()
        {
            long tick = DateTime.Now.Ticks;
            Random random = new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));

            int R = random.Next(255);
            int G = random.Next(255);
            int B = random.Next(255);
            B = (R + G > 400) ? R + G - 400 : B;
            B = (B > 255) ? 255 : B;
            return Color.FromRgb((byte)R, (byte)G, (byte)B);
        }
    }
    /// <summary>
    /// AlgoComOne.xaml 的交互逻辑
    /// </summary>
    public partial class AlgoComOne : UserControl
    {
        private const string HEAD_SPEC = "SPEC";
        private List<int> xList;
        private List<List<string>> yListCollect;
        private int lineNum = 0;
        //光谱数据
        private Dictionary<string, XYDataSeries> dataSeriesMap = new Dictionary<string, XYDataSeries>();
        public const string XAxisTitle = "像素";
        public const string YAxisTitle = "积分值";
        private DataSeries currentDataSeries = new DataSeries();
        private bool hadCurrent = false;

        public AlgoComOne()
        {
            InitializeComponent();
            AlgoChart.ActionEnter += new EventHandler(Actions_Enter);
            AlgoChart.ActionLeave += new EventHandler(Actions_Leave);
            UpdateScrollbars();
            InitParam();
        }
        /// <summary>
        /// 初始化参数
        /// </summary>
        private void InitParam()
        {
            AlgoChart.BeginUpdate();
            AlgoChart.ChartType = ChartType.Line;
            AlgoChart.View.AxisX.Title = XAxisTitle;
            AlgoChart.View.AxisY.Title = YAxisTitle;
            AlgoChart.View.AxisX.Min = 1;
            UpdateData();
            AlgoChart.EndUpdate();
        }
        private void UpdateData()
        {
            foreach (DataSeries ds in AlgoChart.Data.Children)
            {
                ds.SymbolStyle = FindResource("sstyle") as Style;
                ds.ConnectionStyle = FindResource("sstyle") as Style;
                ds.PointTooltipTemplate = FindResource("lbl") as DataTemplate;
            }
        }

        #region 导入曲线相关方法
        /// <summary>
        /// 导入历史数据
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="riDataMap"></param>
        public void ImportHistoricalData(string fileName, out Dictionary<int, float[]> riDataMap)
        {
            FileInfo file = new FileInfo(fileName);
            TextReader textReader = file.OpenText();
            string line;
            bool startAnalyze = false;
            List<string[]> vocsCollectData = new List<string[]>();
            while ((line = textReader.ReadLine()) != null)
            {
                if (startAnalyze)
                {
                    string[] lineData = ParseLine(line);
                    vocsCollectData.Add(lineData);
                }
                if (line.Trim().Equals(HEAD_SPEC))
                {
                    startAnalyze = true;
                }
            }
            ParseVocsCollectData(vocsCollectData, out riDataMap);
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
        private void ParseVocsCollectData(List<string[]> vocsCollectData, out Dictionary<int, float[]> riDataMap)
        {
            riDataMap = new Dictionary<int, float[]>();
            List<int> xList = new List<int>();
            List<List<string>> yListCollect = new List<List<string>>();
            if (vocsCollectData.Count > 0)
            {
                int lineNum = vocsCollectData[0].Length;
                //y轴集合
                for (int i = 0; i < lineNum; i++)
                {
                    yListCollect.Add(new List<string>());
                }
                //x轴，从1递增;y轴
                for (int i = 1; i < vocsCollectData.Count; i++)
                {
                    xList.Add(i);
                    for (int j = 0; j < lineNum; j++)
                    {
                        yListCollect[j].Add(vocsCollectData[i][j]);
                    }
                }
                //赋值缓存数据
                for (int i = 0; i < yListCollect.Count; i++)
                {
                    string[] strArray = yListCollect[i].ToArray();
                    riDataMap.Add(i + 1, Array.ConvertAll(strArray, s => float.Parse(s)));
                }
                this.xList = xList;
                this.yListCollect = yListCollect;
                this.lineNum = lineNum;
                AlgoChart.BeginUpdate();
                for (int i = 0; i < lineNum; i++)
                {
                    AlgoChart.Data.Children.Add(SetDataSeries(i));
                }
                UpdateData();
                AlgoChart.EndUpdate();
            }
        }
        /// <summary>
        /// 设置并返回历史数据曲线
        /// </summary>
        /// <param name="i">lineNum</param>
        /// <returns>数据线</returns>
        private DataSeries SetDataSeries(int i)
        {
            string index = Convert.ToString(i + 1);
            XYDataSeries dataSeries = new XYDataSeries
            {
                Label = "数据_" + (i + 1),
                ConnectionStrokeThickness = 1
            };
            double[] valueY = new double[SpecComOne.pixelNumber];
            for (int j = 0; j < xList.Count; j++)
            {
                valueY[j] = double.Parse(yListCollect[i][j]);
            }
            dataSeries.XValuesSource = xList;
            dataSeries.ValuesSource = valueY;
            dataSeriesMap.Add(index, dataSeries);
            return dataSeries;
        }
        #endregion

        /// <summary>
        /// 创建显示平均光谱数据
        /// </summary>
        /// <param name="index">index</param>
        /// <param name="lineData">lineData</param>
        /// <returns>数据线</returns>
        public bool CreateAvgChart(string index, float[] lineData)
        {
            bool flag;
            if (!dataSeriesMap.ContainsKey(index) && lineData != null)
            {
                XYDataSeries dataSeries = new XYDataSeries
                {
                    Label = "数据_" + index,
                    ConnectionStrokeThickness = 1,
                    ConnectionFill = new SolidColorBrush(ColorRandom.ColorSelect())
                };
                double[] valueY = new double[lineData.Length];
                double[] valueX = new double[lineData.Length];
                for (int i = 0; i < lineData.Length; i++)
                {

                    valueX[i] = i + 1;
                    valueY[i] = lineData[i];
                }
                dataSeries.XValuesSource = valueX;
                dataSeries.ValuesSource = valueY;
                dataSeriesMap.Add(index, dataSeries);
                AddNewSeries(dataSeries);
                flag = true;
            }
            else
            {
                flag = false;
            }
            return flag;
        }
        /// <summary>
        /// 创建显示当前光谱数据
        /// </summary>
        public void CreateCurrentChart(float[] currentData)
        {
            AlgoChart.BeginUpdate();
            if (hadCurrent)
            {
                AlgoChart.Data.Children.Remove(currentDataSeries);
            }
            XYDataSeries dataSeries = new XYDataSeries
            {
                Label = "实时数据",
                ConnectionStrokeThickness = 1
            };
            double[] valueY = new double[currentData.Length];
            double[] valueX = new double[currentData.Length];
            for (int i = 0; i < currentData.Length; i++)
            {

                valueX[i] = i + 1;
                valueY[i] = currentData[i];
            }
            dataSeries.XValuesSource = valueX;
            dataSeries.ValuesSource = valueY;
            currentDataSeries = dataSeries;
            currentDataSeries.ConnectionFill = new SolidColorBrush(Colors.Red);
            AlgoChart.Data.Children.Add(currentDataSeries);
            hadCurrent = true;
            UpdateData();
            AlgoChart.EndUpdate();
        }
        /// <summary>
        /// 显示数据线
        /// </summary>
        /// <param name="index">lineNum</param>
        /// <returns>数据线</returns>
        public void RecoveryDataSeries(string index)
        {
            if (dataSeriesMap.ContainsKey(index))
            {
                XYDataSeries dataSeries = dataSeriesMap[index];
                AddNewSeries(dataSeries);
            }
        }
        /// <summary>
        /// 清除对应曲线
        /// </summary>
        public void RemoveSeriesByIndex(string index)
        {
            if (dataSeriesMap.ContainsKey(index))
            {
                RemoveSeries(index);
            }
        }
        /// <summary>
        /// 清除全部曲线
        /// </summary>
        public void RemoveAllSeries()
        {
            hadCurrent = false;
            RemoveSeries(null);
        }
        void Actions_Enter(object sender, EventArgs e)
        {
            if (sender is ScaleAction)
            {
                AlgoChart.Cursor = Cursors.SizeNS;
            }
            else if (sender is TranslateAction)
            {
                AlgoChart.Cursor = Cursors.SizeAll;
            }
            else
            {
                AlgoChart.Cursor = Cursors.Hand;
            }
        }
        private void AddNewSeries(XYDataSeries dataSeries)
        {
            AlgoChart.BeginUpdate();
            AlgoChart.Data.Children.Add(dataSeries);
            UpdateData();
            AlgoChart.EndUpdate();
        }
        private void RemoveSeries(string index)
        {
            AlgoChart.BeginUpdate();
            if(index != null)
            {
                AlgoChart.Data.Children.Remove(dataSeriesMap[index]);
            }
            else
            {
                dataSeriesMap.Clear();
                AlgoChart.Data.Children.Clear();
            }
            UpdateData();
            AlgoChart.EndUpdate();
        }
        void Actions_Leave(object sender, EventArgs e)
        {
            AlgoChart.Cursor = Cursors.Arrow;
            UpdateScrollbars();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AlgoChart.BeginUpdate();
            AlgoChart.View.AxisX.Scale = 1;
            AlgoChart.View.AxisX.Value = 0.5;
            AlgoChart.View.AxisY.Scale = 1;
            AlgoChart.View.AxisY.Value = 0.5;
            UpdateScrollbars();
            AlgoChart.EndUpdate();
        }
        private void UpdateScrollbars()
        {
            double sx = AlgoChart.View.AxisX.Scale;
            AxisScrollBar sbx = (AxisScrollBar)AlgoChart.View.AxisX.ScrollBar;
            if (sx >= 1.0)
            {
                sbx.Visibility = Visibility.Collapsed;
            }
            else
            {
                sbx.Visibility = Visibility.Visible;
            }
            double sy = AlgoChart.View.AxisY.Scale;
            AxisScrollBar sby = (AxisScrollBar)AlgoChart.View.AxisY.ScrollBar;
            if (sy >= 1.0)
            {
                sby.Visibility = Visibility.Collapsed;
            }
            else
            {
                sby.Visibility = Visibility.Visible;
            }
        }
    }
}
