using UAssetEditor.Binary;
using UsmapDotNet;


namespace UAssetEditor.Unreal.Properties.Types;

public class MapProperty : AbstractProperty<Dictionary<object, object>>
{
    // TODO
    public override void Read(Reader reader, UsmapPropertyData? data, BaseAsset? asset = null, bool isZero = false)
    {
        /*var numKeysToRemove = reader.Read<int>();
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
        }*/
    }

    // TODO
    public override void Write(Writer writer, UProperty property, BaseAsset? asset = null)
    {
        
    }
}