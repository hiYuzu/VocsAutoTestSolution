using System;
using VocsAutoTestBLL.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VocsAutoTestBLL.Interface
{
    /// <summary>
    /// 委托
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void PassPortDelegate(object sender, PassPortEventArgs e);

    /// <summary>
    /// 接口
    /// </summary>
    public interface IPassPort
    {
        event PassPortDelegate PassValueEvent;
        void GetPort(PortModel portModel);
    }

    /// <summary>
    /// 事件
    /// </summary>
    public class PassPortEventArgs : EventArgs
    {
        public PortModel portModel;
        public void UpdatePort(PortModel port)
        {
            this.portModel = port;
        }
    }
}
