﻿using System;
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
        public ConcentrationMeasurePage()
        {
            InitializeComponent();
        }

        public void initPage() {
            MeasureChart.Children.Clear();
            CreateChartSpline("浓度测量", LsTime, cherry, pineapple);
        }

        private List<DateTime> LsTime = new List<DateTime>()
            {
               new DateTime(2012,1,1),
               new DateTime(2012,2,1),
               new DateTime(2012,3,1),
               new DateTime(2012,4,1),
               new DateTime(2012,5,1),
               new DateTime(2012,6,1),
               new DateTime(2012,7,1),
               new DateTime(2012,8,1),
               new DateTime(2012,9,1),
               new DateTime(2012,10,1),
               new DateTime(2012,11,1),
               new DateTime(2012,12,1),
            };
        private List<string> cherry = new List<string>() { "33", "75", "60", "98", "67", "88", "39", "45", "13", "22", "45", "80" };
        private List<string> pineapple = new List<string>() { "13", "34", "38", "12", "45", "76", "36", "80", "97", "22", "76", "39" };

        #region 折线图
        private void CreateChartSpline(string name, List<DateTime> lsTime, List<string> cherry, List<string> pineapple)
        {
            //创建一个图标
            Chart chart = new Chart();

            chart.Margin = new Thickness(5, 5, 5, 5);
            //是否启用打印和保持图片
            chart.ToolBarEnabled = false;

            //设置图标的属性
            chart.ScrollingEnabled = false;//是否启用或禁用滚动
            chart.View3D = true;//3D效果显示

            //创建一个标题的对象
            Title title = new Title();

            //设置标题的名称
            title.Text = name;
            title.Padding = new Thickness(0, 10, 5, 0);

            //向图标添加标题
            chart.Titles.Add(title);
            //设置缩放
            chart.ZoomingEnabled = true;

            //初始化一个新的Axis
            Axis xaxis = new Axis();
            //设置Axis的属性
            //图表的X轴坐标按什么来分类，如时分秒
            xaxis.IntervalType = IntervalTypes.Months;
            //图表的X轴坐标间隔如2,3,20等，单位为xAxis.IntervalType设置的时分秒。
            xaxis.Interval = 1;
            //设置X轴的时间显示格式为7-10 11：20           
            xaxis.ValueFormatString = "MM月";
            //给图标添加Axis            
            chart.AxesX.Add(xaxis);

            Axis yAxis = new Axis();
            //设置图标中Y轴的最小值永远为0           
            yAxis.AxisMinimum = 0;
            //设置图表中Y轴的后缀          
            yAxis.Suffix = "斤";
            chart.AxesY.Add(yAxis);


            // 创建一个新的数据线。               
            DataSeries dataSeries = new DataSeries();
            // 设置数据线的格式。               
            dataSeries.LegendText = "樱桃";

            dataSeries.RenderAs = RenderAs.Spline;//折线图

            dataSeries.XValueType = ChartValueTypes.DateTime;
            // 设置数据点              
            DataPoint dataPoint;
            for (int i = 0; i < lsTime.Count; i++)
            {
                // 创建一个数据点的实例。                   
                dataPoint = new DataPoint();
                // 设置X轴点                    
                dataPoint.XValue = lsTime[i];
                //设置Y轴点                   
                dataPoint.YValue = double.Parse(cherry[i]);
                dataPoint.MarkerSize = 8;
                //dataPoint.Tag = tableName.Split('(')[0];
                //设置数据点颜色                  
                // dataPoint.Color = new SolidColorBrush(Colors.LightGray);                   
                dataPoint.MouseLeftButtonDown += new MouseButtonEventHandler(dataPoint_MouseLeftButtonDown);
                //添加数据点                   
                dataSeries.DataPoints.Add(dataPoint);
            }

            // 添加数据线到数据序列。                
            chart.Series.Add(dataSeries);


            // 创建一个新的数据线。               
            DataSeries dataSeriesPineapple = new DataSeries();
            // 设置数据线的格式。         

            dataSeriesPineapple.LegendText = "菠萝";

            dataSeriesPineapple.RenderAs = RenderAs.Spline;//折线图

            dataSeriesPineapple.XValueType = ChartValueTypes.DateTime;
            // 设置数据点              

            DataPoint dataPoint2;
            for (int i = 0; i < lsTime.Count; i++)
            {
                // 创建一个数据点的实例。                   
                dataPoint2 = new DataPoint();
                // 设置X轴点                    
                dataPoint2.XValue = lsTime[i];
                //设置Y轴点                   
                dataPoint2.YValue = double.Parse(pineapple[i]);
                dataPoint2.MarkerSize = 8;
                //dataPoint2.Tag = tableName.Split('(')[0];
                //设置数据点颜色                  
                // dataPoint.Color = new SolidColorBrush(Colors.LightGray);                   
                dataPoint2.MouseLeftButtonDown += new MouseButtonEventHandler(dataPoint_MouseLeftButtonDown);
                //添加数据点                   
                dataSeriesPineapple.DataPoints.Add(dataPoint2);
            }
            // 添加数据线到数据序列。                
            chart.Series.Add(dataSeriesPineapple);

            //将生产的图表增加到Grid，然后通过Grid添加到上层Grid.           
            Grid gr = new Grid();
            gr.Children.Add(chart);
            MeasureChart.Children.Add(gr);
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
