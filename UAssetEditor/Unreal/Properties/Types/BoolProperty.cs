using UAssetEditor.Binary;
using UsmapDotNet;


namespace UAssetEditor.Unreal.Properties.Types;

public class BoolProperty : AbstractProperty<bool>
{
    public override void Read(Reader reader, UsmapPropertyData? data, BaseAsset? asset = null, bool isZero = false)
    {
        if (isZero) // TODO determine what the default value is
        {
            Value = false;
            return;
        }
        
        Value = reader.ReadByte() != 0;
    }

    public override void Write(Writer writer, UProperty property, BaseAsset? asset = null)
    {
        if (property.Value is null)
        {
            writer.WriteByte(0);
            return;
        }
        
        writer.WriteByte((byte)property.Value);
    }
}