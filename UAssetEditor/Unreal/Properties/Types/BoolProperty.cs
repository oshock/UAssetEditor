using UnrealExtractor.Binary;
using UnrealExtractor.Utils;
using UsmapDotNet;


namespace UAssetEditor.Unreal.Properties.Types;

public class BoolProperty : AbstractProperty<bool>
{
    public override void Read(Reader reader, UsmapPropertyData? data, Asset? asset = null, ESerializationMode mode = ESerializationMode.Normal)
    {
        if (mode == ESerializationMode.Zero) // TODO determine what the default value is
        {
            Value = false;
            return;
        }
        
        Value = reader.ReadByte() != 0;
    }

    public override void Write(Writer writer, UProperty property, Asset? asset = null, ESerializationMode mode = ESerializationMode.Normal)
    {
        if (property.Value is null)
        {
            writer.WriteByte(0);
            return;
        }

        writer.WriteByte((byte)(property.Value.As<BoolProperty>().Value ? 1 : 0));
    }
}