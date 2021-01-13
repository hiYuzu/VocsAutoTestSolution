using GaussFit;
using MathWorks.MATLAB.NET.Arrays;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using VocsAutoTestCOMM;

namespace VocsAutoTest
{
    /// <summary>
    /// PixelRangeSettingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PixelRangeSettingWindow : Window
    {
        private int PixelStart { get; set; }
        private int PixelEnd { get; set; }
        private int Count { get; set; }

        private bool fiting = false;
        public PixelRangeSettingWindow()
        {
            ExceptionUtil.Instance.ShowLoadingAction(true);
            InitializeComponent();
        }

        /// <summary>
        /// 确认按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if(fiting)
            {
                MessageBox.Show("正在拟合，请稍后");
                return;
            }
            if (!CheckData())
            {
                MessageBox.Show("非法参数！");
                return;
            }
            ExceptionUtil.Instance.LogMethod("开始拟合计算...");
            try
            {
                Thread fitThread = new Thread(GaussFitThread)
                {
                    Name = "GaussFitThread",
                    IsBackground = true
                };
                fitThread.Start();
            }
            catch (Exception ex)
            {
                ExceptionUtil.Instance.ExceptionMethod(ex.Message, true);
            }
        }

        /// <summary>
        /// 验证所有数据正确输入
        /// </summary>
        /// <returns></returns>
        private bool CheckData()
        {
            try
            {
                PixelStart = int.Parse(pixelStart.Text);
                PixelEnd = int.Parse(pixelEnd.Text);
                Count = PixelEnd - PixelStart + 1;
                if (Count < 4 || PixelStart < 1 || PixelEnd > 512)
                {
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                ExceptionUtil.Instance.LogMethod("非法参数：" + e.Message);
                return false;
            }
        }

        private void GaussFitThread()
        {
            fiting = true;
            String showMsg = String.Empty;
            float[,] data = new float[Count, 2];
            string[] currentData = SpecComOne.CurrentData;
            List<List<string>> historyDataList = SpecComOne.YListCollect;
            if (currentData != null && currentData.Length > 0)
            {
                //当前测量数据
                for (int i = 0; i < Count; i++)
                {
                    data[i, 0] = i;
                    data[i, 1] = float.Parse(currentData[i + PixelStart - 1]);
                }
                showMsg = "当前测量拟合半高宽：" + FitResult(data) + "\n";
            }
            if (historyDataList.Count > 0)
            {
                //导入的历史数据
                foreach (List<string> historyData in historyDataList)
                {
                    //当前测量数据
                    for (int i = 0; i < Count; i++)
                    {
                        data[i, 0] = i;
                        data[i, 1] = float.Parse(historyData[i + PixelStart - 1]);
                    }
                    showMsg = showMsg + "历史数据拟合半高宽：" + FitResult(data) + "\n";
                }
            }
            if (showMsg.Equals(string.Empty))
            {
                showMsg = "当前无任何数据！\n";
            }
            MessageBox.Show(showMsg.Substring(0, showMsg.Length - 1));
            fiting = false;
        }

        private String FitResult(float[,] data)
        {
            try
            {
                GaussFitParam gaussFit = new GaussFitParam();
                //y = y0+(A/(w*sqrt(pi/2)))*exp(-2*((x-xc)/w)*2);
                //其中y0为最小值，A为积分值，w为半高宽，xc为峰值对应坐标值
                //第一列为像素位置，第二列为积分值
                Array current = gaussFit.GetGaussFitParam(ToMWArray(data)).ToArray();
                double[,] d = (double[,])current;
                return d[0, 2].ToString("0.00");
            }
            catch (Exception ex)
            {
                ExceptionUtil.Instance.ExceptionMethod("拟合过程出现异常：" + ex.Message, true);
                return "error";
            }
        }

        private MWArray ToMWArray(float[,] data)
        {
            double[,] doubleData = new double[data.GetLength(0), data.GetLength(1)];
            for (int i = 0; i < data.GetLength(0); i++)
            {
                for (int j = 0; j < data.GetLength(1); j++)
                {
                    doubleData[i, j] = data[i, j];
                }
            }
            return (MWNumericArray)doubleData;
        }


        /// <summary>
        /// 取消按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        override protected void OnClosed(EventArgs e)
        {
            ExceptionUtil.Instance.ShowLoadingAction(false);
        }
    }
}
