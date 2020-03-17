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
        private int pixelNumber = 0;
        private float[] waveLength = null; //像素对应的波长
        private const string HEAD_SPEC = "SPEC";
        private string name;
        private List<int> xList;
        private List<List<string>> yListCollect;
        private int lineNum = 0;
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
            DataSeries dataSeries = new DataSeries
            {
                RenderAs = RenderAs.Spline,
                LegendText = "数据" + i,
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
                        title.Enabled = false;
                        break;
                    case 1:
                        title.Enabled = true;
                        break;
                    case 2:
                        Console.WriteLine("隐藏Tag");
                        break;
                    case 3:
                        Console.WriteLine("显示Tag");
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
        public float GetWaveByPixel(int pixel)
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
    }
}
