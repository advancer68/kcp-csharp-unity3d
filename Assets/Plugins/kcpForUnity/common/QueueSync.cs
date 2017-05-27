using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

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
    public T DequeueUnsafe()
    {
        return outQue.Dequeue();
    }
    public void Enqueue(T item)
    {
        lock (locker)
        {
            inQue.Enqueue(item);
        }
    }
    public void Switch()
    {
        lock (locker)
        {
            var temp = inQue;
            inQue = outQue;
            outQue = inQue;
        }
    }
    public void DequeueAll(Action<T> func)
    {
        //lock (locker)
        {
            while (outQue.Count > 0)
            {
                T it = outQue.Dequeue();
                if (it == null)
                {
                    break;
                }
                else
                {
                    func(it);
                }
            }
            Switch();
        }
    }
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