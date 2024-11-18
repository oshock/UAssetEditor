using UnrealExtractor.Binary;
using UnrealExtractor.Classes.Containers;
using UnrealExtractor.Unreal.Names;

namespace UnrealExtractor.Unreal.Readers.IoStore;

public readonly struct FScriptObjectEntry
{
    public readonly FMappedName ObjectName;
    public readonly ulong GlobalIndex;
    public readonly ulong OuterIndex;
    public readonly ulong CDOClassIndex;
}

public class IoGlobalReader : Reader
{
    public NameMapContainer GlobalNameMap;
    public Dictionary<ulong, FScriptObjectEntry> ScriptObjectEntriesMap = new();
 
    public string GetScriptName(ulong index) => GlobalNameMap[ScriptObjectEntriesMap[index].ObjectName.NameIndex];
    
    public IoGlobalReader(byte[] data) : base(data)
    { }

    public static IoGlobalReader InitializeGlobalData(string path)
    {
        var ioStoreReader = new IoStoreReader(null, path);
        var data = ioStoreReader.ExtractChunk(0, EIoChunkType5.ScriptObjects);
        var reader = new IoGlobalReader(data);
        
        reader.GlobalNameMap = NameMapContainer.ReadNameMap(reader);

        var numScriptObjects = reader.Read<int>();
        var scriptObjectEntries = reader.ReadArray<FScriptObjectEntry>(numScriptObjects);

        foreach (var obj in scriptObjectEntries)
            reader.ScriptObjectEntriesMap[obj.GlobalIndex] = obj;

        return reader;
    }
}