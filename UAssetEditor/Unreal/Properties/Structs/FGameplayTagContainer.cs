using System.Data;
using UAssetEditor.Binary;
using UAssetEditor.Unreal.Names;
using UsmapDotNet;

namespace UAssetEditor.Unreal.Properties.Structs;

public class FGameplayTagContainer : UStruct
{
    public List<FName> GameplayTags;
    public List<FName> ParentTags;

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
    
    public override void Read(Reader reader, UsmapPropertyData? data, BaseAsset? asset = null, bool isZero = false)
    {
        GameplayTags = ReadGameplayTagArray(asset, reader);
        ParentTags = ReadGameplayTagArray(asset, reader);
    }

    public override void Write(Writer writer, BaseAsset? asset = null)
    {
        
    }
}