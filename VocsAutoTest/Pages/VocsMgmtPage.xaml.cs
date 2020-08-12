using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using VocsAutoTestBLL;
using VocsAutoTestBLL.Impl;
using VocsAutoTestCOMM;

namespace VocsAutoTest.Pages
{
    /// <summary>
    /// VocsMgmtPage.xaml 的交互逻辑
    /// </summary>
    public partial class VocsMgmtPage : Page
    {
        public VocsMgmtPage()
        {
            InitializeComponent();
        }
        private string currentFilePath = "";
        #region 公共参数
        private void ReadCommParam_Click(object sender, RoutedEventArgs e)
        {
            DataForward.Instance.ReadCommParam += new DataForwardDelegate(SetComm);
            SuperSerialPort.Instance.Send(new Command {Cmn = "20", ExpandCmn = "55", Data = "" }, true);
        }

        private void SetComm(object sender, Command command)
        {
            byte[] data = ByteStrUtil.HexToByte(command.Data);
            Dispatcher.Invoke(new Action(() =>
            {
                //解析光源信息字
                string[] lightSource = ByteToBinary(data[0]);
                meamtObjType.SelectedIndex = int.Parse(lightSource[7]);
                lPType.SelectedIndex = int.Parse(lightSource[6]);
                pixel.SelectedIndex = Convert.ToInt32(lightSource[4] + lightSource[5] ,2);
                sensorType.SelectedIndex = int.Parse(lightSource[3]);
                lSType.SelectedIndex = int.Parse(lightSource[2]);
                //设置控制电压参数
                byte[] vol = new byte[4];
                Array.Copy(data, 1, vol, 0, 4);
                Array.Reverse(vol, 0, vol.Length);
                lSControlVol.Text = BitConverter.ToSingle(vol, 0).ToString("f1");
                //解析双光路应用模式信息字
                string[] appModeInfo = ByteToBinary(Convert.ToByte(data[5]));
                dualLPAppModeInfo.SelectedIndex = Convert.ToInt32(appModeInfo[6] + appModeInfo[7], 2);
                //调零对应浓度计算次数
                byte[] calTimes = new byte[2];
                Array.Copy(data, 6, calTimes, 0, 2);
                Array.Reverse(calTimes, 0, calTimes.Length);
                zeroConcCalTimes.Text = BitConverter.ToUInt16(calTimes, 0).ToString();
            }));
            ExceptionUtil.Instance.LogMethod("读取光谱仪公共参数成功");
        }

        private void SetCommParam_Click(object sender, RoutedEventArgs e)
        {
            string data = GetCommParamData();
            if(data != "")
            {
                SuperSerialPort.Instance.Send(new Command { Cmn = "20", ExpandCmn = "66", Data = data }, true);
            }
        }

        private string GetCommParamData()
        {
            try
            {
                //光源信息字 1字节
                string meamtObjTypeB = Convert.ToString(meamtObjType.SelectedIndex, 2);
                string lPTypeB = Convert.ToString(lPType.SelectedIndex, 2);
                string pixelB = Convert.ToString(pixel.SelectedIndex, 2);
                if (pixelB.Length == 1)
                {
                    pixelB = "0" + pixelB;
                }
                string sensorTypeB = Convert.ToString(sensorType.SelectedIndex, 2);
                string lSTypeB = Convert.ToString(lSType.SelectedIndex, 2);
                string lightSource = Convert.ToByte(lSTypeB + sensorTypeB + pixelB + lPTypeB + meamtObjTypeB, 2).ToString("x2").ToUpper();
                //光源控制电压 4字节
                byte[] controlVol = BitConverter.GetBytes(Convert.ToSingle(lSControlVol.Text));
                Array.Reverse(controlVol);
                //应用模式信息字 1字节
                string appModeInfoB = Convert.ToString(dualLPAppModeInfo.SelectedIndex, 2);
                string appModeInfo = Convert.ToByte(appModeInfoB, 2).ToString("x2").ToUpper();
                //调零次数 2字节
                byte[] calTimes = BitConverter.GetBytes(Convert.ToUInt16(zeroConcCalTimes.Text));
                Array.Reverse(calTimes);
                return lightSource + ByteStrUtil.ByteToHexStr(controlVol) + appModeInfo + ByteStrUtil.ByteToHexStr(calTimes);
            }
            catch
            {
                MessageBox.Show("请检查输入参数！");
                return "";
            }
        }
        #endregion

