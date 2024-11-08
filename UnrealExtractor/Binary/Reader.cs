using System.Runtime.CompilerServices;

namespace UnrealExtractor.Binary;

public class Reader : BinaryReader
{
    public Reader(string path) : base(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
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

    public T ReadBytesInterpreted<T>(int size, bool flip = false)
    {
        var buffer = new byte[Unsafe.SizeOf<T>()];

        if (size > buffer.Length)
            throw new InvalidDataException($"'{typeof(T).Name}' must be able to hold {size} bytes");
        
        var block = ReadBytes(size);

        if (flip)
            block = block.Reverse().ToArray();
        
        Buffer.BlockCopy(block, 0, buffer, 0, block.Length);
        
        return Unsafe.ReadUnaligned<T>(ref buffer[0]);
    }
}