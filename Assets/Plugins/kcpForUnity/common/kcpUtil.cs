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
    public static void sliceTo<T>(this IList<T> self, IList<T> target, int start, int stop, bool isclear = false)
    {
        if (isclear)
        {
            target.Clear();
        }
        //var length = stop - start;
        for (int i = start; i < stop; i++)
        {
            target.Add(self[i]);
        }
    }
    /// <summary>
    /// retain index start to stop,content start,not content stop
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <param name="start"></param>
    /// <param name="stop"></param>
    public static void retainAndRemoveOther<T>(this List<T> self, int start, int stop)
    {
        var count = stop - start;
        self.RemoveRange(0, start);
        self.RemoveRange(count, self.Count);
    }
    public static List<T> subToList<T>(this List<T> self, int start, int stop)
    {
        var length = stop - start;
        var slist = new List<T>(length);
        self.sliceTo(slist, start, stop, false);
        return slist;
    }
}
