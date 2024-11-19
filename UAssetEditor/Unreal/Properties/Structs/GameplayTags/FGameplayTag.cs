using UAssetEditor.Unreal.Names;
using UnrealExtractor.Binary;
using UsmapDotNet;

namespace UAssetEditor.Unreal.Properties.Structs;

public class FGameplayTag : UStruct
{
    public FName Name;

    public virtual void Read(Reader reader, UsmapPropertyData? data, Asset? asset = null, ESerializationMode mode = ESerializationMode.Normal)
    {
        
    } 
    
    public void Write(Writer writer, Asset? asset = null)
    { }
}