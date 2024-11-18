using System.Data;
using UAssetEditor.Unreal.Names;
using UnrealExtractor.Binary;
using UsmapDotNet;


namespace UAssetEditor.Unreal.Properties.Types;

public class EnumProperty : AbstractProperty<string>
{
    public override void Read(Reader reader, UsmapPropertyData? data, Asset? asset = null, EReadMode mode = EReadMode.Normal)
    {
        
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(data.InnerType);
        ArgumentNullException.ThrowIfNull(asset);
        ArgumentNullException.ThrowIfNull(asset.Mappings);
        
        object enumObj = 0;

        switch (mode)
        {
            case EReadMode.Zero:
                break;
            case EReadMode.Normal:
                var value = PropertyUtils.ReadProperty(data.InnerType.Type.ToString(), reader, data.InnerType, asset);
                if (value is not AbstractProperty prop)
                    throw new InvalidCastException("Property is not a AbstractProperty?");

                enumObj = prop.ValueAsObject ?? 0;
                break;
            case EReadMode.Map:
                Value = new FName(reader, asset.NameMap).Name;
                return;
        }
        
        var index = Convert.ToInt32(enumObj);
        var enumData = asset.Mappings.Enums.FirstOrDefault(x => x.Name == data.EnumName);
        
        if (enumData is null)
            throw new NullReferenceException($"Enum {data.EnumName} not found in mappings.");
        
        Value = enumData.Names.Length >= index
            ? enumData.Names[index]
            : throw new KeyNotFoundException($"Could not find a enum name ('{data.EnumName}') at index {index}.");
    }
    
    public override void Write(Writer writer, UProperty property, Asset? asset = null)
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