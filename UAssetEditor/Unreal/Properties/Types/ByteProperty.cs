using UAssetEditor.Binary;
using UsmapDotNet;


namespace UAssetEditor.Unreal.Properties.Types;

public class ByteProperty : AbstractProperty<byte>
{
    public override void Read(Reader reader, UsmapPropertyData? data, BaseAsset? asset = null, bool isZero = false)
    {
        Value = isZero ? (byte)0 : reader.ReadByte();
    }

    public override void Write(Writer writer, UProperty property, BaseAsset? asset = null)
    {
        writer.WriteByte(Value);
    }
}