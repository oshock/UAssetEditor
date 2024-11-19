using UnrealExtractor.Binary;
using UsmapDotNet;


namespace UAssetEditor.Unreal.Properties.Types;

public class UInt16Property : AbstractProperty<ushort>
{
    public override void Read(Reader reader, UsmapPropertyData? data, Asset? asset = null, ESerializationMode mode = ESerializationMode.Normal)
    {
        Value = reader.Read<ushort>();
    }

    public override void Write(Writer writer, UProperty property, Asset? asset = null, ESerializationMode mode = ESerializationMode.Normal)
    {
        writer.Write(Value);
    }
}