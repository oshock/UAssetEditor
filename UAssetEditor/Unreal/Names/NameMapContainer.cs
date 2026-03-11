using System.Runtime.CompilerServices;
using System.Text;
using Serilog;
using UAssetEditor.Binary;
using UAssetEditor.Classes.Containers;
using UAssetEditor.Unreal.Misc;
using UAssetEditor.Unreal.Objects;
using UAssetEditor.Unreal.Summaries.IO;
using ZstdSharp.Unsafe;

namespace UAssetEditor.Unreal.Names;

public class NameMapContainer : Container<string>
{
	public uint NumBytes => (uint)Items.Sum(str => string.IsNullOrEmpty(str) ? 0 : str.Length);
    public ulong HashVersion;
    public override int GetIndex(string str) => Items.FindIndex(x => x == str);

    private const ulong DEFAULT_HASH_VERSION = 0xC164000000;
    
    public NameMapContainer(List<string> items) : base(items)
    { }

    public int GetIndexOrAdd(string str)
    {
        var index = GetIndex(str);
        if (index > 0)
            return index;

        Add(str);
        return Length - 1;
    }

    public NameMapContainer(ulong hashVersion, List<string> strings) : base(strings)
    {
        HashVersion = hashVersion;
    }
    
    public NameMapContainer() : this(DEFAULT_HASH_VERSION, new())
    { }
    
    public static void WriteNameMap(Writer writer, NameMapContainer nameMap)
    {
        writer.Write(nameMap.Length);
        writer.Write(nameMap.NumBytes);
        writer.Write(nameMap.HashVersion);

        foreach (var s in nameMap)
            writer.Write(CityHash.TransformString(s));

        foreach (var s in nameMap)
        {
            writer.Write((byte)0);
            writer.Write((byte)s.Length);
        }

        foreach (var s in nameMap)
            writer.WriteString(s);
    }
    
    public static void WriteNameMap(Writer writer, NameMapContainer nameMap, ref FPackageSummary summary)
    {
        var start = writer.Position;
        summary.NameMapNamesOffset = (int)start;
        
        foreach (var str in nameMap)
        {
            // TODO utf16
            var buffer = new byte[str.Length + 1];
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(str), 0, buffer, 0, str.Length);
            
            var header = new FSerializedNameHeader((byte)buffer.Length);
            writer.Write(header);
            writer.WriteBytes(buffer);
        }

        var namesSize = writer.Position - start;
        var hashStart = writer.Position;
        summary.NameMapHashesOffset = (int)hashStart;
        
        writer.Write(nameMap.HashVersion);
        foreach (var str in nameMap)
            writer.Write(CityHash.TransformString(str));

        var hashesSize = writer.Position - hashStart;
        summary.NameMapNamesSize = (int)namesSize;
        summary.NameMapHashesSize = (int)hashesSize;
    }
    
    public static NameMapContainer ReadNameMap(Reader reader)
    {
        var count = reader.Read<int>();
        switch (count)
        {
            case < 0:
                throw new IndexOutOfRangeException($"Name map cannot have a length of {count}!");
            case 0:
                return new NameMapContainer([]);
        }

        var numBytes = reader.Read<uint>();
        var hashVersion = reader.Read<ulong>();

        var hashes = new ulong[count];
        for (int i = 0; i < hashes.Length; i++) 
            hashes[i] = reader.Read<ulong>();
        
        var headers = new byte[count];
        for (int i = 0; i < headers.Length; i++)
        {
            reader.Position++; // skip utf-16 check
            headers[i] = reader.ReadByte();
        }

        var start = reader.Position;

        var strings = headers.Select(t => Encoding.UTF8.GetString(reader.ReadBytes(t))).ToList();

        var read = reader.Position - start;
        if (read != numBytes)
            Warning($"Actual read bytes ({read}) did not equal 'numBytes': {numBytes}");
        
        return new NameMapContainer(hashVersion, strings);
    }

    public static NameMapContainer ReadNameMap(Reader reader, FPackageSummary summary)
    {
        return ReadNameMap(reader, summary.NameMapNamesSize, summary.NameMapHashesSize);
    }
    
    public static NameMapContainer ReadNameMap(Reader reader, int nameMapNamesSize, int nameMapHashesSize, bool readHashes = true)
    {
        var start = reader.Position;
        var nameMapHashesSizeWithoutVersion = nameMapHashesSize - sizeof(ulong);
        var nameCount = nameMapHashesSizeWithoutVersion / sizeof(ulong);
        var strings = new List<string>();
        for (int i = 0; i < nameCount; i++)
        {
            var header = reader.Read<FSerializedNameHeader>();
            var length = (int)header.Length;

            if (header.IsUtf16)
            {
                if (reader.Position % 2 == 1)
                    reader.Position++;

                var utf16Length = length * 2;
                var buffer = reader.ReadBytes(utf16Length);
                strings.Add(Encoding.Unicode.GetString(buffer).TrimEnd('\0'));
            }
            else
            {
                var buffer = reader.ReadBytes(length);
                strings.Add(Encoding.ASCII.GetString(buffer).TrimEnd('\0'));
            }
        }

        if (!readHashes) 
            return new NameMapContainer(0, strings);
        
        var hashVersion = reader.Read<ulong>();
        reader.Position += nameMapHashesSizeWithoutVersion;

        var expectedPos = start + nameMapNamesSize + nameMapHashesSize;
        if (reader.Position != expectedPos)
            Warning($"Name map size mismatch ({reader.Position} != {expectedPos}");

        return new NameMapContainer(hashVersion, strings);
    }
}