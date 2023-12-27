using System.Text;
using UAssetEditor.IoStore;
using UAssetEditor.Misc;
using UAssetEditor.Names;
using UAssetEditor.Properties.Unversioned;
using UAssetEditor.Summaries;
using UAssetEditor.Unreal.Exports;
using Usmap.NET;

namespace UAssetEditor;

public class ZenAsset : UAsset
{
    public IoGlobalReader GlobalData;

    public ulong[] ImportedPublicExportHashes;
    public ulong[] ImportMap;
    public FExportMapEntry[] ExportMap;
    public FExportBundleEntry[] ExportBundleEntries;
    public FDependencyBundleHeader[] DependencyBundleHeaders;
    public int[] DependencyBundleEntries;
    
    public void LoadMappings(string path)
    {
	    Mappings = new Usmap.NET.Usmap(path, new UsmapOptions
	    {
		    OodlePath = "oo2core_9_win64.dll",
		    SaveNames = false
	    });
    }
    
    public ZenAsset(byte[] data) : base(data)
    { }
    
    public ZenAsset(string path) : this(File.ReadAllBytes(path))
    { }

    /// <summary>
    /// Initialize the global objects required to read this package.
    /// </summary>
    /// <param name="globalContainerPath"></param>
    public void Initialize(string globalContainerPath)
    {
	    GlobalData = new IoGlobalReader(globalContainerPath);
    }
    
    /// <summary>
    /// Read the entirety of this asset
    /// </summary>
    public void ReadAll()
    {
	    var headerSize = Position = ReadHeader();
	    var propReader = new UnversionedReader(this);
	    foreach (var entry in ExportBundleEntries)
	    {
		    if (entry.CommandType != EExportCommandType.ExportCommandType_Serialize)
			    continue;
		    
		    var export = ExportMap[entry.LocalExportIndex];
		    var name = NameMap[(int)export.ObjectName.NameIndex];
		    var @class = GlobalData.GetScriptName(export.ClassIndex);

		    Position = headerSize + (long)export.CookedSerialOffset;
		    Properties.Add(name, new PropertyContainer(propReader.ReadProperties(@class)));
	    }
    }

    public uint ReadHeader()
    {
        var summary = new FZenPackageSummary(this);
        Flags = summary.PackageFlags;
        NameMap = ReadNameMap(this);
        Name = NameMap[(int)summary.Name.NameIndex];

        // Not needed
        // var padSize = Read<long>();
        // Position += padSize;
        // Position += Read
        // Position += (long)Read<ulong>(); // Skip BulkDataMap
        Position = summary.ImportedPublicExportHashesOffset;
        ImportedPublicExportHashes =
            ReadArray<ulong>((summary.ImportMapOffset - summary.ImportedPublicExportHashesOffset) / sizeof(ulong));

        Position = summary.ImportMapOffset;
        ImportMap = ReadArray<ulong>((summary.ExportMapOffset - summary.ImportMapOffset) / sizeof(ulong));

        Position = summary.ExportMapOffset;
        ExportMap = ReadArray(() => new FExportMapEntry(this),
	        (summary.ExportBundleEntriesOffset - summary.ExportMapOffset) / FExportMapEntry.Size);
        ExportBundleEntries = ReadArray<FExportBundleEntry>(ExportMap.Length * (byte)EExportCommandType.ExportCommandType_Count);

        Position = summary.DependencyBundleHeadersOffset;
        DependencyBundleHeaders = ReadArray(() => new FDependencyBundleHeader
		        { FirstEntryIndex = Read<int>(), EntryCount = ReadArray(() => ReadArray<uint>(2), 2) },
	        (summary.DependencyBundleEntriesOffset - summary.DependencyBundleHeadersOffset) / 20);

        Position = summary.DependencyBundleEntriesOffset;
        DependencyBundleEntries =
	        ReadArray<int>((summary.ImportedPackageNamesOffset - summary.DependencyBundleEntriesOffset) / sizeof(int));
        
        return summary.HeaderSize;
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
	    return new NameMapContainer
	    {
		    HashVersion = hashVersion,
		    Hashes = hashes.ToList(),
		    Strings = strings
	    };
    }

