using UAssetEditor.Properties;
using UsmapDotNet;


namespace UAssetEditor;

public struct NameValuePair
{
    public string Name;
    public object Value;

    public NameValuePair(string name, object value)
    {
        Name = name;
        Value = value;
    }
}

public static class PropertyUtils
{
    // TODO NOT DONE
    public static UProperty? CreateStruct(Usmap mappings, string name, params NameValuePair[] properties)
    {
        var schema = mappings.Schemas.FirstOrDefault(x => x.Name == name);
        if (schema.PropCount == 0)
            return null;

        if (schema.SuperType != "StructProperty")
            throw new InvalidOperationException("Use TODO to create a non-struct property");
        
        var prop = new UProperty
        {
            Name = name,
            Type = "StructProperty",
            Value = new List<UProperty>()
        };

        foreach (var p in properties)
        {
            var _p = schema.Properties.FirstOrDefault(x => x.Name == p.Name);
            if (string.IsNullOrEmpty(_p.Name))
                continue;

            prop.Value.AppendObject(HandleProperty(_p.Name, _p.Data.StructType, p.Value, _p.Data) ?? new UProperty());
        }

        return prop;
    }

    public static void AppendObject(this object obj, object a) => (obj as List<object>)!.Add(a);

    private static UProperty? HandleProperty(string name, string type, object value, UsmapPropertyData? data, Usmap? mappings = null)
    {
        switch (type)
        {
            case "ArrayProperty":
                if (value is not object[] elms)
                    throw new NullReferenceException($"Invalid array for {name}!");

                var inner = data?.InnerType.StructType!;
                var newProp = new UProperty
                {
                    Type = "ArrayProperty",
                    InnerType = inner,
                    Value = new List<UProperty>(),
                    Name = name
                };
                foreach (var elm in elms)
                    newProp.Value.AppendObject(HandleProperty("child", inner, elm, data!.InnerType));

                return newProp;
            case "BoolProperty":
            case "DoubleProperty":
            case "FloatProperty":
            case "Int8Property":
            case "Int16Property":
            case "IntProperty":
            case "Int64Property":
            case "UInt16Property":
            case "UInt64Property":
            case "StrProperty":
            case "ScriptInterface":
            case "ObjectProperty":
                return new UProperty
                {
                    Type = type,
                    Value = value,
                    Name = name
                };
            case "EnumProperty":
                throw new NotImplementedException("Enum creation is not implemented yet.. :(");
            case "StructProperty":
                return CreateStruct(mappings!, name, null); // TODO
            case "TextProperty":
                switch (value)
                {
                    case string:
                        return new UProperty
                        {
                            Type = "TextProperty",
                            Name = name,
                            Value = new TextProperty
                            {
                                Type = ETextHistoryType.Base,
                                Value = value
                            }
                        };
                    case TextProperty:
                        return new UProperty
                        {
                            Type = type,
                            Value = value,
                            Name = name
                        };
                    default:
                        throw new InvalidOperationException(
                            "Invalid type for 'TextProperty'. Should be type 'TextProperty' or 'string'.");
                }
            case "SoftClassProperty":
            case "SoftObjectProperty":
                return new UProperty
                {
                    Type = type,
                    Name = name,
                    Value = SoftObjectProperty.Create((string)value)
                };
        }

        return null;
    }
}