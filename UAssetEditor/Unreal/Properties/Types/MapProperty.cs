using UAssetEditor.Binary;
using Usmap.NET;

namespace UAssetEditor.Properties;

public class MapProperty : AbstractProperty
{
    public Dictionary<object, object> Value;
    
    public override void Read(Reader reader, UsmapPropertyData? data, BaseAsset asset = null)
    {
        var numKeysToRemove = reader.Read<int>();
        for (int i = 0; i < numKeysToRemove; i++)
            ReadProperty(data.InnerType.Type.ToString(), reader, null, asset);

        var num = reader.Read<int>();
        Value = new Dictionary<object, object>();

        var keyType = data.InnerType.Type.ToString();
        var valueType = data.ValueType.Type.ToString();

        var mappings = asset.Mappings;
        
        for (int i = 0; i < num; i++)
        {
            var key = ReadProperty(keyType, reader, null, asset);
            var value = ReadProperty(valueType, reader, new UsmapProperty(data.ValueType.StructType, 0, 1, data.ValueType), asset);
            Value.Add(key ?? "None", value ?? "None");
        }
    }

    public override void Write(Writer writer, UProperty property, BaseAsset asset = null)
    {
        
    }
}