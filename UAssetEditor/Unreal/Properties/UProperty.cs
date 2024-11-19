using UnrealExtractor.Binary;
using UsmapDotNet;


namespace UAssetEditor;

public class UProperty
{
    public UsmapPropertyData Data;
    
    public string Name;
    public object? Value;
    public bool IsZero;

    public UProperty() 
    { }
    
    public UProperty(UsmapPropertyData data, string name, object? value, bool isZero = false)
    {
        Data = data;
        Name = name;
        Value = value;
        IsZero = isZero;
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
    
    public virtual void Read(Reader reader, UsmapPropertyData? data, Asset? asset = null, ESerializationMode mode = ESerializationMode.Normal)
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