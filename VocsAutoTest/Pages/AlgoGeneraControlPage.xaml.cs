using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using VocsAutoTest.Algorithm;
using VocsAutoTest.Tools;
using VocsAutoTestBLL.Impl;
using VocsAutoTestBLL.Interface;
using VocsAutoTestBLL.Model;
using VocsAutoTestCOMM;

namespace VocsAutoTest.Pages
{
    /// <summary>
    /// AlgoGeneraControlPage.xaml 的交互逻辑
    /// </summary>
    public partial class AlgoGeneraControlPage : Page
    {
        private ObservableCollection<string[]> _obervableCollection = new ObservableCollection<string[]>();//测量数据
        private Dictionary<int, float[]> riDataMap = new Dictionary<int, float[]>();//光谱数据
        //private AlgoGeneraPage algoPage;
        private readonly AlgoComOne algoPage;
        private int _gasIndex = 0;
        //选择文件默认地址
        private string importRoad = null;
        private const string HEAD_GAS = "GAS";
        private const string HEAD_FLOW = "FLOW";
        private const string HEAD_SPEC = "SPEC";
        private const int GAS_NUMBER = 4;//最多选择气体种类
        private int pixelSize = 512;//TODO 512需要确认来源
        //private const int MAX_DIST_COUNT = 60;//去掉
        //private int distCount = 0;//去掉
        //private float[] distArray = new float[MAX_DIST_COUNT];//去掉
        //记录接收到的数据
        private ArrayList dataList = new ArrayList();
        //平均次数全局变量
        private int averageTime;
        private int gasCount;

        public AlgoGeneraControlPage()
        {
            InitializeComponent();
            InitPage(0);
        }

