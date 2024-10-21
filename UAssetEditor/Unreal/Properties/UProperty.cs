using System.Reflection;
using Newtonsoft.Json.Linq;
using UAssetEditor.Binary;
using UAssetEditor.Names;
using UAssetEditor.Properties;
using UAssetEditor.Properties.Structs;
using UAssetEditor.Unreal.Names;
using UsmapDotNet;


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

    /*public static AbstractProperty? CreateAndRead(Reader reader, UsmapPropertyData data, BaseAsset? asset = null)
    {
        
    }*/

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
    
    public virtual void Read(Reader reader, UsmapPropertyData data, BaseAsset? asset = null)
    { }

    public virtual void Write(Writer writer, UProperty property, BaseAsset? asset = null)
    { }

    // TODO put in different file
    // TODO remake using reflection
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
    
    // TODO put in different file
    public static object? ReadProperty(string type, string name, Reader reader, UsmapPropertyData? data, BaseAsset? asset = null, bool isZero = false)
    {
        if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(name))
            throw new ArgumentNullException($"'{nameof(type)}' and '{nameof(name)}' cannot be null or empty.");
        
        ArgumentNullException.ThrowIfNull(data);
        
        switch (type) // TODO Integrate reflection
        {
            case "ArrayProperty":
            {
                var arrayProperty = new ArrayProperty();
                arrayProperty.Read(reader, data);

                return arrayProperty;
            }
            case "BoolProperty":
                var boolProperty = new BoolProperty();
                boolProperty.Read(reader, data);
                
                return boolProperty;
            case "DoubleProperty":
                var doubleProperty = new DoubleProperty();
                doubleProperty.Read(reader, data);
                
                return doubleProperty;
            case "EnumProperty":
                var enumProp = new EnumProperty(isZero);
                enumProp.Read(reader, data);
                
                return enumProp;
            case "FloatProperty":
                var floatProperty = new FloatProperty();
                floatProperty.Read(reader, data);
                
                return floatProperty;
            case "Int8Property":
            case "ByteProperty":
                var byteProperty = new ByteProperty();
                byteProperty.Read(reader, data);
                
                return byteProperty;
            case "Int16Property":
                var shortProperty = new Int16Property();
                shortProperty.Read(reader, data);
                
                return shortProperty;
            case "IntProperty":
                var intProperty = new IntProperty();
                intProperty.Read(reader, data);
                
                return intProperty;
            case "Int64Property":
                var int64Property = new Int64Property();
                int64Property.Read(reader, data);
                
                return int64Property;
            case "UInt16Property":
                var uInt16Property = new UInt16Property();
                uInt16Property.Read(reader, data);
                
                return uInt16Property;
            case "UInt64Property":
                var uInt64Property = new UInt64Property();
                uInt64Property.Read(reader, data);
                
                return uInt64Property;
            case "StructProperty":
                if (isZero)
                    return null;
                
                // TODO unskunkify
                GameplayTagArrayProperty ReadGameplayTagArray()
                {
                    var gameplayTags = new GameplayTagArrayProperty();
                    gameplayTags.Read(reader, data);
                    
                    return gameplayTags;
                }

                // TODO unskunkify
                return data.StructType switch
                {
                    "GameplayTagContainer" => new FGameplayTagContainer
                    {
                        GameplayTags = ReadGameplayTagArray().Value, ParentTags = ReadGameplayTagArray().Value
                    },
                    "InstancedStruct" => new FInstancedStruct(reader),
                    _ => name switch
                    {
                        "GameplayTags" => ReadGameplayTagArray(),
                        _ => asset!.ReadProperties(data.StructType ?? data.InnerType.StructType)
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
                var value = new FName(reader, asset!.NameMap);
                return value;
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
                map.Read(reader, data);
                return map.Value;
            default:
                throw new KeyNotFoundException($"Could not find a property named '{type}'.");
        }
    }
}