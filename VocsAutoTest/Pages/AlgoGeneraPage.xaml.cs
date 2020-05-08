using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Visifire.Charts;
using VocsAutoTestBLL.Impl;
using VocsAutoTestBLL.Interface;
using VocsAutoTestCOMM;

namespace VocsAutoTest.Pages
{
    /// <summary>
    /// VocsMgmtPage.xaml 的交互逻辑
    /// </summary>
    public partial class AlgoGeneraPage : Page
    {
        private int pixelNumber = 512;
        private float[] waveLength = null; //像素对应的波长
        private const string HEAD_SPEC = "SPEC";
        private const string TITLE = "光谱曲线";
        private string name;
        private List<int> xList;
        private List<List<string>> yListCollect;
        private int lineNum = 0;
        private Dictionary<string, DataSeries> dataSeriesMap = new Dictionary<string, DataSeries>();//光谱数据
        private Chart chart;
        private Title title = null;
        public bool IsPixel { get; set; }
        public bool IsVoltage { get; set; }
        public string XAxisTitle { get; set; }
        public string YAxisTitle { get; set; }
        public bool TitleEnabled { get; set; }
        //电压积分转换系数
        private double FACTOR_VOL_TO_INTEG = 2.5 / 65536.0;

        public AlgoGeneraPage()
        {
            InitializeComponent();
            SpectrumChart.Children.Clear();
            InitParam();
        }
        private void InitParam()
        {
            IsPixel = true;
            IsVoltage = true;
            XAxisTitle = "像素";
            YAxisTitle = "积分值";
            InitChart();
        }

        /// <summary>
        /// 初始化折线图
        /// </summary>
        private void InitChart()
        {
            SpectrumChart.Children.Clear();
            chart = new Chart
            {
                Margin = new Thickness(5, 5, 5, 5),
                ToolBarEnabled = false,
                ScrollingEnabled = false,
                View3D = true
            };
            title = new Title
            {
                Text = TITLE,
                Padding = new Thickness(0, 10, 5, 0)
            };
            chart.Titles.Add(title);
            chart.ZoomingEnabled = true;
            chart.ZoomingMode = ZoomingMode.MouseDragAndWheel;
            Axis xAxis = new Axis
            {
                AxisMinimum = 0,
                Title = XAxisTitle,
                IntervalType = IntervalTypes.Auto,
                Interval = pixelNumber / 32
            };
            chart.AxesX.Add(xAxis);
            Axis yAxis = new Axis
            {
                AxisMinimum = 0,
                Title = YAxisTitle,
                AxisType = AxisTypes.Primary
            };
            chart.AxesY.Add(yAxis);
            Grid gr = new Grid();
            gr.Children.Add(chart);
            SpectrumChart.Children.Add(gr);
        }

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
                for (int i=0;i< yListCollect.Count;i++) {
                    string[] strArray = yListCollect[i].ToArray();
                    riDataMap.Add(i+1, Array.ConvertAll(strArray,s=>float.Parse(s)));
                }
                name = "光谱曲线";
                this.xList = xList;
                this.yListCollect = yListCollect;
                this.lineNum = lineNum;
                CreateChartSpline();
            }
        }
        /// <summary>
        /// 绘制折线图
        /// </summary>
        public void CreateChartSpline()
        {
            SpectrumChart.Children.Clear();
            dataSeriesMap.Clear();
            if (lineNum == 0)
            {
                return;
            }
            chart = new Chart
            {
                Margin = new Thickness(5, 5, 5, 5),
                ToolBarEnabled = false,
                ScrollingEnabled = false,
                View3D = true
            };
            title = new Title
            {
                Text = name,
                Padding = new Thickness(0, 10, 5, 0)
            };
            chart.Titles.Add(title);
            chart.ZoomingEnabled = true;

            Axis xAxis = new Axis
            {
                AxisMinimum = 0,
                Title = XAxisTitle,
                IntervalType = IntervalTypes.Auto,
                Interval = yListCollect[0].Count / 32
            };
            chart.AxesX.Add(xAxis);
            Axis yAxis = new Axis
            {
                AxisMinimum = 0,
                Title = YAxisTitle,
                AxisType = AxisTypes.Primary
            };
            chart.AxesY.Add(yAxis);
            for (int i = 0; i < lineNum; i++)
            {
                chart.Series.Add(SetDataSeries(i));
            }
            Grid gr = new Grid();
            gr.Children.Add(chart);
            SpectrumChart.Children.Add(gr);
        }
        /// <summary>
        /// 设置并返回数据线
        /// </summary>
        /// <param name="i">lineNum</param>
        /// <returns>数据线</returns>
        private DataSeries SetDataSeries(int i)
        {
            string index = Convert.ToString(i + 1);
            DataSeries dataSeries = new DataSeries
            {
                RenderAs = RenderAs.Spline,
                LegendText = index,
                XValueType = ChartValueTypes.Auto
            };
            for (int j = 0; j < xList.Count; j++)
            {
                DataPoint dataPoint = new DataPoint
                {
                    MarkerSize = 3
                };
                dataPoint.XValue = xList[j];
                dataPoint.YValue = double.Parse(yListCollect[i][j]);
                dataSeries.DataPoints.Add(dataPoint);
            }
            dataSeriesMap.Add(index, dataSeries);
            return dataSeries;
        }
        /// <summary>
        /// 设置曲线数据并显示
        /// </summary>
        /// <param name="index">index</param>
        /// <param name="lineData">lineData</param>
        /// <returns>数据线</returns>
        public bool CreateCurrentChart(string index,float[] lineData)
        {
            bool flag = false;
            if (!dataSeriesMap.ContainsKey(index) && lineData != null)
            {

                DataSeries dataSeries = new DataSeries
                {
                    RenderAs = RenderAs.Spline,
                    LegendText = index,
                    XValueType = ChartValueTypes.Auto
                };
                for (int i = 0; i < lineData.Length; i++)
                {
                    DataPoint dataPoint = new DataPoint
                    {
                        MarkerSize = 3
                    };
                    dataPoint.XValue = i;
                    dataPoint.YValue = lineData[i];
                    dataSeries.DataPoints.Add(dataPoint);
                }
                dataSeriesMap.Add(index, dataSeries);
                chart.Series.Add(dataSeries);
                flag = true;
            }
            else {
                flag = false;
            }
            return flag;
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
                DataSeries dataSeries = dataSeriesMap[index];
                chart.Series.Add(dataSeries);
            }
        }

        /// <summary>
        /// 清除对应曲线
        /// </summary>
        public void RemoveSeriesByIndex(string index)
        {
            if (dataSeriesMap.ContainsKey(index))
            {
                chart.Series.Remove(dataSeriesMap[index]);
            }
        }

        /// <summary>
        /// 清除全部曲线
        /// </summary>
        public void RemoveAllSeries()
        {
            dataSeriesMap.Clear();
            chart.Series.Clear();
        }

    }
}
