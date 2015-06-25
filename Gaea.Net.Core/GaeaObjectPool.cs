using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gaea.Net.Core
{
    /// <summary>
    ///  对象池
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GaeaObjectPool<T>
    {
        private Queue<T> pool = new Queue<T>();        

        /// <summary>
        ///  从池中获取一个对象。
        /// </summary>
        /// <returns>如果池中没有对象，则返回null</returns>
        public T GetObject()
        {
            T obj = default(T);

            lock (pool)
            {                
                if (pool.Count > 0)
                {
                    obj = pool.Dequeue();
                }
            }
            return obj;
        }

        /// <summary>
        ///  归还一个对象到池
        /// </summary>
        /// <param name="obj"></param>
        public void ReleaseObject(T obj)
        {
            lock (pool)
            {
                pool.Enqueue(obj);
            }
        }

        /// <summary>
        ///  池中对象的数量
        /// </summary>
        public int Count { get { return pool.Count; } }
    }
}
