using UAssetEditor.Names;
using UAssetEditor.Unreal.Names;

namespace UAssetEditor.Properties.Structs;

public struct FGameplayTagContainer
{
    public List<FName> GameplayTags;
    public List<FName> ParentTags;
}