﻿using System;
using System.Threading;
using VocsAutoTestBLL.Interface;
using VocsAutoTestCOMM;
using VocsAutoTestCOMM.LogUtil;


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
                        Thread.Sleep(100);
                    }
                }
                catch (ThreadAbortException)
                {
                    Log4NetUtil.Info("手动停止操作线程");
                    break;
                }
                catch (Exception ex)
                {
                    Log4NetUtil.Error(ex.GetType().ToString() + ":" + ex.Message);
                }
            }
        }

        #region Action/事件
        //写命令返回结果Action
        public Action<bool> WriteResult { get; set; }

        //读取公共参数
        public event DataForwardDelegate ReadCommParam;
        //读取光谱仪光路x参数
        public event DataForwardDelegate ReadVocsParam;
        //读取光谱数据命令
        public event DataForwardDelegate ReadVocsData;
        //读取浓度测量数据
        public event DataForwardDelegate ReadSpecMeasure;
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
                        ReadVocsParam(this, command);
                        break;
                    case "22":
                        break;
                    case "23":
                        break;
                    case "24":
                        ReadVocsData(this, command);
                        break;
                    case "29":
                        ReadSpecMeasure(this, command);
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
                        WriteResult?.Invoke(true);
                        break;
                    case "99":
                        WriteResult?.Invoke(false);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
