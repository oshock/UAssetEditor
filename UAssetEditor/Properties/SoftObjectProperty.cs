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
        Value = $"{AssetPathName}.{AssetPathName}";
    }

    public override void Write(Writer writer, UProperty property, UAsset? asset = null)
    {
        var assetIndex = asset!.NameMap.Strings.FindIndex(x => x == AssetPathName);
        var packageIndex = asset!.NameMap.Strings.FindIndex(x => x == PackageName);
        
        writer.Write(assetIndex);
        writer.Write(packageIndex);
        FString.Write(writer, SubPathName);
    }
}