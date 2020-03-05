using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VocsAutoTestBLL.Interface;
using VocsAutoTestBLL.Model;
using VocsAutoTestCOMM;

namespace VocsAutoTestBLL.Impl
{
    public class SpecMeasureImpl : ISpecMeasure
    {
        //单例模式
        private static SpecMeasureImpl instance;
        private static object _lock = new object();
        private SpecMeasureImpl()
        {

        }
        public static SpecMeasureImpl GetInstance()
        {
            if (instance == null)
            {
                lock (_lock)
                {
                    instance = new SpecMeasureImpl();
                }
            }
            return instance;
        }

        /// <summary>
        /// 浓度测量事件
        /// </summary>
        public event QuerySpecMeasureDelegate QuerySpecMeasureCompleted;

        public void PostSpecMeasureRequest(SpecMeasureModel specMeasureModel) {
            //声明结果
            BoolWithString<SpecMeasureModel> boolWithString = new BoolWithString<SpecMeasureModel>();
            //调用事件
            if (QuerySpecMeasureCompleted != null)
            {
                QuerySpecMeasureEventArgs e = new QuerySpecMeasureEventArgs(boolWithString);
                QuerySpecMeasureCompleted(this, e);
            }
        }

        public void OnQuerySpecMeasureCompleted(BaseMsg baseMsg)
        {
            if (QuerySpecMeasureCompleted != null)
            {
                //声明结果
                long lCount = 0;
                BoolWithString<SpecMeasureModel> boolWithString = new BoolWithString<SpecMeasureModel>();
                //赋值结果
                //boolWithString.ResultsT = this.ConvertByteToPerform(baseMsg, ref lCount);
                //boolWithString.CountT = (int)lCount;
                //触发事件
                QuerySpecMeasureEventArgs e = new QuerySpecMeasureEventArgs(boolWithString);
                QuerySpecMeasureCompleted(this, e);
            }
        }

    }
}
