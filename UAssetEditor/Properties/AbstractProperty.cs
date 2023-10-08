using System.Reflection;
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

    public static object? CreateAndRead(string propertyName, Reader reader, UsmapPropertyData? data, UAsset? asset = null)
    {
        var type = Assembly.GetAssembly(typeof(AbstractProperty))!.GetTypes().FirstOrDefault(x => x.Name == propertyName);
        if (type == default)
            throw new KeyNotFoundException(
                $"Could not find property inheriting from '{nameof(AbstractProperty)}' named '{propertyName}'.");
        
        var instance = (AbstractProperty)Activator.CreateInstance(type)!;
        instance.Read(reader, data, asset);
        return instance.Value;
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

                List<FName> ReadGameplayTagArray()
                {
                    var tags = new List<FName>();
                    var tagCount = reader.Read<uint>();
                    
                    for (int i = 0; i < tagCount; i++)
                        tags.Add(new FName(reader, asset!.NameMap));
                    
                    return tags;
                }

                return prop!.Value.Data.StructType switch
                {
                    "GameplayTagContainer" => new FGameplayTagContainer
                    {
                        GameplayTags = ReadGameplayTagArray(), ParentTags = ReadGameplayTagArray()
                    },
                    "InstancedStruct" => new FInstancedStruct(reader),
                    "GameplayTag" => new FName(reader, asset!.NameMap),
                    _ => prop.Value.Name switch
                    {
                        "GameplayTags" => ReadGameplayTagArray(),
                        _ => asset!.ReadProperties(prop.Value.Data!.StructType ?? prop.Value.Data.InnerType.StructType)
                    }
                };

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

public struct FGameplayTagContainer
{
    public List<FName> GameplayTags;
    public List<FName> ParentTags;
}