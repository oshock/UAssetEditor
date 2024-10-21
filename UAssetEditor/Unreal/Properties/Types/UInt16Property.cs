using UAssetEditor.Binary;
using UsmapDotNet;


namespace UAssetEditor.Properties;

public class UInt16Property : AbstractProperty
{
    public ushort Value;
    
    public override void Read(Reader reader, UsmapPropertyData data, BaseAsset? asset = null)
    {
        Value = reader.Read<ushort>();
    }

    public override void Write(Writer writer, UProperty property, BaseAsset? asset = null)
    {
        
    }
}