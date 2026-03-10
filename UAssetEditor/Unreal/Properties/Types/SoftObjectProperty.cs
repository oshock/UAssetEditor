using UAssetEditor.Unreal.Names;
using UAssetEditor.Binary;
using UAssetEditor.Classes;
using UAssetEditor.Unreal.Assets;


namespace UAssetEditor.Unreal.Properties.Types;

public class SoftObjectProperty : AbstractProperty<string>
{
    public FName AssetPathName;
    public FName PackageName;
    public string SubPathName = "";

    public SoftObjectProperty()
    {
        AssetPathName = new FName();
        PackageName = new FName();
    }
    
    public SoftObjectProperty(string value) : this()
    {
        Value = value;
    }
    
    public override string ToString()
    {
        return Value ?? "None";
    }

    public static SoftObjectProperty Create(string path)
    {
        var split = path.Split('.');
        return new SoftObjectProperty
        {
            AssetPathName = new FName(split[0]),
            PackageName = new FName(split[1]),
            Value = $"{split[0]}.{split[1]}"
        };
    }
    
    public override void Read(Reader reader, PropertyData? data, Asset? asset = null,
        ESerializationMode mode = ESerializationMode.Normal)
    {
        ArgumentNullException.ThrowIfNull(asset);

        if (mode == ESerializationMode.Zero)
        {
            AssetPathName = new FName();
            PackageName = new FName();
            SubPathName = "";
            return;
        }

        if (asset.FileVersion >= EUnrealEngineObjectUE5Version.FSOFTOBJECTPATH_REMOVE_ASSET_PATH_FNAMES)
        {
            AssetPathName = new FName(reader, asset.NameMap);
            PackageName = new FName(reader, asset.NameMap);
        }
        else
        {
            var path = new FName(reader, asset.NameMap).Name.Split(".");
            AssetPathName = new FName(path[0]);
            PackageName = new FName(path[1]);
        }

        Value = $"{AssetPathName}.{PackageName}";
        SubPathName = FString.Read(reader); // TODO fortnite version branch object
    }

    public override void Write(Writer writer, UProperty property, Asset? asset = null,
        ESerializationMode mode = ESerializationMode.Normal)
    {
        ArgumentNullException.ThrowIfNull(asset);
        ArgumentNullException.ThrowIfNull(Value);

        var split = Value.Split('.');
        AssetPathName = new FName(split[0]);
        PackageName = new FName(split[1]);

        if (asset.FileVersion >= EUnrealEngineObjectUE5Version.FSOFTOBJECTPATH_REMOVE_ASSET_PATH_FNAMES)
        {
            AssetPathName.Serialize(writer, asset.NameMap);
            PackageName.Serialize(writer, asset.NameMap);
        }
        else
        {
            var path = new FName(Value);
            path.Serialize(writer, asset.NameMap);
        }
        
        FString.Write(writer, SubPathName);
    }
}