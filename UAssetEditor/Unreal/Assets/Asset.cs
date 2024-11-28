using System.Data;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UAssetEditor.Summaries;
using UAssetEditor.Unreal.Exports;
using UAssetEditor.Unreal.Objects;
using UAssetEditor.Unreal.Properties.Structs;
using UAssetEditor.Unreal.Properties.Unversioned;
using UAssetEditor.Binary;
using UAssetEditor.Classes.Containers;
using UAssetEditor.Unreal.Properties.Types;
using UsmapDotNet;

namespace UAssetEditor;

public class ObjectContainer : Container<UObject>
{
    public List<UObject> Objects => Items;

    public override int GetIndex(string str)
    {
        return Items.FindIndex(x => x.Name == str);
    }
    
    public UObject? this[string name]
    {
        get
        {
            foreach (var property in this)
            {
                if (property.Name != name)
                    continue;

                return property;
            }

            return null;
        }
        set
        {
            for (int i = 0; i < Length; i++)
            {
                var property = this[i];
                if (property.Name != name)
                    continue;

                if (value is null)
                    throw new NoNullAllowedException("Cannot set property to null");
                
                this[i] = value;
            }
        }
    }

    public ObjectContainer(List<UObject> objects) : base(objects)
    { }
    
    public ObjectContainer() : base(new List<UObject>())
    { }
}

public abstract class Asset : Reader
{
    public string Name { get; set; }
    public Usmap? Mappings;
    public NameMapContainer NameMap;
    public EPackageFlags Flags;

    public bool HasUnversionedProperties => Flags.HasFlag(EPackageFlags.PKG_UnversionedProperties);

    public StructureContainer DefinedStructures = new();

    public ObjectContainer Exports = new();
    
    public Asset(byte[] data) : base(data)
    { }

    public UObject? this[string name] => Exports[name];
    
    public Asset(string path) : this(File.ReadAllBytes(path))
    { }

    public void CheckMappings()
    {
        if (Mappings is null)
            throw new NoNullAllowedException("Mappings cannot be null");
    }

    /// <summary>
    /// Read the entirety of this asset
    /// </summary>
    public abstract void ReadAll();
    public abstract uint ReadHeader();
    
    // TODO eventually redo when I add pak assets (not unversioned)
    public List<UProperty> ReadProperties(string type)
    {
        if (!DefinedStructures.Contains(type))
        {
            var schema = Mappings?.FindSchema(type);
            if (schema is null)
                throw new KeyNotFoundException($"Cannot find schema with name '{type}'");
		    
            DefinedStructures.Add(new UStruct(schema, Mappings!));
        }

        return ReadProperties(DefinedStructures[type] ?? throw new KeyNotFoundException("How'd we get here?"));
    }
    
    // TODO eventually redo when I add pak assets (not unversioned)
    public abstract List<UProperty> ReadProperties(UStruct structure);
    public abstract void WriteProperties(Writer writer, string type, List<UProperty> properties);

    /// <summary>
    /// Serializes the entire asset to a stream.
    /// </summary>
    /// <param name="writer"></param>
    public abstract void WriteAll(Writer writer);

    public abstract void Fix();
    public abstract void WriteHeader(Writer writer);

    public static int ReferenceOrAddString(Asset asset, string str)
    {
        if (!asset.NameMap.Contains(str))
            asset.NameMap.Add(str);

        return asset.NameMap.GetIndex(str);
    }

    public abstract ResolvedObject? ResolvePackageIndex(FPackageIndex? index);

    public object ToObject()
    {
        var obj = new List<object>();

        foreach (var export in Exports)
        {
            obj.Add(new
            {
                Type = export.Class?.Name,
                Name = export.Name,
                Properties = export.Properties.Select(PropertyToObject).ToList(),
            });
        }
        
        object PropertyToObject(AbstractProperty property)
        {
            if (property.Data is null)
                throw new NoNullAllowedException("Property data cannot be null.");

            dynamic? value = null;
            
