using System.Text;
using UnrealExtractor.Binary;

namespace UnrealExtractor.Unreal.Names;


public class FString
{
    public string Text;

    public FString(string str)
    {
        Text = str;
    }

    // TODO unicode
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

    public static FString Read(Reader reader) => new(reader);

    public override string ToString()
    {
        return Text;
    }
}