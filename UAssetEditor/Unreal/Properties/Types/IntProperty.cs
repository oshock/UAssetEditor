using UAssetEditor.Binary;
using UsmapDotNet;


namespace UAssetEditor.Unreal.Properties.Types;

public class IntProperty : AbstractProperty<int>
{
    public override void Read(Reader reader, UsmapPropertyData? data, BaseAsset? asset = null, bool isZero = false)
    {
        Value = isZero ? 0 : reader.Read<int>();
    }

    public override void Write(Writer writer, UProperty property, BaseAsset? asset = null)
    {
        writer.Write(Value);
    }
}