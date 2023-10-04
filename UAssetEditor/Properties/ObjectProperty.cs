using Usmap.NET;

namespace UAssetEditor.Properties;

public class ObjectProperty : AbstractProperty
{
    // TODO find asset reference
    
    public override void Read(Reader reader, UsmapPropertyData? data, UAsset? asset = null)
    {
        Value = reader.Read<int>();
    }
}