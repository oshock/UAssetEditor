using UnrealExtractor.Binary;
using UsmapDotNet;


namespace UAssetEditor.Unreal.Properties.Types;

public class UInt64Property : AbstractProperty<ulong>
{
    public override void Read(Reader reader, UsmapPropertyData? data, Asset? asset = null, EReadMode mode = EReadMode.Normal)
    {
        Value = reader.Read<ulong>();
    }

    public override void Write(Writer writer, UProperty property, Asset? asset = null)
    {
        writer.Write(Value);
    }
}