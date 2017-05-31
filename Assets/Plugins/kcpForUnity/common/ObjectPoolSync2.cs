using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CxExtension
{
    /// <summary>
    /// 用于两个线程间的通信,一个只线程get 一个线程里面只return,切记 只用于两个线程的对象池,单线程请使用单线程版
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectPoolSync2<T> : ObjectPool<T> where T : class, new()
    {
        private object locker = new object();
        public ObjectPoolSync2(Func<T> creatNew, Action<T> resetFunc) : base(creatNew, resetFunc)
        {
            returnQue = new Queue<T>();
        }
        public override T Get()
        {
            T t = null;
            if (getQue.Count > 0)
            {
                t = getQue.Dequeue();
            }
            else
            {
                Swap();
                if (getQue.Count > 0)
                {
                    t = getQue.Dequeue();
                }
                else
                {
                    t = m_createNew();
                }
            }
            return t;
        }
        public void Swap()
        {
            kcpUtil.Swap(ref getQue, ref returnQue);
        }
        public override void Return(T item)
        {
            lock (locker)
            {
                base.Return(item);
            }
        }
    }
}
