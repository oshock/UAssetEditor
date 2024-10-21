using UAssetEditor.Binary;
using UsmapDotNet;


namespace UAssetEditor.Properties;

public class ByteProperty : AbstractProperty
{
    public byte Value;
    
    public override void Read(Reader reader, UsmapPropertyData data, BaseAsset? asset = null)
    {
        Value = reader.ReadByte();
    }

    public override void Write(Writer writer, UProperty property, BaseAsset? asset = null)
    {
        
    }
}