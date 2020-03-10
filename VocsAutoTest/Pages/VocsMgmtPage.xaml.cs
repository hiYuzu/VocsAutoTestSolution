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

namespace VocsAutoTest.Pages
{
    /// <summary>
    /// VocsMgmtPage.xaml 的交互逻辑
    /// </summary>
    public partial class VocsMgmtPage : Page
    {
        private const string HEAD_SPEC = "SPEC";
        private string name;
        private List<int> xList;
        private List<List<string>> yListCollect;
        private int lineNum = 0;
        private Title title = null;
        public bool IsVoltage { get; set; }
        public string XAxisTitle { get; set; }
        public string YAxisTitle { get; set; }
        public bool TitleEnabled { get; set; }
        //电压积分转换系数
        private const double FACTOR_VOL_TO_INTEG = 2.5 / 65536.0;

        public VocsMgmtPage()
        {
            InitializeComponent();
            SpectrumChart.Children.Clear();
            InitParam();
        }
        private void InitParam()
        {
            IsVoltage = true;
            XAxisTitle = "像素";
            YAxisTitle = "电压值(V)";
        }
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
        public void IsShow(int isShow)
        {
            if(title != null)
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
        private void ParseVocsCollectData(List<string[]> vocsCollectData)
        {
            List<int> xList = new List<int>();
            List<List<string>> yListCollect = new List<List<string>>();
            if(vocsCollectData.Count > 0)
            {
                int lineNum = vocsCollectData[0].Length;
                //y轴集合
                for(int i = 0; i < lineNum; i++)
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

        #region 折线图
        public void CreateChartSpline()
        {
            if (lineNum == 0)
            {
                return;
            }
            //创建一个图标
            Chart chart = new Chart
            {
                Margin = new Thickness(5, 5, 5, 5),
                ToolBarEnabled = false,
                //禁用滚动
                ScrollingEnabled = false,
                //3D效果显示
                View3D = true
            };

            //创建一个标题的对象
            title = new Title
            {
                Text = name,
                Padding = new Thickness(0, 10, 5, 0)
            };
            //向图标添加标题
            chart.Titles.Add(title);
            //设置缩放
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
                Title = YAxisTitle
            };
            chart.AxesY.Add(yAxis);

            // 创建一个新的数据线
            DataSeries dataSeries;
            DataPoint dataPoint;
            for (int i = 0; i < lineNum; i++)
            {
                dataSeries = new DataSeries
                {
                    // 设置数据线的格式为折线图
                    RenderAs = RenderAs.Spline,
                    XValueType = ChartValueTypes.Auto
                };
                for (int j = 0; j < xList.Count; j++)
                {   
                    dataPoint = new DataPoint
                    {
                        // 设置X轴点                    
                        XValue = xList[j],
                        MarkerSize = 8
                    };
                    if (IsVoltage)
                    {                        
                        dataPoint.YValue = double.Parse(yListCollect[i][j]) / 1000;
                    }
                    else
                    {
                        dataPoint.YValue = double.Parse(yListCollect[i][j]) / 1000 / FACTOR_VOL_TO_INTEG;
                    }
                    dataPoint.MouseLeftButtonDown += new MouseButtonEventHandler(dataPoint_MouseLeftButtonDown);
                    //添加数据点                   
                    dataSeries.DataPoints.Add(dataPoint);
                }
                // 添加数据线到数据序列。                
                chart.Series.Add(dataSeries);
            }
            //将生产的图表增加到Grid，然后通过Grid添加到上层Grid.           
            Grid gr = new Grid();
            gr.Children.Add(chart);
            SpectrumChart.Children.Add(gr);
        }
        #endregion

        #region 点击事件
        //点击事件
        void dataPoint_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
        #endregion
    }
}
