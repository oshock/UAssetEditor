using System.Text;

namespace UAssetEditor.Properties;

public class FString
{
    public string Text;
    public long Hash => GetHashCode(); // CityHash it
    public override string ToString() => Text;

    public FString(string str)
    {
        Text = str;
    }

    public FString(Reader reader)
    {
        var length = reader.Read<int>();
        Text = Encoding.ASCII.GetString(reader.ReadBytes(length));
    }

    public static string Read(Reader reader)
    {
        return new FString(reader).Text;
    }
}

public class FName
{
    public string Name;

    public FName(Reader reader, NameMapContainer nameMap)
    {
        var nameIndex = reader.Read<int>();
        var extraIndex = reader.Read<int>();
        Name = nameMap[nameIndex];
    }
}