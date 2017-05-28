using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Diagnostics;

namespace KcpProject.v2
{
    // 客户端随机生成conv并作为后续与服务器通信
    public class UdpSocket
    {
        public enum State
        {
            Connect,
            Disconnect
        }
        public static int lostPackRate = 10;
        private BytePacker packer;
        private ByteBuf rcvCache = new ByteBuf(1500);
        private State state = State.Disconnect;
        public bool rcvSync = false;

        private UdpClient mUdpClient;
        private IPEndPoint mIPEndPoint;
        private IPEndPoint mSvrEndPoint;
        private int nxtPacketSize = -1;
        private Kcp mKcp;
        public int rtt = 0;

        private QueueSync<ByteBuf> rcv_queue = new QueueSync<ByteBuf>(128);
        private QueueSync<ByteBuf> snd_queue = new QueueSync<ByteBuf>(128);
        public UdpSocket(Action<byte[]> handler)
        {
            packer = new BytePacker(handler);
        }

        public void Connect(string host, int port)
        {
            mSvrEndPoint = new IPEndPoint(IPAddress.Parse(host), port);
            mUdpClient = new UdpClient(host, port);
            UnityEngine.Debug.LogFormat("snd buff size:{0},rcv buff size:{1}", mUdpClient.Client.SendBufferSize, mUdpClient.Client.ReceiveBufferSize);
            mUdpClient.Connect(mSvrEndPoint);
            state = State.Connect;
            //init_kcp((UInt32)new Random((int)DateTime.Now.Ticks).Next(1, Int32.MaxValue));
            init_kcp(0);
            if (rcvSync)
            {
                mUdpClient.BeginReceive(ReceiveCallback, this);
            }
            else
            {
                //ThreadPool.QueueUserWorkItem(new WaitCallback(process_rcv_queue));
            }
            ThreadPool.QueueUserWorkItem(new WaitCallback(process_kcpio_queue));
        }

        public bool Connected { get { return state == State.Connect; } }
        void init_kcp(UInt32 conv)
        {
            mKcp = new Kcp((int)conv, (ByteBuf buf) =>
            {
                //if (kcpUtil.CountIncludePercent(lostPackRate))
                if (testUtil.RandIncludePercent(100- lostPackRate))
                {
                    mUdpClient.Send(buf.GetRaw(), buf.PeekSize());
                }
            });

            // fast mode.
            mKcp.NoDelay(1, 1, 2, 1);
            mKcp.WndSize(4096, 4096);
        }

        void ReceiveCallback(IAsyncResult ar)
        {
            Byte[] data = (mIPEndPoint == null) ?
                mUdpClient.Receive(ref mIPEndPoint) :
                mUdpClient.EndReceive(ar, ref mIPEndPoint);

            if (null != data)
            {
                var dt = new ByteBuf(data);
                rcv_queue.Enqueue(dt);
                //rcv_notify.Set();
            }

            if (mUdpClient != null)
            {
                // try to receive again.
                mUdpClient.BeginReceive(ReceiveCallback, this);
            }
        }

        public void Send(byte[] buf)
        {
            var btBuf = new ByteBuf(buf.Length + 4);
            btBuf.WriteIntLE(buf.Length);
            btBuf.WriteBytes(buf);
            snd_queue.Enqueue(btBuf);
        }
        private void SendPacket(ByteBuf content)
        {
            mKcp.Send(content);
        }
        public void Close()
        {
            state = State.Disconnect;
            mUdpClient.Close();
        }
        public void Unpack(ByteBuf buf)
        {
            while (true)
            {
                if (nxtPacketSize < 0)
                {
                    if (buf.PeekSize() >= 4)
                    {
                        nxtPacketSize = buf.ReadIntLE();
                    }
                    else
                    {
                        break;
                    }
                }
                if (buf.PeekSize() >= nxtPacketSize )
                {
                    //var data = buf.read
                    
                }
                else
                {
                    break;
                }
            }
        }
        void process_kcpio_queue(object state)
        {
            while (Connected)
            {
                try
                {
                    //send process
                    snd_queue.DequeueAll(it =>
                    {
                        SendPacket(it);
                        //mKcp.Send(it);
                    });
                    Stopwatch t = new Stopwatch();
                    t.Start();
                    mKcp.Flush((int)kcpUtil.nowTotalMilliseconds);
                    rcv_queue.DequeueAll(it =>
                    {
                        mKcp.Input(it);
                    });
                    rcvCache.Clear();
                    //rcvCache.Capacity(peekSize);
                    while (true)
                    {
                        int peekSize = mKcp.PeekSize();
                        if (peekSize > 0)
                        {
                            int rcvSize = mKcp.Receive(rcvCache);
                            if (rcvSize > 0)
                            {
                                //packer.Recv(rcvCache);
                                Unpack(rcvCache);
                            }
                            else { break; }
                        }
                        else { break; }
                    }
                    t.Stop();
                    if (t.ElapsedMilliseconds > 10)
                    {
                        Console.WriteLine(string.Format("used time:{0}", t.ElapsedMilliseconds));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally
                {
                    //Console.WriteLine("thread run error");
                }
                Thread.Sleep(1);
            }
        }
    }
}
