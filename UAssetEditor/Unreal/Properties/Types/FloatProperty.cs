using UAssetEditor.Binary;
using UsmapDotNet;


namespace UAssetEditor.Properties;

public class FloatProperty : AbstractProperty
{
    public float Value;
    
    public override void Read(Reader reader, UsmapPropertyData data, BaseAsset? asset = null)
    {
        Value = reader.Read<float>();
    }

    public override void Write(Writer writer, UProperty property, BaseAsset? asset = null)
    {
        
    }
}