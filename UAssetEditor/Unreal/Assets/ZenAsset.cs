using System.Data;
using System.Runtime.InteropServices;
using Serilog;
using UAssetEditor.Unreal.Names;
using UAssetEditor.Unreal.Properties.Unversioned;
using UAssetEditor.Summaries;
using UAssetEditor.Unreal.Exports;
using UAssetEditor.Unreal.Objects;
using UAssetEditor.Binary;
using UAssetEditor.Classes.Containers;
using UAssetEditor.Unreal.Containers;
using UAssetEditor.Unreal.Objects.IO;
using UAssetEditor.Unreal.Packages;
using UAssetEditor.Unreal.Properties;
using UAssetEditor.Unreal.Readers;
using UAssetEditor.Unreal.Readers.IoStore;
using UAssetEditor.Unreal.Summaries;
using UAssetEditor.Unreal.Summaries.IO;
using UAssetEditor.Unreal.Versioning;
using UAssetEditor.Utils;

namespace UAssetEditor.Unreal.Assets;

public class ZenAsset : Asset
{
    public IoGlobalReader? GlobalData;
    public IoStoreReader? IoReader => Reader as IoStoreReader;

    public FBulkDataMapEntry[] BulkDataMap = [];
    public ulong[]? ImportedPublicExportHashes;
    public FPackageObjectIndex[] ImportMap;
    public ExportContainer ExportMap;
    public FPackageObjectIndex[] CellImportMap = [];
    FCellExportMapEntry[] CellExportMap = [];
    public FExportBundleHeader[]? ExportBundleHeaders;
    public FExportBundleEntry[] ExportBundleEntries;
    public Tuple<FPackageId, FArc[]>[] SortedExternalArcs = [];
    public FDependencyBundleHeader[] DependencyBundleHeaders = [];
    public int[] DependencyBundleEntries = [];
    public FZenPackageImportedPackageNamesContainer ImportedPackageNamesContainer = new();

    public List<FPackageId>? ImportedPackageIds;
    
    public ZenAsset(byte[] data, UnrealFileSystem? system = null, UnrealFileReader? reader = null) : base(data, system, reader)
    { }
    
    public ZenAsset(string path) : this(File.ReadAllBytes(path))
    { }

    /// <summary>
    /// Initialize the global objects required to read this package.
    /// </summary>
    /// <param name="globalContainerPath"></param>
    public void Initialize(string globalContainerPath)
    {
	    GlobalData = IoGlobalReader.InitializeGlobalData(globalContainerPath, Game);
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

        if (Game >= EGame.GAME_UE5_0)
            PopulateImportIds();

        if (ExportBundleHeaders != null)
        {
            var position = headerSize;
            foreach (var header in ExportBundleHeaders)
            {
                for (var i = 0u; i < header.EntryCount; i++)
                {
                    position += ProcessEntry(ExportBundleEntries[header.FirstEntryIndex + i], (int)position, true);
                }
            }
        }
        else
        {
            foreach (var entry in ExportBundleEntries)
            {
                ProcessEntry(entry, 0);
            }
        }

        return;

        int ProcessEntry(FExportBundleEntry entry, int pos, bool isFromHeader = false)
        {
            if (entry.CommandType != EExportCommandType.ExportCommandType_Serialize)
                return 0;

            var export = ExportMap[entry.LocalExportIndex];
            var name = NameMap[(int)export.ObjectName.NameIndex];
            var className = export.Class.Value;

            var obj = UObject.ConstructObject(this, className);
            obj.Name = name;
            obj.Outer = new Lazy<UObject?>(() =>
                ResolveObjectIndex(export.OuterIndex)?.As<ResolvedExportObject>().Object);
            obj.Super = new Lazy<ResolvedObject?>(() => ResolveObjectIndex(export.OuterIndex));
            obj.Template = new Lazy<ResolvedObject?>(() => ResolveObjectIndex(export.TemplateIndex));
            obj.Flags |= export.ObjectFlags;

            var schema = Mappings?.Schemas.FirstOrDefault(x => x.Name == className);
            if (schema == null)
            {
                Log.Error($"Could not find schema named '{className}'. Skipping deserialization.");
                return 0;
            }

            obj.Class = new UStruct(schema, Mappings);

            var position = isFromHeader ? pos:
                headerSize + (long)export.CookedSerialOffset;
            var validPos = position + (long)export.CookedSerialSize;
            obj.Deserialize(position);

            if (Position != validPos)
            {
                Warning(
                    $"'{obj.Class.Name}.{obj.Name}' did not read correctly! Expected serial size: {export.CookedSerialSize}, but got {Position - (long)export.CookedSerialOffset}");
            }

            Exports.Add(obj);
            return (int)export.CookedSerialSize;
        }
    }

