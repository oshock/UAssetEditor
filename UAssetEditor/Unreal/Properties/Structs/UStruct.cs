using UAssetEditor.Binary;
using UsmapDotNet;

namespace UAssetEditor.Unreal.Properties.Structs;

public abstract class UStruct
{
    public virtual void Read(Reader reader, UsmapPropertyData? data, BaseAsset? asset = null, bool isZero = false)
    { } 

    public virtual void Write(Writer writer, BaseAsset? asset = null)
    { }
}