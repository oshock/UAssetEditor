using UAssetEditor.Binary;
using UAssetEditor.Unreal.Assets;
using UAssetEditor.Unreal.Versioning;

namespace UAssetEditor.Unreal.Objects.IO;

public class FExportBundleHeader
{
    public ulong SerialOffset;
    public uint FirstEntryIndex;
    public uint EntryCount;

    public FExportBundleHeader()
    { }
    
    public FExportBundleHeader(Asset asset)
    {
        SerialOffset = asset.Game >= EGame.GAME_UE5_0 ? asset.Read<ulong>() : ulong.MaxValue;
        FirstEntryIndex = asset.Read<uint>();
        EntryCount = asset.Read<uint>();
    }

    public void Serialize(Writer writer, Asset asset)
    {
        if (asset.Game >= EGame.GAME_UE5_0)
            writer.Write(SerialOffset);
        
        writer.Write(FirstEntryIndex);
        writer.Write(EntryCount);
    }
}