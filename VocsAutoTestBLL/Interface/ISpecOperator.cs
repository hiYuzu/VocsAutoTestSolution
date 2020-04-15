using System;
using VocsAutoTestCOMM;

namespace VocsAutoTestBLL.Interface
{
    public delegate void SpecDataDelegate(object sender, ushort[] specData);
    public interface ISpecOperator
    {
        event SpecDataDelegate SpecDataEvent;
        event SpecDataDelegate AlgoDataEvent;
        void SendSpecCmn(string lightPath, string dataType, ushort pageFlag);
    }
}
