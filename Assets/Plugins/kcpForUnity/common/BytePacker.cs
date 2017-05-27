using System;
using System.Collections;
using System.Collections.Generic;
public class BytePacker
{
    private List<byte> cache = new List<byte>(1024);
    private int pkgLengthByteSize = 4;//
    private byte[] pkgLengthBytes;
    private int curPkgSize = -1;
    private Action<byte[]> func;
    public byte[] Pack(byte[] content)
    {
        pkgLengthBytes = BitConverter.GetBytes(content.Length);
        pkgLengthByteSize = pkgLengthBytes.Length;
        var buf = new byte[content.Length + pkgLengthByteSize];
        int offset = 0;
        Array.Copy(pkgLengthBytes, buf, pkgLengthBytes.Length);
        offset += pkgLengthBytes.Length;
        Array.Copy(content, 0, buf, offset, content.Length);
        return buf;
    }
    public BytePacker(Action<byte[]> call)
    {
        func = call;
    }
    public void Recv(byte[] buff)
    {
        cache.AddRange(buff);
        while (true)
        {
            if (curPkgSize < 0)// if not pkg size
            {
                if (cache.Count > pkgLengthByteSize)
                {
                    //get pkg size
                    cache.CopyTo(0, pkgLengthBytes, 0, pkgLengthByteSize);
                    cache.RemoveRange(0, pkgLengthByteSize);
                    curPkgSize = BitConverter.ToInt32(pkgLengthBytes, 0);
                }
                else
                {
                    break;
                }
            }

            if (cache.Count >= curPkgSize)
            {//get pkg data
                var pkgData = new byte[curPkgSize];
                cache.CopyTo(0, pkgData, 0, pkgData.Length);
                cache.RemoveRange(0, curPkgSize);
                func.Invoke(pkgData);
                //reset pkg size
                curPkgSize = -1;
            }
            else
            {
                break;
            }
        }
    }
}