using System.Data;
using UAssetEditor.Unreal.Properties.Unversioned;

namespace UAssetEditor.Unreal.Exports;

public class UObject
{
    public string Name;
    public UObject? Outer;
    public UStruct? Class;
    public ResolvedObject? Super;
    public ResolvedObject? Template;

    public List<UProperty> Properties;

    public Asset? Owner;

    public UObject()
    {
        Properties = new();
    }
    
    public UObject(Asset asset) : this()
    {
        Owner = asset;
    }

    public UProperty? this[string name]
    {
        get => Properties.FirstOrDefault(x => x.Name == name);
        set
        {
            var property = Properties.FirstOrDefault(x => x.Name == name);
            
            if (property != null)
            {
                if (value == null)
                {
                    Properties.Remove(property);
                    return;
                }
                
                property.Data = value.Data;
                property.Value = value.Value;
                property.ArraySize = value.ArraySize;
                property.IsZero = value.IsZero;
                property.SchemaIdx = value.SchemaIdx;
                return;
            }

            if (value == null)
                return;
            
            Properties.Add(value);
        }
    }

    public virtual void Deserialize(long position)
    {
        if (Owner is null)
            throw new NoNullAllowedException("Cannot deserialize UObject without a valid asset to work with.");

        if (Class is null)
            throw new NoNullAllowedException($"Class cannot be null.");

        Owner.Position = position;
        
        if (Owner.HasUnversionedProperties)
        {
            Properties = UnversionedPropertyHandler.DeserializeProperties(Owner, Class);
        }
        else
        {
            // TODO tagged properties
        }
    }
}