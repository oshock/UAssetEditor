using System.Runtime.CompilerServices;

namespace UAssetEditor;

public class Reader : BinaryReader
{
    public Reader(byte[] data) : base(new MemoryStream(data))
    { }
    
    public long Position
    {
        get => BaseStream.Position;
        set => BaseStream.Position = value;
    }

    public T Read<T>()
    {
        var buffer = ReadBytes(Unsafe.SizeOf<T>());
        return Unsafe.ReadUnaligned<T>(ref buffer[0]);
    }

    public T[] ReadArray<T>(int length)
    {
        var result = new T[length];
        for (int i = 0; i < result.Length; i++)
            result[i] = Read<T>();
        return result;
    }

    public T[] ReadArray<T>(Func<T> func, int length)
    {
        var result = new T[length];
        for (int i = 0; i < result.Length; i++)
            result[i] = func();
        return result;
    }
}