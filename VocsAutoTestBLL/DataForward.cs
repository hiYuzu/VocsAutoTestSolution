using System;
using System.Threading;
using VocsAutoTestCOMM;


namespace VocsAutoTestBLL
{
    /// <summary>
    /// 委托
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="command"></param>
    public delegate void DataForwardDelegate(object sender, Command command);
    /// <summary>
    /// 数据转发类
    /// </summary>
    public class DataForward
    {
        private static volatile DataForward instance;
        private static readonly object _obj = new object();
        public static DataForward Instance
        {
            get
            {
                if(instance == null)
                {
                    lock (_obj)
                    {
                        instance = new DataForward();
                    }
                }
                return instance;
            }
        }
        //数据操作守护线程
        private Thread dataGuardThread;
        //数据操作线程
        private Thread dataThread;
        //运行状态标志
        private bool isStart = false;
        /// <summary>
        /// 开始服务
        /// </summary>
        public void StartService()
        {
            try
            {
                isStart = true;
                //启动获取数据守护线程
                if (dataGuardThread == null || !dataGuardThread.IsAlive)
                {
                    dataGuardThread = new Thread(new ThreadStart(GuardThread))
                    {
                        Name = "DataGuardThread",
                        IsBackground = true
                    };
                    dataGuardThread.Start();
                }
            }
            catch (Exception e)
            {
                Log4NetUtil.Error("服务启动失败，原因：" + e.Message);
            }
        }
        /// <summary>
        /// 守护线程
        /// </summary>
        private void GuardThread()
        {
            while (isStart)
            {
                try
                {
                    //启动获取数据线程
                    if (dataThread == null || !dataThread.IsAlive)
                    {
                        dataThread = new Thread(new ThreadStart(DataThread))
                        {
                            Name = "DataThread",
                            IsBackground = true
                        };
                        dataThread.Start();
                    }
                }
                catch (ThreadAbortException)
                {
                    Log4NetUtil.Info("手动停止守护线程");
                    break;
                }
                catch (Exception ex)
                {
                    Log4NetUtil.Error("守护线程运行错误，信息为：" + ex.Message);
                }
                finally
                {
                    Thread.Sleep(50);
                }
            }
        }
        /// <summary>
        /// 数据获取线程
        /// </summary>
        private void DataThread()
        {
            while (isStart)
            {
                try
                {
                    if(CacheData.GetCount() > 0)
                    {
                        Command command = (Command)CacheData.GetDataFromQueue();
                        if (command != null)
                        {
                            DataForwardMethod(command);
                        }
                    }
                }
                catch (ThreadAbortException)
                {
                    Log4NetUtil.Info("手动停止操作线程");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Log4NetUtil.Error(ex.GetType().ToString() + ":" + ex.Message);
                }
                finally
                {
                    Thread.Sleep(50);
                }
            }
        }

        #region 事件
        //读取公共参数
        public event DataForwardDelegate ReadCommParam;
        //读取光谱仪光路x参数
        public event DataForwardDelegate ReadLPParam;
        //读取光谱数据命令
        public event DataForwardDelegate ReadSpecData;
        //读取浓度测量数据
        public event DataForwardDelegate ReadConcMeasure;
        //读取量程切换判据
        public event DataForwardDelegate ReadRangeSwitch;
        //零点系数
        public event DataForwardDelegate ReadZeroParam;
        //标定系数
        public event DataForwardDelegate ReadCaliParam;
        //读写向量表信息
        public event DataForwardDelegate ReadVectorInfo;
        //读取设备号
        public event DataForwardDelegate ReadDeviceNo;
        #endregion

        /// <summary>
        /// 转发分配实现
        /// </summary>
        /// <param name="command"></param>
        public void DataForwardMethod(Command command)
        {
            //读回应
            if (command.ExpandCmn == "AA")
            {
                switch (command.Cmn)
                {
                    case "20":
                        ReadCommParam(this, command);
                        break;
                    case "21":
                        ReadLPParam(this, command);
                        break;
                    case "24":
                        ReadSpecData(this, command);
                        break;
                    case "25":
                        ReadDeviceNo(this, command);
                        break;
                    case "26":
                        ReadRangeSwitch(this, command);
                        break;
                    case "27":
                        ReadZeroParam(this, command);
                        break;
                    case "28":
                        ReadCaliParam(this, command);
                        break;
                    case "29":
                        ReadConcMeasure(this, command);
                        break;
                    case "2C":
                        ReadVectorInfo(this, command);
                        break;
                    default:
                        break;
                }
            }
            //写回应
            if (command.ExpandCmn == "99")
            {
                switch (command.Data)
                {
                    case "88":
                        ExceptionUtil.Instance.LogMethod("设置成功");
                        //Console.WriteLine("设置成功");
                        break;
                    case "99":
                        ExceptionUtil.Instance.ExceptionMethod("设置失败", true);
                        //Console.WriteLine("设置失败");
                        break;
                    default:
                        byte[] data = ByteStrUtil.HexToByte(command.Data);
                        if(data.Length == 6 && data[3] == data[4])
                        {
                            switch (data[5])
                            {
                                case 0x88:
                                    ExceptionUtil.Instance.LogMethod("设置成功");
                                    //Console.WriteLine("设置成功");
                                    break;
                                case 0x99:
                                    ExceptionUtil.Instance.ExceptionMethod("设置失败", true);
                                    //Console.WriteLine("设置失败");
                                    break;
                                case 0xAA:
                                    ExceptionUtil.Instance.ExceptionMethod("向量更新失败，全部重传", true);
                                    //Console.WriteLine("向量更新失败，全部重传");
                                    break;
                            }
                        }
                        break;
                }
            }
        }
    }
}
