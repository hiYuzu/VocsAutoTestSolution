using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VocsAutoTestBLL.Model
{
    public class SpecDataModel
    {
        //缓存队列
        private readonly Queue<byte> specDataQueue = new Queue<byte>();
        private readonly List<SpecDataModel> specDatas = new List<SpecDataModel>();
        /// <summary>
        /// 添加一组光谱数据
        /// </summary>
        /// <param name="bytes">光谱数据</param>
        public void AddDataArray(byte[] bytes)
        {
            foreach(byte b in bytes)
            {
                specDataQueue.Enqueue(b);
            }
        }
        /// <summary>
        /// 获取队列中所有数据
        /// </summary>
        /// <param name="isDel">是否删除</param>
        /// <returns>队列中的所有数据</returns>
        public byte[] GetAllData(bool isDel)
        {
            if (isDel)
            {
                byte[] data = specDataQueue.ToArray();
                specDataQueue.Clear();
                return data;
            }
            return specDataQueue.ToArray();
        }
        public void ClearAllData()
        {
            specDataQueue.Clear();
        }
    }
}
