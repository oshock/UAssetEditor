using UAssetEditor.Unreal.Properties.Reflection;
using UnrealExtractor.Binary;
using UsmapDotNet;


namespace UAssetEditor.Unreal.Properties;

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
    public static object ReadProperty(string type, Reader reader, UsmapPropertyData? data, Asset? asset = null, EReadMode mode = EReadMode.Normal)
    {
        if (string.IsNullOrEmpty(type))
            throw new ArgumentNullException($"'{nameof(type)}' cannot be null or empty.");
        
        return PropertyReflector.ReadProperty(type, reader, data, asset, mode);
    }
    
    public static void WriteProperty(Writer writer, UProperty prop, Asset? asset = null)
    {
        PropertyReflector.WriteProperty(writer, prop, asset);
    }
}