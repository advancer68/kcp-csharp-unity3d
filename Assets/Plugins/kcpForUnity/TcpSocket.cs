using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System;

public class TcpSocket
{
    TcpClient client;
    private BytePacker packer;
    private NetworkStream tcpStream;
    private byte[] readCache = new byte[1000];
    public int rtt;
    public TcpSocket()
    {
        packer = new BytePacker(OnPacked);
    }
    public void OnPacked(byte[] buff)
    {
        rtt = buff.GetRTT();
    }
    public void Connect(string host, int port)
    {
        client = new TcpClient();
        client.Connect(host, port);
        tcpStream = client.GetStream();
        BeginRead();
    }
    public void BeginRead()
    {
        tcpStream.BeginRead(readCache, 0, readCache.Length, RecvCall, tcpStream);
    }
    public void Send(byte[] buff)
    {
        buff.PackInSendTime();
        buff.PackInInt(rtt, 4);
        //buff.PackInInt()
        buff = packer.Pack(buff);
        tcpStream.Write(buff, 0, buff.Length);
    }
    public void RecvCall(IAsyncResult ar)
    {
        int rcvSize = tcpStream.EndRead(ar);
        var rcvBuff = new byte[rcvSize];
        Array.Copy(readCache, 0, rcvBuff, 0, rcvSize);
        packer.Recv(rcvBuff);
        if (tcpStream.CanRead)
        {
        }

        BeginRead();
    }
    public void Close()
    {
        client.Close();
    }
}
