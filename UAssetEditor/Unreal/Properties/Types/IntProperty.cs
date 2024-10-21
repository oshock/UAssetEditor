using UAssetEditor.Binary;
using UsmapDotNet;


namespace UAssetEditor.Properties;

public class IntProperty : AbstractProperty
{
    public int Value;
    
    public override void Read(Reader reader, UsmapPropertyData data, BaseAsset? asset = null)
    {
        Value = reader.Read<int>();
    }

    public override void Write(Writer writer, UProperty property, BaseAsset? asset = null)
    {
        
    }
}