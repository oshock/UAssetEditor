using System.Runtime.CompilerServices;
using Usmap.NET;

namespace UAssetEditor.Properties;

public class EnumProperty : AbstractProperty
{
    public override void Read(Reader reader, UsmapPropertyData? data)
    {
        var index = Convert.ToInt32(ReadProperty(data.InnerType.StructType, reader));
        var enumData = reader.Mappings.Enums.FirstOrDefault(x => x.Name == data.EnumName);
        Value = $"{data.EnumName}::{enumData.Names[index]}";
    }
}