using UAssetEditor.Binary;
using UAssetEditor.Unreal.Properties.Reflection;
using UAssetEditor.Unreal.Properties.Structs;

namespace UAssetEditor.Unreal.Properties.Types;

// TODO
public class FInstancedStruct : UStruct
{
    public byte[] Buffer;
    
    public FInstancedStruct(Reader reader)
    {
        var index = PropertyReflector.ReadProperty("ObjectProperty", reader, null);
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