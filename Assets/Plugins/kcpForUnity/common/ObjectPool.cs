

namespace CxExtension
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    /// <summary>
    /// a sample pool  and your self ensure multile tiems return.used to single thread
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectPool<T> where T : class, new()
    {
        protected Queue<T> getQue = new Queue<T>();
        protected Queue<T> returnQue;
        protected Func<T> m_createNew;
        protected Action<T> m_resetFunc;
        public ObjectPool(Func<T> createNew, Action<T> resetFunc)
        {
            m_createNew = createNew;
            m_resetFunc = resetFunc;
            returnQue = getQue;
        }
        public ObjectPool()
        {
            returnQue = getQue;
        }
        public virtual T Get()
        {
            var it = getQue.Dequeue();
            if (it == null)
            {
                if (m_createNew == null)
                {
                    it = new T();
                }
                else
                {
                    it = m_createNew();
                }
            }
            return it;
        }
        public virtual void Return(T item)
        {
            if (m_resetFunc != null)
            {
                m_resetFunc(item);
            }
            getQue.Enqueue(item);
        }
    }
}
