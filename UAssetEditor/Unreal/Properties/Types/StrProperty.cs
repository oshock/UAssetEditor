using UAssetEditor.Binary;
using UAssetEditor.Names;
using UsmapDotNet;


namespace UAssetEditor.Unreal.Properties.Types;

public class StrProperty : AbstractProperty<string>
{
    public override void Read(Reader reader, UsmapPropertyData? data, Asset? asset = null, EReadMode mode = EReadMode.Normal)
    {
        Value = FString.Read(reader);
    }

    public override void Write(Writer writer, UProperty property, Asset? asset = null)
    {
        FString.Write(writer, Value ?? string.Empty);
    }
}