using UnrealExtractor.Binary;
using UsmapDotNet;


namespace UAssetEditor.Unreal.Properties.Types;

public class MapProperty : AbstractProperty<Dictionary<object, object>>
{
    public override void Read(Reader reader, UsmapPropertyData? data, Asset? asset = null, EReadMode mode = EReadMode.Normal)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(data.InnerType);
        ArgumentNullException.ThrowIfNull(data.ValueType);
        //ArgumentNullException.ThrowIfNull(asset);
        
        var numKeysToRemove = reader.Read<int>();
        for (int i = 0; i < numKeysToRemove; i++)
        {
            PropertyUtils.ReadProperty(data.InnerType.Type.ToString(), reader, null, asset);
        }

        var num = reader.Read<int>();
        Value = new Dictionary<object, object>();

        var keyType = data.InnerType.Type.ToString();
        var valueType = data.ValueType.Type.ToString();

        //var mappings = asset.Mappings;
        
        for (int i = 0; i < num; i++)
        {
            var key = PropertyUtils.ReadProperty(keyType, reader, data.InnerType, asset, EReadMode.Map);
            var value = PropertyUtils.ReadProperty(valueType, reader, data.ValueType, asset, EReadMode.Map);
            Value.Add(key, value);
        }
    }

    // TODO
    public override void Write(Writer writer, UProperty property, Asset? asset = null)
    {
        
    }
}