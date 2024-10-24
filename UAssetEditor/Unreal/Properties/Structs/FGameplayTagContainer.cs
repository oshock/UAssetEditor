using System.Data;
using UAssetEditor.Binary;
using UAssetEditor.Unreal.Names;
using UsmapDotNet;

namespace UAssetEditor.Unreal.Properties.Structs;

public class FGameplayTagContainer : UStruct
{
    public List<FName> Tags;

    public static List<FName> ReadGameplayTagArray(BaseAsset? asset, Reader reader)
    {
        if (asset is null)
            throw new NoNullAllowedException("Asset cannot be null.");
        
        var tags = new List<FName>();
        var tagCount = reader.Read<uint>();

        for (int i = 0; i < tagCount; i++)
            tags.Add(new FName(reader, asset.NameMap));

        return tags;
    }
    
    public static void WriteGameplayTagArray(Writer writer, List<FName> names, BaseAsset asset)
    {
        if (asset is null)
            throw new NoNullAllowedException("Asset cannot be null.");
        
        writer.Write(names.Count);

        foreach (var name in names)
            name.Serialize(writer, asset.NameMap);
    }
    
    public override void Read(Reader reader, UsmapPropertyData? data, BaseAsset? asset = null, bool isZero = false)
    {
        Tags = ReadGameplayTagArray(asset, reader);
    }

    public override void Write(Writer writer, BaseAsset? asset = null)
    {
        ArgumentNullException.ThrowIfNull(asset);
        WriteGameplayTagArray(writer, Tags, asset);
    }
}