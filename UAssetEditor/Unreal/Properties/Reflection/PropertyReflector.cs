﻿using System.Data;
using UAssetEditor.Binary;
using UAssetEditor.Unreal.Properties.Structs;
using UAssetEditor.Unreal.Properties.Types;
using UsmapDotNet;

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
        { "ObjectProperty", typeof(ObjectProperty) }
    };
    
    public static Dictionary<string, Type> DefinedStructsByName = new()
    {
        { "GameplayTagContainer", typeof(FGameplayTagContainer) },
        { "InstancedStruct", typeof(FInstancedStruct) },
        { "DoubleProperty", typeof(DoubleProperty) }
    };

    public static object ReadProperty(string type, Reader reader, UsmapPropertyData? data,
        BaseAsset? asset = null, bool isZero = false)
    {
        foreach (var kvp in PropertiesByName)
        {
            if (type != kvp.Key)
                continue;

            if (Activator.CreateInstance(kvp.Value) is not AbstractProperty instance)
                throw new ApplicationException(
                    $"{kvp.Value.Name} is listed as a property but does not inherit 'AbstractProperty'.");
            
            instance.Read(reader, data, asset, isZero);
            return instance;
        }

        throw new NotImplementedException($"Property type '{type}' is not implemented!");
    }
    
    public static void WriteProperty(Writer writer, UProperty prop, BaseAsset? asset = null)
    {
        if (prop.Value is not AbstractProperty property)
            throw new ApplicationException($"{prop.Name} is not a 'AbstractProperty'.");
        
        property.Write(writer, prop, asset);
    }
    
    public static object ReadStruct(Reader reader, UsmapPropertyData? data,
        BaseAsset? asset = null, bool isZero = false)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (data.StructType is not null)
        {
            foreach (var kvp in DefinedStructsByName)
            {
                if (data.StructType != kvp.Key)
                    continue;

                if (Activator.CreateInstance(kvp.Value) is not UStruct instance)
                    throw new ApplicationException(
                        $"{kvp.Value.Name} is listed as a property but does not inherit 'UStruct'.");

                instance.Read(reader, data, asset, isZero);
                return instance;
            }
        }

        ArgumentNullException.ThrowIfNull(asset);

        var type = data.StructType ?? data.InnerType?.StructType ??
                   throw new NoNullAllowedException("Struct type cannot be null.");
        return asset.ReadProperties(type);
    }
    
    public static void WriteStruct(Writer writer, object property, string type, BaseAsset? asset = null)
    {
        ArgumentNullException.ThrowIfNull(asset);
        
        if (property is List<UProperty> properties)
        {
            var propertyWriter = asset.WriteProperties(type, -1, properties);
            propertyWriter.CopyTo(writer);
            return;
        }
        
        foreach (var kvp in DefinedStructsByName)
        {
            if (type != kvp.Key)
                continue;
            
            if (property is not UStruct @struct)
                throw new ApplicationException("Defined struct is not a 'UStruct'.");
            
            @struct.Write(writer, asset);
            return;
        }
    }
}