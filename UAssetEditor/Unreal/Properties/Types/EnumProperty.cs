using System.Data;
using UAssetEditor.Binary;
using UsmapDotNet;


namespace UAssetEditor.Unreal.Properties.Types;

public class EnumProperty : AbstractProperty<string>
{
    public override void Read(Reader reader, UsmapPropertyData? data, BaseAsset? asset = null, bool isZero = false)
    {
        if (data is null)
            throw new NoNullAllowedException($"'{nameof(data)}' cannot be null");
        
        if (reader.Mappings is null)
            throw new NoNullAllowedException($"'{nameof(reader.Mappings)}' cannot be null");

        if (data.InnerType is null)
            throw new NoNullAllowedException($"'{nameof(data.InnerType)}' cannot be null");

        object enumObj = 0;

        if (!isZero)
        {
            var value = PropertyUtils.ReadProperty(data.InnerType.Type.ToString(), reader, data.InnerType, asset);
            if (value is not AbstractProperty prop)
                throw new InvalidCastException("Property is not a AbstractProperty?");

            enumObj = prop.ValueAsObject ?? 0;
        }
        
        var index = Convert.ToInt32(enumObj);
        var enumData = reader.Mappings.Enums.FirstOrDefault(x => x.Name == data.EnumName);
        
        if (enumData is null)
            throw new NullReferenceException($"Enum {data.EnumName} not found in mappings.");
        
        Value = enumData.Names.Length >= index
            ? enumData.Names[index]
            : throw new KeyNotFoundException($"Could not find enum name ('{data.EnumName}') at index {index}.");
    }
    
    public override void Write(Writer writer, UProperty property, BaseAsset? asset = null)
    {
        if (asset is null)
            throw new NoNullAllowedException($"'{nameof(asset)}' cannot be null");
        
        if (asset.Mappings is null)
            throw new NoNullAllowedException($"'{nameof(asset.Mappings)}' cannot be null");
        
        var enumData = asset.Mappings.Enums.FirstOrDefault(x => x.Name == property.EnumName);
        
        if (enumData is null)
            throw new NullReferenceException($"Enum {property.EnumName} not found in mappings.");

        if (property.Value is not EnumProperty enumProperty)
            throw new InvalidCastException($"Property must be a enum property to serialize as a enum.");
        
        ArgumentNullException.ThrowIfNull(enumProperty.Value);
        
        var index = enumData.Names.ToList().IndexOf(enumProperty.Value);
        writer.WriteByte((byte)index);
    }
}