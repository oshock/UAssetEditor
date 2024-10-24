using System.Data;
using UAssetEditor.Binary;
using UAssetEditor.Unreal.Names;
using UsmapDotNet;


namespace UAssetEditor.Unreal.Properties.Types;

public class NameProperty : AbstractProperty<FName>
{
    public override string ToString()
    {
        return Value?.Name ?? "None";
    }

    public override void Read(Reader reader, UsmapPropertyData? data, BaseAsset? asset = null, bool isZero = false)
    {
        if (isZero)
            Value = new FName("None");

        ArgumentNullException.ThrowIfNull(asset);

        var value = new FName(reader, asset.NameMap);
        Value = value;
    }

    public override void Write(Writer writer, UProperty property, BaseAsset? asset = null)
    {
        if (Value is null)
            throw new NoNullAllowedException("FName cannot be null. Remove from properties.");
        
        ArgumentNullException.ThrowIfNull(asset);
        
        Value.Serialize(writer, asset.NameMap);
    }
}