using UAssetEditor.Binary;
using UAssetEditor.Unreal.Assets;
using UAssetEditor.Unreal.Objects;

namespace UAssetEditor.Unreal.Exports.Engine;

public class UBlueprint : UObject
{
    public FPackageIndex? SkeletonGeneratedClass;
    public FPackageIndex? GeneratedClass;
    
    public UBlueprint(Asset asset) : base(asset)
    { }

    public override void Deserialize(long position)
    {
        base.Deserialize(position);

        SkeletonGeneratedClass = FPackageIndex.Read(Owner);
        GeneratedClass = FPackageIndex.Read(Owner);
    }

    public override void Serialize(Writer writer)
    {
        base.Serialize(writer);
    }
}