            switch (property.Data.Type)
            {
                case "ArrayProperty":
                {
                    var arrayValue = (ArrayProperty)property.Value!;

                    if (arrayValue.Value is null)
                        break;

                    value = new List<object>();
                    
                    foreach (var item in arrayValue.Value)
                    {
                        var prop = (UProperty)item;
                        value.Add(PropertyToObject(prop));
                    }
                    
                    break;
                }
                case "MapProperty":
                {
                    var mapValue = (ArrayProperty)property.Value!;

                    if (mapValue.Value is null)
                        break;

                    value = new Dictionary<object, object>();
                    
                    foreach (var item in mapValue.Value)
                    {
                        var itemValue = (KeyValuePair<object, object>)item;
                        var propKey = (UProperty)itemValue.Key;
                        var propValue = (UProperty)itemValue.Value;
                        value.Add(PropertyToObject(propKey), PropertyToObject(propValue));
                    }
                    
                    break;
                }
                case "StructProperty":
                {
                    var structValue = (StructProperty)property.Value!;
                    
                    if (structValue.Value is null)
                        break;

                    value = new List<object>();

                    if (structValue.Value is CustomStructHolder customStruct)
                    {
                        foreach (var prop in customStruct.Properties)
                            value.Add(PropertyToObject(prop));

                        break;
                    }

                    var fields = structValue.Value.GetType().GetFields();
                    foreach (var field in fields)
                    {
                        value.Add(new KeyValuePair<string, object>(field.Name, field.GetValue(structValue.Value)));
                    }
                    
                    break;
                }
                default:
                {
                    var abstractValue = (AbstractProperty)property.Value!;
                    var type = property.GetType();
                    
                    var propertyValue = new
                    {
                        Type = type.Name,
                        Value = new List<object>(),
                    };
                    
                    var fields = type.GetFields();
                    foreach (var field in fields)
                    {
                        propertyValue.Value.Add(new KeyValuePair<string, object>(field.Name, field.GetValue(abstractValue)));
                    }
                    
                    break;
                }
            }

            return new
            {
                Type = property.Data?.Type,
                Name = property.Name,
                Value = value
            };
        }

        return obj;
    }
    
    // TODO
    /*public static void HandleProperties(BaseAsset asset, List<UProperty> properties)
    {
        foreach (var p in properties)
        {
            switch (p.Type)
            {
                case "ArrayProperty":
                {
                    if (p.Value is not List<object> values)
                        continue;

                    HandleProperties(asset, Array.ConvertAll(values.ToArray(),
                        x => new UProperty { Value = x, Type = p.InnerType!, StructType = p.StructType }).ToList());
                    break;
                }
                case "StructProperty":
                {
                    switch (p.StructType)
                    {
                        case "GameplayTags":
                        case "GameplayTagContainer":
                        {
                            var names = (List<FName>)p.Value!;
                            foreach (var tag in names)
                                ReferenceOrAddString(asset, tag.Name);

                            break;
                        }
                        case "GameplayTag":
                            var name = (FName)p.Value!;
                            ReferenceOrAddString(asset, name.Name);
                            break;
                        default:
                            HandleProperties(asset, (List<UProperty>)p.Value!);
                            break;
                    }

                    break;
                }
                case "SoftObjectProperty":
                {
                    if (p.Value is not SoftObjectProperty value)
                        continue;

                    ReferenceOrAddString(asset, value.AssetPathName);
                    ReferenceOrAddString(asset, value.PackageName);
                    break;
                }
                case "NameProperty":
                {
                    if (p.Value is not FName value)
                        continue;

                    ReferenceOrAddString(asset, value.Name);
                    break;
                }
                case "ObjectProperty":
                {
                    if (p.Value is not ObjectProperty value)
                        continue;

                    if (asset is ZenAsset zen)
                    {
                        var nameIndex = zen.ExportMap.GetIndex(value.Text);
                        if (nameIndex < 0)
                            throw new KeyNotFoundException($"Could not find name {p.Name} in export map");

                        p.Value = new ObjectProperty { Value = nameIndex + 1 };
                    }

                    break;
                }
            }
        }
    }*/


}