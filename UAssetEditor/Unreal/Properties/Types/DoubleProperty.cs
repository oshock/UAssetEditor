using UnrealExtractor.Binary;
using UsmapDotNet;


namespace UAssetEditor.Unreal.Properties.Types;

public class DoubleProperty : AbstractProperty<double>
{
    public override void Read(Reader reader, UsmapPropertyData? data, Asset? asset = null, ESerializationMode mode = ESerializationMode.Normal)
    {
        Value = mode == ESerializationMode.Zero ? 0.0 : reader.Read<double>();
    }

    public override void Write(Writer writer, UProperty property, Asset? asset = null, ESerializationMode mode = ESerializationMode.Normal)
    {
        writer.Write(Value);
    }
}