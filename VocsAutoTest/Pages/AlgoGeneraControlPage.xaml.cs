using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using VocsAutoTest.Algorithm;
using VocsAutoTest.Tools;
using VocsAutoTestBLL.Impl;
using VocsAutoTestBLL.Interface;
using VocsAutoTestBLL.Model;

namespace VocsAutoTest.Pages
{
    /// <summary>
    /// AlgoGeneraControlPage.xaml 的交互逻辑
    /// </summary>
    public partial class AlgoGeneraControlPage : Page
    {
        private ObservableCollection<string[]> _obervableCollection = new ObservableCollection<string[]>();//测量数据
        private Dictionary<int, float[]> riDataMap = new Dictionary<int, float[]>();//光谱数据
        private AlgoGeneraPage algoPage;
        private int _gasCount = 0;
        //选择文件默认地址
        private string importRoad = null;
        private const string HEAD_GAS = "GAS";
        private const string HEAD_FLOW = "FLOW";
        private const string HEAD_SPEC = "SPEC";
        private const int GAS_NUMBER = 4;//最多选择气体种类
        private int pixelNumber = 2048;//TODO 2048需要确认来源
        private int pixelSize = 512;//TODO 512需要确认来源
        //记录接收到的数据
        private const int MAX_DIST_COUNT = 60;
        private int distCount = 0;
        private float[] distArray = new float[MAX_DIST_COUNT];
        private ArrayList dataList = new ArrayList();
        //缓存
        private readonly SpecDataModel dataCache;

        public AlgoGeneraControlPage()
        {
            InitializeComponent();
            InitPage(0);
        }

        public AlgoGeneraControlPage(AlgoGeneraPage algoPage)
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
            AlgoGeneralImpl.Instance.AlgoDataEvent += new AlgoDataDelegate(ImportCurrentData);

        }

        private void InitPage(int optInt)
        {
            if (optInt == 0)
            {
                //初始化
                InitCombox();
                combobox_gas1_name.SelectedValue = 1;
                combobox_gas1_name.IsEnabled = false;
                textbox_gas1_ppm.IsEnabled = false;
                textbox_gas1_ppm.Text = "9999";
                combobox_gas2_name.SelectedValue = 2;
                combobox_gas2_name.IsEnabled = true;
                textbox_gas2_ppm.IsEnabled = true;
                combobox_gas3_name.SelectedValue = 3;
                combobox_gas3_name.IsEnabled = true;
                textbox_gas3_ppm.IsEnabled = true;
                combobox_gas4_name.SelectedValue = 0;
                combobox_gas4_name.IsEnabled = true;
                textbox_gas4_ppm.IsEnabled = false;
                button_begin_set.IsEnabled = false;
                button_finish_set.IsEnabled = true;
                button_cancel_set.IsEnabled = true;
            }
            else if (optInt == 1)
            {
                //开始设定
                combobox_gas1_name.IsEnabled = false;
                textbox_gas1_ppm.IsEnabled = false;
                combobox_gas2_name.IsEnabled = true;
                textbox_gas2_ppm.IsEnabled = true;
                combobox_gas3_name.IsEnabled = true;
                textbox_gas3_ppm.IsEnabled = true;
                combobox_gas4_name.IsEnabled = true;
                textbox_gas4_ppm.IsEnabled = true;
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
            }
            else if (optInt == 2)
            {
                //完成设定
                combobox_gas1_name.IsEnabled = false;
                textbox_gas1_ppm.IsEnabled = false;
                combobox_gas2_name.IsEnabled = false;
                textbox_gas2_ppm.IsEnabled = false;
                combobox_gas3_name.IsEnabled = false;
                textbox_gas3_ppm.IsEnabled = false;
                combobox_gas4_name.IsEnabled = false;
                textbox_gas4_ppm.IsEnabled = false;
                button_begin_set.IsEnabled = true;
                button_finish_set.IsEnabled = false;
                button_cancel_set.IsEnabled = false;
                if (Convert.ToInt32(combobox_gas1_name.SelectedValue) != 0)
                {
                    label_gas1_input.Content = combobox_gas1_name.Text;
                    textbox_gas1_input.IsEnabled = true;
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
                }
                else
                {
                    label_gas4_input.Content = "气体4";
                    textbox_gas4_input.IsEnabled = false;
                }
                AddComlumns();

            }
        }

