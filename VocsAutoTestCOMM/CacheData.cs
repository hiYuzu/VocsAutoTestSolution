using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VocsAutoTestCOMM
{
    /// <summary>
    /// 缓存数据
    /// </summary>
    public class CacheData
    {
        //缓存队列
        private static readonly Queue<object> queue = new Queue<object>();
        /// <summary>
        /// 插入缓存数据
        /// </summary>
        /// <param name="obj"></param>
        public static void AddDataToQueue(object obj)
        {
            queue.Enqueue(obj);
        }
        /// <summary>
        /// 获取顶端缓存数据并移除
        /// </summary>
        /// <returns>注：无数据返回null</returns>
        public static object GetDataFromQueue()
        {
            try
            {
                return queue.Dequeue();
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 获取缓存队列中数据个数
        /// </summary>
        /// <returns></returns>
        public static int GetCount()
        {
            return queue.Count();
        }
        /// <summary>
        /// 清空所有缓存数据
        /// </summary>
        public static void ClearAllData()
        {
            queue.Clear();
        }
        /// <summary>
        /// 获取所有数据
        /// </summary>
        /// <param name="isDel">是否清空缓存队列</param>
        /// <returns></returns>
        public static object[] GetAllData(bool isDel)
        {
            object[] objArray = queue.ToArray();
            if(isDel)
                queue.Clear();
            return objArray;
        }
    }
}
