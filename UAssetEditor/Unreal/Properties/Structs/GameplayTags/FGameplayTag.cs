using UAssetEditor.Binary;
using UAssetEditor.Unreal.Names;
using UsmapDotNet;

namespace UAssetEditor.Unreal.Properties.Structs;

public class FGameplayTag : UStruct
{
    public FName Name;

    public virtual void Read(Reader reader, UsmapPropertyData? data, BaseAsset? asset = null, EReadMode mode = EReadMode.Normal)
    {
        
    } 
    
    public void Write(Writer writer, BaseAsset? asset = null)
    { }
}