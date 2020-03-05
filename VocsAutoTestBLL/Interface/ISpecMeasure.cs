using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VocsAutoTestBLL.Model;
using VocsAutoTestCOMM;

namespace VocsAutoTestBLL.Interface
{
    /// <summary>
    /// 浓度测量委托参数
    /// </summary>
    public class QuerySpecMeasureEventArgs : EventArgs
    {
        public BoolWithString<string> argSpecMeasureExport { get; set; }
        /// <summary>
        /// 导出参数
        /// </summary>
        /// <param name="argPerformExport">参数类</param>
        public QuerySpecMeasureEventArgs(BoolWithString<string> argSpecMeasureExport)
        {
            this.argSpecMeasureExport = argSpecMeasureExport;
        }

        public BoolWithString<SpecMeasureModel> argSpecMeasureState { get; set; }
        /// <summary>
        /// 查询参数
        /// </summary>
        /// <param name="argPerformState">参数类</param>
        public QuerySpecMeasureEventArgs(BoolWithString<SpecMeasureModel> argSpecMeasureState)
        {
            this.argSpecMeasureState = argSpecMeasureState;
        }
    }

    public delegate void QuerySpecMeasureDelegate(object sender, QuerySpecMeasureEventArgs e);   //delegate declaration

    /// <summary>
    /// <para>[功能描述]：浓度测量接口</para>
    /// <para>[接口名称]：VocsAutoTestBLL.Interface.ISpecMeasure</para>
    /// <para>[版权信息]：Copyright (c) 2020-2030 TCB Corporation</para>
    /// <para>[    作者]：王垒</para>
    /// <para>[日期时间]：2020-3-4 09:39</para>
    /// </summary>
    public interface ISpecMeasure
    {
        /// <summary>
        /// 获取测试数据
        /// </summary>
        event QuerySpecMeasureDelegate QuerySpecMeasureCompleted;

        /// <summary>
        /// 发送浓度测量请求
        /// </summary>
        /// <param name="specMeasureModel">参数类</param>
        void PostSpecMeasureRequest(SpecMeasureModel specMeasureModel);

        /// <summary>
        /// 浓度测量事件应答
        /// </summary>
        /// <param name="baseMsg">基本回复类</param>
        void OnQuerySpecMeasureCompleted(BaseMsg baseMsg);

    }
}
