using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using C1.WPF.C1Chart;

namespace VocsAutoTest.Pages
{
    /// <summary>
    /// ConcentrationComOne.xaml 的交互逻辑
    /// </summary>
    public partial class ConcentrationComOne : UserControl
    {
        int counter = 0;
        readonly Random rnd = new Random();
        readonly DispatcherTimer dt;
        readonly XYDataSeries ds1;
        readonly XYDataSeries ds2;
        readonly XYDataSeries ds3;
        readonly XYDataSeries ds4;
        public ConcentrationComOne()
        {
            InitializeComponent();
            ConcChart.ChartType = ChartType.Line;

            ds1 = new XYDataSeries()
            {
                ConnectionStrokeThickness = 1,
                Label = "气体1",
            };
            ds2 = new XYDataSeries()
            {
                ConnectionStrokeThickness = 1,
                Label = "气体2",
            };
            ds3 = new XYDataSeries()
            {
                ConnectionStrokeThickness = 1,
                Label = "气体3",
            };
            ds4 = new XYDataSeries()
            {
                ConnectionStrokeThickness = 1,
                Label = "气体4",
            };
            ConcChart.Data.Children.Add(ds1);
            ConcChart.Data.Children.Add(ds2);
            ConcChart.Data.Children.Add(ds3);
            ConcChart.Data.Children.Add(ds4);
            ConcChart.View.AxisY.AutoMin = true;
            ConcChart.View.AxisY.AutoMax = true;
            dt = new DispatcherTimer(){ 
                Interval = TimeSpan.FromSeconds(0.4) 
            };
            dt.Tick += (s, e) => Update();
            dt.Start();
        }
        void Update()
        {
            ConcChart.BeginUpdate();
            for (int i = 0; i < 1; i++)
            {
                double r = rnd.NextDouble();
                double y = (10 * r * Math.Sin(0.1 * counter) * Math.Sin(0.6 * rnd.NextDouble() * counter));
            }
            ConcChart.EndUpdate();
        }
        public void UpdateChart(List<float> a) { }
        public void ClearConcChart() { }
    }
}
