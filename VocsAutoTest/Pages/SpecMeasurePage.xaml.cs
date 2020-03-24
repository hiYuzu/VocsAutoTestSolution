using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Visifire.Charts;
using VocsAutoTest.Tools;
using VocsAutoTestBLL.Impl;
using VocsAutoTestBLL.Interface;

namespace VocsAutoTest.Pages
{
    /// <summary>
    /// VocsMgmtPage.xaml 的交互逻辑
    /// </summary>
    public partial class SpecMeasurePage : Page
    {
        //默认像素
        private int pixelNumber = 512;
        //波长
        private float[] waveLength = null;
        private const string TITLE = "光谱曲线";
        private int lineNum = 0;
        //测量数据线
        private DataSeries currentDataSeries = null;
        private Chart chart;
        private Title title = null;
        public bool IsPixel { get; set; }
        public bool IsVoltage { get; set; }
        public string XAxisTitle { get; set; }
        public string YAxisTitle { get; set; }
        public bool TitleEnabled { get; set; }
        //电压积分转换系数
        private double FACTOR_VOL_TO_INTEG = 4.096 / 65536.0;
        public List<List<string>> YListCollect { get; } = new List<List<string>>();
        public string[] CurrentData { get; private set; }

        public SpecMeasurePage()
        {
            InitializeComponent();
            SpectrumChart.Children.Clear();
            InitParam();
            SpecOperatorImpl.Instance.SpecDataEvent += new SpecDataDelegate(ImportCurrentData);
        }
        /// <summary>
        /// 初始化参数
        /// </summary>
        private void InitParam()
        {
            IsPixel = true;
            IsVoltage = true;
            XAxisTitle = "像素";
            YAxisTitle = "电压值(V)";
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
            Dispatcher.BeginInvoke(new Action(() =>
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
            chart.AxesX[0].Title = XAxisTitle;
            chart.AxesY[0].Title = YAxisTitle;         
            if(currentDataSeries != null)
            {
                chart.Series.Remove(currentDataSeries);
            }
            currentDataSeries = SetDataSeries(new List<string>(CurrentData), -1);
            chart.Series.Add(currentDataSeries);
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
            chart.AxesX[0].Interval = pixelNumber / 32;
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
                ClearHistoricalSeries();
                int lineNum = vocsCollectData[0].Length;
                //y轴，[数据线数量][每条线数据量]
                for(int i = 0; i < lineNum; i++)
                {
                    YListCollect.Add(new List<string>());
                }
                for (int i = 0; i < vocsCollectData.Count; i++)
                {
                    for (int j = 0; j < lineNum; j++)
                    {
                        YListCollect[j].Add(vocsCollectData[i][j]);
                    }
                }
                this.lineNum = lineNum;
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
                chart.AxesX[0].Title = XAxisTitle;
                chart.AxesY[0].Title = YAxisTitle;
                for (int i = 0; i < lineNum; i++)
                {
                    chart.Series.Add(SetDataSeries(YListCollect[i], i));
                }
            }
        }
        /// <summary>
        /// 设置并返回数据线
        /// </summary>
        /// <param name="i">lineNum</param>
        /// <returns>数据线</returns>
        private DataSeries SetDataSeries(List<string> yListCollect, int i)
        {
            DataSeries dataSeries = new DataSeries
            {
                RenderAs = RenderAs.Spline,
                LegendText = "当前测量",
                XValueType = ChartValueTypes.Auto
            };
            if (i > -1)
            {
                dataSeries.LegendText = "历史数据" + i;
            }
            //像素-电压
            if (IsPixel && IsVoltage)
            {
                for (int j = 0; j < yListCollect.Count; j++)
                {
                    DataPoint dataPoint = new DataPoint
                    {
                        MarkerSize = 3
                    };
                    dataPoint.XValue = j + 1;
                    dataPoint.YValue = double.Parse(yListCollect[j]) / 1000;
                    dataSeries.DataPoints.Add(dataPoint);
                }
            }
            //像素-积分
            else if (IsPixel && !IsVoltage)
            {
                for (int j = 0; j < yListCollect.Count; j++)
                {
                    DataPoint dataPoint = new DataPoint
                    {
                        MarkerSize = 3
                    };
                    dataPoint.XValue = j + 1;
                    dataPoint.YValue = double.Parse(yListCollect[j]) / 1000 / FACTOR_VOL_TO_INTEG;
                    dataSeries.DataPoints.Add(dataPoint);
                }
            }
            //波长-电压
            else if (!IsPixel && IsVoltage)
            {
                for (int j = 0; j < yListCollect.Count; j++)
                {
                    DataPoint dataPoint = new DataPoint
                    {
                        MarkerSize = 3
                    };
                    dataPoint.XValue = GetWaveByPixel(j + 1);
                    dataPoint.YValue = double.Parse(yListCollect[j]) / 1000;
                    dataSeries.DataPoints.Add(dataPoint);
                }
            }
            //波长-积分
            else if (!IsPixel && !IsVoltage)
            {
                for (int j = 0; j < yListCollect.Count; j++)
                {
                    DataPoint dataPoint = new DataPoint
                    {
                        MarkerSize = 3
                    };
                    dataPoint.XValue = GetWaveByPixel(j + 1);
                    dataPoint.YValue = double.Parse(yListCollect[j]) / 1000 / FACTOR_VOL_TO_INTEG;
                    dataSeries.DataPoints.Add(dataPoint);
                }
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
            chart.Series.Remove(currentDataSeries);
            currentDataSeries = null;
        }
        /// <summary>
        /// 清除历史数据线
        /// </summary>
        public void ClearHistoricalSeries()
        {
            lineNum = 0;
            YListCollect.Clear();
            chart.Series.Clear();
            if(currentDataSeries != null)
            {
                chart.Series.Add(currentDataSeries);
            }
        }
        /// <summary>
        /// 清除全部曲线
        /// </summary>
        public void ClearAllSeries()
        {
            currentDataSeries = null;
            lineNum = 0;
            YListCollect.Clear();
            chart.Series.Clear();
        }
    }
}
