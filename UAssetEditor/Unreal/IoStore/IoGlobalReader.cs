using Astro.App.IoReader;
using UAssetEditor.Names;

namespace UAssetEditor.IoStore;

public readonly struct FScriptObjectEntry
{
    public readonly FMappedName ObjectName;
    public readonly ulong GlobalIndex;
    public readonly ulong OuterIndex;
    public readonly ulong CDOClassIndex;
}

public class IoGlobalReader
{
    public readonly List<string> GlobalNameMap;
    public readonly Dictionary<ulong, FScriptObjectEntry> ScriptObjectEntriesMap = new();
 
    public string GetScriptName(ulong index) => GlobalNameMap[(int)ScriptObjectEntriesMap[index].ObjectName.NameIndex];
    
    public IoGlobalReader(string path)
    {
        var ioStoreReader = new IoStoreReader(path);
        var reader = ioStoreReader.Extract(ioStoreReader.ExtractChunk(0, IoStoreReader.EIoChunkType5.ScriptObjects));
        GlobalNameMap = ZenAsset.ReadNameMap(reader).Strings;

        var numScriptObjects = reader.Read<int>();
        var scriptObjectEntries = reader.ReadArray<FScriptObjectEntry>(numScriptObjects);

        foreach (var obj in scriptObjectEntries)
            ScriptObjectEntriesMap[obj.GlobalIndex] = obj;
    }
}