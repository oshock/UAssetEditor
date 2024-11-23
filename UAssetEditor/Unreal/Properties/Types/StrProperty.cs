using UAssetEditor.Unreal.Names;
using UAssetEditor.Binary;


namespace UAssetEditor.Unreal.Properties.Types;

public class StrProperty : AbstractProperty<string>
{
    public override void Read(Reader reader, PropertyData? data, Asset? asset = null,
        ESerializationMode mode = ESerializationMode.Normal)
    {
        Value = FString.Read(reader);
    }

    public override void Write(Writer writer, UProperty property, Asset? asset = null, ESerializationMode mode = ESerializationMode.Normal)
    {
        FString.Write(writer, Value ?? string.Empty);
    }
}