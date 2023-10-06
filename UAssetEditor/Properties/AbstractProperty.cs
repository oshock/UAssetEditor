using Usmap.NET;

namespace UAssetEditor.Properties;

public abstract class AbstractProperty
{
    public object? Value;

    protected AbstractProperty()
    { }

    public AbstractProperty(object value)
    {
        Value = value;
    }

    public virtual void Read(Reader reader, UsmapPropertyData? data, UAsset? asset = null)
    { }

    public virtual void Write(Writer writer)
    { }

    public static object? ReadProperty(string type, Reader reader, UsmapProperty? prop, UAsset? asset = null)
    {
        switch (type)
        {
            case "ArrayProperty":
                var count = reader.Read<int>();
                var result = new List<object>();
                
                for (int i = 0; i < count; i++)
                    result.Add(ReadProperty(prop.Value.Data.InnerType.Type.ToString(), reader, prop, asset)!);
                
                return result;
            case "BoolProperty":
                return reader.ReadByte() == 1;
            case "DoubleProperty":
                return reader.Read<double>();
            case "EnumProperty":
                var enumProp = new EnumProperty();
                enumProp.Read(reader, prop.Value.Data);
                return enumProp.Value;
            case "FloatProperty":
                return reader.Read<float>();
            case "Int8Property":
                return reader.Read<byte>();
            case "Int16Property":
                return reader.Read<short>();
            case "IntProperty":
                return reader.Read<int>();
            case "Int64Property":
                return reader.Read<long>();
            case "UInt16Property":
                return reader.Read<ushort>();
            case "UInt64Property":
                return reader.Read<ulong>();
            case "StructProperty":
                if (prop.Value.Name == "GameplayTags" || prop.Value.Data.StructType == "GameplayTagContainer" || prop.Value.Data!.StructType == "GameplayTag")
                {
                    var tags = new List<FName>();
                    var tagCount = reader.Read<uint>();
                    
                    for (int i = 0; i < tagCount; i++)
                        tags.Add(new FName(reader, asset!.NameMap));
                    
                    return tags;
                }
                return asset!.ReadProperties(prop.Value.Data!.StructType ?? prop.Value.Data.InnerType.StructType);
            case "TextProperty":
                var text = new TextProperty();
                text.Read(reader, null);
                return text.Value;
            case "StrProperty":
                return FString.Read(reader);
            case "SoftClassProperty":
            case "SoftObjectProperty":
                var softProp = new SoftObjectProperty();
                softProp.Read(reader, null, asset);
                return softProp.Value;
            case "ScriptInterface":
            case "ObjectProperty":
                var objProp = new ObjectProperty();
                objProp.Read(reader, null);
                return objProp.Value;
            default:
                throw new KeyNotFoundException($"Could not find a property named '{type}'.");
        }
    }
}