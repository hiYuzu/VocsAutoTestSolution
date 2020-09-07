using System;
using VocsAutoTestBLL.Interface;
using VocsAutoTestBLL.Model;
using VocsAutoTestCOMM;

namespace VocsAutoTestBLL.Impl
{
    public class SpecOperatorImpl : ISpecOperator
    {
        private bool resetFlag = false;
        private int errorCount = 0;
        private ushort pageFlag;
        //当前包号
        private int currentPackage;
        private string dataType;
        private string lightPath;
        //缓存
        private readonly SpecDataModel dataCache;
        //单例模式
        private static SpecOperatorImpl instance;
        private readonly static object _obj = new object();
        private SpecOperatorImpl()
        {
            currentPackage = 1;
            dataCache = new SpecDataModel();
            DataForward.Instance.ReadSpecData += new DataForwardDelegate(GetSpecData);
        }
        public static SpecOperatorImpl Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_obj)
                    {
                        instance = new SpecOperatorImpl();
                    }
                }
                return instance;
            }
        }
        public event SpecDataDelegate SpecDataEvent;
        public event SpecDataDelegate AlgoDataEvent;
        public void SendSpecCmn(string lightPath, string dataType, ushort pageFlag) 
        {
            this.pageFlag = pageFlag;
            if (resetFlag)
            {
                dataCache.ClearAllData();
                currentPackage = 1;
                if (errorCount > 5)
                {
                    ExceptionUtil.Instance.ExceptionMethod("等待超时，请检查设备或尝试调整读数间隔", true);
                }
            }
            this.dataType = dataType;
            this.lightPath = lightPath;
            SuperSerialPort.Instance.Send(new Command
            {
                Cmn = "24",
                ExpandCmn = "55",
                Data = lightPath + dataType + "0" + currentPackage.ToString()
            });
            resetFlag = true;
            errorCount++;
        }
        /// <summary>
        /// 得到一段光谱采集数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="command"></param>
        private void GetSpecData(object sender, Command command)
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
                        msg = "数据没有准备好，请等待";
                        break;
                    case 03:
                        msg = "没有处于读光谱数据模式，请等待";
                        break;
                    case 04:
                        msg = "下传的当前拆分包号超限，请等待";
                        break;
                    case 05:
                        msg = "读取本次光谱超时（时间间隔超时），请重新读取新的光谱数据";
                        break;
                }
                dataCache.ClearAllData();
                currentPackage = 1;
                ExceptionUtil.Instance.ExceptionMethod(msg, true);
                return;
            }
            currentPackage = data[3];
            if (currentPackage < data[4])
            {
                //缓存
                byte[] bytes = new byte[data.Length - 5];
                Array.Copy(data, 5, bytes, 0, bytes.Length);
                dataCache.AddDataArray(bytes);
                //再发送
                currentPackage++;
                SendSpecCmn(lightPath, dataType, pageFlag);
            }
            else if (currentPackage == data[4])
            {
                //添加最后一段数据
                byte[] bytes = new byte[data.Length - 5];
                Array.Copy(data, 5, bytes, 0, bytes.Length);
                dataCache.AddDataArray(bytes);
                //存储本次数据
                currentPackage = 1;
                //从缓存中取出所有数据并解析
                ParseSpecData(dataCache.GetAllData(true));
            }
        }
        private void ParseSpecData(byte[] datas)
        {
            ushort[] specData = new ushort[datas.Length / 2];
            byte[] shortNum = new byte[2];
            for(int i = 0, j = 0; i < datas.Length - 1; i += 2, j++)
            {
                shortNum[0] = datas[i];
                shortNum[1] = datas[i + 1];
                Array.Reverse(shortNum);
                specData[j] = BitConverter.ToUInt16(shortNum, 0);
            }
            if(pageFlag == 1)
            {
                SpecDataEvent(this, specData);
            }
            else if(pageFlag == 3)
            {
                AlgoDataEvent(this, specData);
            }
        }
    }
}
