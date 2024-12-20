using UAssetEditor.Unreal.Names;
using UAssetEditor.Binary;


namespace UAssetEditor.Unreal.Properties.Types;

public class SoftObjectProperty : AbstractProperty<string>
{
    public string AssetPathName;
    public string PackageName;
    public string SubPathName = "";

    public override string ToString()
    {
        return Value ?? "None";
    }

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
    
    public override void Read(Reader reader, PropertyData? data, Asset? asset = null,
        ESerializationMode mode = ESerializationMode.Normal)
    {
        ArgumentNullException.ThrowIfNull(asset);

        if (mode == ESerializationMode.Zero)
        {
            AssetPathName = "None";
            PackageName = "None";
            SubPathName = "";
            return;
        }
        
        AssetPathName = new FName(reader, asset.NameMap).Name;
        PackageName = new FName(reader, asset.NameMap).Name;
        SubPathName = FString.Read(reader); // idk
        
        Value = $"{AssetPathName}.{PackageName}";
    }

    public override void Write(Writer writer, UProperty property, Asset? asset = null, ESerializationMode mode = ESerializationMode.Normal)
    {
        ArgumentNullException.ThrowIfNull(asset);
        ArgumentNullException.ThrowIfNull(Value);

        var split = Value.Split('.');
        AssetPathName = split[0];
        PackageName = split[1];
        
        new FName(AssetPathName).Serialize(writer, asset.NameMap);
        new FName(PackageName).Serialize(writer, asset.NameMap);
        FString.Write(writer, SubPathName);
    }
}