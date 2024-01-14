using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UAssetEditor.Binary;
using UAssetEditor.Classes;
using UAssetEditor.Misc;
using UAssetEditor.Properties;
using UAssetEditor.Summaries;
using UAssetEditor.Unreal.Names;

namespace UAssetEditor;

public class PropertyContainer : Container<UProperty>
{
    public List<UProperty> GetProperties() => Items;

    public override int GetIndex(string str)
    {
        return Items.FindIndex(x => x.Name == str);
    }

    public PropertyContainer(List<UProperty> items) : base(items)
    { }
}

public abstract class BaseAsset : Reader
{
    public string Name { get; set; }
    public NameMapContainer NameMap;
    public EPackageFlags Flags;

    
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
    public abstract List<UProperty> ReadProperties(string type);
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

    void HandleProperties(BaseAsset asset, List<UProperty> properties)
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
    }

    public JArray ToJson()
    {
	    var result = new List<KeyValuePair<string, Dictionary<string, object>>>();

	    foreach (var prop in Properties)
	    {
		    result.Add(new KeyValuePair<string, Dictionary<string, object>>(prop.Key,
			    prop.Value.GetProperties().ToDictionary(x => x.Name, x => x.Value) ?? new()));
	    }

	    return JArray.FromObject(result);
    }

    public override string ToString()
    {
	    return JsonConvert.SerializeObject(ToJson(), Formatting.Indented);
    }
}

public class NameMapContainer : Container<string>
{
	public uint NumBytes => (uint)Items.Sum(str => str.Length);
    public ulong HashVersion;
    public override int GetIndex(string str) => Items.FindIndex(x => x == str);

    public NameMapContainer(List<string> items) : base(items)
    { }

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