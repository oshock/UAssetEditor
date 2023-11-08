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
    public int NameIndex;
    public int ExtraIndex;

    public FName(Reader reader, NameMapContainer nameMap)
    {
        NameIndex = reader.Read<int>();
        ExtraIndex = reader.Read<int>();
        if (nameMap.Length > NameIndex && NameIndex >= 0)
            Name = nameMap[NameIndex];
    }

    public void Serialize(Writer writer, NameMapContainer nameMapContainer)
    {
        writer.Write(nameMapContainer.Strings.FindIndex(x => x == Name));
        writer.Write(ExtraIndex);
    }
}