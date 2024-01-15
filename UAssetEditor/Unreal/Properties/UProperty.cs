using System.Reflection;
using Newtonsoft.Json.Linq;
using UAssetEditor.Binary;
using UAssetEditor.Names;
using UAssetEditor.Properties;
using UAssetEditor.Properties.Structs;
using UAssetEditor.Unreal.Names;
using Usmap.NET;

namespace UAssetEditor;

public class UProperty
{
    public string Type;
    public string? InnerType;
    public string? EnumName;
    public string? StructType;
    public string Name;
    public object? Value;
    public bool IsZero;
    
    public override string ToString() => $"{Value} ({Type})";
}

public abstract class AbstractProperty
{
    public object? Value;

    public override string ToString()
    {
        return Value?.ToString() ?? "None";
    }

    protected AbstractProperty()
    { }

    public AbstractProperty(object value)
    {
        Value = value;
    }

    public static object? CreateAndRead(string propertyName, Reader reader, UsmapPropertyData? data, BaseAsset? asset = null)
    {
        var type = Assembly.GetAssembly(typeof(AbstractProperty))!.GetTypes().FirstOrDefault(x => x.Name == propertyName);
        if (type == default)
            throw new KeyNotFoundException(
                $"Could not find property inheriting from '{nameof(AbstractProperty)}' named '{propertyName}'.");
        
        var instance = (AbstractProperty)Activator.CreateInstance(type)!;
        instance.Read(reader, data, asset);
        return instance.Value;
    }
    
    public virtual void Read(Reader reader, UsmapPropertyData? data, BaseAsset asset = null)
    { }

    public virtual void Write(Writer writer, UProperty property, BaseAsset asset = null)
    { }

    public static void WriteProperty(Writer writer, UProperty prop, BaseAsset? asset = null)
    {
        if (prop.IsZero)
            return;
        
        switch (prop.Type)
        {
            case "ArrayProperty":
                var arr = (List<object>)prop.Value!;
                writer.Write(arr.Count);
                foreach (var elm in arr)
                    WriteProperty(writer, new UProperty { Value = elm, Type = prop.InnerType!, StructType = prop.StructType }, asset);
                break;
            case "BoolProperty":
                writer.Write((byte)prop.Value!);
                break;
            case "DoubleProperty":
                writer.Write((double)prop.Value!);
                 break;
            case "EnumProperty":
                var enumProp = new EnumProperty();
                enumProp.Write(writer, prop, asset);
                break;
            case "FloatProperty":
                writer.Write((float)prop.Value!);
                break;
            case "Int8Property":
                writer.Write((byte)prop.Value!);
                break;
            case "Int16Property":
                writer.Write((short)prop.Value!);
                break;
            case "IntProperty":
                writer.Write((int)prop.Value!);
                break;
            case "Int64Property":
                writer.Write((long)prop.Value!);
                break;
            case "UInt16Property":
                writer.Write((ushort)prop.Value!);
                break;
            case "UInt64Property":
                writer.Write((ulong)prop.Value!);
                break;
            case "StructProperty":
                
                void WriteGameplayTagArray()
                {
                    var tags = (List<FName>)prop.Value!;
                    writer.Write(tags.Count);

                    foreach (var tag in tags)
                        tag.Serialize(writer, asset!.NameMap);
                }

                switch (prop.StructType)
                {
                    case "GameplayTags":
                    case "GameplayTagContainer":
                        WriteGameplayTagArray();
                        break;
                    case "InstancedStruct":
                        var @struct = (FInstancedStruct)prop.Value!;
                        @struct.Serialize(writer);
                        break;
                    case "GameplayTag":
                        var name = (FName)prop.Value!;
                        name.Serialize(writer, asset!.NameMap);
                        break;
                    default:
                        var buffer = asset!.WriteProperties(prop.StructType!, -1, (List<UProperty>)prop.Value!);
                        buffer.CopyTo(writer);
                        break;
                }

                break;
            case "TextProperty":
                var text = (TextProperty)prop.Value!;
                text.Write(writer, prop, asset);
                break;
            case "StrProperty":
                writer.WriteString((string)prop.Value!);
                break;
            case "SoftClassProperty":
            case "SoftObjectProperty":
                var softPath = (SoftObjectProperty)prop.Value!;
                softPath.Write(writer, prop, asset);
                break;
            case "ScriptInterface":
            case "ObjectProperty":
                writer.Write((int)prop.Value!);
                break;
            default:
                throw new KeyNotFoundException($"Could not find a property named '{prop.StructType ?? prop.Type}'.");
        }
    }
    
    public static object? ReadProperty(string type, Reader reader, UsmapProperty? prop, BaseAsset? asset = null, bool isZero = false)
    {
        switch (type)
        {
            case "ArrayProperty":
                if (isZero)
                    return Array.Empty<object>();
                
                var count = reader.Read<int>();
                var result = new List<object>();
                
                for (int i = 0; i < count; i++)
                    result.Add(ReadProperty(prop.Value.Data.InnerType.Type.ToString(), reader, prop, asset)!);
                
                return result;
            case "BoolProperty":
                return !isZero && reader.ReadByte() == 1;
            case "DoubleProperty":
                return isZero ? 0.0 : reader.Read<double>();
            case "EnumProperty":
                var enumProp = new EnumProperty(isZero);
                enumProp.Read(reader, prop.Value.Data);
                return enumProp.Value;
            case "FloatProperty":
                return isZero ? 0 : reader.Read<float>();
            case "Int8Property":
            case "ByteProperty":
                return isZero ? 0 : reader.Read<byte>();
            case "Int16Property":
                return isZero ? 0 : reader.Read<short>();
            case "IntProperty":
                return isZero ? 0 : reader.Read<int>();
            case "Int64Property":
                return isZero ? 0 : reader.Read<long>();
            case "UInt16Property":
                return isZero ? 0U : reader.Read<ushort>();
            case "UInt64Property":
                return isZero ? 0U : reader.Read<ulong>();
            case "StructProperty":
                if (isZero)
                    return null;
                
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
                    "GameplayTagQuery" => new FGameplayTagQuery(),
                    _ => prop.Value.Name switch
                    {
                        "GameplayTags" => ReadGameplayTagArray(),
                        _ => asset!.ReadProperties(prop.Value.Data!.StructType ?? prop.Value.Data.InnerType.StructType)
                    }
                };

            case "TextProperty":
                if (isZero)
                    return default(FTextHistory);
                var text = new TextProperty();
                text.Read(reader, null);
                return text.Value;
            case "NameProperty":
                if (isZero)
                    return default(FName);
                var name = new FName(reader, asset!.NameMap);
                return name;
            case "StrProperty":
                return isZero ? "None" : FString.Read(reader);
            case "SoftClassProperty":
            case "SoftObjectProperty":
                if (isZero)
                    return "None";
                var softProp = new SoftObjectProperty();
                softProp.Read(reader, null, asset);
                return softProp;
            case "ScriptInterface":
            case "ObjectProperty":
                if (isZero)
                    return int.MinValue;
                var objProp = new ObjectProperty();
                objProp.Read(reader, null);
                return objProp.Value;
            case "MapProperty":
                if (isZero)
                    return new Dictionary<object, object>();
                var map = new MapProperty();
                map.Read(reader, prop.Value.Data);
                return map.Value;
            default:
                throw new KeyNotFoundException($"Could not find a property named '{type}'.");
        }
    }
}