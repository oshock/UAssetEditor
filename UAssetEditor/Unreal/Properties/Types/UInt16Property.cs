using UAssetEditor.Binary;
using UsmapDotNet;


namespace UAssetEditor.Unreal.Properties.Types;

public class UInt16Property : AbstractProperty<ushort>
{
    public override void Read(Reader reader, UsmapPropertyData? data, BaseAsset? asset = null, EReadMode mode = EReadMode.Normal)
    {
        Value = reader.Read<ushort>();
    }

    public override void Write(Writer writer, UProperty property, BaseAsset? asset = null)
    {
        writer.Write(Value);
    }
}