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
        if (length == 0)
        {
            Text = string.Empty;
            return;
        }
        
        Text = Encoding.ASCII.GetString(reader.ReadBytes(length)).TrimEnd('\0');
    }

    public static string Read(Reader reader)
    {
        var text = new FString(reader).Text;
        return text;
        
    }
}

public class FName
{
    public string Name;

    public FName(Reader reader, NameMapContainer nameMap)
    {
        var nameIndex = reader.Read<int>();
        var extraIndex = reader.Read<int>();
        //Name = nameMap[nameIndex];
    }
}