using UAssetEditor.Binary;
using UsmapDotNet;


namespace UAssetEditor.Properties;

public class ArrayProperty : AbstractProperty
{
    public List<object> Value;
    
    public override void Read(Reader reader, UsmapPropertyData data, BaseAsset? asset = null)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(data.InnerType);

        var count = reader.Read<int>();
        var result = new List<object>();

        for (int i = 0; i < count; i++)
        {
            var item = ReadProperty(data.InnerType.Type.ToString(), "", reader, data, asset);
            result.Add(item);
        }

        Value = result;
    }

    public override void Write(Writer writer, UProperty property, BaseAsset? asset = null)
    {
        
    }
}