using System.Data;
using UAssetEditor.Unreal.Properties.Reflection;
using UnrealExtractor.Binary;
using UnrealExtractor.Utils;
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

    public override void Read(Reader reader, UsmapPropertyData? data, Asset? asset = null, ESerializationMode mode = ESerializationMode.Normal)
    {
        Type = data?.StructType ?? "None";
        Value = PropertyReflector.ReadStruct(reader, data, asset, mode);
    }

    public override void Write(Writer writer, UProperty property, Asset? asset = null, ESerializationMode mode = ESerializationMode.Normal)
    {
        if (Value is null)
            throw new NoNullAllowedException("Cannot write struct property without a non-null value.");
        
        PropertyReflector.WriteStruct(writer, Value, property.Data, asset);
    }
}