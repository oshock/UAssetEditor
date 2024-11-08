using System.Data;
using UAssetEditor.Binary;
using UAssetEditor.Unreal.Names;
using UsmapDotNet;

namespace UAssetEditor.Unreal.Properties.Types;

public class GameplayTagArrayProperty : AbstractProperty<List<FName>>
{
    public override void Read(Reader reader, UsmapPropertyData? data, Asset? asset = null, EReadMode mode = EReadMode.Normal)
    {
        Value = [];

        if (mode == EReadMode.Zero)
            return;
        
        if (asset is null)
            throw new NoNullAllowedException("Asset cannot be null.");
        
        var tagCount = reader.Read<uint>();

        for (int i = 0; i < tagCount; i++)
            Value.Add(new FName(reader, asset.NameMap));
    }

    public override void Write(Writer writer, UProperty property, Asset? asset = null)
    {
        if (Value is null)
        {
            writer.Write(0);
            return;
        }
        
        if (asset is null)
            throw new NoNullAllowedException("Asset cannot be null.");
        
        writer.Write(Value.Count);
        
        foreach (var tag in Value)
        {
            tag.Serialize(writer, asset.NameMap);
        }
    }
}