using UAssetEditor.Binary;
using UsmapDotNet;


namespace UAssetEditor.Unreal.Properties.Types;

public class Int16Property : AbstractProperty<short>
{
    public override void Read(Reader reader, UsmapPropertyData? data, BaseAsset? asset = null, bool isZero = false)
    {
        Value = isZero ? (short)0 : reader.Read<short>();
    }

    public override void Write(Writer writer, UProperty property, BaseAsset? asset = null)
    {
        writer.Write(Value);
    }
}