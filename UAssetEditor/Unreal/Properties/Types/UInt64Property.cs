using UAssetEditor.Binary;
using UsmapDotNet;


namespace UAssetEditor.Unreal.Properties.Types;

public class UInt64Property : AbstractProperty<ulong>
{
    public override void Read(Reader reader, UsmapPropertyData? data, BaseAsset? asset = null, bool isZero = false)
    {
        Value = reader.Read<ulong>();
    }

    public override void Write(Writer writer, UProperty property, BaseAsset? asset = null)
    {
        writer.Write(Value);
    }
}