﻿using UAssetEditor.Unreal.Names;
using UAssetEditor.Unreal.Properties.Unversioned;
using UAssetEditor.Summaries;
using UAssetEditor.Unreal.Exports;
using UAssetEditor.Unreal.Objects;
using UAssetEditor.Binary;
using UAssetEditor.Classes.Containers;
using UAssetEditor.Unreal.Readers.IoStore;
using UsmapDotNet;
using Oodle = OodleDotNet.Oodle;

namespace UAssetEditor;

public class ZenAsset : Asset
{
    public IoGlobalReader? GlobalData;

    public ulong[] ImportedPublicExportHashes;
    public ulong[] ImportMap;
    public ExportContainer ExportMap;
    public FExportBundleEntry[] ExportBundleEntries;
    public FDependencyBundleHeader[] DependencyBundleHeaders;
    public int[] DependencyBundleEntries;
    
    public void LoadMappings(string path)
    {
	    Mappings = new Usmap(path, new UsmapOptions
	    {
		    Oodle = new Oodle("oo2core_9_win64.dll"),
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
	    GlobalData = IoGlobalReader.InitializeGlobalData(globalContainerPath);
    }
    
    /// <summary>
    /// Initialize the global objects required to read this package.
    /// </summary>
    /// <param name="reader"></param>
    public void Initialize(IoGlobalReader reader)
    {
	    GlobalData = reader;
    }
    
    /// <summary>
    /// Read the entirety of this asset
    /// </summary>
    public override void ReadAll()
    {
	    var headerSize = Position = ReadHeader();
	    foreach (var entry in ExportBundleEntries)
	    {
		    if (entry.CommandType != EExportCommandType.ExportCommandType_Serialize)
			    continue;
		    
		    var export = ExportMap[entry.LocalExportIndex];
		    var name = NameMap[(int)export.ObjectName.NameIndex];
		    var className = export.Class;

		    
		    var obj = new UObject(this);
		    obj.Name = name;
		    obj.Class = new UStruct(Mappings?.Schemas.FirstOrDefault(x => x.Name == className) 
		                            ?? throw new KeyNotFoundException($"Could not find schema named '{className}'"));
		    
		    var position = headerSize + (long)export.CookedSerialOffset;
		    obj.Deserialize(position);
		    
		    Exports.Add(obj);
	    }
    }

    public override uint ReadHeader()
    {
        var summary = new FZenPackageSummary(this);
        Flags = summary.PackageFlags;
        NameMap = NameMapContainer.ReadNameMap(this);
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
        ExportMap = ExportContainer.Read(this, summary);
        ExportBundleEntries = ReadArray<FExportBundleEntry>(ExportMap.Length * (byte)EExportCommandType.ExportCommandType_Count);

        Position = summary.DependencyBundleHeadersOffset;
        DependencyBundleHeaders = ReadArray(() => 
		        new FDependencyBundleHeader
		        {
			        FirstEntryIndex = Read<int>(), 
			        EntryCount = ReadArray(() => ReadArray<uint>(2), 2)
		        },
	        (summary.DependencyBundleEntriesOffset - summary.DependencyBundleHeadersOffset) / 20);

        Position = summary.DependencyBundleEntriesOffset;
        DependencyBundleEntries =
	        ReadArray<int>((summary.ImportedPackageNamesOffset - summary.DependencyBundleEntriesOffset) / sizeof(int));
        
        return summary.HeaderSize;
    }

    public override List<UProperty> ReadProperties(UStruct structure)
    {
	    return UnversionedReader.ReadProperties(this, structure);
    }

    /// <summary>
    /// Serializes the entire asset to a stream.
    /// </summary>
    /// <param name="writer"></param>
    public override void WriteAll(Writer writer)
    {
	    var uexp = new Writer();
	    var propWriter = new UnversionedWriter(this);
	    
	    for (int i = 0; i < ExportMap.Length; i++)
	    {
		    var name = NameMap[(int)ExportMap[i].ObjectName.NameIndex];
		    var @class = GlobalData!.GetScriptName(ExportMap[i].ClassIndex);
 
		    ExportMap[i].CookedSerialOffset = (ulong)uexp.Position;

		    var properties = Exports[name]?.Properties
		                     ?? throw new KeyNotFoundException(
			                     "Object exists in the export map, but not in the loaded exports.");
		    var props = propWriter.WriteProperties(@class, properties);
		    props.Write(0); // Padding
		    props.CopyTo(uexp);

		    ExportMap[i].CookedSerialSize = (ulong)props.Length;
	    }
	    
	    WriteHeader(writer);
	    uexp.CopyTo(writer);
	    uexp.Close();
    }

    // TODO
    public override void Fix()
    {
	    throw new NotImplementedException();
	    
	    /*//ExportBundleEntries = new FExportBundleEntry[ExportMap.Length * 2];

	    for (var i = 0; i < ExportMap.Length; i++)
	    {
		    var e = ExportMap[i];
		    e.SetObjectName(e.Name);
		    /*ExportBundleEntries[i] = new FExportBundleEntry
		    {
			    LocalExportIndex = (uint)i,
			    CommandType = EExportCommandType.ExportCommandType_Create
		    };
		    ExportBundleEntries[ExportMap.Length + i] = new FExportBundleEntry
		    {
			    LocalExportIndex = (uint)i,
			    CommandType = EExportCommandType.ExportCommandType_Serialize
		    };#2#

		    if (e.TryGetProperties(out var ctn))
			    HandleProperties(this, ctn!.Properties);
	    }*/
    }

    public override void WriteHeader(Writer writer)
    {
	    var summary = default(FZenPackageSummary);
	    summary.Name = new FMappedName((uint)NameMap.GetIndexOrAdd(Name), 0);

	    writer.Position = FZenPackageSummary.Size;
	    NameMapContainer.WriteNameMap(writer, NameMap);

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

    public override ResolvedObject? ResolvePackageIndex(FPackageIndex? index)
    {
	    throw new NotImplementedException();
    }

    public override Writer WriteProperties(string type, List<UProperty> properties) =>
	    new UnversionedWriter(this).WriteProperties(type, properties);
}

public class ExportContainer : Container<FExportMapEntry>
{
	public override int GetIndex(string str) => Items.FindIndex(x => x.Name == str);

	public ExportContainer(List<FExportMapEntry> items) : base(items)
	{ }

	public static ExportContainer Read(ZenAsset asset, FZenPackageSummary summary)
	{
		return new ExportContainer(asset.ReadArray(() => new FExportMapEntry(asset),
			(summary.ExportBundleEntriesOffset - summary.ExportMapOffset) / FExportMapEntry.Size).ToList());
	}
}