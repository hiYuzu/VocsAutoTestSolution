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
        //读数间隔时间单位：毫秒
        public int TimeInterval { get; set; } = 5000;
        //开始测量标志
        public bool StartMeasure { get; set; } = false;
        //光谱数据类型
        public string specType = string.Empty;
        public string lightPath = string.Empty;
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
            lightPath = "0" + lightPath;
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
                    if (!StartMeasure) break;                
                    Thread.Sleep(TimeInterval);
                    times++;
                    errorCount = 0;
                }
                catch (Exception ex)
                {
                    errorCount++;
                    Thread.Sleep(200);
                    ExceptionUtil.Instance.ExceptionMethod(ex.Message, false);
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
                SpecOperatorImpl.Instance.SendSpecCmn(lightPath, specType, pageFlag);
            }
            else if (pageFlag == 2)
            {
                //浓度测量
                string data = lightPath + ByteStrUtil.ByteToHexStr(tempValues) + ByteStrUtil.ByteToHexStr(pressValues);
                SuperSerialPort.Instance.Send(new Command { Cmn = "29", ExpandCmn = "55", Data = data });
            }
            else if (pageFlag == 3)
            {
                //算法生成
                SpecOperatorImpl.Instance.SendSpecCmn(lightPath, specType, pageFlag);
            }
        }
    }
}
