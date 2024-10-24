﻿using UAssetEditor.Binary;
using UAssetEditor.Unreal;
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
    
    public override string ToString() => $"{Name}: {Value} ({Type})";
}

public abstract class AbstractProperty : ICloneable
{
    public virtual void Read(Reader reader, UsmapPropertyData? data, BaseAsset? asset = null, bool isZero = false)
    { } 

    public virtual void Write(Writer writer, UProperty property, BaseAsset? asset = null)
    { }

    public object Clone()
    {
        return MemberwiseClone();
    }
}

public abstract class AbstractProperty<T> : AbstractProperty
{
    public T? Value;

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