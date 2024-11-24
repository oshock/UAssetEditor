using System.Text;
using UAssetEditor.Binary;

namespace UAssetEditor.Unreal.Names;


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

    public static string Read(Reader reader)
    {
        var text = new FString(reader).Text;
        return text;
    }

    public static void Write(Writer writer, string text)
    {
        writer.Write(text.Length);
        if (text.Length > 0)
            writer.WriteString(text);
    }

    public override string ToString()
    {
        return Text;
    }
}