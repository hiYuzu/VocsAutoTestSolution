using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VocsAutoTestBLL.Interface;
using VocsAutoTestBLL.Model;
using VocsAutoTestCOMM;

namespace VocsAutoTestBLL.Impl
{
    public class SpecOperatorImpl : ISpecOperator
    {
        //当前包号
        private int currentPackage;
        //数据类型
        private string dataType;
        private long sendTime;
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
        public void SendSpecCmn(string dataType) 
        {
            //sendTime = ConvertDataTimeLong(DateTime.Now);
            this.dataType = dataType;
            //首次发送读取光谱数据命令
            SuperSerialPort.Instance.Send(new Command
            {
                Cmn = "24",
                ExpandCmn = "55",
                Data = "00 " + dataType + " 0" + currentPackage
            }, true);
        }
        /// <summary>
        /// 得到一段光谱采集数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="command"></param>
        private void GetSpecData(object sender, Command command)
        {
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
                Console.WriteLine(msg);
                dataCache.ClearAllData();
                currentPackage = 1;
                ExceptionUtil.Instance.ExceptionMethod(msg);
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
                SendSpecCmn(this.dataType);
            }
            else if (currentPackage == data[4])
            {
                //添加最后一段数据
                byte[] bytes = new byte[data.Length - 5];
                Array.Copy(data, 5, bytes, 0, bytes.Length);
                dataCache.AddDataArray(bytes);
                //存储本次数据
                byte[] datas = dataCache.GetAllData(true);
                currentPackage = 1;
                dataCache.StorgeSpecModel(new SpecDataModel() { LightInfo = data[1], DataType = data[2], DataInfo = datas});
                //从缓存中取出所有数据并解析
                ParseSpecData(datas);
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
                specData[j] = BitConverter.ToUInt16(DataConvertUtil.ByteReverse(shortNum), 0);
            }
            SpecDataEvent(this, specData);
        }

        private long ConvertDataTimeLong(DateTime dt)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            TimeSpan toNow = dt.Subtract(dtStart);
            long timeStamp = toNow.Ticks;
            timeStamp = long.Parse(timeStamp.ToString().Substring(0, timeStamp.ToString().Length - 4));
            return timeStamp;
        }
    }
}
