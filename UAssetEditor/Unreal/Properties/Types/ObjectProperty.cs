

using System.Data;
using UAssetEditor.Unreal.Objects;
using UAssetEditor.Binary;
using UAssetEditor.Unreal.Assets;
using UAssetEditor.Unreal.Versioning;

namespace UAssetEditor.Unreal.Properties.Types;

public class ObjectProperty : AbstractProperty<FPackageIndex>
{
    public ObjectProperty()
    { }
    
    public ObjectProperty(FPackageIndex value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value?.ResolvedObject?.Name.ToString() ?? "None";
    }

    public override void Read(Reader reader, PropertyData? data, Asset? asset = null,
        ESerializationMode mode = ESerializationMode.Normal)
    {
        Value = new FPackageIndex(asset, mode == ESerializationMode.Zero ? 0 : reader.Read<int>());

        if (asset == null)
            return;

        if (asset.ForceLoadImportedObjects)
            Value.Resolve();
    }
    
    public override void Write(Writer writer, UProperty property, Asset? asset = null, ESerializationMode mode = ESerializationMode.Normal)
    {
        if (Value != null)
        {
            if (asset is PakAsset)
                throw new NotImplementedException();
            
            if (Value.IsImport)
            {
                if (asset.Game >= EGame.GAME_UE5_0)
                {
                    // TODO
                }
                else
                {
                    var obj = Value.ResolvedObject;
                    if (obj == null)
                        throw new NoNullAllowedException(
                            "Cannot remap object property becasue resolved object was never found.");

                    var exportName = obj.Package.Exports[obj.ExportIndex].Name;
                    obj.ReloadPackage(asset.System);
                    
                    var zen = (ZenAsset)obj.Package;
                    var index = zen.ExportMap.GetIndex(exportName);
                    var globalImportIndex = zen.ExportMap[index].GlobalImportIndex;

                    var thisAsset = (ZenAsset)asset;
                    thisAsset.ImportMap[-Value.Index - 1] = globalImportIndex;
                    
                    Information($"Remapped '{exportName}'...");
                }
            }
        }

        writer.Write(Value?.Index ?? 0);
    }
}