    public override uint ReadHeader()
    {
        if (Game >= EGame.GAME_UE5_0)
        {
            var summary = Read<FZenPackageSummary>();
            Flags = summary.PackageFlags;

            // EUnrealEngineObjectUE5Version::VERSE_CELLS
            var cellOffsets = Read<FZenPackageCellOffsets>();

            NameMap = NameMapContainer.ReadNameMap(this);
            Name = NameMap[(int)summary.Name.NameIndex];

            var padSize = Read<long>();
            Position += padSize;

            var bulkDataMapSize = Read<long>();
            BulkDataMap = ReadArray(() => new FBulkDataMapEntry(this), (int)(bulkDataMapSize / FBulkDataMapEntry.SIZE));

            Position = summary.ImportedPublicExportHashesOffset;
            ImportedPublicExportHashes =
                ReadArray<ulong>((summary.ImportMapOffset - summary.ImportedPublicExportHashesOffset) / sizeof(ulong));

            Position = summary.ImportMapOffset;
            ImportMap = ReadArray<FPackageObjectIndex>((summary.ExportMapOffset - summary.ImportMapOffset) /
                                                       FPackageObjectIndex.Size);

            Position = summary.ExportMapOffset;
            ExportMap = ExportContainer.Read(this, summary, cellOffsets);

            Position = cellOffsets.CellImportMapOffset;
            CellImportMap = ReadArray<FPackageObjectIndex>(
                (cellOffsets.CellExportMapOffset - cellOffsets.CellImportMapOffset) / FPackageObjectIndex.Size);

            Position = cellOffsets.CellExportMapOffset;
            CellExportMap = ReadArray(() => new FCellExportMapEntry(this),
                summary.ExportBundleEntriesOffset - cellOffsets.CellExportMapOffset);

            Position = summary.ExportBundleEntriesOffset;
            ExportBundleEntries =
                ReadArray<FExportBundleEntry>(ExportMap.Length * (byte)EExportCommandType.ExportCommandType_Count);

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
                ReadArray<int>((summary.ImportedPackageNamesOffset - summary.DependencyBundleEntriesOffset) /
                               sizeof(int));

            Position = summary.ImportedPackageNamesOffset;
            ImportedPackageNamesContainer = new FZenPackageImportedPackageNamesContainer(this);

            return summary.HeaderSize;
        }
        else
        {
            var summary = Read<FPackageSummary>();
            Flags = summary.PackageFlags;

            Position = summary.NameMapNamesOffset;
            NameMap = NameMapContainer.ReadNameMap(this, summary);
            Name = NameMap[(int)summary.Name.NameIndex];

            Position = summary.ImportMapOffset;
            ImportMap = ReadArray<FPackageObjectIndex>((summary.ExportMapOffset - summary.ImportMapOffset) /
                                                       FPackageObjectIndex.Size);
            
            Position = summary.ExportMapOffset;
            ExportMap = ExportContainer.Read(this, summary);

            Position = summary.ExportBundlesOffset;
            LoadExportBundles(summary.GraphDataOffset - summary.ExportBundlesOffset, out ExportBundleHeaders,
                out ExportBundleEntries);

            // https://github.com/EpicGames/UnrealEngine/blob/1598cf219e46e521f6049ebb6822a534071b2782/Engine/Source/Developer/IoStoreUtilities/Private/IoStoreUtilities.cpp#L2275
            Position = summary.GraphDataOffset;
            var referencedPackagesCount = Read<int>();
            SortedExternalArcs = new Tuple<FPackageId, FArc[]>[referencedPackagesCount];
            ImportedPackageIds = new();
            
            for (int i = 0; i < SortedExternalArcs.Length; i++)
            {
                var importedPackageId = Read<FPackageId>();
                var externalArcCount = Read<int>();
                var arcs = ReadArray<FArc>(externalArcCount);

                SortedExternalArcs[i] = new(importedPackageId, arcs);
                ImportedPackageIds.Add(importedPackageId);
            }

            return (uint)(summary.GraphDataOffset + summary.GraphDataSize);
        }
    }
    
