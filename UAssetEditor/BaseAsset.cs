﻿using System.Data;
using System.Dynamic;
using System.Security.Principal;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UAssetEditor.Binary;
using UAssetEditor.Classes;
using UAssetEditor.Misc;
using UAssetEditor.Summaries;
using UAssetEditor.Unreal.Properties.Structs;
using UAssetEditor.Unreal.Properties.Types;
using UAssetEditor.Unreal.Properties.Unversioned;
using UsmapDotNet;

namespace UAssetEditor;

public class PropertyContainer : Container<UProperty>
{
    public string Type;
    public List<UProperty> Properties => Items;

    public override int GetIndex(string str)
    {
        return Items.FindIndex(x => x.Name == str);
    }
    
    public UProperty? this[string name]
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

    public PropertyContainer(string type, List<UProperty> items) : base(items)
    {
        Type = type;
    }
}

public abstract class BaseAsset : Reader
{
    public string Name { get; set; }
    public NameMapContainer NameMap;
    public EPackageFlags Flags;

    public StructureContainer DefinedStructures = new();
    
    public readonly Dictionary<string, PropertyContainer> Properties = new();
    
    public BaseAsset(byte[] data) : base(data)
    { }
    
    public BaseAsset(string path) : this(File.ReadAllBytes(path))
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
		    
            DefinedStructures.Add(schema);
        }

        return ReadProperties(DefinedStructures[type] ?? throw new KeyNotFoundException("How'd we get here?"));
    }
    
    // TODO eventually redo when I add pak assets (not unversioned)
    public abstract List<UProperty> ReadProperties(UsmapSchema structure);
    public abstract Writer WriteProperties(string type, int exportIndex, List<UProperty> properties);

    /// <summary>
    /// Serializes the entire asset to a stream.
    /// </summary>
    /// <param name="writer"></param>
    public abstract void WriteAll(Writer writer);

    public abstract void Fix();
    public abstract void WriteHeader(Writer writer);

    public static int ReferenceOrAddString(BaseAsset asset, string str)
    {
        if (!asset.NameMap.Contains(str))
            asset.NameMap.Add(str);

        return asset.NameMap.GetIndex(str);
    }

    public void SetPropertyContainerKey(string current, string newName)
    {
        var properties = Properties[current];
        Properties.Remove(current);
        Properties.Add(newName, properties);
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

    private struct PropertyObject
    {
        public string Type;
        public dynamic? Value;
    }
    
    public static object PropertyToObject(AbstractProperty? property)
    {
        var obj = new PropertyObject { Type = property?.GetType().Name ?? "None" };
        dynamic expando = new ExpandoObject();
        
        switch (property)
        {
            case null:
                return obj;
            case ArrayProperty array:
            {
                var arrayObj = new List<object>();
                foreach (var elm in array.Value ?? new())
                {
                    var value = PropertyToObject((AbstractProperty)elm);
                    arrayObj.Add(value);
                }

                ((IDictionary<string, object>)expando)[array.Name ?? "Unnamed Array"] = arrayObj;

                break;
            }
            case MapProperty map:
            {
                var dict = new Dictionary<object, object>();
                foreach (var kvp in map.Value ?? new())
                {
                    var key = PropertyToObject((AbstractProperty)kvp.Key);
                    var value = PropertyToObject((AbstractProperty)kvp.Value);
                    dict.Add(key, value);
                }
                
                ((IDictionary<string, object>)expando)[map.Name ?? "Unnamed Map"] = dict;

                break;
            }
            case StructProperty struc:
            {
                
                if (struc.Value is List<UProperty> props)
                {
                    
                    foreach (var kvp in props)
                    {
                        var key = kvp.Name;
                        var value = PropertyToObject((AbstractProperty?)kvp.Value);
                        ((IDictionary<string, object>)expando)[key] = value;
                    }
                }
                else
                {
                    obj.Value = struc.Value;
                }

                break;
            }
            default:
                ((IDictionary<string, object?>)expando)[property.Name ?? "Value"] = property.ValueAsObject;
                break;
        }

        obj.Value = expando;
        return obj;
    }
    
    /// <summary>
    /// Serializes the asset into json
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var objs = new List<object>();
        
        foreach (var kvp in Properties)
        {
            var properties = new List<object>();

            foreach (var prop in kvp.Value)
            {
                var propObj = PropertyToObject((AbstractProperty?)prop.Value);
                properties.Add(propObj);
            }
            
            var obj = new
            {
                Type = kvp.Value.Type,
                Name = kvp.Key,
                Properties = properties
            };
            
            objs.Add(obj);
        }
        
        return JsonConvert.SerializeObject(objs, Formatting.Indented);
    }
}

public class NameMapContainer : Container<string>
{
	public uint NumBytes => (uint)Items.Sum(str => str.Length);
    public ulong HashVersion;
    public override int GetIndex(string str) => Items.FindIndex(x => x == str);

    public NameMapContainer(List<string> items) : base(items)
    { }

    public int GetIndexOrAdd(string str)
    {
        var index = GetIndex(str);
        if (index > 0)
            return index;

        Add(str);
        return Length - 1;
    }

    public NameMapContainer(ulong hashVersion, List<string> strings) : base(strings)
    {
        HashVersion = hashVersion;
    }
    
    public static void WriteNameMap(Writer writer, NameMapContainer nameMap)
    {
        writer.Write(nameMap.Length);
        writer.Write(nameMap.NumBytes);
        writer.Write(nameMap.HashVersion);

        foreach (var s in nameMap)
            writer.Write(CityHash.TransformString(s));

        foreach (var s in nameMap)
        {
            writer.Write((byte)0);
            writer.Write((byte)s.Length);
        }

        foreach (var s in nameMap)
            writer.WriteString(s);
    }
    
    public static NameMapContainer ReadNameMap(Reader reader)
    {
        var count = reader.Read<int>();
        switch (count)
        {
            case < 0:
                throw new IndexOutOfRangeException($"Name map cannot have a length of {count}!");
            case 0:
                return default;
        }

        var numBytes = reader.Read<uint>();
        var hashVersion = reader.Read<ulong>();

        var hashes = new ulong[count];
        for (int i = 0; i < hashes.Length; i++) 
            hashes[i] = reader.Read<ulong>();
        
        var headers = new byte[count];
        for (int i = 0; i < headers.Length; i++)
        {
            reader.Position++; // skip utf-16 check
            headers[i] = reader.ReadByte();
        }

        var strings = headers.Select(t => Encoding.UTF8.GetString(reader.ReadBytes(t))).ToList();
        return new NameMapContainer(hashVersion, strings);
    }
}