using UAssetEditor.Binary;
using UAssetEditor.Names;
using UAssetEditor.Unreal.Names;
using Usmap.NET;

namespace UAssetEditor.Properties;

public class SoftObjectProperty : AbstractProperty
{
    public string AssetPathName;
    public string PackageName;
    public string SubPathName = "";

    public static SoftObjectProperty Create(string path)
    {
        var split = path.Split('.');
        return new SoftObjectProperty
        {
            AssetPathName = split[0],
            PackageName = split[1],
            Value = $"{split[0]}.{split[1]}"
        };
    }
    
    public override void Read(Reader reader, UsmapPropertyData? data, BaseAsset asset = null)
    {
        AssetPathName = new FName(reader, asset.NameMap).Name;
        PackageName = new FName(reader, asset.NameMap).Name;
        SubPathName = FString.Read(reader); // idk
        Value = $"{AssetPathName}.{PackageName}";
    }

    public override void Write(Writer writer, UProperty property, BaseAsset asset = null)
    {
        new FName(asset.NameMap, AssetPathName, 0).Serialize(writer);
        new FName(asset.NameMap, PackageName, 0).Serialize(writer);
        FString.Write(writer, SubPathName);
    }
}