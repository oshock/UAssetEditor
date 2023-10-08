namespace UAssetEditor.Properties;

public class FInstancedStruct
{
    public object? Value;

    public FInstancedStruct(Reader reader)
    {
        var index = AbstractProperty.CreateAndRead("ObjectProperty", reader, null);
        var serialSize = reader.Read<int>();

        /*if (TryGetStructByIndexOrWhatever(index, out var @struct))
        {
            
        }
        else*/
        {
            reader.Position += serialSize;
        }
    }
}