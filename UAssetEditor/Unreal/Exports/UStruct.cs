using UAssetEditor.Binary;
using UsmapDotNet;

namespace UAssetEditor.Unreal.Exports;

public class UStruct : UObject
{
    public string SuperType;
    
    public UStruct()
    { }

    public UStruct(string superType)
    {
        SuperType = superType;
    }
    
    public UStruct(UsmapSchema schema) : base()
    {
        Name = schema.Name;
        
        foreach (var prop in schema.Properties)
        {
            Properties.Add(new UProperty(prop.Data, prop.Name, null, prop.ArraySize, prop.SchemaIdx));
        }
    }
    
    public virtual void Read(Reader reader, PropertyData? data, Asset? asset = null,
        ESerializationMode mode = ESerializationMode.Normal)
    { } 

    public virtual void Write(Writer writer, Asset? asset = null)
    { }
}