using UAssetEditor.Binary;
using UsmapDotNet;


namespace UAssetEditor.Properties;

public class Int16Property : AbstractProperty
{
    public short Value;
    
    public override void Read(Reader reader, UsmapPropertyData data, BaseAsset? asset = null)
    {
        Value = reader.Read<short>();
    }

    public override void Write(Writer writer, UProperty property, BaseAsset? asset = null)
    {
        
    }
}