using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VocsAutoTestBLL.Model
{
    public class BoolWithString<T>
    {
        private bool result = true;
        private string resultsShow = string.Empty;
        private List<T> resultsT;
        private int countT;

        /// <summary>
        /// 返回结果
        /// </summary>
        public bool Result
        {
            get { return result; }
            set { result = value; }
        }

        /// <summary>
        /// 结果说明
        /// </summary>
        public string ResultsShow
        {
            get { return resultsShow; }
            set { resultsShow = value; }
        }

        /// <summary>
        /// 返回泛型结果
        /// </summary>
        public List<T> ResultsT
        {
            get { return resultsT; }
            set { resultsT = value; }
        }

        /// <summary>
        /// 返回泛型结果数量
        /// </summary>
        public int CountT
        {
            get { return countT; }
            set { countT = value; }
        }
    }
}
