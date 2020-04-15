using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VocsAutoTestBLL.Interface;
using VocsAutoTestBLL.Model;

namespace VocsAutoTestBLL.Impl
{
    public class PassPortImpl : IPassPort
    {
        //单例模式
        private static PassPortImpl instance;
        private static object _lock = new object();
        private PassPortImpl()
        {

        }
        public static PassPortImpl GetInstance()
        {
            if (instance == null)
            {
                lock (_lock)
                {
                    instance = new PassPortImpl();
                }
            }
            return instance;
        }

        public event PassPortDelegate PassValueEvent;

        public void GetPort(PortModel portModel)
        {
            PassValueEvent(this, portModel);
        }
    }
}
