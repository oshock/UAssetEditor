using System.Data;
using UAssetEditor.Binary;
using UAssetEditor.Unreal.Properties.Reflection;
using UsmapDotNet;


namespace UAssetEditor.Unreal.Properties.Types;

public class StructProperty : AbstractProperty<object>
{
    public string Type = "None";
    
    public override string ToString()
    {
        if (Value is List<UProperty> properties)
            return $"[{properties.Count}]";
        
        return $"({Type})";
    }

    public override void Read(Reader reader, UsmapPropertyData? data, BaseAsset? asset = null, bool isZero = false)
    {
        Type = data?.InnerType?.StructType ?? "None";
        Value = PropertyReflector.ReadStruct(reader, data, asset, isZero);
    }

    public override void Write(Writer writer, UProperty property, BaseAsset? asset = null)
    {
        if (Value is null)
            throw new NoNullAllowedException("Cannot write struct property without a non-null value.");
        
        PropertyReflector.WriteStruct(writer, Value, property.StructType ?? "None", asset);
    }
}