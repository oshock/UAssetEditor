using System.Runtime.CompilerServices;
using UsmapDotNet;

namespace UAssetEditor;

public class Reader : BinaryReader
{
    public Usmap? Mappings;
    
    public Reader(byte[] data, Usmap? mappings = null) : base(new MemoryStream(data))
    {
        Mappings = mappings;
    }
    
    public Reader(string path) : this(File.ReadAllBytes(path))
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