using UAssetEditor.Binary;
using UAssetEditor.Classes;
using UAssetEditor.Unreal.Assets;
using UAssetEditor.Unreal.Misc;
using UAssetEditor.Unreal.Names;
using UsmapDotNet;


namespace UAssetEditor.Unreal.Properties;

[Flags]
public enum EPropertyTagExtension : byte
{
    NoExtension					= 0x00,
    ReserveForFutureUse			= 0x01, // Can be use to add a next group of extension

    ////////////////////////////////////////////////
    // First extension group
    OverridableInformation		= 0x02,

    //
    // Add more extension for the first group here
    //
}

public class PropertyData
{
    public string? Type;
    public string? EnumName;
    public string? StructType;
    public PropertyData? InnerType;
    public PropertyData? ValueType;
    
    // Tagged
    public FGuid StructGuid;
    public bool Bool;
    public string? InnerTypeName;
    public string? ValueTypeName;

    public void SetClassData(string type)
    {
        Type = type;
        EnumName = null;
        StructType = null;
        InnerType = null;
        ValueType = null;
    }
    
    public void SetEnumData(string name)
    {
        Type = "EnumProperty";
        EnumName = name;
        StructType = null;
        InnerType = null;
        ValueType = null;
    }
    
    public void SetStructData(string structType)
    {
        Type = "StructProperty";
        EnumName = null;
        StructType = structType;
        InnerType = null;
        ValueType = null;
    }
    
    public PropertyData()
    { }

    public PropertyData(string type)
    {
        Type = type;
    }
    
    public PropertyData(UsmapPropertyData data)
    {
        Type = data.Type.ToString();
        EnumName = data.EnumName;
        StructType = data.StructType;
        
        if (data.InnerType != null)
            InnerType = new PropertyData(data.InnerType);
        
        if (data.ValueType != null)
            ValueType = new PropertyData(data.ValueType);
    }

    // https://github.com/FabianFG/CUE4Parse/blob/ae9c85c8b6bae523b3194be29c72ea889f801a50/CUE4Parse/UE4/Assets/Objects/FPropertyTagData.cs#L29
    public PropertyData(Asset asset, string type)
    {
        Type = type;
        switch (type)
        {
            case "StructProperty":
                StructType = new FName(asset, asset.NameMap).Name;
                if (asset.FileVersion >= EUnrealEngineObjectUE4Version.STRUCT_GUID_IN_PROPERTY_TAG)
                    StructGuid = asset.Read<FGuid>();
                break;
            case "BoolProperty":
                Bool = asset.ReadBool();
                break;
            case "ByteProperty":
            case "EnumProperty":
                EnumName = new FName(asset, asset.NameMap).Name;
                break;
            case "ArrayProperty":
                if (asset.FileVersion >= EUnrealEngineObjectUE4Version.ARRAY_PROPERTY_INNER_TAGS)
                    InnerTypeName = new FName(asset, asset.NameMap).Name;
                break;
            // Serialize the following if version is past PROPERTY_TAG_SET_MAP_SUPPORT
            case "SetProperty":
                if (asset.FileVersion >= EUnrealEngineObjectUE4Version.PROPERTY_TAG_SET_MAP_SUPPORT)
                    InnerTypeName = new FName(asset, asset.NameMap).Name;
                break;
            case "MapProperty":
                if (asset.FileVersion >= EUnrealEngineObjectUE4Version.PROPERTY_TAG_SET_MAP_SUPPORT)
                {
                    InnerTypeName = new FName(asset, asset.NameMap).Name;
                    ValueTypeName = new FName(asset, asset.NameMap).Name;
                }
                break;
            case "OptionalProperty":
                InnerTypeName = new FName(asset, asset.NameMap).Name;
                break;
        }
    }
    
    public static bool operator==(PropertyData left, PropertyData right)
    {
        return left.Type == right.Type
               && left.EnumName == right.EnumName
               && left.StructType == right.StructType
               && ((left.InnerType is null && right.InnerType is null)
                   || left.InnerType! == right.InnerType!)
               && ((left.ValueType is null && right.ValueType is null)
                   || left.ValueType! == right.ValueType!);
    }
    
    public static bool operator!=(PropertyData left, PropertyData right)
    {
        return left.Type != right.Type
               || left.EnumName != right.EnumName
               || left.StructType != right.StructType
               || left.InnerType is not null && right.InnerType is null
               || left.InnerType is null && right.InnerType is not null
               || left.InnerType! != right.InnerType!
               || left.ValueType is not null && right.ValueType is null
               || left.ValueType is null && right.ValueType is not null
               || left.ValueType! != right.ValueType!;
    }
}

public class UProperty
{
    public PropertyData? Data;
    public string Name;
    public object? Value;
    public byte ArraySize;
    public int SchemaIdx;
    public bool IsZero;

    public UProperty()
    {
        Name = "None";
    }
    
    public UProperty(UsmapPropertyData data, string name, object? value, byte arraySize = 1, int schemaIdx = -1, bool isZero = false)
    {
        Data = new PropertyData(data);
        Name = name;
        Value = value;
        ArraySize = arraySize;
        SchemaIdx = schemaIdx;
        IsZero = isZero;
    } 
    
    public UProperty(PropertyData data, string name, object? value, byte arraySize = 1, int schemaIdx = -1, bool isZero = false)
    {
        Data = data;
        Name = name;
        Value = value;
        ArraySize = arraySize;
        SchemaIdx = schemaIdx;
        IsZero = isZero;
    }

    public T? GetValue<T>() where T : class
    {
        return Value as T;
    }

    public override string ToString() => $"{Name}: {Value} ({Data.Type})";
}

public enum ESerializationMode
{
    Zero,
    Normal,
    Map
}

public abstract class AbstractProperty : ICloneable
{
    public string? Name { get; set; }
    public abstract object? ValueAsObject { get; }

    public string GetPropertyType() => GetType().Name;
    
    public virtual void Read(Reader reader, PropertyData? data, Asset? asset = null,
        ESerializationMode mode = ESerializationMode.Normal)
    { } 

    public virtual void Write(Writer writer, UProperty property, Asset? asset = null, ESerializationMode mode = ESerializationMode.Normal)
    { }

    public object Clone()
    {
        return MemberwiseClone();
    }
}

public abstract class AbstractProperty<T> : AbstractProperty
{
    public T? Value;
    
    public override object? ValueAsObject => Value;

    public override string ToString()
    {
        return Value?.ToString() ?? "None";
    }

    public AbstractProperty()
    { }
    
    public AbstractProperty(T value)
    {
        Value = value;
    }
}