using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VocsAutoTestBLL.Interface
{
    public delegate void AlgoDataDelegate(object sender, ushort[] specData);
    public interface IAlgoGeneral
    {
        event AlgoDataDelegate  AlgoDataEvent;
        void SendAlgoCmn(string dataType);
    }
}
