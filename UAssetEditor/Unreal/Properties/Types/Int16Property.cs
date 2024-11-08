using UAssetEditor.Binary;
using UsmapDotNet;


namespace UAssetEditor.Unreal.Properties.Types;

public class Int16Property : AbstractProperty<short>
{
    public override void Read(Reader reader, UsmapPropertyData? data, Asset? asset = null, EReadMode mode = EReadMode.Normal)
    {
        Value = mode == EReadMode.Zero ? (short)0 : reader.Read<short>();
    }

    public override void Write(Writer writer, UProperty property, Asset? asset = null)
    {
        writer.Write(Value);
    }
}