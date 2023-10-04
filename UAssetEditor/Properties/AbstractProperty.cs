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

    public static object? ReadProperty(string type, Reader reader, UsmapPropertyData? innerData = null, UAsset? asset = null)
    {
        switch (type)
        {
            case "ArrayProperty":
                var count = reader.Read<int>();
                var result = new List<object>();
                for (int i = 0; i < count; i++)
                {
                    result.Add(ReadProperty(innerData!.InnerType.Type.ToString(), reader, innerData, asset)!);
                }
                return result;
            case "BoolProperty":
                return reader.ReadByte() == 1;
            case "DoubleProperty":
                return reader.Read<double>();
            case "EnumProperty":
                var prop = new EnumProperty();
                prop.Read(reader, innerData);
                return prop.Value;
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
                return asset!.ReadProperties(innerData!.StructType);
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