    public List<UProperty> ReadProperties(string type) => new UnversionedReader(this).ReadProperties(type);

    /// <summary>
    /// Serializes the entire asset to a stream.
    /// </summary>
    /// <param name="writer"></param>
    public void WriteAll(Writer writer)
    {
	    var properties = new Writer();
	    var propWriter = new UnversionedWriter(this);
	    var offset = 0UL;
	    for (int i = 0; i < ExportMap.Length; i++)
	    {
		    var name = NameMap[(int)ExportMap[i].ObjectName.NameIndex];
		    var @class = GlobalData.GetScriptName(ExportMap[i].ClassIndex);
		    
		    ExportMap[i].CookedSerialOffset = offset;
		    var props = propWriter.WriteProperties(@class, i, Properties[name].GetProperties());
		    properties.Position = (long)offset;
		    props.CopyTo(properties);
		    properties.Position += 4; // Padding;
		    
		    ExportMap[i].CookedSerialSize = (ulong)props.BaseStream.Length + 4; // Padding
		    offset += (ulong)props.BaseStream.Length;
	    }
	    
	    WriteHeader(writer);
	    properties.CopyTo(writer);
    }

    public void WriteHeader(Writer writer)
    {
	    var summary = default(FZenPackageSummary);
	    summary.Name = new FMappedName((uint)NameMap.Strings.FindIndex(x => x == Name), 0);
	    
	    writer.Position = FZenPackageSummary.Size;
	    WriteNameMap(writer, NameMap);

	    writer.Position += sizeof(long) + sizeof(ulong); // pad size + bulk data size
	    
	    summary.ImportedPublicExportHashesOffset = (int)writer.Position;
	    writer.WriteArray(ImportedPublicExportHashes);

	    summary.ImportMapOffset = (int)writer.Position;
	    writer.WriteArray(ImportMap);
	    
	    summary.ExportMapOffset = (int)writer.Position;
	    foreach (var export in ExportMap)
		    export.Serialize(writer);
	    
	    summary.ExportBundleEntriesOffset = (int)writer.Position;
	    writer.WriteArray(ExportBundleEntries);

	    summary.DependencyBundleHeadersOffset = (int)writer.Position;
	    foreach (var depHeader in DependencyBundleHeaders)
	    {
		    writer.Write(depHeader.FirstEntryIndex);
		    foreach (var a in depHeader.EntryCount)
			    writer.WriteArray(a);
	    }

	    summary.DependencyBundleEntriesOffset = (int)writer.Position;
	    writer.WriteArray(DependencyBundleEntries);
	    
	    var end = writer.Position;
	    summary.ImportedPackageNamesOffset = (int)end;
	    
	    summary.HeaderSize = (uint)writer.Position;
	    summary.PackageFlags = Flags;

	    writer.Position = 0;
	    summary.Serialize(writer);
	    
	    writer.Position = end;
    }

    public static void WriteNameMap(Writer writer, NameMapContainer nameMap)
    {
	    writer.Write(nameMap.Length);
	    writer.Write(nameMap.NumBytes);
	    writer.Write(nameMap.HashVersion);

	    foreach (var s in nameMap.Strings)
		    writer.Write(CityHash.TransformString(s));

	    foreach (var s in nameMap.Strings)
	    {
		    writer.Write((byte)0);
		    writer.Write((byte)s.Length);
	    }

	    foreach (var s in nameMap.Strings)
		    writer.WriteString(s);
    }

    public Writer WriteProperties(string type, int exportIndex, List<UProperty> properties) =>
	    new UnversionedWriter(this).WriteProperties(type, exportIndex, properties);
}