using UAssetEditor.Binary;
using UsmapDotNet;


namespace UAssetEditor.Unreal.Properties.Types;

public class IntProperty : AbstractProperty<int>
{
    public override void Read(Reader reader, UsmapPropertyData? data, Asset? asset = null, EReadMode mode = EReadMode.Normal)
    {
        Value = mode == EReadMode.Zero ? 0 : reader.Read<int>();
    }

    public override void Write(Writer writer, UProperty property, Asset? asset = null)
    {
        writer.Write(Value);
    }
}