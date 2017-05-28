using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

public static class kcpUtil
{
    private static Random random = new Random();
    private static int count = 0;
    public static uint iclock { get { return (uint)(DateTime.UtcNow.Ticks / 1000); } }
    public static T[] copyAll<T>(this T[] self)
    {
        var it = new T[self.Length];
        Array.Copy(self, it, self.Length);
        return it;
    }
    public static void CopyTo<T>(this T[] self,int sourceIndex,T[] dest,int destIndex,int count)
    {
        Array.Copy(self, sourceIndex, dest, destIndex, count);
    }
}
