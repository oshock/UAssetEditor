using UAssetEditor.Binary;


namespace UAssetEditor.Unreal.Properties.Types;

public class UInt16Property : AbstractProperty<ushort>
{
    public override void Read(Reader reader, PropertyData? data, Asset? asset = null,
        ESerializationMode mode = ESerializationMode.Normal)
    {
        Value = reader.Read<ushort>();
    }

    public override void Write(Writer writer, UProperty property, Asset? asset = null, ESerializationMode mode = ESerializationMode.Normal)
    {
        writer.Write(Value);
    }
}