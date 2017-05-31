using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

public static class kcpUtil
{
    private static DateTime offTime = new DateTime(2017, 1, 1);
    /// <summary>
    /// DateTime.Now.Subtract(offTime).TotalMilliseconds
    /// </summary>
    public static ulong nowTotalMilliseconds
    {
        get
        {
#if true
            return (ulong)(DateTime.Now.Subtract(offTime).TotalMilliseconds);
#else
            return (ulong)(DateTime.Now.Ticks / 10000);
#endif
        }
    }
    public static T[] copyAll<T>(this T[] self)
    {
        var it = new T[self.Length];
        Array.Copy(self, it, self.Length);
        return it;
    }
    public static UInt32 ReadUint32(this byte[] self, int startIndx)
    {
        return BitConverter.ToUInt32(self, startIndx);
    }
    public static byte[] Recapacity(this byte[] self, int length, bool copyData = false)
    {
        byte[] newBytes = self;
        if (self.Length < length)
        {
            newBytes = new byte[length];
            if (copyData)
            {
                self.CopyTo(0, newBytes, 0, self.Length);
            }
        }
        return newBytes;
    }


    public static void CopyTo<T>(this T[] self, int sourceIndex, T[] dest, int destIndex, int count)
    {
        Array.Copy(self, sourceIndex, dest, destIndex, count);
    }
    public static void Swap<T>(ref T left, ref T right)
    {
        T temp = left;
        left = right;
        right = temp;
    }
}