        public AlgoGeneraControlPage(AlgoComOne algoPage)
        {
            InitializeComponent();
            InitPage(0);
            if (algoPage != null)
            {
                this.algoPage = algoPage;
            }
            else
            {
                MessageBox.Show("无法加载图形显示功能，请重启软件尝试！", "错误提示", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            SpecOperatorImpl.Instance.AlgoDataEvent += new SpecDataDelegate(ImportCurrentData);

        }

        private void InitPage(int optInt)
        {
            if (optInt == 0)
            {
                //初始化
                InitCombox();
                combobox_gas1_name.SelectedValue = 22;
                combobox_gas1_name.IsEnabled = false;
                textbox_gas1_ppm.IsEnabled = false;
                gasRange1.IsEnabled = false;
                textbox_gas1_ppm.Text = "99.999";

                combobox_gas2_name.SelectedValue = 9;
                combobox_gas2_name.IsEnabled = true;
                textbox_gas2_ppm.IsEnabled = true;
                gasRange2.IsEnabled = true;

                combobox_gas3_name.SelectedValue = 8;
                combobox_gas3_name.IsEnabled = true;
                textbox_gas3_ppm.IsEnabled = true;
                gasRange3.IsEnabled = true;

                combobox_gas4_name.SelectedValue = 0;
                combobox_gas4_name.IsEnabled = true;
                textbox_gas4_ppm.IsEnabled = false;
                gasRange4.IsEnabled = true;

                button_begin_set.IsEnabled = false;
                button_finish_set.IsEnabled = true;
                button_cancel_set.IsEnabled = true;
            }
            else if (optInt == 1)
            {
                //开始设定
                combobox_gas1_name.IsEnabled = false;
                textbox_gas1_ppm.IsEnabled = false;
                gasRange1.IsEnabled = false;

                combobox_gas2_name.IsEnabled = true;
                textbox_gas2_ppm.IsEnabled = true;
                gasRange2.IsEnabled = true;

                combobox_gas3_name.IsEnabled = true;
                textbox_gas3_ppm.IsEnabled = true;
                gasRange3.IsEnabled = true;

                combobox_gas4_name.IsEnabled = true;
                textbox_gas4_ppm.IsEnabled = true;
                gasRange4.IsEnabled = true;

                button_begin_set.IsEnabled = false;
                button_finish_set.IsEnabled = true;
                button_cancel_set.IsEnabled = true;
                label_gas1_input.Content = "气体1";
                textbox_gas1_input.IsEnabled = false;
                label_gas2_input.Content = "气体2";
                textbox_gas2_input.IsEnabled = false;
                label_gas3_input.Content = "气体3";
                textbox_gas3_input.IsEnabled = false;
                label_gas4_input.Content = "气体4";
                textbox_gas4_input.IsEnabled = false;
                button_gas_input.IsEnabled = false;

                //解锁测量信息
                //LockMeasInfo(false);
            }
            else if (optInt == 2)
            {
                gasCount = 0;
                //完成设定
                combobox_gas1_name.IsEnabled = false;
                textbox_gas1_ppm.IsEnabled = false;
                gasRange1.IsEnabled = false;

                combobox_gas2_name.IsEnabled = false;
                textbox_gas2_ppm.IsEnabled = false;
                gasRange2.IsEnabled = false;

                combobox_gas3_name.IsEnabled = false;
                textbox_gas3_ppm.IsEnabled = false;
                gasRange3.IsEnabled = false;

                combobox_gas4_name.IsEnabled = false;
                textbox_gas4_ppm.IsEnabled = false;
                gasRange4.IsEnabled = false;

                button_begin_set.IsEnabled = true;
                button_finish_set.IsEnabled = false;
                button_cancel_set.IsEnabled = false;
                if (Convert.ToInt32(combobox_gas1_name.SelectedValue) != 0)
                {
                    label_gas1_input.Content = combobox_gas1_name.Text;
                    textbox_gas1_input.IsEnabled = true;
                    gasCount++;
                }
                else
                {
                    label_gas1_input.Content = "气体1";
                    textbox_gas1_input.IsEnabled = false;
                }
                if (Convert.ToInt32(combobox_gas2_name.SelectedValue) != 0)
                {
                    label_gas2_input.Content = combobox_gas2_name.Text;
                    textbox_gas2_input.IsEnabled = true;
                    gasCount++;
                }
                else
                {
                    label_gas2_input.Content = "气体2";
                    textbox_gas2_input.IsEnabled = false;
                }
                if (Convert.ToInt32(combobox_gas3_name.SelectedValue) != 0)
                {
                    label_gas3_input.Content = combobox_gas3_name.Text;
                    textbox_gas3_input.IsEnabled = true;
                    gasCount++;
                }
                else
                {
                    label_gas3_input.Content = "气体3";
                    textbox_gas3_input.IsEnabled = false;
                }
                if (Convert.ToInt32(combobox_gas4_name.SelectedValue) != 0)
                {
                    label_gas4_input.Content = combobox_gas4_name.Text;
                    textbox_gas4_input.IsEnabled = true;
                    gasCount++;
                }
                else
                {
                    label_gas4_input.Content = "气体4";
                    textbox_gas4_input.IsEnabled = false;
                }
                button_gas_input.IsEnabled = true;
                //锁定测量信息
                //LockMeasInfo(true);
                AddComlumns();
            }
            SetAverageTime();
            AlgorithmPro.GetInstance();
        }
        private void LockMeasInfo(bool lockText)
        {
            if (lockText)
            {
                text_mach_id.IsEnabled = false;
                text_room_id.IsEnabled = false;
                text_in_fine.IsEnabled = false;
                text_temp.IsEnabled = false;
                text_vol.IsEnabled = false;
                text_person.IsEnabled = false;

                text_instr_id.IsEnabled = false;
                text_light_id.IsEnabled = false;
                text_out_fine.IsEnabled = false;
                text_press.IsEnabled = false;
                text_times.IsEnabled = false;
                XePosition.IsEnabled = false;
                text_peak_position.IsEnabled = false;

                lrType.IsEnabled = false;
                sensorType.IsEnabled = false;
                pixel.IsEnabled = false;
                lightPath.IsEnabled = false;
                lightDistance.IsEnabled = false;
                gasChamberType.IsEnabled = false;

                button_info_import.IsEnabled = false;
            }
            else
            {
                text_mach_id.IsEnabled = true;
                text_room_id.IsEnabled = true;
                text_in_fine.IsEnabled = true;
                text_temp.IsEnabled = true;
                text_vol.IsEnabled = true;
                text_person.IsEnabled = true;

                text_instr_id.IsEnabled = true;
                text_light_id.IsEnabled = true;
                text_out_fine.IsEnabled = true;
                text_press.IsEnabled = true;
                text_times.IsEnabled = true;
                XePosition.IsEnabled = true;
                text_peak_position.IsEnabled = true;

                lrType.IsEnabled = true;
                sensorType.IsEnabled = true;
                pixel.IsEnabled = true;
                lightPath.IsEnabled = true;
                lightDistance.IsEnabled = true;
                gasChamberType.IsEnabled = true;

                button_info_import.IsEnabled = true;
            }
        }

        private void InitCombox()
        {
            List<ComboxSource> list = new List<ComboxSource>
            {
                new ComboxSource { ID=0, Name=""},
                new ComboxSource { ID = 22, Name = "N2" },
                new ComboxSource { ID = 8, Name = "NO" },
                new ComboxSource { ID = 9, Name = "SO2" },
                new ComboxSource { ID = 11, Name = "NH3" },
                new ComboxSource { ID = 36, Name = "NO2" },
                new ComboxSource { ID = 39, Name = "Cl2" },
                new ComboxSource { ID = 15, Name = "HCl" },
                new ComboxSource { ID = 31, Name = "H2S" }
            };
            combobox_gas1_name.ItemsSource = list;
            combobox_gas2_name.ItemsSource = list;
            combobox_gas3_name.ItemsSource = list;
            combobox_gas4_name.ItemsSource = list;
        }

        private void Button_finish_set_Click(object sender, RoutedEventArgs e)
        {
            if (CheckFinish(out string errorMessage))
            {
                InitPage(2);
            }
            else
            {
                MessageBox.Show(errorMessage, "错误提示", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CheckFinish(out string errorMessage)
        {
            errorMessage = string.Empty;
            bool flag = true;
            int gas1 = Convert.ToInt32(combobox_gas1_name.SelectedValue);
            int gas2 = Convert.ToInt32(combobox_gas2_name.SelectedValue);
            int gas3 = Convert.ToInt32(combobox_gas3_name.SelectedValue);
            int gas4 = Convert.ToInt32(combobox_gas4_name.SelectedValue);
            if (gas1 != 0)
            {
                if (gas1 == gas2 || gas1 == gas3 || gas1 == gas4)
                {
                    errorMessage = "气体1与其他选项重复！";
                    flag = false;
                }
            }
            if (gas2 != 0)
            {
                if (gas2 == gas3 || gas2 == gas4)
                {
                    errorMessage = "气体2与其他选项重复！";
                    flag = false;
                }
                else
                {
                    string gas2Text = textbox_gas2_ppm.Text;
                    if (string.IsNullOrEmpty(gas2Text))
                    {
                        errorMessage = "气体2数值未填写！";
                        flag = false;
                    }
                }
            }
            if (gas3 != 0)
            {
                if (gas3 == gas4)
                {
                    errorMessage = "气体3与其他选项重复！";
                    flag = false;
                }
                else
                {
                    string gas3Text = textbox_gas3_ppm.Text;
                    if (string.IsNullOrEmpty(gas3Text))
                    {
                        errorMessage = "气体3数值未填写！";
                        flag = false;
                    }
                }
            }
            if (gas4 != 0)
            {
                string gas4Text = textbox_gas4_ppm.Text;
                if (string.IsNullOrEmpty(gas4Text))
                {
                    errorMessage = "气体4数值未填写！";
                    flag = false;
                }
            }
            return flag;
        }

        private void Button_begin_set_Click(object sender, RoutedEventArgs e)
        {
            InitPage(1);
        }

        private void Button_cancel_set_Click(object sender, RoutedEventArgs e)
        {
            InitPage(2);
        }

        private void Combobox_gas2_name_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var itemValue = combobox_gas2_name.SelectedValue;
            if (itemValue == null || Convert.ToInt32(itemValue) == 0)
            {
                textbox_gas2_ppm.IsEnabled = false;
            }
            else
            {
                textbox_gas2_ppm.IsEnabled = true;
            }
        }

        private void Combobox_gas3_name_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var itemValue = combobox_gas3_name.SelectedValue;
            if (itemValue == null || Convert.ToInt32(itemValue) == 0)
            {
                textbox_gas3_ppm.IsEnabled = false;
            }
            else
            {
                textbox_gas3_ppm.IsEnabled = true;
            }
        }
        private void Combobox_gas4_name_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var itemValue = combobox_gas4_name.SelectedValue;
            if (itemValue == null || Convert.ToInt32(itemValue) == 0)
            {
                textbox_gas4_ppm.IsEnabled = false;
            }
            else
            {
                textbox_gas4_ppm.IsEnabled = true;
            }
        }
        /// <summary>
        /// 点击新增
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_gas_input_Click(object sender, RoutedEventArgs e)
        {
            float[] lineData = GetAverageData();
            if (lineData == null)
            {
                MessageBox.Show("没有光谱数据", "错误信息", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                double totalFlow = 0;
                List<string> list = new List<string>();
                int orderNumber = _obervableCollection.Count + 1;
                list.Add(Convert.ToString(orderNumber));
                list.Add("True");
                if (textbox_gas1_input.IsEnabled)
                {
                    string gas1 = textbox_gas1_input.Text;
                    if (string.IsNullOrEmpty(gas1))
                    {
                        MessageBox.Show("请填写气体流量信息！", "错误提示", MessageBoxButton.OK, MessageBoxImage.Error);
                        textbox_gas1_input.Focus();
                        return;
                    }
                    else
                    {
                        list.Add(gas1);
                        totalFlow += double.Parse(gas1);
                    }
                }
                if (textbox_gas2_input.IsEnabled)
                {
                    string gas2 = textbox_gas2_input.Text;
                    if (string.IsNullOrEmpty(gas2))
                    {
                        MessageBox.Show("请填写气体流量信息！", "错误提示", MessageBoxButton.OK, MessageBoxImage.Error);
                        textbox_gas2_input.Focus();
                        return;
                    }
                    else
                    {
                        list.Add(gas2);
                        totalFlow += double.Parse(gas2);
                    }
                }
                if (textbox_gas3_input.IsEnabled)
                {
                    string gas3 = textbox_gas3_input.Text;
                    if (string.IsNullOrEmpty(gas3))
                    {
                        MessageBox.Show("请填写气体流量信息！", "错误提示", MessageBoxButton.OK, MessageBoxImage.Error);
                        textbox_gas3_input.Focus();
                        return;
                    }
                    else
                    {
                        list.Add(gas3);
                        totalFlow += double.Parse(gas3);
                    }
                }
                if (textbox_gas4_input.IsEnabled)
                {
                    string gas4 = textbox_gas4_input.Text;
                    if (string.IsNullOrEmpty(gas4))
                    {
                        MessageBox.Show("请填写气体流量信息！", "错误提示", MessageBoxButton.OK, MessageBoxImage.Error);
                        textbox_gas4_input.Focus();
                        return;
                    }
                    else
                    {
                        list.Add(gas4);
                        totalFlow += double.Parse(gas4);
                    }
                }
                if (list != null && list.Count > 0)
                {
                    int count = list.Count - 2;
                    //添加浓度
                    for (int i = 0; i < count - 1; i++)
                    {
                        double result = 0;
                        double gasFlow = double.Parse(list[i + 3]);//除去序号、勾选和N2，从3开始
                        double gasPpm = 0;
                        if (i == 0)
                        {
                            gasPpm = double.Parse(textbox_gas2_ppm.Text.Trim());
                        }
                        if (i == 1)
                        {
                            gasPpm = double.Parse(textbox_gas3_ppm.Text.Trim());
                        }
                        if (i == 2)
                        {
                            gasPpm = double.Parse(textbox_gas4_ppm.Text.Trim());
                        }
                        if (totalFlow == 0)
                        {
                            result = 0;
                        }
                        else
                        {
                            result = gasPpm * (gasFlow / totalFlow);
                        }
                        list.Add(Convert.ToString(Math.Round(result, 2)));
                    }
                    //添加误差
                    for (int i = 0; i < count; i++)
                    {
                        list.Add(string.Empty);
                    }
                    _obervableCollection.Add(list.ToArray());
                    riDataMap.Add(orderNumber, lineData);
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (!algoPage.CreateAvgChart(orderNumber.ToString(), lineData))
                        {
                            ExceptionUtil.Instance.LogMethod("算法生成图表数据异常！");
                        }
                    }));
                }
                else
                {
                    MessageBox.Show("请填写气体流量信息！", "错误提示", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddComlumns()
        {
            dataGrid.Columns.Clear();
            dataGrid.IsEnabled = true;
            int i = 0;
            //序号
            dataGrid.Columns.Add(new DataGridTextColumn() { Header = "序号", Binding = new Binding("[" + i.ToString() + "]"), IsReadOnly = true, Width = 30 });
            i++;
            //勾选框
            dataGrid.Columns.Add(new DataGridCheckBoxColumn() { Header = "选择", Binding = new Binding("[" + i.ToString() + "]"), IsReadOnly = true, Width = 30 });
            i++;
            //流量
            if (textbox_gas1_input.IsEnabled)
            {
                dataGrid.Columns.Add(new DataGridTextColumn() { Header = label_gas1_input.Content + "流量", Binding = new Binding("[" + i.ToString() + "]"), IsReadOnly = true });
                i++;
            }
            if (textbox_gas2_input.IsEnabled)
            {
                dataGrid.Columns.Add(new DataGridTextColumn() { Header = label_gas2_input.Content + "流量", Binding = new Binding("[" + i.ToString() + "]"), IsReadOnly = true });
                i++;
            }
            if (textbox_gas3_input.IsEnabled)
            {
                dataGrid.Columns.Add(new DataGridTextColumn() { Header = label_gas3_input.Content + "流量", Binding = new Binding("[" + i.ToString() + "]"), IsReadOnly = true });
                i++;
            }
            if (textbox_gas4_input.IsEnabled)
            {
                dataGrid.Columns.Add(new DataGridTextColumn() { Header = label_gas4_input.Content + "流量", Binding = new Binding("[" + i.ToString() + "]"), IsReadOnly = true });
                i++;
            }
            _gasIndex = i;
            
            //浓度
            //if (textbox_gas1_input.IsEnabled)
            //{
            //    dataGrid.Columns.Add(new DataGridTextColumn() { Header = label_gas1_input.Content + "浓度", Binding = new Binding("[" + i.ToString() + "]"), IsReadOnly = true });
            //    i++;
            //}
            if (textbox_gas2_input.IsEnabled)
            {
                dataGrid.Columns.Add(new DataGridTextColumn() { Header = label_gas2_input.Content + "浓度", Binding = new Binding("[" + i.ToString() + "]"), IsReadOnly = true });
                i++;
            }
            if (textbox_gas3_input.IsEnabled)
            {
                dataGrid.Columns.Add(new DataGridTextColumn() { Header = label_gas3_input.Content + "浓度", Binding = new Binding("[" + i.ToString() + "]"), IsReadOnly = true });
                i++;
            }
            if (textbox_gas4_input.IsEnabled)
            {
                dataGrid.Columns.Add(new DataGridTextColumn() { Header = label_gas4_input.Content + "浓度", Binding = new Binding("[" + i.ToString() + "]"), IsReadOnly = true });
                i++;
            }
            //误差
            //if (textbox_gas1_input.IsEnabled)
            //{
            //    dataGrid.Columns.Add(new DataGridTextColumn() { Header = label_gas1_input.Content + "误差", Binding = new Binding("[" + i.ToString() + "]"), IsReadOnly = true });
            //    i++;
            //}
            if (textbox_gas2_input.IsEnabled)
            {
                dataGrid.Columns.Add(new DataGridTextColumn() { Header = label_gas2_input.Content + "误差", Binding = new Binding("[" + i.ToString() + "]"), IsReadOnly = true });
                i++;
            }
            if (textbox_gas3_input.IsEnabled)
            {
                dataGrid.Columns.Add(new DataGridTextColumn() { Header = label_gas3_input.Content + "误差", Binding = new Binding("[" + i.ToString() + "]"), IsReadOnly = true });
                i++;
            }
            if (textbox_gas4_input.IsEnabled)
            {
                dataGrid.Columns.Add(new DataGridTextColumn() { Header = label_gas4_input.Content + "误差", Binding = new Binding("[" + i.ToString() + "]"), IsReadOnly = true });
            }
            //设置显示
            int indexCount = _gasIndex - 2;
            if (checkbox_flow.IsChecked == false)
            {
                for (int n = 2; n < _gasIndex; n++)
                {
                    dataGrid.Columns[n].Visibility = Visibility.Hidden;
                }
            }
            if (checkbox_density.IsChecked == false)
            {
                for (int n = _gasIndex; n < 2 + (indexCount - 1) * 2; n++)
                {
                    dataGrid.Columns[n].Visibility = Visibility.Hidden;
                }
            }
            if (checkbox_error.IsChecked == false)
            {
                for (int n = indexCount * 2; n < 2 + (indexCount - 1) * 3; n++)
                {
                    dataGrid.Columns[n].Visibility = Visibility.Hidden;
                }
            }
            dataGrid.CanUserSortColumns = false;
            dataGrid.HorizontalAlignment = HorizontalAlignment.Center;
            dataGrid.ItemsSource = _obervableCollection;
        }

        private void Checkbox_flow_CheckChange(object sender, RoutedEventArgs e)
        {
            if (checkbox_flow.IsChecked == true)
            {
                for (int n = 2; n < _gasIndex; n++)
                {
                    dataGrid.Columns[n].Visibility = Visibility.Visible;
                }
            }
            else
            {
                for (int n = 2; n < _gasIndex; n++)
                {
                    dataGrid.Columns[n].Visibility = Visibility.Hidden;
                }
            }
        }

        private void Checkbox_density_CheckChange(object sender, RoutedEventArgs e)
        {
            int indexCount = _gasIndex - 2;
            if (checkbox_density.IsChecked == true)
            {
                for (int n = _gasIndex; n < (_gasIndex + indexCount - 1); n++)
                {
                    dataGrid.Columns[n].Visibility = Visibility.Visible;
                }
            }
            else
            {
                for (int n = _gasIndex; n < (_gasIndex + indexCount - 1); n++)
                {
                    dataGrid.Columns[n].Visibility = Visibility.Hidden;
                }
            }
        }

        private void Checkbox_error_CheckChange(object sender, RoutedEventArgs e)
        {
            int indexCount = _gasIndex - 2;
            if (checkbox_error.IsChecked == true)
            {
                for (int n = (_gasIndex + indexCount - 1); n < (_gasIndex + (indexCount - 1) * 2); n++)
                {
                    dataGrid.Columns[n].Visibility = Visibility.Visible;
                }
            }
            else
            {
                for (int n = (_gasIndex + indexCount - 1); n < (_gasIndex + (indexCount - 1) * 2); n++)
                {
                    dataGrid.Columns[n].Visibility = Visibility.Hidden;
                }
            }
        }

        private void Button_densityCalculate_Click(object sender, RoutedEventArgs e)
        {
            List<string[]> list = new List<string[]>();
            int indexCount = _gasIndex - 2;
            if (_obervableCollection != null && _obervableCollection.Count > 0)
            {
                foreach (string[] arrays in _obervableCollection)
                {
                    if (arrays != null && arrays.Length > 0)
                    {
                        string[] arraysNew = new string[_gasIndex + (indexCount - 1) * 2];
                        for (int i = 0; i < arraysNew.Length; i++)
                        {
                            if (i >= _gasIndex && i < _gasIndex + (indexCount - 1))
                            {
                                arraysNew[i] = Convert.ToString(Math.Round(ConcentrationCalc(arrays, i), 2));
                            }
                            else if (arrays.Length <= i)
                            {
                                arraysNew[i] = "";
                            }
                            else
                            {
                                arraysNew[i] = arrays[i];
                            }

                        }
                        list.Add(arraysNew);
                    }
                }
                _obervableCollection.Clear();
                foreach (string[] arrays in list)
                {
                    _obervableCollection.Add(arrays);
                }
            }
            else
            {
                MessageBox.Show("数据列表不能为空！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private double ConcentrationCalc(string[] arrays, int gasPosition)
        {
            double result = 0;
            int indexCount = _gasIndex - 2;
            for (int n = indexCount; n >= 0; n--)
            {
                double totalFlow = 0;
                double gasFlow = 0;
                double gasPpm = 0;
                for (int i = 2; i < _gasIndex; i++)
                {
                    string flow = arrays[i];
                    if (!string.IsNullOrEmpty(flow))
                    {
                        totalFlow += double.Parse(flow);
                        if ((gasPosition - indexCount) == i)
                        {
                            gasFlow = double.Parse(arrays[i + 1]);
                            if ((gasPosition - _gasIndex) == 0)
                            {
                                gasPpm = double.Parse(textbox_gas2_ppm.Text.Trim());
                            }
                            else if ((gasPosition - _gasIndex) == 1)
                            {
                                gasPpm = double.Parse(textbox_gas3_ppm.Text.Trim());
                            }
                            else if ((gasPosition - _gasIndex) == 2)
                            {
                                gasPpm = double.Parse(textbox_gas4_ppm.Text.Trim());
                            }
                        }
                    }

                }
                if (totalFlow == 0)
                {
                    result = 0;
                }
                else
                {
                    result = gasPpm * (gasFlow / totalFlow);
                }
            }

            return result;
        }

        private void Button_gas_import_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ExceptionUtil.Instance.ShowLoadingAction(true);
                OpenFileDialog op = new OpenFileDialog();
                op.InitialDirectory = System.Windows.Forms.Application.StartupPath + "\\ParameterGen\\"; ;//默认的打开路径
                if (importRoad != null)
                {
                    op.InitialDirectory = importRoad;
                }
                op.RestoreDirectory = true;
                op.Filter = " 文本文件(*.txt)|*.txt|所有文件(*.*)|*.* ";
                if (op.ShowDialog() == true)
                {
                    importRoad = op.FileName.Substring(0, op.FileName.LastIndexOf('\\'));
                    LoadSpecFile(op.FileName);
                }
            }
            finally
            {
                ExceptionUtil.Instance.ShowLoadingAction(false);
            }

        }

        private void LoadSpecFile(string fileName)
        {
            TextReader textReader = null;

            //开始设定
            this.Button_begin_set_Click(null, null);

            try
            {
                FileInfo file = new FileInfo(fileName);
                textReader = file.OpenText();
                string line = null;
                //read gas
                while ((line = textReader.ReadLine()) != null)
                {
                    if (line.Trim().Equals(HEAD_GAS))
                        break;
                }


                //读取所有气体
                int gasIndex = 0;
                while ((line = textReader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.Equals(HEAD_FLOW))
                        break;
                    string[] gas = this.parseLine(line);
                    if ((gas != null) && (gas.Length == 2))
                    {
                        switch (gasIndex)
                        {
                            case 0:
                                {
                                    this.combobox_gas1_name.Text = gas[0];
                                    textbox_gas1_ppm.Text = gas[1];
                                }
                                break;
                            case 1:
                                {
                                    this.combobox_gas2_name.Text = gas[0];
                                    textbox_gas2_ppm.Text = gas[1];
                                }
                                break;
                            case 2:
                                {
                                    this.combobox_gas3_name.Text = gas[0];
                                    textbox_gas3_ppm.Text = gas[1];
                                }
                                break;
                            case 3:
                                {
                                    this.combobox_gas4_name.Text = gas[0];
                                    textbox_gas4_ppm.Text = gas[1];
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        switch (gasIndex)
                        {
                            case 0:
                                {
                                    this.combobox_gas1_name.Text = "";
                                    textbox_gas1_ppm.Text = "";
                                }
                                break;
                            case 1:
                                {
                                    this.combobox_gas2_name.Text = "";
                                    textbox_gas2_ppm.Text = "";
                                }
                                break;
                            case 2:
                                {
                                    this.combobox_gas3_name.Text = "";
                                    textbox_gas3_ppm.Text = "";
                                }
                                break;
                            case 3:
                                {
                                    this.combobox_gas4_name.Text = "";
                                    textbox_gas4_ppm.Text = "";
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    if (++gasIndex >= GAS_NUMBER)
                        break;
                }

                //气体设置完成
                this.Button_finish_set_Click(null, null);

                ArrayList itemList = new ArrayList();

                _obervableCollection.Clear();
                while ((line = textReader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.Equals(HEAD_SPEC))
                        break;
                    string[] lineData = this.parseLine(line);
                    if ((lineData != null) && (lineData.Length > 2))
                    {
                        List<string> list = new List<string>(lineData);
                        _obervableCollection.Add(list.ToArray());
                    }
                }
                algoPage.RemoveAllSeries();
                algoPage.ImportHistoricalData(fileName, out riDataMap);
                //设置部分未选中数据隐藏曲线数据
                for (int i = 0; i < _obervableCollection.Count; i++)
                {
                    if (_obervableCollection[i][1].Equals("False"))
                    {
                        algoPage.RemoveSeriesByIndex(_obervableCollection[i][0]);
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("LoadSpecFile error : " + e.Message);
            }
            finally
            {
                if (textReader != null)
                {
                    textReader.Close();
                }
            }
        }

        private string[] parseLine(string line)
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
        /// 测量信息导入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_info_import_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.InitialDirectory = System.Windows.Forms.Application.StartupPath + "\\ParameterGen\\"; ;//默认的打开路径
            if (importRoad != null)
            {
                op.InitialDirectory = importRoad;
            }
            op.RestoreDirectory = true;
            op.Filter = " 文本文件(*.txt)|*.txt|所有文件(*.*)|*.* ";
            if (op.ShowDialog() == true)
            {
                importRoad = op.FileName.Substring(0, op.FileName.LastIndexOf('\\'));
                LoadParameterInfo(op.FileName);
            }
        }

        private void LoadParameterInfo(string fileName)
        {
            try
            {
                ParamInfo paramInfo = new ParamInfo();
                paramInfo.LoadParameterInfo(fileName);
                text_mach_id.Text = paramInfo.MachId.Trim();
                text_instr_id.Text = paramInfo.InstrId.Trim();
                text_temp.Text = paramInfo.Temp.Trim();
                text_press.Text = paramInfo.Press.Trim();
                text_in_fine.Text = paramInfo.InFine.Trim();
                text_out_fine.Text = paramInfo.OutFine.Trim();
                text_room_id.Text = paramInfo.RoomId.Trim();
                text_light_id.Text = paramInfo.LightId.Trim();
                text_vol.Text = paramInfo.Vol.Trim();
                text_times.Text = paramInfo.AvgTimes.Trim();
                text_person.Text = paramInfo.Person.Trim();
                lrType.SelectedIndex = paramInfo.LrType;
                sensorType.SelectedIndex = paramInfo.SensorType;
                pixel.SelectedIndex = paramInfo.Pixel;
                lightPath.SelectedIndex = paramInfo.LightPath;
                lightDistance.Text = paramInfo.LightDistance.Trim();
                gasChamberType.SelectedIndex = paramInfo.GasChamberType;
            }
            catch (Exception ex)
            {
                ExceptionUtil.Instance.LogMethod("加载参数错误，异常信息为：" + ex.ToString());
            }
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (this.dataGrid.CurrentCell.Item != null && dataGrid.CurrentCell.Item != DependencyProperty.UnsetValue)
            {
                try
                {
                    string[] str_array = (string[])dataGrid.CurrentCell.Item;
                    for (int i = 0; i < _obervableCollection.Count; i++)
                    {
                        if (_obervableCollection[i][0].Equals(str_array[0]))
                        {
                            if (_obervableCollection[i][1].Equals("True"))
                            {
                                string[] arrays = _obervableCollection[i];
                                arrays[1] = "False";
                                algoPage.RemoveSeriesByIndex(arrays[0]);
                                _obervableCollection.RemoveAt(i);
                                _obervableCollection.Insert(i, arrays);
                            }
                            else
                            {
                                string[] arrays = _obervableCollection[i];
                                arrays[1] = "True";
                                algoPage.RecoveryDataSeries(arrays[0]);
                                _obervableCollection.RemoveAt(i);
                                _obervableCollection.Insert(i, arrays);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ExceptionUtil.Instance.LogMethod("出现异常错误，信息为：" + ex.Message);
                }


            }
        }
        /// <summary>
        /// 生成参量
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_generateParameter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ExceptionUtil.Instance.ShowLoadingAction(true);
                //压力
                string press = text_press.Text.Trim();
                if (string.IsNullOrEmpty(press))
                {
                    MessageBox.Show("请输入压力参数！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                    text_press.Focus();
                    return;
                }
                //温度
                string temp = text_temp.Text.Trim();
                if (string.IsNullOrEmpty(temp))
                {
                    MessageBox.Show("请输入温度参数！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                    text_temp.Focus();
                    return;
                }
                //整机ID
                string matchId = text_mach_id.Text.Trim();
                if (string.IsNullOrEmpty(matchId))
                {
                    MessageBox.Show("请输入整机ID参数！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                    text_mach_id.Focus();
                    return;
                }
                //光谱仪ID
                string instrId = text_instr_id.Text.Trim();
                if (string.IsNullOrEmpty(instrId))
                {
                    MessageBox.Show("请输入光谱仪ID参数！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                    text_instr_id.Focus();
                    return;
                }
                MessageBoxResult result = MessageBox.Show("请确认只勾选一条零点曲线！", "提示", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result == MessageBoxResult.No)
                {
                    return;
                }
                //开始计算
                dataGrid.IsEnabled = false;
                double[,] V;
                double[,] E;
                //浓度矩阵,每行对应1次测量，每列对应1种气体
                float[,] thicknessData;
                //光谱矩阵,每行对应1次测量，每列对应1个象素
                float[,] riData;
                try
                {
                    GetThicknessAndRiData(out thicknessData, out riData);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("错误信息", ex.ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                //测量气体信息
                string[] gasName;
                string[] gasValue;
                ArrayList arrayList = GetGasNode(out gasName, out gasValue);

                AlgorithmPro.GetInstance().Process(out V, out E, thicknessData, riData, float.Parse(press), float.Parse(temp));

                int index = 0;
                for (int i = 0; i < _obervableCollection.Count; i++)
                {
                    string[] arrays = _obervableCollection[i];
                    if (arrays[1].Equals("True"))
                    {
                        int gasCount = _gasIndex - 2 - 1;
                        int start = _gasIndex + gasCount;
                        for (int j = 0; j < gasCount; j++)
                        {
                            arrays[start + j] = (Math.Round(E[index, j], 2)).ToString();
                        }
                        _obervableCollection.RemoveAt(i);
                        _obervableCollection.Insert(i, arrays);
                        index++;
                    }
                }
                Dictionary<string, List<string>> map = new Dictionary<string, List<string>>();
                string[] gasInfo = new string[] { instrId, lrType.SelectedIndex.ToString(), sensorType.SelectedIndex.ToString(), pixel.SelectedIndex.ToString(), (float.Parse(temp) - 273).ToString(), (float.Parse(press) / 1000).ToString(), lightPath.SelectedIndex.ToString(), lightDistance.Text, gasChamberType.SelectedIndex.ToString(), "0" };
                List<string> gas1, gas2, gas3;
                if (Convert.ToInt32(combobox_gas1_name.SelectedValue) != 0)
                {
                    gas1 = new List<string>(gasInfo)
                    {
                        gasRange2.SelectedIndex.ToString(),
                        combobox_gas2_name.SelectedValue.ToString()
                    };
                    map.Add(((ComboxSource)combobox_gas2_name.SelectedItem).Name, gas1);
                }
                if (Convert.ToInt32(combobox_gas2_name.SelectedValue) != 0)
                {
                    gas2 = new List<string>(gasInfo)
                    {
                        gasRange3.SelectedIndex.ToString(),
                        combobox_gas3_name.SelectedValue.ToString()
                    };
                    map.Add(((ComboxSource)combobox_gas3_name.SelectedItem).Name, gas2);
                }
                if (Convert.ToInt32(combobox_gas3_name.SelectedValue) != 0)
                {
                    gas3 = new List<string>(gasInfo)
                    {
                        gasRange4.SelectedIndex.ToString(),
                        combobox_gas4_name.SelectedValue.ToString()
                    };
                    map.Add(((ComboxSource)combobox_gas4_name.SelectedItem).Name, gas3);
                }

                //保存测量数据文件
                string path = AlgorithmPro.GetInstance().SaveParameter(V, E, matchId, instrId, arrayList, map);
                //保存光谱数据
                AlgorithmPro.GetInstance().SaveSpecData(path, gasName, gasValue, _obervableCollection, riDataMap);
                //保存参量数据
                SaveParameterInfo(path + "参量生成信息_" + matchId + ".txt");
                //提示信息
                if (IsMeasureSuccess(E))
                {
                    ExceptionUtil.Instance.LogMethod("生成参量已完成，输出地址为：" + path);
                    MessageBox.Show("生成参量已完成！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    ExceptionUtil.Instance.LogMethod("误差过大,请重试！");
                    MessageBox.Show("误差过大,请重试！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {

                ExceptionUtil.Instance.ExceptionMethod("生成参量数据异常，异常信息为：" + ex.ToString(), true);
            }
            finally
            {
                dataGrid.IsEnabled = true;
                ExceptionUtil.Instance.ShowLoadingAction(false);
            }
        }

        //判断测量是否在误差范围内
        public bool IsMeasureSuccess(double[,] E)
        {
            for (int j = 0; j < E.GetLength(1); j++)
            {
                for (int i = 0; i < E.GetLength(0); i++)
                {
                    if (E[i, j] > 2)
                        return false;
                }
            }
            return true;
        }

        public void SaveParameterInfo(string fileName)
        {
            TextWriter textWriter = null;
            try
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
                textWriter = File.CreateText(fileName);
                StringBuilder sb = new StringBuilder();
                sb.Append("整机ID: ").Append(text_mach_id.Text.Trim()).Append("\r\n");
                sb.Append("光谱仪ID: ").Append(text_instr_id.Text.Trim()).Append("\r\n");
                sb.Append("温度: ").Append(text_temp.Text.Trim()).Append("\r\n");
                sb.Append("压力: ").Append(text_press.Text.Trim()).Append("\r\n");
                sb.Append("输入光纤ID: ").Append(text_in_fine.Text.Trim()).Append("\r\n");
                sb.Append("输出光纤ID: ").Append(text_out_fine.Text.Trim()).Append("\r\n");
                sb.Append("气体室编号: ").Append(text_room_id.Text).Append("\r\n");
                sb.Append("氙灯ID: ").Append(text_light_id.Text.Trim()).Append("\r\n");
                sb.Append("电压: ").Append(text_vol.Text.Trim()).Append("\r\n");
                sb.Append("光谱平均次数: ").Append(text_times.Text.Trim()).Append("\r\n");
                sb.Append("实验人员: ").Append(text_person.Text.Trim()).Append("\r\n");
                sb.Append("光源类型: ").Append((lrType.SelectedItem as ComboBoxItem).Content).Append("\r\n");
                sb.Append("传感器类型: ").Append((sensorType.SelectedItem as ComboBoxItem).Content).Append("\r\n");
                sb.Append("像素: ").Append((pixel.SelectedItem as ComboBoxItem).Content).Append("\r\n");
                sb.Append("光路: ").Append((lightPath.SelectedItem as ComboBoxItem).Content).Append("\r\n");
                sb.Append("光程(mm): ").Append(lightDistance.Text.Trim()).Append("\r\n");
                sb.Append("气体室类型: ").Append((gasChamberType.SelectedItem as ComboBoxItem).Content).Append("\r\n");
                textWriter.Write(sb.ToString());
            }
            catch (Exception e)
            {
                ExceptionUtil.Instance.ExceptionMethod("测量信息参数异常：" + e.Message, true);
                throw e;
            }
            finally
            {
                if (textWriter != null)
                    textWriter.Close();
            }
        }

        private ArrayList GetGasNode(out string[] gasNameArray, out string[] gasValueArray)
        {
            ArrayList arrayList = new ArrayList();
            List<string> gasNameList = new List<string>();
            List<string> gasValueList = new List<string>();
            int i = 0;
            if (textbox_gas1_input.IsEnabled)
            {
                int gasIndex = i;
                string gasName = combobox_gas1_name.Text.Trim();
                string gasValue = textbox_gas1_ppm.Text.Trim();
                string weight = "";
                if ("N2".Equals(gasName, StringComparison.CurrentCultureIgnoreCase))
                {
                    weight = "28.01";
                }
                else if ("SO2".Equals(gasName, StringComparison.CurrentCultureIgnoreCase))
                {
                    weight = "64.07";
                }
                else if ("NO".Equals(gasName, StringComparison.CurrentCultureIgnoreCase))
                {
                    weight = "30.01";
                }
                else if ("HCl".Equals(gasName, StringComparison.CurrentCultureIgnoreCase))
                {
                    weight = "36.46";
                }
                else
                {
                    weight = "70.91";
                }
                gasNameList.Add(gasName);
                gasValueList.Add(gasValue);
                arrayList.Add(new GasNode(gasIndex, gasName, weight));
                i++;
            }
            if (textbox_gas2_input.IsEnabled)
            {
                int gasIndex = i;
                string gasName = combobox_gas2_name.Text.Trim();
                string gasValue = textbox_gas2_ppm.Text.Trim();
                string weight = "";
                if ("N2".Equals(gasName, StringComparison.CurrentCultureIgnoreCase))
                {
                    weight = "28.01";
                }
                else if ("SO2".Equals(gasName, StringComparison.CurrentCultureIgnoreCase))
                {
                    weight = "64.07";
                }
                else if ("NO".Equals(gasName, StringComparison.CurrentCultureIgnoreCase))
                {
                    weight = "30.01";
                }
                else if ("HCl".Equals(gasName, StringComparison.CurrentCultureIgnoreCase))
                {
                    weight = "36.46";
                }
                else
                {
                    weight = "70.91";
                }
                gasNameList.Add(gasName);
                gasValueList.Add(gasValue);
                arrayList.Add(new GasNode(gasIndex, gasName, weight));
                i++;
            }
            if (textbox_gas3_input.IsEnabled)
            {
                int gasIndex = i;
                string gasName = combobox_gas3_name.Text.Trim();
                string gasValue = textbox_gas3_ppm.Text.Trim();
                string weight = "";
                if ("N2".Equals(gasName, StringComparison.CurrentCultureIgnoreCase))
                {
                    weight = "28.01";
                }
                else if ("SO2".Equals(gasName, StringComparison.CurrentCultureIgnoreCase))
                {
                    weight = "64.07";
                }
                else if ("NO".Equals(gasName, StringComparison.CurrentCultureIgnoreCase))
                {
                    weight = "30.01";
                }
                else if ("HCl".Equals(gasName, StringComparison.CurrentCultureIgnoreCase))
                {
                    weight = "36.46";
                }
                else
                {
                    weight = "70.91";
                }
                gasNameList.Add(gasName);
                gasValueList.Add(gasValue);
                arrayList.Add(new GasNode(gasIndex, gasName, weight));
                i++;
            }
            if (textbox_gas4_input.IsEnabled)
            {
                int gasIndex = i;
                string gasName = combobox_gas4_name.Text.Trim();
                string gasValue = textbox_gas4_ppm.Text.Trim();
                string weight = "";
                if ("N2".Equals(gasName, StringComparison.CurrentCultureIgnoreCase))
                {
                    weight = "28.01";
                }
                else if ("SO2".Equals(gasName, StringComparison.CurrentCultureIgnoreCase))
                {
                    weight = "64.07";
                }
                else if ("NO".Equals(gasName, StringComparison.CurrentCultureIgnoreCase))
                {
                    weight = "30.01";
                }
                else if ("HCl".Equals(gasName, StringComparison.CurrentCultureIgnoreCase))
                {
                    weight = "36.46";
                }
                else
                {
                    weight = "70.91";
                }
                gasNameList.Add(gasName);
                gasValueList.Add(gasValue);
                arrayList.Add(new GasNode(gasIndex, gasName, weight));
            }
            gasNameArray = gasNameList.ToArray();
            gasValueArray = gasValueList.ToArray();
            return arrayList;
        }

        //获得浓度和光谱矩阵数据
        private void GetThicknessAndRiData(out float[,] thicknessData, out float[,] riData)
        {
            int measureCount = 0;
            int gasCount = _gasIndex - 2;
            foreach (string[] arrays in _obervableCollection)
            {
                if (arrays[1].Equals("true", StringComparison.CurrentCultureIgnoreCase))
                {
                    measureCount++;
                }
            }
            thicknessData = new float[measureCount, gasCount - 1];

            riData = new float[pixelSize, measureCount];//pixelNumber

            int index = 0;
            //循环每次测量数据
            foreach (string[] arrays in _obervableCollection)
            {
                if (arrays[1].Equals("true", StringComparison.CurrentCultureIgnoreCase))
                {
                    for (int j = 0; j < gasCount - 1; j++)
                    {
                        float oneMeasureThicknessData = float.Parse(arrays[(_gasIndex) + j]);
                        thicknessData[index, j] = oneMeasureThicknessData;
                    }
                    for (int j = 0; j < this.pixelSize; j++)//pixelNumber
                    {
                        foreach (int key in riDataMap.Keys)
                        {
                            if (Convert.ToString(key).Equals(arrays[0]))
                            {
                                riData[j, index] = riDataMap[key][j];

                            }
                        }
                    }
                    index++;
                }
            }
        }


        /// <summary>
        /// 得到一段光谱采集数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="command"></param>
        public void ImportCurrentData(object sender, ushort[] specData)
        {
            float[] currentData = Array.ConvertAll<ushort, float>(specData, new Converter<ushort, float>(UshortToFloat));
            Dispatcher.BeginInvoke(new Action(() =>
            {
                algoPage.CreateCurrentChart(currentData);
            }));
            ParseSpecData(currentData);
        }
        private float UshortToFloat(ushort us)
        {
            return float.Parse(us.ToString());
        }
        private void ParseSpecData(float[] data)
        {
            if (data != null && data.Length > 0)
            {
                int averageTime = GetAverageTime();
                if (pixelSize != data.Length)
                {
                    return;
                }
                else
                {
                    lock (dataList)
                    {
                        //if (dataList.Count > 0)
                        //{
                        //    float dist = GetDist(data, (float[])dataList[dataList.Count - 1]);
                        //    if (distCount < MAX_DIST_COUNT)
                        //    {
                        //        distArray[distCount++] = dist;
                        //    }
                        //    else
                        //    {
                        //        int size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(System.Single));
                        //        Buffer.BlockCopy(distArray, size, distArray, 0,
                        //            (MAX_DIST_COUNT - 1) * size);
                        //        distArray[MAX_DIST_COUNT - 1] = dist;
                        //    }
                        //}
                        dataList.Add(data);
                        if (dataList.Count > averageTime)
                        {
                            dataList.RemoveAt(0);
                        }
                    }
                }
            }
        }

        private float GetDist(float[] data1, float[] data2)
        {
            float total = 0;
            for (int i = 0; i < data1.Length; i++)
            {
                float temp = data1[i] - data2[i];
                total += temp * temp;
            }
            return (float)Math.Sqrt(total);
        }

        public float[] GetAverageData()
        {
            float[] data = new float[pixelSize];
            int averageTime = GetAverageTime();
            lock (dataList)
            {
                if (dataList.Count < averageTime || averageTime <= 0)
                {
                    return null;
                }
                for (int i = 0; i < pixelSize; i++)
                {
                    data[i] = 0;
                    for (int j = 0; j < averageTime; j++)
                    {
                        float tempData = ((float[])dataList[j])[i];
                        //tempData = SpecDataConvert.GetInstance().VolToInteg(tempData / 1000); 错误的算法
                        data[i] += tempData;
                    }
                    data[i] /= averageTime;
                }
            }
            return data;
        }

        public int GetAverageTime()
        {
            return this.averageTime;
        }

        public bool SetAverageTime()
        {
            try
            {
                this.averageTime = int.Parse(textbox_average_time.Text.Trim());
                return true;
            }
            catch
            {
                return false;
            }

        }

        private void Textbox_average_time_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetAverageTime();
        }

        private void Button_clearData_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult dr = MessageBox.Show("是否在清空数据", "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (dr == MessageBoxResult.OK)
            {
                _obervableCollection.Clear();
                riDataMap.Clear();
                algoPage.RemoveAllSeries();
            }
        }
    }
}
