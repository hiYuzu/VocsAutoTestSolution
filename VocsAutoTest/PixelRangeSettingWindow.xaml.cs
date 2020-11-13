using GaussFit;
using MathWorks.MATLAB.NET.Arrays;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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
            if (CheckData())
            {
                try
                {
                    GaussFitParam gaussFit = new GaussFitParam();
                    //y = y0+(A/(w*sqrt(pi/2)))*exp(-2*((x-xc)/w)*2);
                    //其中y0为最小值，A为积分值，w为半高宽，xc为峰值对应坐标值
                    //第一列为像素位置，第二列为积分值
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
                        Array current = gaussFit.GetGaussFitParam(ToMWArray(data)).ToArray();
                        double[,] d = (double[,])current;
                        showMsg = "当前测量拟合半高宽：" + d[0, 2].ToString("0.00") + "\n";
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
                            Array array = gaussFit.GetGaussFitParam(ToMWArray(data)).ToArray();
                            double[,] d = (double[,])array;
                            showMsg = showMsg + "历史数据拟合半高宽：" + d[0, 2].ToString("0.00") + "\n";
                        }
                    }
                    showMsg = showMsg.Substring(0, showMsg.Length - 1);
                    if(showMsg.Equals(string.Empty))
                    {
                        showMsg = "当前无任何数据！";
                    }
                    MessageBox.Show(showMsg);
                }
                catch (Exception ex)
                {
                    ExceptionUtil.Instance.ExceptionMethod(ex.Message, true);
                }
            }
            else
            {
                MessageBox.Show("非法参数！");
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
