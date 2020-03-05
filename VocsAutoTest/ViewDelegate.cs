using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VocsAutoTest
{
    class ViewDelegate
    {

        /// <summary>
        /// 定义获取性能统计数据委托
        /// </summary>
        /// <param name="result">The result</param>
        public delegate void VocsAutoTestdelegate(List<String> result);

        /// <summary>
        /// 获取性能统计数据的方法
        /// </summary>
        public void getVocsAutoTestdelegate(VocsAutoTestdelegate initdatamethod, List<String> result, Window window)
        {
            VocsAutoTestdelegate datadelegate = new VocsAutoTestdelegate(initdatamethod);
            window.Dispatcher.BeginInvoke(datadelegate, result);
        }

    }
}