    // https://github.com/FabianFG/CUE4Parse/blob/ae9c85c8b6bae523b3194be29c72ea889f801a50/CUE4Parse/UE4/Assets/IoPackage.cs#L353-L373
    private void LoadExportBundles(int graphDataSize, out FExportBundleHeader[] bundleHeadersArray, out FExportBundleEntry[] bundleEntriesArray)
    {
        var remainingBundleEntryCount = graphDataSize / (4 + 4);
        var foundBundlesCount = 0;
        var foundBundleHeaders = new List<FExportBundleHeader>();
        while (foundBundlesCount < remainingBundleEntryCount)
        {
            // This location is occupied by header, so it is not a bundle entry
            remainingBundleEntryCount--;
            var bundleHeader = new FExportBundleHeader(this);
            foundBundlesCount += (int) bundleHeader.EntryCount;
            foundBundleHeaders.Add(bundleHeader);
        }

        if (foundBundlesCount != remainingBundleEntryCount)
            throw new DataException($"{nameof(foundBundlesCount)} != {nameof(remainingBundleEntryCount)} ({foundBundlesCount} != {remainingBundleEntryCount})");

        // Load export bundles into arrays
        bundleHeadersArray = foundBundleHeaders.ToArray();
        bundleEntriesArray = ReadArray<FExportBundleEntry>(foundBundlesCount);
    }

    public override List<UProperty> ReadProperties(UStruct structure)
    {
	    return UnversionedPropertyHandler.DeserializeProperties(this, structure);
    }

    // https://github.com/FabianFG/CUE4Parse/blob/11a92870024a088888aae79c74d8ae0c6c8af3e5/CUE4Parse/UE4/Assets/IoPackage.cs#L102
    public void PopulateImportIds()
    {
	    Information($"Attempting to get package imports for '{Name}'");
	    
	    var header = IoReader?.ContainerHeader;
	    if (header == null)
	    {
		    Log.Warning("Could not populate imports because ContainerHeader is null.");
		    return;
	    }

	    var packageId = FPackageId.FromName(Name);
	    var storeEntryIdx = Array.IndexOf(header.PackageIds, packageId);
	    FFilePackageStoreEntry? storeEntry = null;
	    
	    if (storeEntryIdx != -1)
	    {
		    storeEntry = header.StoreEntries[storeEntryIdx];
	    }
	    else
	    {
		    var optionalSegmentStoreEntryIdx = Array.IndexOf(header.OptionalSegmentPackageIds, packageId);
		    if (optionalSegmentStoreEntryIdx != -1)
		    {
			    storeEntry = header.OptionalSegmentStoreEntries[optionalSegmentStoreEntryIdx];
		    }
		    else
		    {
			    Log.Warning("Could not populate imports because store entry could not be found.");
		    }
	    }

	    if (storeEntry == null)
		    return;

	    ImportedPackageIds = storeEntry.ImportedPackages.ToList();
	    Information($"Populated {ImportedPackageIds.Count} imported package ids.");
    }

    /// <summary>
    /// Serializes the entire asset to a stream.
    /// </summary>
    /// <param name="writer"></param>
    public override void WriteAll(Writer writer)
    {
	    CheckMappings();
	    
	    var uexp = new Writer();
	    
	    for (int i = 0; i < ExportMap.Length; i++)
	    {
		    var name = NameMap[(int)ExportMap[i].ObjectName.NameIndex];
		    
		    ExportMap[i].CookedSerialOffset = (ulong)uexp.Position;

		    var start = uexp.Position;
		    var export = Exports[name];

		    if (export == null)
			    throw new KeyNotFoundException("Object exists in the export map, but not in the loaded exports.");
		    
            export.Class = new UStruct(Mappings.FindSchema(export.Class.Name), Mappings);
		    export.Serialize(uexp);

		    ExportMap[i].CookedSerialSize = (ulong)(uexp.Position - start);
	    }
	    
	    WriteHeader(writer);
	    uexp.CopyTo(writer);
	    uexp.Close();
    }

