using UnrealExtractor.Binary;
using UsmapDotNet;


namespace UAssetEditor.Unreal.Properties.Types;

public class ArrayProperty : AbstractProperty<List<object>>
{
    public override string ToString()
    {
        return $"[{Value?.Count}]";
    }

    public override void Read(Reader reader, UsmapPropertyData? data, Asset? asset = null, ESerializationMode mode = ESerializationMode.Normal)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(data.InnerType);

        Value = [];
        
        if (mode == ESerializationMode.Zero)
            return;
        
        var count = reader.Read<int>();

        for (int i = 0; i < count; i++)
        {
            var item = PropertyUtils.ReadProperty(data.InnerType.Type.ToString(), reader, data.InnerType, asset);
            Value.Add(item);
        }
    }

    public override void Write(Writer writer, UProperty property, Asset? asset = null, ESerializationMode mode = ESerializationMode.Normal)
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