        private void InitCombox()
        {
            List<ComboxSource> list = new List<ComboxSource>
            {
                new ComboxSource { ID=0,Name=""},
                new ComboxSource { ID = 1, Name = "N2" },
                new ComboxSource { ID = 2, Name = "SO2" },
                new ComboxSource { ID = 3, Name = "NO" },
                new ComboxSource { ID = 4, Name = "Cl2(low)" },
                new ComboxSource { ID = 5, Name = "Cl2(high)" },
                new ComboxSource { ID = 6, Name = "HC1" },
                new ComboxSource { ID = 7, Name = "H2S" }
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

        private void Button_gas_input_Click(object sender, RoutedEventArgs e)
        {
            float[] lineData = GetAverageData();
            if (lineData == null)
            {
                MessageBox.Show("没有光谱数据", "错误信息", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else {
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
                    }
                }
                if (list != null && list.Count > 0)
                {
                    int count = list.Count;
                    for (int i = 0; i < count * 2; i++)
                    {
                        list.Add(string.Empty);
                    }
                    _obervableCollection.Add(list.ToArray());
                    riDataMap.Add(orderNumber, lineData);
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        CreateCurrentChart();
                    }));
                }
                else
                {
                    MessageBox.Show("请填写气体流量信息！", "错误提示", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// 绘制当前数据线
        /// </summary>
        public void CreateCurrentChart()
        {
            //TODO 绘制图表数据
            //chart.AxesX[0].Title = XAxisTitle;
            //chart.AxesY[0].Title = YAxisTitle;
            //if (currentDataSeries != null)
            //{
            //    chart.Series.Remove(currentDataSeries);
            //}
            //currentDataSeries = SetDataSeries(new List<string>(currentData), -1);
            //chart.Series.Add(currentDataSeries);
        }

        private void AddComlumns()
        {
            dataGrid.Columns.Clear();
            dataGrid.IsEnabled = true;
            int i = 0;
            //序号
            dataGrid.Columns.Add(new DataGridTextColumn() { Header = "序号", Binding = new Binding("[" + i.ToString() + "]") });
            i++;
            //勾选框
            dataGrid.Columns.Add(new DataGridCheckBoxColumn() { Header = "选择", Binding = new Binding("[" + i.ToString() + "]") });
            i++;
            //流量
            if (textbox_gas1_input.IsEnabled)
            {
                dataGrid.Columns.Add(new DataGridTextColumn() { Header = label_gas1_input.Content + "流量", Binding = new Binding("[" + i.ToString() + "]") });
                i++;
            }
            if (textbox_gas2_input.IsEnabled)
            {
                dataGrid.Columns.Add(new DataGridTextColumn() { Header = label_gas2_input.Content + "流量", Binding = new Binding("[" + i.ToString() + "]") });
                i++;
            }
            if (textbox_gas3_input.IsEnabled)
            {
                dataGrid.Columns.Add(new DataGridTextColumn() { Header = label_gas3_input.Content + "流量", Binding = new Binding("[" + i.ToString() + "]") });
                i++;
            }
            if (textbox_gas4_input.IsEnabled)
            {
                dataGrid.Columns.Add(new DataGridTextColumn() { Header = label_gas4_input.Content + "流量", Binding = new Binding("[" + i.ToString() + "]") });
                i++;
            }
            _gasCount = i;
            //浓度
            if (textbox_gas1_input.IsEnabled)
            {
                dataGrid.Columns.Add(new DataGridTextColumn() { Header = label_gas1_input.Content + "浓度", Binding = new Binding("[" + i.ToString() + "]") });
                i++;
            }
            if (textbox_gas2_input.IsEnabled)
            {
                dataGrid.Columns.Add(new DataGridTextColumn() { Header = label_gas2_input.Content + "浓度", Binding = new Binding("[" + i.ToString() + "]") });
                i++;
            }
            if (textbox_gas3_input.IsEnabled)
            {
                dataGrid.Columns.Add(new DataGridTextColumn() { Header = label_gas3_input.Content + "浓度", Binding = new Binding("[" + i.ToString() + "]") });
                i++;
            }
            if (textbox_gas4_input.IsEnabled)
            {
                dataGrid.Columns.Add(new DataGridTextColumn() { Header = label_gas4_input.Content + "浓度", Binding = new Binding("[" + i.ToString() + "]") });
                i++;
            }
            //误差
            if (textbox_gas1_input.IsEnabled)
            {
                dataGrid.Columns.Add(new DataGridTextColumn() { Header = label_gas1_input.Content + "误差", Binding = new Binding("[" + i.ToString() + "]") });
                i++;
            }
            if (textbox_gas2_input.IsEnabled)
            {
                dataGrid.Columns.Add(new DataGridTextColumn() { Header = label_gas2_input.Content + "误差", Binding = new Binding("[" + i.ToString() + "]") });
                i++;
            }
            if (textbox_gas3_input.IsEnabled)
            {
                dataGrid.Columns.Add(new DataGridTextColumn() { Header = label_gas3_input.Content + "误差", Binding = new Binding("[" + i.ToString() + "]") });
                i++;
            }
            if (textbox_gas4_input.IsEnabled)
            {
                dataGrid.Columns.Add(new DataGridTextColumn() { Header = label_gas4_input.Content + "误差", Binding = new Binding("[" + i.ToString() + "]") });
            }
            //设置显示
            if (checkbox_flow.IsChecked == false)
            {
                for (int n = 0; n < _gasCount; n++)
                {
                    dataGrid.Columns[n].Visibility = Visibility.Hidden;
                }
            }
            if (checkbox_density.IsChecked == false)
            {
                for (int n = _gasCount; n < _gasCount * 2; n++)
                {
                    dataGrid.Columns[n].Visibility = Visibility.Hidden;
                }
            }
            if (checkbox_error.IsChecked == false)
            {
                for (int n = _gasCount * 2; n < _gasCount * 3; n++)
                {
                    dataGrid.Columns[n].Visibility = Visibility.Hidden;
                }
            }
            dataGrid.IsReadOnly = true;
            dataGrid.ItemsSource = _obervableCollection;
        }

        private void Checkbox_flow_CheckChange(object sender, RoutedEventArgs e)
        {
            if (checkbox_flow.IsChecked == true)
            {
                for (int n = 2; n < _gasCount; n++)
                {
                    dataGrid.Columns[n].Visibility = Visibility.Visible;
                }
            }
            else
            {
                for (int n = 2; n < _gasCount; n++)
                {
                    dataGrid.Columns[n].Visibility = Visibility.Hidden;
                }
            }
        }

        private void Checkbox_density_CheckChange(object sender, RoutedEventArgs e)
        {
            int indexCount = _gasCount - 2;
            if (checkbox_density.IsChecked == true)
            {
                for (int n = _gasCount; n < (indexCount * 2 + 2); n++)
                {
                    dataGrid.Columns[n].Visibility = Visibility.Visible;
                }
            }
            else
            {
                for (int n = _gasCount; n < (indexCount * 2 + 2); n++)
                {
                    dataGrid.Columns[n].Visibility = Visibility.Hidden;
                }
            }
        }

        private void Checkbox_error_CheckChange(object sender, RoutedEventArgs e)
        {
            int beginIndex = _gasCount - 2;
            if (checkbox_error.IsChecked == true)
            {
                for (int n = (beginIndex * 2 + 2); n < (beginIndex * 3 + 2); n++)
                {
                    dataGrid.Columns[n].Visibility = Visibility.Visible;
                }
            }
            else
            {
                for (int n = (beginIndex * 2 + 2); n < (beginIndex * 3 + 2); n++)
                {
                    dataGrid.Columns[n].Visibility = Visibility.Hidden;
                }
            }
        }

        private void Button_densityCalculate_Click(object sender, RoutedEventArgs e)
        {
            List<string[]> list = new List<string[]>();
            Random rd = new Random();
            int indexCount = _gasCount - 2;
            if (_obervableCollection != null && _obervableCollection.Count > 0)
            {
                foreach (string[] arrays in _obervableCollection)
                {
                    if (arrays != null && arrays.Length > 0)
                    {
                        string[] arraysNew = new string[indexCount * 2 + 2];
                        for (int i = 0; i < (indexCount * 2 + 2); i++)
                        {
                            if (i < _gasCount)
                            {
                                arraysNew[i] = arrays[i];
                            }
                            else
                            {
                                arraysNew[i] = rd.Next(100).ToString();
                            }

                        }
                        list.Add(arraysNew);
                    }
                }
            }
            _obervableCollection.Clear();
            foreach (string[] arrays in list)
            {
                _obervableCollection.Add(arrays);
            }

        }

        private void Button_gas_import_Click(object sender, RoutedEventArgs e)
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
                LoadSpecFile(op.FileName);
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
                algoPage.ImportHistoricalData(fileName);
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
                    textReader.Close();
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
                txt_room_id.Text = paramInfo.RoomId.Trim();
                text_light_id.Text = paramInfo.LightId.Trim();
                text_vol.Text = paramInfo.Vol.Trim();
                text_times.Text = paramInfo.AvgTimes.Trim();
                text_person.Text = paramInfo.Person.Trim();
            }
            catch (Exception e)
            {
                //FpiMessageBox.ShowError(CustomResource.ImpFileErr + e.Message);
            }
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (this.dataGrid.CurrentCell.Item != null)
            {

                string[] str_array = (string[])dataGrid.CurrentCell.Item;
                for (int i = 0; i < _obervableCollection.Count; i++)
                {
                    for (int j = 0; j < _obervableCollection[i].Length; j++)
                    {
                        if (_obervableCollection[i][j].Equals(str_array[0]))
                        {
                            if (_obervableCollection[i][j + 1].Equals("True"))
                            {
                                _obervableCollection[i][j + 1] = "False";
                                algoPage.RemoveSeriesByIndex(_obervableCollection[i][j]);
                            }
                            else
                            {
                                _obervableCollection[i][j + 1] = "True";
                                algoPage.RecoveryDataSeries(_obervableCollection[i][j]);
                            }
                        }
                    }
                }

            }
            List<string[]> list = new List<string[]>();
            foreach (string[] arrays in _obervableCollection)
            {
                list.Add(arrays);
            }
            _obervableCollection.Clear();
            foreach (string[] arrays in list)
            {
                _obervableCollection.Add(arrays);
            }
        }

        private void Button_generateParameter_Click(object sender, RoutedEventArgs e)
        {
            dataGrid.IsEnabled = false;
            double[,] V;
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

            //压力
            string press = text_press.Text.Trim();
            //温度
            string temp = text_temp.Text.Trim();
            try
            {
                AlgorithmPro.GetInstance().Process(out V, thicknessData, riData, float.Parse(press), float.Parse(temp));
            }
            catch (Exception ex)
            {
                return;
            }

            dataGrid.IsEnabled = true;
        }

        //获得浓度和光谱矩阵数据
        private void GetThicknessAndRiData(out float[,] thicknessData, out float[,] riData)
        {
            int measureCount = 0;
            foreach (string[] arrays in _obervableCollection)
            {
                if (arrays[1].Equals("true", StringComparison.CurrentCultureIgnoreCase))
                {
                    measureCount++;
                }
            }
            thicknessData = new float[measureCount, _gasCount - 2];

            riData = new float[measureCount, pixelSize];//pixelNumber

            int index = 0;
            //循环每次测量数据
            foreach (string[] arrays in _obervableCollection)
            {
                if (arrays[1].Equals("true", StringComparison.CurrentCultureIgnoreCase))
                {
                    for (int j = 0; j < _gasCount - 2; j++)
                    {
                        float oneMeasureThicknessData = float.Parse(arrays[(_gasCount - 2 - 1) + j]);
                        thicknessData[index, j] = oneMeasureThicknessData;
                    }
                    foreach (int key in riDataMap.Keys)
                    {
                        if (key.Equals(arrays[0]))
                        {
                            for (int j = 0; j < this.pixelSize; j++)//pixelNumber
                            {
                                riData[index, j] = riDataMap[key][j];
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
                else {
                    lock (dataList)
                    {
                        if (dataList.Count > 0)
                        {
                            float dist = GetDist(data, (float[])dataList[dataList.Count - 1]);
                            if (distCount < MAX_DIST_COUNT)
                            {
                                distArray[distCount++] = dist;
                            }
                            else
                            {
                                int size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(System.Single));
                                Buffer.BlockCopy(distArray, size, distArray, 0,
                                    (MAX_DIST_COUNT - 1) * size);
                                distArray[MAX_DIST_COUNT - 1] = dist;
                            }
                        }

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
                if (dataList.Count < averageTime)
                {
                    return null;
                }
                for (int i = 0; i < pixelSize; i++)
                {
                    data[i] = 0;
                    for (int j = 0; j < averageTime; j++)
                    {
                        data[i] += ((float[])dataList[j])[i];
                    }
                    data[i] /= averageTime;
                }
            }
            return data;
        }

        private int GetAverageTime() {
            int averageTime;
            if (!int.TryParse(textbox_average_time.Text.Trim(), out averageTime))
            {
                averageTime = 5;
            }
            return averageTime;
        }


    }
}
