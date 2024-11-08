using UAssetEditor.Binary;
using UsmapDotNet;

namespace UAssetEditor.Unreal.Properties.Structs;

public abstract class UStruct
{
    public virtual void Read(Reader reader, UsmapPropertyData? data, Asset? asset = null, EReadMode mode = EReadMode.Normal)
    { } 

    public virtual void Write(Writer writer, Asset? asset = null)
    { }
}