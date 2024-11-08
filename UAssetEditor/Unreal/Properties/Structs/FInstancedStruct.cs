using UAssetEditor.Binary;
using UAssetEditor.Unreal.Properties.Reflection;
using UAssetEditor.Unreal.Properties.Structs;
using UsmapDotNet;

namespace UAssetEditor.Unreal.Properties.Types;

// TODO
public class FInstancedStruct : UStruct
{
    public ObjectProperty Index;
    public byte[] Buffer;

    public override void Read(Reader reader, UsmapPropertyData? data, Asset? asset = null, EReadMode mode = EReadMode.Normal)
    {
        Index = (ObjectProperty)PropertyReflector.ReadProperty("ObjectProperty", reader, null);
        var serialSize = reader.Read<int>();

        /*if (TryGetStructByIndexOrWhatever(index, out var @struct))
        {

        }
        else*/
        {
            Buffer = reader.ReadBytes(serialSize);
        }
    }

    public override void Write(Writer writer, Asset? asset = null)
    {
        PropertyReflector.WriteProperty(writer, Index, asset);
        
        writer.Write(Buffer.Length);
        writer.WriteBytes(Buffer);
    }
}