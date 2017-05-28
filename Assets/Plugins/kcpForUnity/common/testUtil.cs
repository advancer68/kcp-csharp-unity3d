using System;
using UnityEngine;
using System.Collections.Generic;

public static class testUtil{

    private static System.Random random = new System.Random();
    private static int count = 0;

    public static bool RandInclude(int min, int max, int rate)
    {
        var value = random.Next(min, max);
        return value < rate;
    }
    public static bool RandIncludePercent(int rate)
    {
        return RandInclude(0, 100, rate);
    }
    public static bool CountIncludePercent(int rate)
    {
        if (count < rate)
        {
            count = 0;
            return true;
        }
        count++;
        return false;
    }
    public static void PackInSendTime(this ByteBuf self)
    {
        //self.PackInInt((int)kcpUtil.nowTotalMilliseconds, 0);
        self.WriteIntLE((int)kcpUtil.nowTotalMilliseconds);
    }
    public static void PackInSendTime(this byte[] self)
    {
        self.PackInInt((int)kcpUtil.nowTotalMilliseconds, 0);
    }
    public static void PackInInt(this byte[] self, int value, int index)
    {
        var bytes = BitConverter.GetBytes(value);
        Array.Copy(bytes, 0, self, index, bytes.Length);
    }
    public static int Unpack2Int(this byte[] self, int indx)
    {
        return BitConverter.ToInt32(self, indx);
    }
    public static int GetRTT(this byte[] self)
    {
        var sndTime = self.Unpack2Int(0);
        return (int)kcpUtil.nowTotalMilliseconds - sndTime;
    }
}
