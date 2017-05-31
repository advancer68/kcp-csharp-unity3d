/**
		 *  缓冲区
		 **/
using System;
using System.Text;

public class ByteBuf
{
    private byte[] data;
    private int mReadPos;
    private int mWritePos;
    private static byte[] emptyArray = new byte[0];
    #region Constroct
    public ByteBuf()
    {
        Init();
        data = emptyArray;
    }
    public ByteBuf(int capacity) : this()
    {
        data = new byte[capacity];
    }
    public ByteBuf(byte[] content) : this()
    {
        data = content;
        mWritePos = content.Length;
    }
    public ByteBuf(ByteBuf oldbuf) : this()
    {
        mReadPos = oldbuf.mReadPos;
        mWritePos = oldbuf.mWritePos;
        data = new byte[oldbuf.data.Length];
        if (data.Length > 0)
        {
            Array.Copy(oldbuf.data, data, data.Length);
        }
    }
    #endregion
    #region Private Mothed

    private void Init()
    {
        mReadPos = 0;
        mWritePos = 0;
    }
    public int RawReadIndex(int offset)
    {
        return mReadPos + offset;
    }
    private void MoveReadPos(int offset)
    {
        var newpos = mReadPos + offset;
        if (newpos <= mWritePos)
        {
            mReadPos = newpos;
        }
    }
    public int RawWriteIndex(int offset)
    {
        return mWritePos + offset;
    }
    private void MoveWritePos(int offset)
    {
        var newpos = mWritePos + offset;
        if (newpos <= data.Length)
        {
            mWritePos = newpos;
        }
    }
    #endregion
    public void Recapacity(int newCapacity)
    {
        byte[] old = data;
        data = new byte[newCapacity];
        Array.Copy(old, data, old.Length);
    }

    public void EnsureCapacity(int newCapacity)
    {
        if (newCapacity > data.Length)
        {
            Recapacity(newCapacity);
        }
    }
    public void EnsureCapacityChange(int changevalue)
    {
        int newCapacity = mWritePos + changevalue;
        EnsureCapacity(newCapacity);
    }

    public void Clear()
    {
        Init();
    }
    public byte GetByte(int index)
    {
        if (CanRead(1))
        {
            return data[RawReadIndex(index)];
        }
        return 0;
    }
    public short GetInt16(int index)
    {
        if (CanRead(2))
        {
            return BitConverter.ToInt16(data, RawReadIndex(index));
        }
        return 0;
    }
    public int GetInt32(int index)
    {
        if (CanRead(4))
        {
            return BitConverter.ToInt32(data, RawReadIndex(index));
        }
        return 0;
    }
    public bool CanRead(int index, int length)
    {
        return mReadPos + index + length <= mWritePos;
    }
    public bool CanRead(int length)
    {
        return CanRead(0, length);
    }
    public byte ReadByte()
    {
        var bt = GetByte(0);
        MoveReadPos(1);
        return bt;
    }
    public short ReadInt16()
    {
        var v = GetInt16(0);
        MoveReadPos(2);
        return v;
    }
    public short ReadShort()
    {
        return ReadInt16();
    }
    public int ReadInt32()
    {
        var v = GetInt32(0);
        MoveReadPos(4);
        return v;
    }
    public int GetBytes(int offset, byte[] destArray, int destIndx, int length)
    {
        if (!CanRead(offset, length))
        {
            return -1;
        }
        if (destIndx + length > destArray.Length)
        {
            return -2;
        }
        Array.Copy(data, RawReadIndex(offset), destArray, destIndx, length);
        return length;
    }
    public int ReadToBytes(int srcIndx, byte[] destArray, int destIndx, int length)
    {
        int ret = GetBytes(srcIndx, destArray, destIndx, length);
        MoveReadPos(length);
        return ret;
    }
    public int PeekSize()
    {
        return mWritePos - mReadPos;
    }
    public int Size { get { return mWritePos - mReadPos; } }
    public int Capacity { get { return data.Length; } }
    public int WritePos { get { return mWritePos; } }
    public int ReadPos { get { return mReadPos; } }
    public byte[] RawData { get { return data; } }
    public bool CanWrite(int index, int length)
    {
        return mWritePos + index + length <= data.Length;
    }
    public bool CanWrite(int length)
    {
        return CanWrite(0, length);
    }
    public ByteBuf SetByte(int offset, byte value)
    {
        if (CanWrite(offset, 1))
        {
            data[RawWriteIndex(offset)] = value;
        }
        return this;
    }
    public void SetBytes(int offset, byte[] src, int srcStartIndex, int len)
    {
        if (CanWrite(offset, len))
        {
            Array.Copy(src, srcStartIndex, data, RawWriteIndex(offset), len);
        }
    }
    public void SkipBytes(int length)
    {
        if (length > 0)
        {
            MoveReadPos(length);
        }
    }
    public void WriteByte(byte value)
    {
        EnsureCapacityChange(1);
        SetByte(0, value);
        MoveWritePos(1);
    }
    public void WriteInt16(short value)
    {
        var bytes = BitConverter.GetBytes(value);
        WriteBytesFrom(bytes);
    }
    public void WriteShort(short value)
    {
        WriteInt16(value);
    }
    public void WriteInt32(int value)
    {
        var bytes = BitConverter.GetBytes(value);
        WriteBytesFrom(bytes);
    }
    public void WriteBytesFrom(ByteBuf src)
    {
        WriteBytesFrom(src, src.Size);
    }
    public void WriteBytesFrom(ByteBuf src, int len)
    {
        WriteBytesFrom(0, src.data, src.ReadPos, len);
    }
    public void WriteBytesFrom(byte[] src)
    {
        WriteBytesFrom(0, src, 0, src.Length);
    }
    public void WriteBytesFrom(int offset, byte[] src, int srcStartIndex, int len)
    {
        if (len > 0)
        {
            EnsureCapacityChange(len);
            Array.Copy(src, srcStartIndex, data, RawWriteIndex(offset), len);
            MoveWritePos(len);
        }
    }
}

