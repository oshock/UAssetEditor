using UAssetEditor.Binary;


namespace UAssetEditor.Unreal.Properties.Types;

public class IntProperty : AbstractProperty<int>
{
    public override void Read(Reader reader, PropertyData? data, Asset? asset = null,
        ESerializationMode mode = ESerializationMode.Normal)
    {
        Value = mode == ESerializationMode.Zero ? 0 : reader.Read<int>();
    }

    public override void Write(Writer writer, UProperty property, Asset? asset = null, ESerializationMode mode = ESerializationMode.Normal)
    {
        writer.Write(Value);
    }
}