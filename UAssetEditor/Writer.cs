using System.Runtime.CompilerServices;

namespace UAssetEditor;

public class Writer : BinaryWriter
{
    public Writer(byte[] data) : base(new MemoryStream(data))
    { }
    
    public long Position
    {
        get => BaseStream.Position;
        set => BaseStream.Position = value;
    }

    public void Write<T>(T value)
    {
        var buffer = new byte[Unsafe.SizeOf<T>()];
        Unsafe.WriteUnaligned(ref buffer[0], value);
        Write(buffer);
    }

    public void WriteArray<T>(IEnumerable<T> values)
    {
        foreach (var value in values)
            Write(value);
    }

    public void WriteArray<T>(Action<T> func, IEnumerable<T> values)
    {
        foreach (var value in values)
            func(value);
    }
}