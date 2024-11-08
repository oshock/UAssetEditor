using UAssetEditor.Binary;
using UsmapDotNet;


namespace UAssetEditor.Unreal.Properties.Types;

public class DoubleProperty : AbstractProperty<double>
{
    public override void Read(Reader reader, UsmapPropertyData? data, Asset? asset = null, EReadMode mode = EReadMode.Normal)
    {
        Value = mode == EReadMode.Zero ? 0.0 : reader.Read<double>();
    }

    public override void Write(Writer writer, UProperty property, Asset? asset = null)
    {
        writer.Write(Value);
    }
}