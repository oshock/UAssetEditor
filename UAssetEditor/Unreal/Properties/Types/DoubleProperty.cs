using UAssetEditor.Binary;
using UsmapDotNet;


namespace UAssetEditor.Properties;

public class DoubleProperty : AbstractProperty
{
    public double Value;
    
    public override void Read(Reader reader, UsmapPropertyData data, BaseAsset? asset = null)
    {
        Value = reader.Read<double>();
    }

    public override void Write(Writer writer, UProperty property, BaseAsset? asset = null)
    {
        
    }
}