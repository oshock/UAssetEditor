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

    public UProperty? this[string name] => Properties.FirstOrDefault(x => x.Name == name);

    public virtual void Deserialize(long position)
    {
        if (Owner is null)
            throw new NoNullAllowedException("Cannot deserialize UObject without a valid asset to work with.");

        if (Class is null)
            throw new NoNullAllowedException($"Class cannot be null.");

        Owner.Position = position;
        
        if (Owner.HasUnversionedProperties)
        {
            Properties = UnversionedReader.ReadProperties(Owner, Class);
        }
        else
        {
            // TODO tagged properties
        }
    }
}