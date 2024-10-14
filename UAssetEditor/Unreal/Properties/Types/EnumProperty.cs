using System.Runtime.CompilerServices;
using UAssetEditor.Binary;
using UsmapDotNet;


namespace UAssetEditor.Properties;

public class EnumProperty : AbstractProperty
{
    public bool IsZero;
    
    public EnumProperty(bool isZero = false)
    {
        IsZero = isZero;
    }
    
    public override void Read(Reader reader, UsmapPropertyData? data, BaseAsset asset = null)
    {
        var index = IsZero ? 0 : Convert.ToInt32(ReadProperty("Int8Property", reader, null));
        var enumData = reader.Mappings!.Enums.FirstOrDefault(x => x.Name == data.EnumName);
        Value = enumData.Names[index];
    }

    public override void Write(Writer writer, UProperty property, BaseAsset asset = null)
    {
        var enumData = asset!.Mappings!.Enums.FirstOrDefault(x => x.Name == property.EnumName);
        var index = enumData.Names.ToList().FindIndex(x => x == (string)property.Value!);
        writer.Write((byte)index);
    }
}