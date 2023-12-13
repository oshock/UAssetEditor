using Usmap.NET;

namespace UAssetEditor.Properties;

public class SoftObjectProperty : AbstractProperty
{
    public string AssetPathName;
    public string PackageName;
    public string SubPathName;
    
    public override void Read(Reader reader, UsmapPropertyData? data, UAsset? asset = null)
    {
        AssetPathName = new FName(reader, asset.NameMap).Name;
        PackageName = new FName(reader, asset.NameMap).Name;
        SubPathName = FString.Read(reader); // idk
        Value = $"{AssetPathName}.{PackageName}";
    }

    public override void Write(Writer writer, UProperty property, UAsset? asset = null)
    {
        new FName(asset.NameMap, AssetPathName, 0).Serialize(writer);
        new FName(asset.NameMap, PackageName, 0).Serialize(writer);
        FString.Write(writer, SubPathName);
    }
}