using System.Collections;
using System.Data;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UAssetEditor.IoStore;
using UAssetEditor.Names;
using UAssetEditor.Properties;
using UAssetEditor.Summaries;
using Usmap.NET;

namespace UAssetEditor;

public class PropertyContainer
{
	private List<UProperty> Properties = new();

	public List<UProperty> GetProperties() => Properties;
	public void SetProperties(List<UProperty> properties) => Properties = properties;

	public PropertyContainer(List<UProperty> properties)
	{
		SetProperties(properties);
	}
	    
	public UProperty this[string name]
	{
		get => Properties.Find(x => x.Name == name) ?? new UProperty { Name = "None" };
		set => Properties[Properties.FindIndex(x => x.Name == name)] = value;
	}
}

public class UAsset : Reader
{
    public string Name { get; set; }
    public NameMapContainer NameMap;
    public EPackageFlags Flags;

    
    public readonly Dictionary<string, PropertyContainer> Properties = new();
    
    public UAsset(byte[] data) : base(data)
    { }
    
    public UAsset(string path) : this(File.ReadAllBytes(path))
    { }
    
    /// <summary>
    /// Read the entirety of this asset
    /// </summary>
    public void ReadAll()
    {
	    throw new NotImplementedException();
    }

    public uint ReadHeader()
    {
	    throw new NotImplementedException();
    }
    
    public static NameMapContainer ReadNameMap(Reader reader)
    {
	    throw new NotImplementedException();
    }
    
    public List<UProperty> ReadProperties(string type)
    {
	    throw new NotImplementedException();
    }
    
    public static void WriteNameMap(Writer writer, NameMapContainer nameMap)
    {
	    throw new NotImplementedException();
    }
    
    public Writer WriteProperties(string type, int exportIndex, List<UProperty> properties)
    {
	    throw new NotImplementedException();
    }
    
    /// <summary>
    /// Serializes the entire asset to a stream.
    /// </summary>
    /// <param name="writer"></param>
    public void WriteAll(Writer writer)
    {
	    throw new NotImplementedException();
    }

    public uint WriteHeader(Writer writer)
    {
	    throw new NotImplementedException();
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

public struct NameMapContainer
{
	public uint NumBytes => (uint)Strings.Sum(str => str.Length);
	
    public ulong HashVersion;
    public List<ulong> Hashes;
    public List<string> Strings;

    public string this[int nameIndex] => Strings[nameIndex];
    public int Length => Strings.Count;
}