using UAssetEditor.Binary;
using UsmapDotNet;


namespace UAssetEditor.Properties;

public class Int64Property : AbstractProperty
{
    public long Value;
    
    public override void Read(Reader reader, UsmapPropertyData data, BaseAsset? asset = null)
    {
        Value = reader.Read<long>();
    }

    public override void Write(Writer writer, UProperty property, BaseAsset? asset = null)
    {
        
    }
}