using UAssetEditor.Binary;
using UAssetEditor.Names;
using UsmapDotNet;


namespace UAssetEditor.Unreal.Properties.Types;

public class StrProperty : AbstractProperty<string>
{
    public override void Read(Reader reader, UsmapPropertyData? data, BaseAsset? asset = null, EReadMode mode = EReadMode.Normal)
    {
        Value = FString.Read(reader);
    }

    public override void Write(Writer writer, UProperty property, BaseAsset? asset = null)
    {
        FString.Write(writer, Value ?? string.Empty);
    }
}