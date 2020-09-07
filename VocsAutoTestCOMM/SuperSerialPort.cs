using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;

namespace VocsAutoTestCOMM
{
    public class SuperSerialPort
    {
        private readonly SerialPort serialPort = new SerialPort();

        private static volatile SuperSerialPort instance;
        private static readonly object obj = new object();
        private List<byte> buffer = new List<byte>(4096);
        public bool isForward = true;
        private SuperSerialPort()
        {
            serialPort.DataReceived += Serialport_DataReceived;
        }

        #region
        /// <summary>
        /// 获取串口单例
        /// </summary>
        public static SuperSerialPort Instance
        {
            get
            {
                if (null == instance)
                {
                    lock (obj)
                    {
                        if (null == instance)
                        {
                            instance = new SuperSerialPort();
                        }
                    }
                }
                return instance;
            }
        }
        #endregion
        private void Serialport_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] readBuffer;
            int n = serialPort.BytesToRead;
            byte[] buf = new byte[n];
            serialPort.Read(buf, 0, n);
            buffer.AddRange(buf);
            //Console.WriteLine(ByteStrUtil.ByteToKHex(buffer.ToArray()));
            while (buffer.Count >= 14)
            {
                if (buffer[0] == 0x7D && buffer[1] == 0x7B)
                {
                    int endIndex = FPI.EndIndex(buffer.ToArray());
                    if (endIndex == 0)
                    {
                        break;
                    }
                    readBuffer = new byte[endIndex + 2];
                    buffer.CopyTo(0, readBuffer, 0, endIndex + 2);
                    buffer.RemoveRange(0, endIndex + 2);

                    if (FPI.IsLength(readBuffer) && FPI.JY(readBuffer))
                    {
                        Command command = FPI.Decoder(readBuffer);
                        CacheData.AddDataToQueue(command);
                    }
                }
                else
                {
                    buffer.Clear();
                }
            }
        }

        #region
        /// <summary>
        /// 设置串口信息
        /// </summary>
        /// <param name="portName">串口号</param>
        /// <param name="baudRate">波特率</param>
        /// <param name="parity">校验位</param>
        /// <param name="dataBits">数据位</param>
        /// <param name="stopBits">停止位</param>
        public void SetPortInfo(string portName, int baudRate, string parity, int dataBits, int stopBits)
        {
            if("Null".Equals(portName))
            {
                return;
            }
            serialPort.PortName = portName;
            serialPort.BaudRate = baudRate;
            serialPort.DataBits = dataBits;
            switch (parity)
            {
                case "无":
                    serialPort.Parity = Parity.None;
                    break;
                case "奇校验":
                    serialPort.Parity = Parity.Odd;
                    break;
                case "偶校验":
                    serialPort.Parity = Parity.Even;
                    break;
                default:
                    serialPort.Parity = Parity.None;
                    break;
            }

            switch (stopBits)
            {
                case 0:
                    serialPort.StopBits = StopBits.None;
                    break;
                case 1:
                    serialPort.StopBits = StopBits.One;
                    break;
                case 2:
                    serialPort.StopBits = StopBits.Two;
                    break;
                default:
                    serialPort.StopBits = StopBits.None;
                    break;
            }
        }
        #endregion

        public bool Open()
        {
            if (!serialPort.IsOpen)
            {
                try
                {
                    serialPort.Open();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;
        }

        public void Close()
        {
            if (serialPort.IsOpen)
            {
                serialPort.Close();
            }
        }

        #region
        /// <summary>
        /// 串口接收委托
        /// </summary>
        public Action<Command> DataReceived { get; set; }
        #endregion

        #region
        /// <summary>
        /// 串口发送命令
        /// </summary>
        /// <param name="command">命令对象</param>
        /// <param name="isForward">是否转发</param>
        public bool Send(Command command)
        {
            if (command != null && Open())
            {
                byte[] data = FPI.Encoder(command, this.isForward);
                Console.WriteLine("发送命令: " + ByteStrUtil.ByteToKHex(data));
                serialPort.Write(data, 0, data.Length);
                return true;
            }
            return false;
        }
        public bool SendAll(List<Command> commands)
        {
            foreach (Command command in commands)
            {
                if (!Send(command))
                {
                    return false;
                }
                Thread.Sleep(DefaultArgument.INTERVAL_TIME);
            }
            return true;
        }
        #endregion

        #region
        /// <summary>
        /// 检测串口名
        /// </summary>
        public static List<string> Check()
        {
            List<string> coms = new List<string>();
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    SerialPort sp = new SerialPort("COM" + (i + 1).ToString());
                    sp.Open();
                    sp.Close();
                    coms.Add("COM" + (i + 1).ToString());
                }
                catch (Exception)
                {
                    continue;
                }
            }
            return coms;
        }
        #endregion
    }
}
