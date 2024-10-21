using UAssetEditor.Binary;
using UAssetEditor.Unreal.Names;
using UsmapDotNet;

namespace UAssetEditor.Properties;

public class GameplayTagArrayProperty : AbstractProperty
{
    public List<FName> Value;

    public override void Read(Reader reader, UsmapPropertyData data, BaseAsset? asset = null)
    {
        var tags = new List<FName>();
        var tagCount = reader.Read<uint>();

        for (int i = 0; i < tagCount; i++)
            tags.Add(new FName(reader, asset!.NameMap));

        Value = tags;
    }

    public override void Write(Writer writer, UProperty property, BaseAsset? asset = null)
    {
        
    }
}