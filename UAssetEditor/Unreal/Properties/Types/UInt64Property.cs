using UAssetEditor.Binary;
using UsmapDotNet;


namespace UAssetEditor.Properties;

public class UInt64Property : AbstractProperty
{
    public ulong Value;
    
    public override void Read(Reader reader, UsmapPropertyData data, BaseAsset? asset = null)
    {
        Value = reader.Read<ulong>();
    }

    public override void Write(Writer writer, UProperty property, BaseAsset? asset = null)
    {
        
    }
}