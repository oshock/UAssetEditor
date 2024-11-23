using System.Data;
using UAssetEditor.Unreal.Exports;
using UAssetEditor.Unreal.Properties.Structs;
using UAssetEditor.Unreal.Properties.Structs.Math;
using UAssetEditor.Unreal.Properties.Types;
using UAssetEditor.Binary;

namespace UAssetEditor.Unreal.Properties.Reflection;

// I know it's not really reflection, but I'm calling it that okay..
public static class PropertyReflector
{
    public static Dictionary<string, Type> PropertiesByName = new()
    {
        { "ArrayProperty", typeof(ArrayProperty) },
        { "BoolProperty", typeof(BoolProperty) },
        { "DoubleProperty", typeof(DoubleProperty) },
        { "EnumProperty", typeof(EnumProperty) },
        { "FloatProperty", typeof(FloatProperty) },
        { "Int8Property", typeof(ByteProperty) },
        { "ByteProperty", typeof(ByteProperty) },
        { "Int16Property", typeof(Int16Property) },
        { "IntProperty", typeof(IntProperty) },
        { "Int64Property", typeof(Int64Property) },
        { "UInt16Property", typeof(UInt16Property) },
        { "UInt64Property", typeof(UInt64Property) },
        { "StructProperty", typeof(StructProperty) },
        { "TextProperty", typeof(TextProperty) },
        { "NameProperty", typeof(NameProperty) },
        { "StrProperty", typeof(StrProperty) },
        { "SoftClassProperty", typeof(SoftObjectProperty) },
        { "SoftObjectProperty", typeof(SoftObjectProperty) },
        { "ScriptInterface", typeof(ObjectProperty) },
        { "ObjectProperty", typeof(ObjectProperty) },
        { "MapProperty", typeof(MapProperty) }
    };
    
    public static Dictionary<string, Type> DefinedStructsByName = new()
    {
        { "GameplayTagContainer", typeof(FGameplayTagContainer) },
        { "InstancedStruct", typeof(FInstancedStruct) },
        { "Vector", typeof(FVector) },
        { "Rotator", typeof(FRotator) }
    };

    public static object ReadProperty(string type, Reader reader, PropertyData? data,
        Asset? asset = null, ESerializationMode mode = ESerializationMode.Normal)
    {
        foreach (var kvp in PropertiesByName)
        {
            if (type != kvp.Key)
                continue;

            if (Activator.CreateInstance(kvp.Value) is not AbstractProperty instance)
                throw new ApplicationException(
                    $"{kvp.Value.Name} is listed as a property but does not inherit 'AbstractProperty'.");
            
            instance.Read(reader, data, asset, mode);
            return instance;
        }

        throw new NotImplementedException($"Property type '{type}' is not implemented!");
    }
    
    public static void WriteProperty(Writer writer, UProperty prop, Asset? asset = null, ESerializationMode mode = ESerializationMode.Normal)
    {
        if (prop.Value is not AbstractProperty property)
            throw new ApplicationException($"{prop.Name} is not a 'AbstractProperty'.");
        
        property.Write(writer, prop, asset, mode);
    }
    
    public static void WriteProperty(Writer writer, AbstractProperty prop, Asset? asset = null, ESerializationMode mode = ESerializationMode.Normal)
    {
        var uProperty = new UProperty
        {
            Value = prop
        };
        
        prop.Write(writer, uProperty, asset, mode);
    }
    
    public static object ReadStruct(Reader reader, PropertyData? data,
        Asset? asset = null, ESerializationMode mode = ESerializationMode.Normal)
    {
        ArgumentNullException.ThrowIfNull(data);

        var type = data.StructType ?? data.InnerType?.StructType ??
            throw new NoNullAllowedException("Struct type cannot be null.");
        
        // TODO determine properly
        var structType = data.InnerType?.StructType ?? data.StructType;

        foreach (var kvp in DefinedStructsByName)
        {
            if (structType != kvp.Key)
                continue;

            if (Activator.CreateInstance(kvp.Value) is not UStruct instance)
                throw new ApplicationException(
                    $"{kvp.Value.Name} is listed as a property but does not inherit 'UStruct'.");

            instance.Read(reader, data, asset, mode);
            return instance;
        }

        ArgumentNullException.ThrowIfNull(asset);
        
        return new CustomStructHolder(asset.ReadProperties(type));
    }
    
    public static void WriteStruct(Writer writer, object property, PropertyData type, Asset? asset = null)
    {
        ArgumentNullException.ThrowIfNull(asset);

        var structType = type.GetStructType();
        
        if (property is List<UProperty> properties)
        {
            var propertyWriter = asset.WriteProperties(structType, properties);
            propertyWriter.CopyTo(writer);
            return;
        }
        
        foreach (var kvp in DefinedStructsByName)
        {
            if (structType != kvp.Key)
                continue;
            
            if (property is not UStruct @struct)
                throw new ApplicationException("Defined struct is not a 'UStruct'.");
            
            @struct.Write(writer, asset);
            return;
        }
    }
}