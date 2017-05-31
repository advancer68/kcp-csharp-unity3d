/// <summary>
///同步版ByteBuf 用于 两个线程的通讯,一个线程写入的同时 另一个线程读取,仅用于两个线程,请不要用于大于一个线程读 或者大于线程写
/// </summary>
namespace System.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    public class ByteBufSync
    {
        private const int defaultSize = 4;
        private ByteBuf m_readQue = new ByteBuf(defaultSize);
        private ByteBuf m_writeQue = new ByteBuf(defaultSize);
        private object locker = new object();
        public ByteBufSync()
        {

        }
        public void WriteFrom(ByteBuf buf)
        {
            lock (locker)
            {
                m_writeQue.WriteBytesFrom(buf);
            }
        }
        public void Swap()
        {
            lock (locker)
            {
                kcpUtil.Swap(ref m_readQue, ref m_writeQue);
            }
        }
        public ByteBuf writeQue
        {
            get { return m_writeQue; }
        }
        public ByteBuf readQue
        {
            get { return m_readQue; }
        }
    }
}
