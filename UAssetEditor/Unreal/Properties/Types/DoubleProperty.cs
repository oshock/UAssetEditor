using UAssetEditor.Binary;
using UsmapDotNet;


namespace UAssetEditor.Unreal.Properties.Types;

public class DoubleProperty : AbstractProperty<double>
{
    public override void Read(Reader reader, UsmapPropertyData? data, BaseAsset? asset = null, bool isZero = false)
    {
        Value = isZero ? 0.0 : reader.Read<double>();
    }

    public override void Write(Writer writer, UProperty property, BaseAsset? asset = null)
    {
        writer.Write(Value);
    }
}