    public override void WriteHeader(Writer writer)
    {
        if (Game >= EGame.GAME_UE5_0)
        {
            var summary = default(FZenPackageSummary);
            summary.Name = new FMappedName((uint)NameMap.GetIndexOrAdd(Name), 0);

            // Reserve space
            var cellOffsets = new FZenPackageCellOffsets();
            writer.Position = FZenPackageSummary.Size + FZenPackageCellOffsets.Size;

            // Write NameMap
            NameMapContainer.WriteNameMap(writer, NameMap);

            writer.Write<long>(0); // pakSize
            writer.Write<long>(BulkDataMap.Length * FBulkDataMapEntry.SIZE); // bulkDataMapSize

            foreach (var entry in BulkDataMap)
                entry.Serialize(writer);

            summary.ImportedPublicExportHashesOffset = (int)writer.Position;
            writer.WriteArray(ImportedPublicExportHashes ?? []);

            summary.ImportMapOffset = (int)writer.Position;
            writer.WriteArray(ImportMap);

            summary.ExportMapOffset = (int)writer.Position;
            foreach (var export in ExportMap)
                export.Serialize(writer, this);

            cellOffsets.CellImportMapOffset = (int)writer.Position;
            writer.WriteArray(CellImportMap);

            cellOffsets.CellExportMapOffset = (int)writer.Position;
            foreach (var cell in CellExportMap)
                cell.Serialize(writer);

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

            summary.ImportedPackageNamesOffset = (int)writer.Position;
            ImportedPackageNamesContainer.Serialize(writer);

            var end = writer.Position;

            summary.HeaderSize = (uint)end;
            summary.PackageFlags = Flags;

            writer.Position = 0;
            writer.Write(summary);
            writer.Write(cellOffsets);

            writer.Position = end;
        }
        else
        {
            var summary = default(FPackageSummary);
            summary.Name = new FMappedName((uint)NameMap.GetIndexOrAdd(Name), 0);
            summary.SourceName = summary.Name; // TODO figure out what this is

            // Reserve space
            writer.Position += FPackageSummary.Size;
            
            // Offset setting is done in WriteNameMap method
            NameMapContainer.WriteNameMap(writer, NameMap, ref summary);
            
            summary.ImportMapOffset = (int)writer.Position;
            writer.WriteArray(ImportMap);
            
            summary.ExportMapOffset = (int)writer.Position;
            foreach (var export in ExportMap)
                export.Serialize(writer, this);

            summary.ExportBundlesOffset = (int)writer.Position;
            SaveExportBundles(writer);

            summary.GraphDataOffset = (int)writer.Position;

            if (ImportedPackageIds != null)
            {
                SortedExternalArcs = new Tuple<FPackageId, FArc[]>[ImportedPackageIds.Count];
                for (var i = 0; i < SortedExternalArcs.Length; i++)
                {
                    // TODO figure out how to properly recreate arcs
                    var id = ImportedPackageIds[i];
                    SortedExternalArcs[i] = new Tuple<FPackageId, FArc[]>(id, [
                        new FArc { FromNodeIndex = 0, ToNodeIndex = 0 }
                    ]);
                }
            }

            writer.Write(SortedExternalArcs.Length);
            foreach (var arc in SortedExternalArcs)
            {
                writer.Write(arc.Item1);
                writer.Write(arc.Item2.Length);
                writer.WriteArray(arc.Item2);
            }

            var end = writer.Position;

            summary.GraphDataSize = (int)(end - summary.GraphDataOffset);
            summary.PackageFlags = Flags;
            summary.CookedHeaderSize = (uint)end; // TODO is this right?
            
            writer.Position = 0;
            writer.Write(summary);

            writer.Position = end;
        }
    }
    
    private void SaveExportBundles(Writer writer)
    {
        if (ExportBundleHeaders == null) // TODO: This eventually needs to be done properly
        {
            var headers = new List<FExportBundleHeader>();
            var entries = new List<FExportBundleEntry>();
            
            foreach (var entry in ExportBundleEntries)
            {
                if (entry.CommandType != EExportCommandType.ExportCommandType_Create)
                    continue;
                
                headers.Add(new FExportBundleHeader 
                {
                    FirstEntryIndex = (uint)entries.Count,
                    EntryCount = 2
                });

                entries.Add(entry with { CommandType = EExportCommandType.ExportCommandType_Create });
                entries.Add(entry with { CommandType = EExportCommandType.ExportCommandType_Serialize });
            }

            ExportBundleHeaders = headers.ToArray();
            ExportBundleEntries = entries.ToArray();
        }
        
        foreach (var bundle in ExportBundleHeaders)
            bundle.Serialize(writer, this);

        writer.WriteArray(ExportBundleEntries);
    }

