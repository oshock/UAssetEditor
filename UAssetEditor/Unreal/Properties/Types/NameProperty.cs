using System.Data;
using UAssetEditor.Unreal.Names;
using UnrealExtractor.Binary;
using UsmapDotNet;


namespace UAssetEditor.Unreal.Properties.Types;

public class NameProperty : AbstractProperty<FName>
{
    public override string ToString()
    {
        return Value?.Name ?? "None";
    }

    public override void Read(Reader reader, UsmapPropertyData? data, Asset? asset = null, EReadMode mode = EReadMode.Normal)
    {
        if (mode == EReadMode.Zero)
        {
            Value = new FName("None");
            return;
        }

        ArgumentNullException.ThrowIfNull(asset);

        var value = new FName(reader, asset.NameMap);
        Value = value;
    }

    public override void Write(Writer writer, UProperty property, Asset? asset = null)
    {
        if (Value is null)
            throw new NoNullAllowedException("FName cannot be null. Remove from properties.");
        
        ArgumentNullException.ThrowIfNull(asset);
        
        Value.Serialize(writer, asset.NameMap);
    }
}