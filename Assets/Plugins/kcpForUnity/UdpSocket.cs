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
        private static readonly DateTime utc_time = new DateTime(1970, 1, 1);
        private BytePacker packer;
        private State state = State.Disconnect;
        public bool rcvSync = false;
        public static UInt32 iclock()
        {
            return (UInt32)(Convert.ToInt64(DateTime.UtcNow.Subtract(utc_time).TotalMilliseconds) & 0xffffffff);
        }

        private UdpClient mUdpClient;
        private IPEndPoint mIPEndPoint;
        private IPEndPoint mSvrEndPoint;
        private KCP mKcp;
        public int rtt = 0;

        private QueueSync<byte[]> rcv_queue = new QueueSync<byte[]>(128);
        private QueueSync<byte[]> snd_queue = new QueueSync<byte[]>(128);
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
                ThreadPool.QueueUserWorkItem(new WaitCallback(process_rcv_queue));
            }
            ThreadPool.QueueUserWorkItem(new WaitCallback(process_kcpio_queue));
        }

        public bool Connected { get { return state == State.Connect; } }
        void init_kcp(UInt32 conv)
        {
            mKcp = new KCP(conv, (byte[] buf, int size) =>
            {
                //if (kcpUtil.CountIncludePercent(lostPackRate))
                if (testUtil.RandIncludePercent(100- lostPackRate))
                {
                    mUdpClient.Send(buf, size);
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
                // push udp packet to switch queue.
                //mRecvQueue.Push(data);
                var dt = new byte[data.Length];//data.copyAll();
                Array.Copy(data, dt, data.Length);
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
            snd_queue.Enqueue(buf);
            //mKcp.Send(buf);
            //snd_notify.Set();
        }
        private void SendPacket(byte[] content)
        {
            content.PackInSendTime();
            content = packer.Pack(content);
            mKcp.Send(content);
        }
        public void Close()
        {
            state = State.Disconnect;
            mUdpClient.Close();
        }
        void process_rcv_queue(object state)
        {
            while (Connected)
            {
                try
                {
                    var obuf = mUdpClient.Receive(ref mIPEndPoint);
                    rcv_queue.Enqueue(obuf.copyAll());
                }
                catch (Exception e)
                {
                    Debug.Print(e.ToString());
                    throw;
                }
                Thread.Sleep(1);
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
                    mKcp.flush(kcpUtil.iclock);
                    rcv_queue.DequeueAll(it =>
                    {
                        mKcp.Input(it);

                    });
                    while (true)
                    {
                        int peekSize = mKcp.PeekSize();
                        if (peekSize > 0)
                        {
                            var peekBuf = new byte[peekSize];
                            int rcvSize = mKcp.Recv(peekBuf);
                            if (rcvSize > 0)
                            {
                                var rcvBuf = new byte[rcvSize];
                                Array.Copy(peekBuf, 0, rcvBuf, 0, rcvSize);
                                packer.Recv(rcvBuf);
                                //evHandler(rcvBuf);
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
