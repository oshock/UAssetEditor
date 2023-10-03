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

    public virtual void Read(Reader reader, UsmapPropertyData? data)
    { }

    public virtual void Write(Writer writer)
    { }

    public static object? ReadProperty(string type, Reader reader, UsmapPropertyData? innerData = null)
    {
        switch (type)
        {
            case "ArrayProperty":
                var count = reader.Read<int>();
                var result = new List<object>();
                for (int i = 0; i < count; i++)
                {
                    result.Add(ReadProperty(innerData!.Type.ToString(), reader)!);
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
            default:
                throw new KeyNotFoundException($"Could not find a property named '{type}'.");
        }
    }
}