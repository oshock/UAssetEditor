using System.Text;
using UAssetEditor.Binary;
using UAssetEditor.Unreal.Misc;

namespace UAssetEditor.Classes.Containers;

public class NameMapContainer : Container<string>
{
	public uint NumBytes => (uint)Items.Sum(str => string.IsNullOrEmpty(str) ? 0 : str.Length);
    public ulong HashVersion;
    public override int GetIndex(string str) => Items.FindIndex(x => x == str);

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
    
    public static NameMapContainer ReadNameMap(Reader reader)
    {
        var count = reader.Read<int>();
        switch (count)
        {
            case < 0:
                throw new IndexOutOfRangeException($"Name map cannot have a length of {count}!");
            case 0:
                return default;
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

        var strings = headers.Select(t => Encoding.UTF8.GetString(reader.ReadBytes(t))).ToList();
        return new NameMapContainer(hashVersion, strings);
    }
}