using System.Runtime.CompilerServices;
using System.Text;

namespace UAssetEditor;

public class Writer : BinaryWriter
{
    public Writer(byte[] data) : base(new MemoryStream(data))
    { }

    public Writer(Stream stream) : base(stream)
    { }
    
    public Writer() : base(new MemoryStream())
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
        BaseStream.Write(buffer);
    }

    public void WriteString(string str) => BaseStream.Write(Encoding.ASCII.GetBytes(str));

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

    public void CopyTo(Writer dest)
    {
        Position = 0;
        dest.Position = dest.BaseStream.Length;
        BaseStream.CopyTo(dest.BaseStream);
    }
}