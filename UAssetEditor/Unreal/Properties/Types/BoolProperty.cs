using UAssetEditor.Binary;
using UsmapDotNet;


namespace UAssetEditor.Properties;

public class BoolProperty : AbstractProperty
{
    public bool Value;
    
    public override void Read(Reader reader, UsmapPropertyData data, BaseAsset? asset = null)
    {
        Value = reader.ReadByte() != 0;
    }

    public override void Write(Writer writer, UProperty property, BaseAsset? asset = null)
    {
        
    }
}