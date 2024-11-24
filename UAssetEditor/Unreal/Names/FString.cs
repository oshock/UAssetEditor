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

    public FString(Reader reader)
    {
        var length = reader.Read<int>();
        
        if (length == 0)
        {
            Text = string.Empty;
            return;
        }

        // TODO unicode
        /*if (length < 0)
        {
            length = -length;
            var bytes = reader.ReadBytes(length * 2);
            
        }*/

        Text = Encoding.ASCII.GetString(reader.ReadBytes(length)).TrimEnd('\0');
    }

    public static string Read(Reader reader)
    {
        var text = new FString(reader).Text;
        return text;
    }

    public static void Write(Writer writer, string text)
    {
        if (text.Length > 0)
        {
            var str = text + '\0';
            writer.Write(str.Length);
            writer.WriteString(str);
            return;
        }
        
        writer.Write(0); // Length
    }

    public override string ToString()
    {
        return Text;
    }
}