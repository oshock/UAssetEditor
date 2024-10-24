using UAssetEditor.Binary;
using UAssetEditor.Unreal.Properties.Structs;
using UsmapDotNet;


namespace UAssetEditor.Unreal.Properties.Types;

public class ArrayProperty : AbstractProperty<List<object>>
{
    public override string ToString()
    {
        return $"[{Value?.Count}]";
    }

    public override void Read(Reader reader, UsmapPropertyData? data, BaseAsset? asset = null, bool isZero = false)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(data.InnerType);

        Value = [];
        
        if (isZero)
            return;
        
        var count = reader.Read<int>();

        for (int i = 0; i < count; i++)
        {
            var item = PropertyUtils.ReadProperty(data.InnerType.Type.ToString(), reader, data, asset);
            Value.Add(item);
        }
    }

    public override void Write(Writer writer, UProperty property, BaseAsset? asset = null)
    {
        if (Value is null)
        {
            writer.Write(0);
            return;
        }
        
        writer.Write(Value.Count);
        
        foreach (var elm in Value)
        {
            switch (elm)
            {
                case AbstractProperty prop:
                    prop.Write(writer, property, asset);
                    break;
                default:
                    throw new ApplicationException("Array element is not a AbstractProperty!");
            }
        }
    }
}