        #region 所有光路参数
        private void ReadLPParam_Click(object sender, RoutedEventArgs e)
        {
            DataForward.Instance.ReadLPParam += new DataForwardDelegate(SetLightPath);
            SuperSerialPort.Instance.Send(new Command { Cmn = "21", ExpandCmn = "55", Data = alpp_lPInfo.SelectedIndex.ToString("x2") + " 0F" }, true);
        }

        private void SetLightPath(object sender, Command command)
        {
            byte[] data = ByteStrUtil.HexToByte(command.Data);
            if ("0F".Equals(data[1].ToString("x2").ToUpper()))
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    //光路信息，组分信息
                    alpp_lPInfo.SelectedIndex = data[0];
                    compntInfo.SelectedIndex = data[2];
                    //量程信息
                    string[] rangeInfo_1 = ByteToBinary(data[3]);
                    gas_liquid_1_range.SelectedIndex = int.Parse(rangeInfo_1[7]);
                    gas_liquid_2_range.SelectedIndex = int.Parse(rangeInfo_1[4]);
                    string[] rangeInfo_2 = ByteToBinary(data[4]);
                    gas_liquid_3_range.SelectedIndex = int.Parse(rangeInfo_2[7]);
                    gas_liquid_4_range.SelectedIndex = int.Parse(rangeInfo_1[4]);
                    //光谱平均次数，浓度滑动平均次数，零点平均次数，标定平均次数，打灯次数
                    specATs.Text = data[5].ToString();
                    concSlidATs.Text = data[6].ToString();
                    zeroATs.Text = data[7].ToString();
                    caliATs.Text = data[8].ToString();
                    lightTimes.Text = data[9].ToString();
                    //积分时间，采样时间间隔，光路切换时间间隔
                    byte[] integrTime = new byte[2];
                    Array.Copy(data, 10, integrTime, 0, 2);
                    Array.Reverse(integrTime, 0, integrTime.Length);
                    integTime.Text = BitConverter.ToUInt16(integrTime, 0).ToString();
                    byte[] samplInter = new byte[2];
                    Array.Copy(data, 12, samplInter, 0, 2);
                    Array.Reverse(samplInter, 0, samplInter.Length);
                    samplInterval.Text = BitConverter.ToUInt16(samplInter, 0).ToString();
                    byte[] lPSwitchInter = new byte[2];
                    Array.Copy(data, 14, lPSwitchInter, 0, 2);
                    Array.Reverse(lPSwitchInter, 0, lPSwitchInter.Length);
                    lPSwitchInterval.Text = BitConverter.ToUInt16(lPSwitchInter, 0).ToString();
                }));
                ExceptionUtil.Instance.LogMethod("读取光谱仪所有光路参数成功");
            }       
        }
        private void SetLPParam_Click(object sender, RoutedEventArgs e)
        {
            string data = GetLPParamData();
            if (data != "")
            {
                SuperSerialPort.Instance.Send(new Command { Cmn = "21", ExpandCmn = "66", Data = data }, true);
            }
        }
        private string GetLPParamData()
        {
            try
            {
                //光路信息 1字节
                string lPInfo = "0" + alpp_lPInfo.SelectedIndex.ToString();
                //组分信息 1字节
                string compntInfoB = Convert.ToString(compntInfo.SelectedIndex, 2);
                string comptInfo = Convert.ToByte(compntInfoB, 2).ToString("x2").ToUpper();
                //量程信息字一 1字节
                string range_1 = Convert.ToString(gas_liquid_1_range.SelectedIndex, 2);
                string range_2 = Convert.ToString(gas_liquid_2_range.SelectedIndex, 2);
                string rangeInfo1 = Convert.ToByte("00" + range_2 + "00" + range_1, 2).ToString("x2").ToUpper();
                //量程信息字二 1字节
                string range_3 = Convert.ToString(gas_liquid_3_range.SelectedIndex, 2);
                string range_4 = Convert.ToString(gas_liquid_4_range.SelectedIndex, 2);
                string rangeInfo2 = Convert.ToByte("00" + range_4 + "00" + range_3, 2).ToString("x2").ToUpper();
                //光谱平均次数，浓度滑动平均次数，零点平均次数，标定平均次数，打灯次数 各1字节
                string sATs = Convert.ToByte(specATs.Text).ToString("x2");
                string coATs = Convert.ToByte(concSlidATs.Text).ToString("x2");
                string zATs = Convert.ToByte(zeroATs.Text).ToString("x2");
                string caATs = Convert.ToByte(caliATs.Text).ToString("x2");
                string lTs = Convert.ToByte(lightTimes.Text).ToString("x2");
                //积分时间，采样时间间隔，光路切换时间间隔 各2字节
                byte[] integrTimes = BitConverter.GetBytes(Convert.ToUInt16(integTime.Text));
                Array.Reverse(integrTimes);
                byte[] samplInter = BitConverter.GetBytes(Convert.ToUInt16(samplInterval.Text));
                Array.Reverse(samplInter);
                byte[] lPSwitchInter = BitConverter.GetBytes(Convert.ToUInt16(lPSwitchInterval.Text));
                Array.Reverse(lPSwitchInter);
                return lPInfo + "0F" + comptInfo + rangeInfo1 + rangeInfo2 + sATs + coATs + zATs + caATs + lTs + ByteStrUtil.ByteToHexStr(integrTimes) + ByteStrUtil.ByteToHexStr(samplInter) + ByteStrUtil.ByteToHexStr(lPSwitchInter);
            }
            catch
            {
                MessageBox.Show("请检查输入参数！");
                return "";
            }
        }
        #endregion

        #region 量程切换判据
        private void ReadRangeSwitch_Click(object sender, RoutedEventArgs e)
        {
            DataForward.Instance.ReadRangeSwitch += new DataForwardDelegate(SetRange);
            SuperSerialPort.Instance.Send(new Command { Cmn = "26", ExpandCmn = "55", Data = rs_lpInfo.SelectedIndex.ToString("x2") + " 0F" }, true);
        }

        private void SetRange(object sender, Command command)
        {
            byte[] data = ByteStrUtil.HexToByte(command.Data);
            Dispatcher.Invoke(new Action(() =>
            {
                //设置气体1，2，3，4判据数据
                byte[] gas_1_crit = new byte[4];
                Array.Copy(data, 2, gas_1_crit, 0, 4);
                Array.Reverse(gas_1_crit, 0, gas_1_crit.Length);
                gas_1_critData.Text = BitConverter.ToSingle(gas_1_crit, 0).ToString();

                byte[] gas_2_crit = new byte[4];
                Array.Copy(data, 6, gas_2_crit, 0, 4);
                Array.Reverse(gas_2_crit, 0, gas_2_crit.Length);
                gas_2_critData.Text = BitConverter.ToSingle(gas_2_crit, 0).ToString();

                byte[] gas_3_crit = new byte[4];
                Array.Copy(data, 10, gas_3_crit, 0, 4);
                Array.Reverse(gas_3_crit, 0, gas_3_crit.Length);
                gas_3_critData.Text = BitConverter.ToSingle(gas_3_crit, 0).ToString();

                byte[] gas_4_crit = new byte[4];
                Array.Copy(data, 14, gas_4_crit, 0, 4);
                Array.Reverse(gas_4_crit, 0, gas_4_crit.Length);
                gas_4_critData.Text = BitConverter.ToSingle(gas_4_crit, 0).ToString();
            }));
            ExceptionUtil.Instance.LogMethod("读取光谱仪量程切换判据成功");
        }
        private void SetRangeSwitch_Click(object sender, RoutedEventArgs e)
        {
            string data = GetRangeSwitchData();
            if (data != "")
            {
                SuperSerialPort.Instance.Send(new Command { Cmn = "26", ExpandCmn = "66", Data = data }, true);
            }
        }
        private string GetRangeSwitchData()
        {
            try
            {
                //气体1，2，3，4判据数据 4*4字节
                byte[] gas_1_crit = BitConverter.GetBytes(Convert.ToSingle(gas_1_critData.Text));
                Array.Reverse(gas_1_crit);
                byte[] gas_2_crit = BitConverter.GetBytes(Convert.ToSingle(gas_2_critData.Text));
                Array.Reverse(gas_2_crit);
                byte[] gas_3_crit = BitConverter.GetBytes(Convert.ToSingle(gas_3_critData.Text));
                Array.Reverse(gas_3_crit);
                byte[] gas_4_crit = BitConverter.GetBytes(Convert.ToSingle(gas_4_critData.Text));
                Array.Reverse(gas_4_crit);
                return rs_lpInfo.SelectedIndex.ToString("x2") + "0F" + ByteStrUtil.ByteToHexStr(gas_1_crit) + ByteStrUtil.ByteToHexStr(gas_2_crit) + ByteStrUtil.ByteToHexStr(gas_3_crit) + ByteStrUtil.ByteToHexStr(gas_4_crit);
            }
            catch
            {
                MessageBox.Show("请检查输入参数！");
                return "";
            }
        }
        #endregion

        #region 零点系数
        private void ReadZeroCoeffi_Click(object sender, RoutedEventArgs e)
        {
            DataForward.Instance.ReadZeroParam += new DataForwardDelegate(SetZero);
            SuperSerialPort.Instance.Send(new Command { Cmn = "27", ExpandCmn = "55", Data = zc_lPInfo.SelectedIndex.ToString("x2") + " 0F" }, true);
        }
        private void SetZero(object sender, Command command)
        {
            byte[] data = ByteStrUtil.HexToByte(command.Data);
            Dispatcher.Invoke(new Action(() =>
            {
                //设置气体1，2，3，4零点值
                byte[] gas_1_lowZC = new byte[4];
                Array.Copy(data, 2, gas_1_lowZC, 0, 4);
                Array.Reverse(gas_1_lowZC, 0, gas_1_lowZC.Length);
                gas_1_lowRangZC.Text = BitConverter.ToSingle(gas_1_lowZC, 0).ToString("f3");
                byte[] gas_1_highZC = new byte[4];
                Array.Copy(data, 6, gas_1_highZC, 0, 4);
                Array.Reverse(gas_1_highZC, 0, gas_1_highZC.Length);
                gas_1_highRangZC.Text = BitConverter.ToSingle(gas_1_highZC, 0).ToString("f3");

                byte[] gas_2_lowZC = new byte[4];
                Array.Copy(data, 10, gas_2_lowZC, 0, 4);
                Array.Reverse(gas_2_lowZC, 0, gas_2_lowZC.Length);
                gas_2_lowRangZC.Text = BitConverter.ToSingle(gas_2_lowZC, 0).ToString("f3");
                byte[] gas_2_highZC = new byte[4];
                Array.Copy(data, 14, gas_2_highZC, 0, 4);
                Array.Reverse(gas_2_highZC, 0, gas_2_highZC.Length);
                gas_2_highRangZC.Text = BitConverter.ToSingle(gas_2_highZC, 0).ToString("f3");

                byte[] gas_3_lowZC = new byte[4];
                Array.Copy(data, 18, gas_3_lowZC, 0, 4);
                Array.Reverse(gas_3_lowZC, 0, gas_3_lowZC.Length);
                gas_3_lowRangZC.Text = BitConverter.ToSingle(gas_3_lowZC, 0).ToString("f3");
                byte[] gas_3_highZC = new byte[4];
                Array.Copy(data, 22, gas_1_highZC, 0, 4);
                Array.Reverse(gas_3_highZC, 0, gas_3_highZC.Length);
                gas_3_highRangZC.Text = BitConverter.ToSingle(gas_3_highZC, 0).ToString("f3");

                byte[] gas_4_lowZC = new byte[4];
                Array.Copy(data, 26, gas_4_lowZC, 0, 4);
                Array.Reverse(gas_4_lowZC, 0, gas_4_lowZC.Length);
                gas_4_lowRangZC.Text = BitConverter.ToSingle(gas_4_lowZC, 0).ToString("f3");
                byte[] gas_4_highZC = new byte[4];
                Array.Copy(data, 30, gas_4_highZC, 0, 4);
                Array.Reverse(gas_4_highZC, 0, gas_4_highZC.Length);
                gas_1_highRangZC.Text = BitConverter.ToSingle(gas_4_highZC, 0).ToString("f3");
            }));
            ExceptionUtil.Instance.LogMethod("读取光谱仪零点系数成功");
        }
        private void SetZeroCoeffi_Click(object sender, RoutedEventArgs e)
        {
            string data = GetZeroData();
            if (data != "")
            {
                SuperSerialPort.Instance.Send(new Command { Cmn = "27", ExpandCmn = "66", Data = data }, true);
            }
        }
        private string GetZeroData()
        {
            try
            {
                //气体1，2，3，4判据数据 4*2*4字节
                byte[] gas_1_lowZC = BitConverter.GetBytes(Convert.ToSingle(gas_1_lowRangZC.Text));
                Array.Reverse(gas_1_lowZC);
                byte[] gas_1_highZC = BitConverter.GetBytes(Convert.ToSingle(gas_1_highRangZC.Text));
                Array.Reverse(gas_1_highZC);

                byte[] gas_2_lowZC = BitConverter.GetBytes(Convert.ToSingle(gas_2_lowRangZC.Text));
                Array.Reverse(gas_2_lowZC);
                byte[] gas_2_highZC = BitConverter.GetBytes(Convert.ToSingle(gas_2_highRangZC.Text));
                Array.Reverse(gas_2_highZC);

                byte[] gas_3_lowZC = BitConverter.GetBytes(Convert.ToSingle(gas_3_lowRangZC.Text));
                Array.Reverse(gas_3_lowZC);
                byte[] gas_3_highZC = BitConverter.GetBytes(Convert.ToSingle(gas_3_highRangZC.Text));
                Array.Reverse(gas_3_highZC);

                byte[] gas_4_lowZC = BitConverter.GetBytes(Convert.ToSingle(gas_4_lowRangZC.Text));
                Array.Reverse(gas_4_lowZC);
                byte[] gas_4_highZC = BitConverter.GetBytes(Convert.ToSingle(gas_4_highRangZC.Text));
                Array.Reverse(gas_4_highZC);

                return zc_lPInfo.SelectedIndex.ToString("x2") + " 0F" + ByteStrUtil.ByteToHexStr(gas_1_lowZC) + ByteStrUtil.ByteToHexStr(gas_1_highZC) + ByteStrUtil.ByteToHexStr(gas_2_lowZC) + ByteStrUtil.ByteToHexStr(gas_2_highZC) + ByteStrUtil.ByteToHexStr(gas_3_lowZC) + ByteStrUtil.ByteToHexStr(gas_3_highZC) + ByteStrUtil.ByteToHexStr(gas_4_lowZC) + ByteStrUtil.ByteToHexStr(gas_4_highZC);
            }
            catch
            {
                MessageBox.Show("请检查输入参数！");
                return "";
            }
        }
        #endregion

        #region 标定系数
        private void ReadCaliCoeffi_Click(object sender, RoutedEventArgs e)
        {
            DataForward.Instance.ReadCaliParam += new DataForwardDelegate(SetCali);
            SuperSerialPort.Instance.Send(new Command { Cmn = "28", ExpandCmn = "55", Data = cc_lPInfo.SelectedIndex.ToString("x2") + " 0F" }, true);
        }
        private void SetCali(object sender, Command command)
        {
            byte[] data = ByteStrUtil.HexToByte(command.Data);
            Dispatcher.Invoke(new Action(() =>
            {
                //设置气体1，2，3，4标定系数
                byte[] gas_1_lowCC = new byte[4];
                Array.Copy(data, 2, gas_1_lowCC, 0, 4);
                Array.Reverse(gas_1_lowCC, 0, gas_1_lowCC.Length);
                gas_1_lowRangCC.Text = BitConverter.ToSingle(gas_1_lowCC, 0).ToString("f3");
                byte[] gas_1_highCC = new byte[4];
                Array.Copy(data, 6, gas_1_highCC, 0, 4);
                Array.Reverse(gas_1_highCC, 0, gas_1_highCC.Length);
                gas_1_highRangCC.Text = BitConverter.ToSingle(gas_1_highCC, 0).ToString("f3");

                byte[] gas_2_lowCC = new byte[4];
                Array.Copy(data, 10, gas_2_lowCC, 0, 4);
                Array.Reverse(gas_2_lowCC, 0, gas_2_lowCC.Length);
                gas_2_lowRangCC.Text = BitConverter.ToSingle(gas_2_lowCC, 0).ToString("f3");
                byte[] gas_2_highCC = new byte[4];
                Array.Copy(data, 14, gas_2_highCC, 0, 4);
                Array.Reverse(gas_2_highCC, 0, gas_2_highCC.Length);
                gas_2_highRangCC.Text = BitConverter.ToSingle(gas_2_highCC, 0).ToString("f3");

                byte[] gas_3_lowCC = new byte[4];
                Array.Copy(data, 18, gas_3_lowCC, 0, 4);
                Array.Reverse(gas_3_lowCC, 0, gas_3_lowCC.Length);
                gas_3_lowRangCC.Text = BitConverter.ToSingle(gas_3_lowCC, 0).ToString("f3");
                byte[] gas_3_highCC = new byte[4];
                Array.Copy(data, 22, gas_1_highCC, 0, 4);
                Array.Reverse(gas_3_highCC, 0, gas_3_highCC.Length);
                gas_3_highRangCC.Text = BitConverter.ToSingle(gas_3_highCC, 0).ToString("f3");

                byte[] gas_4_lowCC = new byte[4];
                Array.Copy(data, 26, gas_4_lowCC, 0, 4);
                Array.Reverse(gas_4_lowCC, 0, gas_4_lowCC.Length);
                gas_4_lowRangCC.Text = BitConverter.ToSingle(gas_4_lowCC, 0).ToString("f3");
                byte[] gas_4_highCC = new byte[4];
                Array.Copy(data, 30, gas_4_highCC, 0, 4);
                Array.Reverse(gas_4_highCC, 0, gas_4_highCC.Length);
                gas_1_highRangCC.Text = BitConverter.ToSingle(gas_4_highCC, 0).ToString("f3");
            }));
            ExceptionUtil.Instance.LogMethod("读取光谱仪标定系数成功");
        }
        private void SetCaliCoeffi_Click(object sender, RoutedEventArgs e)
        {
            string data = GetCaliData();
            if (data != "")
            {
                SuperSerialPort.Instance.Send(new Command { Cmn = "28", ExpandCmn = "66", Data = data }, true);
            }
        }
        private string GetCaliData()
        {
            try
            {
                //气体1，2，3，4判据数据 4*2*4字节
                byte[] gas_1_lowCC = BitConverter.GetBytes(Convert.ToSingle(gas_1_lowRangCC.Text));
                Array.Reverse(gas_1_lowCC);
                byte[] gas_1_highCC = BitConverter.GetBytes(Convert.ToSingle(gas_1_highRangCC.Text));
                Array.Reverse(gas_1_highCC);

                byte[] gas_2_lowCC = BitConverter.GetBytes(Convert.ToSingle(gas_2_lowRangCC.Text));
                Array.Reverse(gas_2_lowCC);
                byte[] gas_2_highCC = BitConverter.GetBytes(Convert.ToSingle(gas_2_highRangCC.Text));
                Array.Reverse(gas_2_highCC);

                byte[] gas_3_lowCC = BitConverter.GetBytes(Convert.ToSingle(gas_3_lowRangCC.Text));
                Array.Reverse(gas_3_lowCC);
                byte[] gas_3_highCC = BitConverter.GetBytes(Convert.ToSingle(gas_3_highRangCC.Text));
                Array.Reverse(gas_3_highCC);

                byte[] gas_4_lowCC = BitConverter.GetBytes(Convert.ToSingle(gas_4_lowRangCC.Text));
                Array.Reverse(gas_4_lowCC);
                byte[] gas_4_highCC = BitConverter.GetBytes(Convert.ToSingle(gas_4_highRangCC.Text));
                Array.Reverse(gas_4_highCC);

                return cc_lPInfo.SelectedIndex.ToString("x2") + "0F" + ByteStrUtil.ByteToHexStr(gas_1_lowCC) + ByteStrUtil.ByteToHexStr(gas_1_highCC) + ByteStrUtil.ByteToHexStr(gas_2_lowCC) + ByteStrUtil.ByteToHexStr(gas_2_highCC) + ByteStrUtil.ByteToHexStr(gas_3_lowCC) + ByteStrUtil.ByteToHexStr(gas_3_highCC) + ByteStrUtil.ByteToHexStr(gas_4_lowCC) + ByteStrUtil.ByteToHexStr(gas_4_highCC);
            }
            catch
            {
                MessageBox.Show("请检查输入参数！");
                return "";
            }
        }
        #endregion

        #region 读取浓度命令
        #endregion

        #region 读写向量表信息
        private void VectorFileFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ExceptionUtil.Instance.ShowLoadingAction(true);
                string path = currentFilePath != "" ? currentFilePath : System.Windows.Forms.Application.StartupPath;
                FileDialog fileDialog;
                //读
                if(rwCombo.SelectedIndex == 0)
                {
                    fileDialog = new SaveFileDialog
                    {
                        InitialDirectory = path,
                        RestoreDirectory = true,
                        Filter = " 文本文件(*.txt)|*.txt|所有文件(*.*)|*.* "
                    };
                  
                }
                //写
                else
                {
                    fileDialog = new OpenFileDialog
                    {
                        InitialDirectory = path,
                        RestoreDirectory = true,
                        Filter = " 文本文件(*.txt)|*.txt|所有文件(*.*)|*.* "
                    };
                }
                if (fileDialog.ShowDialog() == true)
                {
                    vectorFilePath.Text = fileDialog.FileName;
                    ExceptionUtil.Instance.LogMethod("向量文件已设置为：" + fileDialog.FileName);
                    currentFilePath = fileDialog.FileName.Substring(0, fileDialog.FileName.LastIndexOf('\\'));
                }
            }
            catch
            {
                MessageBox.Show("打开文件失败");
            }
            finally
            {
                ExceptionUtil.Instance.ShowLoadingAction(false);
            }
        }
        private void RWVector_Click(object sender, RoutedEventArgs e)
        {
            if (!vectorFilePath.Text.EndsWith(".txt"))
            {
                MessageBox.Show("请设置正确的向量文件！");
                return;
            }
            //读
            if (rwCombo.SelectedIndex == 0)
            {
                ExceptionUtil.Instance.LogMethod("开始读取向量表信息...");
                RWVectorInfoImpl.Instance.SendVectorCmn(lP.SelectedIndex.ToString("x2"), gas.SelectedIndex.ToString("x2"), range.SelectedIndex.ToString("x2"), vectorFilePath.Text);
            }
            //写
            else
            {
                RWVectorInfoImpl.Instance.SetVectorInfo(lP.SelectedIndex.ToString("x2"), gas.SelectedIndex.ToString("x2"), range.SelectedIndex.ToString("x2"), vectorFilePath.Text);
            }
        }
        #endregion

        /// <summary>
        /// 将指定byte转成二进制字符串数组
        /// </summary>
        /// <param name="data">1字节</param>
        /// <returns></returns>
        private string[] ByteToBinary(byte data)
        {
            string b = Convert.ToString(data, 2);
            int count = 8 - b.Length;
            for (int i = 0; i < count; i++)
            {
                b = "0" + b;
            }
            string[] strs = new string[8];
            for (int i = 0; i < b.Length; i++)
            {
                strs[i] = b.Substring(i, 1);
            }
            return strs;
        }
    }
}
