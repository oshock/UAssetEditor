using System.Data;
using UAssetEditor.Summaries;
using UAssetEditor.Unreal.Exports;
using UAssetEditor.Unreal.Objects;
using UAssetEditor.Unreal.Properties.Structs;
using UAssetEditor.Unreal.Properties.Types;
using UAssetEditor.Unreal.Properties.Unversioned;
using UnrealExtractor.Binary;
using UnrealExtractor.Classes.Containers;
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
		    
            DefinedStructures.Add(new UStruct(schema));
        }

        return ReadProperties(DefinedStructures[type] ?? throw new KeyNotFoundException("How'd we get here?"));
    }
    
    // TODO eventually redo when I add pak assets (not unversioned)
    public abstract List<UProperty> ReadProperties(UStruct structure);
    public abstract Writer WriteProperties(string type, List<UProperty> properties);

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