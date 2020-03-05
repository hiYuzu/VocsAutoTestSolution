using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using VocsAutoTest.Tools;

namespace VocsAutoTest.Pages
{
    /// <summary>
    /// AlgoGeneraControlPage.xaml 的交互逻辑
    /// </summary>
    public partial class AlgoGeneraControlPage : Page
    {
        private ObservableCollection<string[]> _obervableCollection = new ObservableCollection<string[]>();
        private int _gasCount = 0;
        //选择文件默认地址
        private string importRoad = null;
        private const string HEAD_GAS = "GAS";
        private const string HEAD_FLOW = "FLOW";
        private const string HEAD_SPEC = "SPEC";
        private const int GAS_NUMBER = 4;//最多选择气体种类

        public AlgoGeneraControlPage()
        {
            InitializeComponent();
            InitPage(0);
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
                else {
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

        private void InitCombox() {
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
            else {
                MessageBox.Show(errorMessage,"错误提示",MessageBoxButton.OK,MessageBoxImage.Error);
            }
        }

        private bool CheckFinish(out string errorMessage) {
            errorMessage = string.Empty;
            bool flag = true;
            int gas1 = Convert.ToInt32(combobox_gas1_name.SelectedValue);
            int gas2 = Convert.ToInt32(combobox_gas2_name.SelectedValue);
            int gas3 = Convert.ToInt32(combobox_gas3_name.SelectedValue);
            int gas4 = Convert.ToInt32(combobox_gas4_name.SelectedValue);
            if (gas1 != 0) {
                if (gas1 == gas2 || gas1 == gas3 || gas1 == gas4) {
                    errorMessage = "气体1与其他选项重复！";
                    flag = false;
                }
            }
            if (gas2 != 0) {
                if (gas2 == gas3 || gas2 == gas4)
                {
                    errorMessage = "气体2与其他选项重复！";
                    flag = false;
                }
                else {
                    string gas2Text = textbox_gas2_ppm.Text;
                    if (string.IsNullOrEmpty(gas2Text)) {
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
                else {
                    string gas3Text = textbox_gas3_ppm.Text;
                    if (string.IsNullOrEmpty(gas3Text))
                    {
                        errorMessage = "气体3数值未填写！";
                        flag = false;
                    }
                }
            }
            if (gas4 != 0) {
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
            else {
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
            else {
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
            else {
                textbox_gas4_ppm.IsEnabled = true;
            }
        }

        private void Button_gas_input_Click(object sender, RoutedEventArgs e)
        {
            List<string> list = new List<string>();
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
                for (int i=0;i< count * 2;i++)
                {
                    list.Add(string.Empty);
                }
                _obervableCollection.Add(list.ToArray());
            }
            else {
                MessageBox.Show("请填写气体流量信息！", "错误提示", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void AddComlumns() {
            dataGrid.Columns.Clear();
            dataGrid.IsEnabled = true;
            int i = 0;
            //序号
            dataGrid.Columns.Add(new DataGridTextColumn() { Header = "序号", Binding = new Binding("[" + i.ToString() + "]")});
            i++;
            //勾选框
            dataGrid.Columns.Add(new DataGridCheckBoxColumn() { Header = "选择", Binding = new Binding("[" + i.ToString() + "]") });
            i++;
            //流量
            if (textbox_gas1_input.IsEnabled)
            {
                dataGrid.Columns.Add(new DataGridTextColumn() { Header = label_gas1_input.Content+"流量", Binding = new Binding("["+i.ToString()+"]") });
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
            if (checkbox_flow.IsChecked == false) {
                for (int n=0;n< _gasCount; n++)
                {
                    dataGrid.Columns[n].Visibility = Visibility.Hidden;
                }
            }
            if (checkbox_density.IsChecked == false) {
                for (int n = _gasCount; n < _gasCount*2; n++)
                {
                    dataGrid.Columns[n].Visibility = Visibility.Hidden;
                }
            }
            if (checkbox_error.IsChecked == false)
            {
                for (int n = _gasCount*2; n < _gasCount * 3; n++)
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
            else {
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
                for (int n = (beginIndex * 2+2); n < (beginIndex * 3+2); n++)
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
            List<String[]> list = new List<string[]>();
            Random rd = new Random();
            if (_obervableCollection != null && _obervableCollection.Count>0) {
                foreach (string[] arrays in _obervableCollection) {
                    if (arrays != null && arrays.Length>0) {
                        for (int i = _gasCount; i < _gasCount * 2; i++)
                        {
                            arrays[i] = rd.Next(100).ToString();
                        }
                        list.Add(arrays);
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
                        //this.uintLabels[gasIndex].Text = (GasNodeConfig.GetInstance().gasNodes[gas[0]] as GasNode).unit;
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

                while ((line = textReader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.Equals(HEAD_SPEC))
                        break;
                    string[] lineData = this.parseLine(line);
                    if ((lineData != null) && (lineData.Length > 2))
                    {
                        List<string> list = new List<string>(lineData);
                        int[] flowData = new int[(lineData.Length - 1) / 2];
                        for (int i = 0; i < flowData.Length; i++)
                        {
                            flowData[i] = System.Int32.Parse(lineData[2 + i]);
                        }
                        float[] thicknessData = new float[(lineData.Length - 3) / 2];
                        for (int i = 0; i < thicknessData.Length; i++)
                            thicknessData[i] = System.Single.Parse(lineData[thicknessData.Length + 3 + i]);
                        //itemList.Add(new ItemNode(lineData[0], lineData[1].Equals("True"),
                        //    new DataNode(flowData, thicknessData, null)));
                        _obervableCollection.Add(list.ToArray());
                    }
                }

                //光谱数据
                //读取编号行
                textReader.ReadLine();
                while ((line = textReader.ReadLine()) != null)
                {
                    line = line.Trim();
                    string[] lineData = this.parseLine(line);

                    if (lineData.Length == itemList.Count)
                    {
                        for (int i = 0; i < itemList.Count; i++)
                        {
                            //((ItemNode)itemList[i]).AddSpecData(lineData[i]);
                        }
                    }

                }

                for (int i = 0; i < itemList.Count; i++)
                {

                    //((ItemNode)itemList[i]).SetSpecData();
                }

                //if (itemList.Count > 0 && ((ItemNode)itemList[0]).dataNode.riData.Length > doasData.PixelSize)
                //{
                //    //开始设定
                //    this.btnStartSet_Click(null, null);
                //    FpiMessageBox.ShowError(CustomResource.ImpFileNotIsSpecData);
                //    return;
                //}
                //增加光谱数据
                //for (int i = 0; i < itemList.Count; i++)
                //{
                //    ItemNode itemNode = (ItemNode)itemList[i];
                //    this.AddOneMeasure(itemNode.dataNode.flowData, itemNode.dataNode.riData,
                //        itemNode.dataNode.thicknessData, false, itemNode.itemNumber, itemNode.itemChecked, this.GetColor(i));
                //}
                //画第i条曲线
                //for (int i = 1; i <= itemList.Count; i++)
                //{
                //    for (int j = 0; j < itemList.Count; j++)
                //    {
                //        ItemNode itemNode = (ItemNode)itemList[j];
                //        if (itemNode.itemNumber.Equals(i.ToString()))
                //        {
                //            ZedGraph.LineItem lineItem = this.specShowControl.AddMeasCurve(itemNode.itemNumber, itemNode.dataNode.riData, true, LineType.History, this.GetColor(i));
                //            lineItem.IsVisible = itemNode.itemChecked;
                //            break;
                //        }
                //    }
                //}
                //this.SetTopIndex(((ItemNode)itemList[0]).dataNode.riData);

                //this.btnGenParam.Enabled = true;
                //this.btnConc.Enabled = this.btnGenParam.Enabled;
                //this.cbxShowConc.Checked = true;
                //this.cbxShowError.Checked = true;
                //this.cbxShowFlux.Checked = true;

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

    }
}
