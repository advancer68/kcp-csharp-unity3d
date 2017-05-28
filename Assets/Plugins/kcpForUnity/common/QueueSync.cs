using System;
using System.Collections.Generic;
/// <summary>
/// this Queue use to multiple thread Enqueue and one thread dequeue;
/// not use to multiple thread dequeue
/// </summary>
/// <typeparam name="T"></typeparam>

public class QueueSync<T>
{
    private Queue<T> inQue;
    private Queue<T> outQue;
    //private int queueNum = 2;
    private object locker = new object();
    public QueueSync()
    {
        inQue = new Queue<T>();
        outQue = new Queue<T>();
    }
    public QueueSync(int size)
    {
        inQue = new Queue<T>(size);
        outQue = new Queue<T>(size);
    }
    public int Count
    {
        get
        {
            int count;
            lock (locker)
            {
                count = inQue.Count + outQue.Count;
            }
            return count;
        }
    }

    private T DequeueUnsafe()
    {
        return outQue.Dequeue();
    }
    public T Dequeue()
    {
        T t;
        if (outQue.Count == 0)
        {
            Switch();
        }
        t = DequeueUnsafe();
        return t;
    }
    /// <summary>
    /// Switch The InQue outQue used to foreach
    /// </summary>
    private void Switch()
    {
        lock (locker)
        {
            var temp = inQue;
            inQue = outQue;
            outQue = temp;
        }
    }
    public void Enqueue(T item)
    {
        lock (locker)
        {
            inQue.Enqueue(item);
        }
    }
    /// <summary>
    /// dequeue All in outQue items only used one thread
    /// </summary>
    /// <param name="func"></param>
    public void DequeueAll(Action<T> func)
    {
        if (func == null)
        {
            return;
        }
        Switch();
        while (outQue.Count > 0)
        {
            T it = outQue.Dequeue();
            func(it);
        }
    }
    /// <summary>
    /// peek on item
    /// </summary>
    /// <returns></returns>
    public T Peek()
    {
        T item;
        if (outQue.Count == 0)
        {
            Switch();
        }
        //lock (locker)
        {
            item = outQue.Peek();
        }
        return item;
    }
}