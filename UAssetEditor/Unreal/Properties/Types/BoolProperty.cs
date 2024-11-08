using UAssetEditor.Binary;
using UsmapDotNet;


namespace UAssetEditor.Unreal.Properties.Types;

public class BoolProperty : AbstractProperty<bool>
{
    public override void Read(Reader reader, UsmapPropertyData? data, Asset? asset = null, EReadMode mode = EReadMode.Normal)
    {
        if (mode == EReadMode.Zero) // TODO determine what the default value is
        {
            Value = false;
            return;
        }
        
        Value = reader.ReadByte() != 0;
    }

    public override void Write(Writer writer, UProperty property, Asset? asset = null)
    {
        if (property.Value is null)
        {
            writer.WriteByte(0);
            return;
        }
        
        writer.WriteByte((byte)property.Value);
    }
}