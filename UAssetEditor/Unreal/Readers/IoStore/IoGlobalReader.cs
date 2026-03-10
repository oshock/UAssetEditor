using System.Data;
using UAssetEditor.Binary;
using UAssetEditor.Unreal.Names;
using UAssetEditor.Unreal.Objects;
using UAssetEditor.Unreal.Versioning;

namespace UAssetEditor.Unreal.Readers.IoStore;

public readonly struct FScriptObjectEntry
{
    public readonly FMappedName ObjectName;
    public readonly FPackageObjectIndex GlobalIndex;
    public readonly FPackageObjectIndex OuterIndex;
    public readonly FPackageObjectIndex CDOClassIndex;

    public FName GetObjectName(IoGlobalReader globalData) => new(ObjectName, globalData.GlobalNameMap);
}

public class IoGlobalReader : Reader
{
    public NameMapContainer GlobalNameMap;
    public Dictionary<FPackageObjectIndex, FScriptObjectEntry> ScriptObjectEntriesMap = new();
 
    public string GetScriptName(FPackageObjectIndex index) => GlobalNameMap[ScriptObjectEntriesMap[index].ObjectName.NameIndex];
    
    public IoGlobalReader(byte[] data) : base(data)
    { }

    public static IoGlobalReader InitializeGlobalData(string path, EGame game = EGame.GAME_UE5_0)
    {
        var ioStoreReader = new IoStoreReader(null, path);
        IoGlobalReader? reader = null;

        if (game >= EGame.GAME_UE5_0)
        {
            var data = ioStoreReader.ExtractChunk(new FIoChunkId(0, 0, EIoChunkType5.ScriptObjects));
            reader = new IoGlobalReader(data);
            reader.GlobalNameMap = NameMapContainer.ReadNameMap(reader);
        }
        else
        {
            reader = new IoGlobalReader(
                ioStoreReader.ExtractChunk(new FIoChunkId(0, 0, EIoChunkType.LoaderInitialLoadMeta)));
            
            var hashesOffsetAndLength =
                (ioStoreReader.FindChunkInternal(new FIoChunkId(0, 0, EIoChunkType.LoaderGlobalNameHashes)) 
                 ?? throw new DataException("Could not find EIoChunkType.LoaderGlobalNameHashes!"))
                .Length;
            
            var namesReader = new Reader(ioStoreReader.ExtractChunk(new FIoChunkId(0, 0, EIoChunkType.LoaderGlobalNames)));
            reader.GlobalNameMap = NameMapContainer.ReadNameMap(namesReader, (int)namesReader.Length, (int)hashesOffsetAndLength, false);
        }

        var numScriptObjects = reader.Read<int>();
        var scriptObjectEntries = reader.ReadArray<FScriptObjectEntry>(numScriptObjects);

        foreach (var obj in scriptObjectEntries)
            reader.ScriptObjectEntriesMap[obj.GlobalIndex] = obj;

        return reader;
    }
}