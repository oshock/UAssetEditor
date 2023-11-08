namespace UAssetEditor.Properties;

public class FInstancedStruct
{
    public byte[] Buffer;
    
    public FInstancedStruct(Reader reader)
    {
        var index = AbstractProperty.CreateAndRead("ObjectProperty", reader, null);
        var serialSize = reader.Read<int>();

        /*if (TryGetStructByIndexOrWhatever(index, out var @struct))
        {
            
        }
        else*/
        {
            Buffer = reader.ReadBytes(serialSize);
        }
    }

    public void Serialize(Writer writer)
    {
        writer.Write(Buffer);
    }
}