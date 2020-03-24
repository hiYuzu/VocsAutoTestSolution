using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VocsAutoTestBLL.Interface;
using VocsAutoTestCOMM;
using static VocsAutoTestCOMM.BaseMsg;

namespace VocsAutoTestBLL.Impl
{
    public class MeasureMgrImpl : IMeasureMgr
    {
        //测量次数
        public int measureTimes = 0;
        //读数间隔时间单位：秒
        public int timeInterval = 5;
        //开始测量标志
        public bool StartMeasure { get; set; } = false;
        //光谱数据类型
        public string specType = string.Empty;
        //当前页面标识 1：光谱采集 2：浓度测量 3：算法生成
        public ushort pageFlag = 0;
        //温度，压力参数
        public byte[] pressValues;
        public byte[] tempValues;
        //错误计数s
        private int errorCount = 0;
        private const int maxError = 10;
        public Action<bool> endAction;
        #region 单例
        private static MeasureMgrImpl instance;
        private readonly static object _obj = new object();
        private MeasureMgrImpl()
        {
            //
        }
        public static MeasureMgrImpl Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_obj)
                    {
                        instance = new MeasureMgrImpl();
                    }
                }
                return instance;
            }
        }
        #endregion
        /// <summary>
        /// 创建连续测量线程
        /// </summary>
        public void StartMultiMeasure()
        {
            StartMeasure = true;
            Thread thread = new Thread(MultiMeasure)
            {
                Name = "MeasureThread",
                IsBackground = true
            };
            thread.Start();
        }
        /// <summary>
        /// 连续测量实现线程
        /// </summary>
        private void MultiMeasure()
        {
            specType += specType;
            int times = 0;
            while (true)
            {
                try
                {
                    if (!StartMeasure || (measureTimes > 0 && times >= measureTimes))
                    {
                        break;
                    }
                    Measure();
                    if(measureTimes == 1)
                    {
                        try
                        {
                            break;
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                    int waite = timeInterval * 1000;
                    while (waite > 0)
                    {
                        if (!StartMeasure) break;
                        if (waite > 1000)
                        {
                            Thread.Sleep(1000);
                            waite -= 1000;
                        }
                        else
                        {
                            Thread.Sleep(waite);
                            waite = 0;
                        }
                    }
                    times++;
                    errorCount = 0;
                }
                catch (Exception ex)
                {
                    errorCount++;
                    Thread.Sleep(200);
                    ExceptionUtil.ExceptionMethod(ex.Message, false);
                    if (errorCount > maxError)
                    {
                        break;
                    }
                }
            }
            StartMeasure = false;
            endAction?.Invoke(true);
        }
        private void Measure()
        {
            
            if (pageFlag == 1)
            {
                //光谱采集
                SpecOperatorImpl.Instance.SendSpecCmn(specType, pageFlag);
            }
            else if (pageFlag == 2)
            {
                //浓度测量
                string data = "00" + ByteStrUtil.ByteToHexStr(tempValues) + ByteStrUtil.ByteToHexStr(pressValues);
                SuperSerialPort.Instance.Send(new Command { Cmn = "29", ExpandCmn = "55", Data = data }, true);
            }
            else if (pageFlag == 3)
            {
                //算法生成
                SpecOperatorImpl.Instance.SendSpecCmn(specType, pageFlag);
            }
        }
    }
}
