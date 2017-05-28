using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

public static class kcpUtil
{
    public static uint nowTotalMilliseconds { get { return (uint)(DateTime.UtcNow.Ticks / 1000); } }
    public static T[] copyAll<T>(this T[] self)
    {
        var it = new T[self.Length];
        Array.Copy(self, it, self.Length);
        return it;
    }
    public static UInt32 ReadUint32(this byte[] self,int startIndx)
    {
        return BitConverter.ToUInt32(self, startIndx);
    }
    public static void CopyTo<T>(this T[] self,int sourceIndex,T[] dest,int destIndex,int count)
    {
        Array.Copy(self, sourceIndex, dest, destIndex, count);
    }
    public static void Swap<T>(ref T left,ref T right)
    {
        T temp = left;
        left = right;
        right = temp;
    }
}
