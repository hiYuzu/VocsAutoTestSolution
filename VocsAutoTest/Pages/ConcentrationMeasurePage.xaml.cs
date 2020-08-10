using System;
using System.Collections.Generic;
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
using Visifire.Charts;

namespace VocsAutoTest.Pages
{
    /// <summary>
    /// ConcentrationMeasurePage.xaml 的交互逻辑
    /// </summary>
    public partial class ConcentrationMeasurePage : Page
    {
        private List<float> concData;
        private DateTime time;
        private Chart chart;
        private DataSeries series1 = null;
        private DataSeries series2 = null;
        private DataSeries series3 = null;
        private DataSeries series4 = null;
        public ConcentrationMeasurePage()
        {
            InitializeComponent();
            ConcChart.Children.Clear();
            InitChart();
        }
        private void InitChart()
        {
            ConcChart.Children.Clear();
            chart = new Chart
            {
                Margin = new Thickness(5, 5, 5, 5),
                ToolBarEnabled = false,
                ScrollingEnabled = false,
                View3D = true
            };
            Title title = new Title
            {
                Text = "浓度曲线",
                Padding = new Thickness(0, 10, 5, 0)
            };
            chart.Titles.Add(title);
            chart.ZoomingEnabled = true;
            chart.ZoomingMode = ZoomingMode.MouseDragAndWheel;
            Axis xAxis = new Axis
            {
                Title = "时间",
                ValueFormatString = "MM-dd HH:mm:ss",
                Interval = 5,
                IntervalType = IntervalTypes.Minutes
            };         
            chart.AxesX.Add(xAxis);
            Axis yAxis = new Axis
            {
                Title = "浓度(ppm)"
            };
            chart.AxesY.Add(yAxis);
            series1 = new DataSeries
            {
                RenderAs = RenderAs.Line,
                LegendText = "气体一",
                XValueType = ChartValueTypes.DateTime
            };
            series2 = new DataSeries
            {
                RenderAs = RenderAs.Line,
                LegendText = "气体二",
                XValueType = ChartValueTypes.DateTime
            };
            series3 = new DataSeries
            {
                RenderAs = RenderAs.Line,
                LegendText = "气体三",
                XValueType = ChartValueTypes.DateTime
            };
            series4 = new DataSeries
            {
                RenderAs = RenderAs.Line,
                LegendText = "气体四",
                XValueType = ChartValueTypes.DateTime
            };
            chart.Series.Add(series1);
            chart.Series.Add(series2);
            chart.Series.Add(series3);
            chart.Series.Add(series4);
            Grid gr = new Grid();
            gr.Children.Add(chart);
            ConcChart.Children.Add(gr);
        }

        public void UpdateChart(List<float> concData)
        {
            time = DateTime.Now;
            this.concData = concData;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                CreatConcChart();
            }));
        }
        private void InitSeries()
        {
            
        }
        private void CreatConcChart()
        {
            for(int i = 0; i < concData.Count; i++)
            {
                AddPointToSeries(i, time);
            }
        }
        private void AddPointToSeries(int i, DateTime time)
        {
            DataPoint dataPoint = new DataPoint
            {
                MarkerSize = 4,
                XValue = time,
                YValue = concData[i]
            };
            switch (i)
            {
                case 0:
                    series1.DataPoints.Add(dataPoint);
                    break;
                case 1:
                    series2.DataPoints.Add(dataPoint);
                    break;
                case 2:
                    series3.DataPoints.Add(dataPoint);
                    break;
                case 3:
                    series4.DataPoints.Add(dataPoint);
                    break;
            }
        }
        public void ClearConcChart()
        {
            List<DataSeries> dataSeries = chart.Series.ToList<DataSeries>();
            foreach(DataSeries series in dataSeries)
            {
                series.DataPoints.Clear();
            }
        }
    }
}
