using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VocsAutoTestCOMM
{
    public delegate void DataForwardDelegate(object sender, DataForwardEventArgs e);
    /// <summary>
    /// 数据转发
    /// </summary>
    public class DataForward
    {
        #region 单例
        private static volatile DataForward instance;
        private static readonly object obj = new object();
        public static DataForward Instance
        {
            get
            {
                if(instance == null)
                {
                    lock (obj)
                    {
                        instance = new DataForward();
                    }
                    
                }
                return instance;
            }
        }
        #endregion

        #region 事件
        //读取公共参数
        public event DataForwardDelegate ReadCommParam;
        //读取光谱仪光路x参数
        public event DataForwardDelegate ReadVocsParam;
        #endregion

        /// <summary>
        /// 转发分配实现
        /// </summary>
        /// <param name="command"></param>
        public void DataForwardMethod(Command command)
        {
            DataForwardEventArgs e = new DataForwardEventArgs();
            e.UpdateCommand(command);
            //读回应
            if(command.ExpandCmn == "AA")
            {
                switch (command.Cmn)
                {
                    case "20":
                        ReadCommParam(this, e);
                        break;
                    case "21":
                        ReadVocsParam(this, e);
                        break;
                    default:
                        break;
                }
            }
            //写回应
            if(command.ExpandCmn == "99")
            {
                switch (command.Data)
                {
                    case "88":
                        Console.WriteLine("写命令成功");
                        break;
                    case "99":
                        Console.WriteLine("写命令失败");
                        break;
                    default:
                        break;
                }
            }
        }
    }
    /// <summary>
    /// EventArgs
    /// </summary>
    public class DataForwardEventArgs : EventArgs
    {
        public Command command;
        public void UpdateCommand(Command command)
        {
            this.command = command;
        }
    }
}
