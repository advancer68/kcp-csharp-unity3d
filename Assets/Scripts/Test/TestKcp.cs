using UnityEngine;
//using System.;
using System.Collections;
using KcpProject.v2;
using System;
using System.Net.Sockets;
using UnityEngine.UI;

public class TestKcp : MonoBehaviour
{
    [Serializable]
    public struct HostAdress
    {
        public string name;
        public string host;
        public int portKcp;
        public int portTcp;
    }
    public HostAdress[] hostss;
    public int hostIndx = 0;
    public UdpSocket kcpClient;
    public TcpSocket tcpClient;
    public bool autoSend = true;
    public uint pkgCountTotal = 0;
    public float interval = 0.1f;
    public float used = 0;
    public int msgSize = 10000;
    public int mLosePackRate = 10;
    public int rttKcpMax = 0;
    public int rttTcpMax = 0;
    public int rttKcp = 10;
    public uint pckCntFrm = 100;
    public double pckCntFrmTime = 0;
    public int pckCalcSize = 100;
    public int pckCalcCur = 0;
    public bool runOnStart = true;
    public bool tcpRuning = false;
    public bool kcpRuning = false;

    public bool rcvSync = false;
    public UnityEngine.UI.Text kcpRttText;
    public UnityEngine.UI.Text tcpRttText;
    public UnityEngine.UI.Text msgSizeText;
    public UnityEngine.UI.Text freNumText;
    public UnityEngine.UI.Text losePackRateText;
    public HostAdress curHost { get { return hostss[hostIndx]; } }
    public void OnSenSizeChange(float value)
    {
        msgSize = (int)value;
        msgSizeText.text = string.Format("msgSize:{0}", msgSize);
        //Debug.LogFormat("set SendBuff:{0}", value);
    }
    public void OnFreQuencyChange(float value)
    {
        interval = 1.0f / value;
        freNumText.text = string.Format("fps:{0}", value);
    }
    public void OnLosePackRateChange(float value)
    {
        mLosePackRate = (int)value;
        UdpSocket.lostPackRate = mLosePackRate;
        losePackRateText.text = string.Format("LosePackRate:{0}", (int)value);
    }
    // Use this for initialization
    void Start()
    {
        Debug.LogFormat("now time:{0}", kcpUtil.nowTotalMilliseconds);
        //enabled = false;
        if (runOnStart)
        {
            TestStart();
        }
    }
    #region KcpTest
    public void KcpStart()
    {
        //enabled = true;
        kcpClient = new UdpSocket((buff,length) =>
        {
            var rcvtime = kcpUtil.nowTotalMilliseconds;
            var sndtime = BitConverter.ToUInt32(buff, 0);
            var usetime = rcvtime - sndtime;
            pckCntFrmTime += usetime;
            pckCalcCur++;
        });
        kcpClient.Connect(curHost.host, curHost.portKcp);
    }
    public void KcpSend(byte[] buff)
    {
        if (kcpClient != null)
        {
            if (kcpClient.Connected)
            {
                kcpClient.Send(buff);
            }
        }
    }
    public void KcpStop()
    {
        //enabled = false;
        kcpClient.Close();
    }
    public void KcpRestart()
    {
        KcpStop();
        KcpStart();
    }
    #endregion
    #region TcpTest
    public void TcpStart()
    {
        //enabled = true;
        tcpClient = new TcpSocket();
        tcpClient.Connect(curHost.host, curHost.portTcp);
        tcpRuning = true;
    }
    public void TcpSend(byte[] buff)
    {
        if (tcpRuning)
        {
            tcpClient.Send(buff);
        }
    }
    public void TcpStop()
    {
        //enabled = false;
        tcpClient.Close();
        tcpRuning = false;
    }
    #endregion
    public void TestSend()
    {
        var buff = new byte[msgSize];
        buff.PackInSendTime();//send time
        //buff.PackInInt(rttKcp, 4);//pre rtt
        buff.PackInInt(mLosePackRate, 8);

        KcpSend(buff);
        TcpSend(buff);
    }
    public void TestStart()
    {
        TcpStart();
        KcpStart();
    }
    public void TestStop()
    {
        TcpStop();
        KcpStop();
    }
    public void SendTestBuff()
    {
        var buff = new byte[msgSize];
        buff.PackInSendTime();
        buff.PackInInt(rttKcp, 4);
        buff.PackInInt(mLosePackRate, 8);
        if (kcpClient != null)
        {
            if (kcpClient.Connected)
            {
                kcpClient.Send(buff);
            }
        }
        if (tcpRuning)
        {
            TcpSend(buff);
        }
        //Debug.LogFormat("send sessage:{0}", content);
    }
    // Update is called once per frame
    void Update()
    {
        if (autoSend)
        {
            used += Time.deltaTime;
            if (used > interval)
            {
                used -= interval;
                 TestSend();
            }
        }
        if (tcpClient != null)
        {
            rttTcpMax = Math.Max(tcpClient.rtt, rttTcpMax);
            tcpRttText.text = string.Format("tcp rtt:{0} max:{1}", tcpClient.rtt, rttTcpMax);
        }
        if (pckCalcCur >= pckCalcSize)
        {
            var meanTime = pckCntFrmTime / pckCalcCur;
            rttKcp = (int)meanTime;
            rttKcpMax = Math.Max((int)meanTime, rttKcpMax);

            kcpRttText.text = string.Format("kcp rtt:{0} max:{1}", (int)meanTime, rttKcpMax);
            pckCalcCur = 0;
            pckCntFrmTime = 0;
        }
    }
}