    // https://github.com/FabianFG/CUE4Parse/blob/87020fa42ab70bb44a08bcd9f5d742ad70c97373/CUE4Parse/UE4/Assets/IoPackage.cs#L331
    public override ResolvedObject? ResolvePackageIndex(FPackageIndex? index)
    {
	    if (index == null || index.IsNull)
			return null;
	    
	    if (index.IsImport && -index.Index - 1 < ImportMap.Length)
		    return ResolveObjectIndex(ImportMap[-index.Index - 1]);
	    
	    if (index.IsExport && -index.Index - 1 < ExportMap.Length)
		    return new ResolvedExportObject(this, index.Index - 1);

	    return null;
    }

    // Loosely based from:
    // https://github.com/FabianFG/CUE4Parse/blob/11a92870024a088888aae79c74d8ae0c6c8af3e5/CUE4Parse/UE4/Assets/IoPackage.cs#L347
    public ResolvedObject? ResolveObjectIndex(FPackageObjectIndex index)
    {
	    if (index.IsNull)
		    return null;

	    if (index.IsExport)
		    return new ResolvedExportObject(this, (int)index.AsExport);

	    if (index.IsScriptImport)
	    {
		    if (GlobalData == null)
			    throw new NoNullAllowedException("Global Data cannot be null.");
		    
		    if (GlobalData.ScriptObjectEntriesMap.TryGetValue(index, out var entry))
			    return new ResolvedScriptObject(entry, this);
	    }

        if (index.IsPackageImport && System != null && ImportedPackageIds != null)
        {
            if (ImportedPublicExportHashes != null)
            {
                var packageImportRef = index.AsPackageImportRef;
                if (packageImportRef.ImportedPackageIndex < ImportedPackageIds.Count)
                {
                    var packageId = ImportedPackageIds[(int)packageImportRef.ImportedPackageIndex];

                    if (!System.TryExtractAndRead(packageId, out var pkg, onlyReadHeader: true))
                    {
                        Error($"Unable to extract and/or read asset with package id: {packageId.Id}");
                        return null;
                    }

                    for (var exportIndex = 0; exportIndex < pkg.ExportMap.Length; exportIndex++)
                    {
                        if (pkg.ExportMap[exportIndex].PublicExportHash ==
                            ImportedPublicExportHashes![packageImportRef.ImportedPublicExportHashIndex])
                        {
                            return new ResolvedExportObject(pkg, exportIndex);
                        }
                    }
                }
                else
                {
                    Log.Error("Reading Pak assets are not implemented yet!");
                    return null;
                }
            }
            else
            {
                foreach (var packageId in ImportedPackageIds)
                {
                    if (!System.TryExtractAndRead(packageId, out var pkg, onlyReadHeader: true))
                    {
                        Error($"Unable to extract and/or read asset with package id: {packageId.Id}");
                        return null;
                    }

                    for (var exportIndex = 0; exportIndex < pkg.ExportMap.Length; exportIndex++)
                    {
                        if (pkg.ExportMap[exportIndex].GlobalImportIndex != index) 
                            continue;
                        
                        pkg.Position = 0;
                        pkg.ReadAll();
                        return new ResolvedExportObject(pkg, exportIndex);
                    }
                }
            }
        }

        return null;
    }

    public override void WriteProperties(Writer writer, string type, List<UProperty> properties)
    {
	    CheckMappings();
	    
	    var schema = Mappings?.FindSchema(type) ?? throw new KeyNotFoundException($"Cannot find schema with name '{type}'");
	    var struc = new UStruct(schema, Mappings);
	    
	    UnversionedPropertyHandler.SerializeProperties(this, writer, struc, properties);
    }
}

public class ExportContainer : Container<FExportMapEntry>
{
	public override int GetIndex(string str) => Items.FindIndex(x => x.Name == str);

	public ExportContainer(List<FExportMapEntry> items) : base(items)
	{ }

	public static ExportContainer Read(ZenAsset asset, FZenPackageSummary summary, FZenPackageCellOffsets? cellOffsets)
	{
		var size = cellOffsets.HasValue
			? cellOffsets.Value.CellImportMapOffset - summary.ExportMapOffset
			: summary.ExportBundleEntriesOffset - summary.ExportMapOffset;
		return new ExportContainer(asset.ReadArray(() => new FExportMapEntry(asset), size / FExportMapEntry.Size)
			.ToList());
	}
    
    public static ExportContainer Read(ZenAsset asset, FPackageSummary summary)
    {
        var size = summary.ExportBundlesOffset - summary.ExportMapOffset;
        return new ExportContainer(asset.ReadArray(() => new FExportMapEntry(asset), size / FExportMapEntry.Size)
            .ToList());
    }
}