using UAssetEditor.Binary;
using UsmapDotNet;


namespace UAssetEditor.Unreal.Properties.Types;

public class FloatProperty : AbstractProperty<float>
{
    public override void Read(Reader reader, UsmapPropertyData? data, Asset? asset = null, EReadMode mode = EReadMode.Normal)
    {
        if (mode == EReadMode.Zero)
        {
            Value = 0f;
            return;
        }
        
        Value = reader.Read<float>();
    }

    public override void Write(Writer writer, UProperty property, Asset? asset = null)
    {
        writer.Write(Value);
    }
}