using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using VocsAutoTestCOMM;

namespace VocsAutoTestBLL.Impl
{
    public class RWVectorInfoImpl
    {
        private bool resetFlag = false;
        private int errorCount = 0;
        private string lpInfo;
        private string gas;
        private string range;
        private string fileName;
        //当前包号
        private int currentPackage;
        //缓存
        private static Queue<byte> vectorInfoQueue;
        //单例模式
        private static RWVectorInfoImpl instance;
        private readonly static object _obj = new object();
        private RWVectorInfoImpl()
        {
            currentPackage = 1;
            DataForward.Instance.ReadVectorInfo += new DataForwardDelegate(GetVectorInfo);
        }
        public static RWVectorInfoImpl Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_obj)
                    {
                        instance = new RWVectorInfoImpl();
                        vectorInfoQueue = new Queue<byte>();
                    }
                }
                return instance;
            }
        }
        /// <summary>
        /// 读取下载向量表信息
        /// </summary>
        /// <param name="lpInfo">光路信息</param>
        /// <param name="gas">气体</param>
        /// <param name="range">量程</param>
        /// <param name="fileName">文件路径</param>
        public void SendVectorCmn(string lpInfo, string gas, string range, string fileName)
        {
            if(lpInfo != null)
            {
                this.lpInfo = lpInfo;
                this.gas = gas;
                this.range = range;
                this.fileName = fileName;
            }
            if (resetFlag)
            {
                vectorInfoQueue.Clear();
                currentPackage = 1;
                if (errorCount > 5)
                {
                    ExceptionUtil.Instance.ExceptionMethod("等待超时，请检查设备或尝试调整读数间隔", true);
                }
            }
            SuperSerialPort.Instance.Send(new Command
            {
                Cmn = "2C",
                ExpandCmn = "55",
                Data = this.lpInfo + this.gas + this.range + "0" + currentPackage
            });
            resetFlag = true;
            errorCount++;
        }
        /// <summary>
        /// 接收向量数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="command"></param>
        private void GetVectorInfo(object sender, Command command)
        {
            resetFlag = false;
            errorCount = 0;
            byte[] data = ByteStrUtil.HexToByte(command.Data);
            if (data.Length == 1)
            {
                string msg = null;
                switch (data[0])
                {
                    case 01:
                        msg = "另一通信口正在读取，请等待";
                        break;
                    case 02:
                        msg = "下传的当前拆分包号超限";
                        break;
                    case 03:
                        msg = "读取本次向量超时（时间间隔超时），请重新读取";
                        break;
                }
                vectorInfoQueue.Clear();
                currentPackage = 1;
                ExceptionUtil.Instance.ExceptionMethod(msg, true);
                return;
            }
            currentPackage = data[4];
            if (currentPackage < data[5])
            {
                //缓存
                byte[] bytes = new byte[data.Length - 6];
                Array.Copy(data, 6, bytes, 0, bytes.Length);
                foreach(byte b in bytes)
                {
                    vectorInfoQueue.Enqueue(b);
                }
                //再发送
                currentPackage++;
                SendVectorCmn(null, null, null, null);
            }
            else if (currentPackage == data[4])
            {
                //添加最后一段数据
                byte[] bytes = new byte[data.Length - 6];
                Array.Copy(data, 6, bytes, 0, bytes.Length);
                foreach (byte b in bytes)
                {
                    vectorInfoQueue.Enqueue(b);
                }
                currentPackage = 1;
                //从缓存中取出所有数据并解析
                byte[] datas = vectorInfoQueue.ToArray();
                vectorInfoQueue.Clear();
                ParseVectorData(datas);
            }
        }
        /// <summary>
        /// 解析byte
        /// </summary>
        /// <param name="datas"></param>
        private void ParseVectorData(byte[] datas)
        {
            List<string> vectorInfo = new List<string>();
            //光谱仪设备号
            byte[] deviceNum = new byte[15];
            Array.Copy(datas, 0, deviceNum, 0, deviceNum.Length);
            vectorInfo.Add(Encoding.Default.GetString(deviceNum).ToUpper());
            //光源类型
            vectorInfo.Add(datas[15].ToString());
            //传感器类型
            vectorInfo.Add(datas[16].ToString());
            //像素
            vectorInfo.Add(datas[17].ToString());
            uint pixel = 512;
            switch (datas[17].ToString())
            {
                case "0":
                    pixel = 256;
                    break;
                case "2":
                    pixel = 1024;
                    break;
                case "3":
                    pixel = 2048;
                    break;
            }
            //温度
            byte[] temp = new byte[4];
            Array.Copy(datas, 18, temp, 0, temp.Length);
            Array.Reverse(temp, 0, temp.Length);
            vectorInfo.Add(BitConverter.ToSingle(temp, 0).ToString());
            //压力
            byte[] press = new byte[4];
            Array.Copy(datas, 22, press, 0, press.Length);
            Array.Reverse(press, 0, press.Length);
            vectorInfo.Add(BitConverter.ToSingle(press, 0).ToString());
            //光路
            vectorInfo.Add(datas[26].ToString());
            //光程
            byte[] lightLen = new byte[2];
            Array.Copy(datas, 27, lightLen, 0, lightLen.Length);
            Array.Reverse(lightLen, 0, lightLen.Length);
            vectorInfo.Add(BitConverter.ToUInt16(lightLen, 0).ToString());
            //气体室类型
            vectorInfo.Add(datas[29].ToString());
            //单位
            vectorInfo.Add(datas[30].ToString());
            //量程
            vectorInfo.Add(datas[31].ToString());
            //气体信息
            vectorInfo.Add(datas[32].ToString());
            //向量数据个数
            byte[] vectorCount = new byte[2];
            Array.Copy(datas, 33, vectorCount, 0, vectorCount.Length);
            Array.Reverse(vectorCount, 0, vectorCount.Length);
            vectorInfo.Add(BitConverter.ToUInt16(vectorCount, 0).ToString());
            //多项式系数个数
            vectorInfo.Add(datas[35].ToString());
            //向量总数据
            //起始地址
            byte[] startAddr = new byte[2];
            Array.Copy(datas, 36, startAddr, 0, startAddr.Length);
            Array.Reverse(startAddr, 0, startAddr.Length);
            uint startAddress = BitConverter.ToUInt16(startAddr, 0);
            //偏移量
            byte[] offset = new byte[2];
            Array.Copy(datas, 38, offset, 0, offset.Length);
            Array.Reverse(offset, 0, offset.Length);
            uint offsetNum = BitConverter.ToUInt16(offset, 0);
            //最后一个0数据所在位置
            uint endZeroNum = pixel - startAddress - offsetNum;
            //实际向量总数据=偏移量字节数*4（float）
            byte[] vector = new byte[offsetNum * 4];
            Array.Copy(datas, 40, vector, 0, vector.Length);
            //多项式系数
            byte[] coeffi = new byte[20];
            Array.Copy(datas, (offsetNum * 4) + 40, coeffi, 0, coeffi.Length);
            StorgeToFile(vectorInfo, startAddress, vector, endZeroNum, coeffi);
        }
        private void StorgeToFile(List<string> info, uint startAddr, byte[] vector, uint endZeroNum, byte[] coeffi)
        {
            try
            {
                if (!File.Exists(fileName))
                {
                    FileStream fs = File.Create(fileName);
                    fs.Close();
                    fs.Dispose();
                }
                StreamWriter sw = new StreamWriter(fileName, false, Encoding.UTF8);
                //14项基本信息
                foreach (string vi in info)
                {
                    sw.WriteLine(vi);
                }
                //补零至第一位非零数据
                for(uint i = 0; i < startAddr; i++)
                {
                    sw.WriteLine(0);
                }
                //向量非零数据
                for (int m = 0; m < vector.Length; m += 4)
                {
                    byte[] vectorData = new byte[4];
                    Array.Copy(vector, m, vectorData, 0, vectorData.Length);
                    Array.Reverse(vectorData, 0, vectorData.Length);
                    sw.WriteLine(BitConverter.ToSingle(vectorData, 0).ToString());
                }
                //补零至向量个数
                for(uint j = 0; j < endZeroNum; j++)
                {
                    sw.WriteLine(0);
                }
                //多项式系数
                for(int n = 0; n < coeffi.Length; n += 4)
                {
                    byte[] coefficient = new byte[4];
                    Array.Copy(coeffi, n, coefficient, 0, coefficient.Length);
                    Array.Reverse(coefficient, 0, coefficient.Length);
                    sw.WriteLine(BitConverter.ToSingle(coefficient, 0).ToString());
                }
                sw.Close();
                sw.Dispose();
                ExceptionUtil.Instance.LogMethod("向量表数据保存到文件:" + fileName);
            }
            catch(Exception e)
            {
                ExceptionUtil.Instance.ExceptionMethod("向量表数据保存到文件失败", true);
                ExceptionUtil.Instance.ExceptionMethod(e.Message, false);
            }
        }
        /// <summary>
        /// 设置上传向量表信息
        /// </summary>
        /// <param name="fileName">向量文件路径</param>
        public void SetVectorInfo(string lpInfo, string gas, string range, string fileName)
        {
            try
            {
                ushort startAddr = 0;
                ushort offset = 0;
                int coeffiNum = 0;
                int pixel = 512;
                List<byte> data = new List<byte>();
                FileInfo file = new FileInfo(fileName);
                TextReader textReader = file.OpenText();
                string[] text = textReader.ReadToEnd().Split('\n');
                for (int i = 0; i < text.Length; i++)
                {
                    text[i] = text[i].Replace("\n", "").Replace("\t", "").Replace("\r", "").Replace(" ", "");
                }
                //设备号
                data = AddByte(data, Encoding.Default.GetBytes(text[0]));
                //14项基本数据
                for (int i = 0; i < 13; i++)
                {
                    if (i == 3 || i == 4)
                    {
                        //温度|| 压力 4字节
                        byte[] bytes = BitConverter.GetBytes(Convert.ToSingle(text[i + 1]));
                        Array.Reverse(bytes);
                        data = AddByte(data, bytes);
                    }
                    else if (i == 6 || i == 11)
                    {
                        //光程||压缩后的向量数据个数 2字节
                        byte[] bytes = BitConverter.GetBytes(Convert.ToUInt16(text[i + 1]));
                        Array.Reverse(bytes);
                        data = AddByte(data, bytes);
                        //另取得偏移量
                        if (i == 11)
                        {
                            offset = (ushort)(Convert.ToInt32(text[i + 1]) - 1);
                        }
                    }
                    else
                    {
                        //1字节
                        data.Add(byte.Parse(text[i + 1]));
                        //另取得像素个数
                        if (i == 2)
                        {
                            switch (Convert.ToInt32(text[i + 1]))
                            {
                                case 0:
                                    pixel = 256;
                                    break;
                                case 2:
                                    pixel = 1024;
                                    break;
                                case 3:
                                    pixel = 2048;
                                    break;
                            }
                        }
                        //另取得系数个数
                        else if (i == 12)
                        {
                            coeffiNum = Convert.ToInt32(text[i + 1]);
                        }
                    }
                }
                //起始地址
                string[] vectorData = new string[pixel];
                Array.Copy(text, 14, vectorData, 0, vectorData.Length);
                foreach (string s in vectorData)
                {
                    try
                    {
                        Convert.ToInt32(s);
                    }
                    catch
                    {
                        break;
                    }
                    startAddr++;
                }
                byte[] reStartAddr = BitConverter.GetBytes(startAddr);
                Array.Reverse(reStartAddr);
                data = AddByte(data, reStartAddr);
                //偏移量
                byte[] reOffset = BitConverter.GetBytes(offset);
                Array.Reverse(reOffset);
                data = AddByte(data, reOffset);
                //压缩后的数据
                for (int i = 0; i < offset; i++)
                {
                    byte[] reVD = BitConverter.GetBytes(Convert.ToSingle(vectorData[startAddr + i]));
                    Array.Reverse(reVD);
                    data = AddByte(data, reVD);
                }
                //系数
                string[] coefficient = new string[coeffiNum];
                Array.Copy(text, 526, coefficient, 0, coeffiNum);
                foreach (string coe in coefficient)
                {
                    byte[] reCoe = BitConverter.GetBytes(Convert.ToSingle(coe));
                    Array.Reverse(reCoe);
                    data = AddByte(data, reCoe);
                }
                string datas = ByteStrUtil.ByteToHex(data.ToArray());
                int pkgNum = (int)Math.Ceiling((double)datas.Length / 512);
                int startIndex = 0, length = 512;
                for (int currPkgNum = 1; currPkgNum <= pkgNum; currPkgNum++)
                {
                    if (currPkgNum < pkgNum)
                    {
                        SuperSerialPort.Instance.Send(new Command
                        {
                            Cmn = "2C",
                            ExpandCmn = "66",
                            Data = lpInfo + gas + range + currPkgNum.ToString("x2") + pkgNum.ToString("x2") + datas.Substring(startIndex, length)
                        });
                        startIndex = length * currPkgNum;
                        Thread.Sleep(DefaultArgument.INTERVAL_TIME);
                    }
                    else
                    {
                        SuperSerialPort.Instance.Send(new Command
                        {
                            Cmn = "2C",
                            ExpandCmn = "66",
                            Data = lpInfo + gas + range + currPkgNum.ToString("x2") + pkgNum.ToString("x2") + datas.Substring(startIndex)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionUtil.Instance.ExceptionMethod(ex.Message, true);
            }
        }
        private List<byte> AddByte(List<byte> list, byte[] bytes)
        {
            foreach(byte b in bytes)
            {
                list.Add(b);
            }
            return list;
        }
    }
}
