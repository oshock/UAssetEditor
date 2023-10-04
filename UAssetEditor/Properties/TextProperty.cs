using System.Runtime.CompilerServices;
using Usmap.NET;

namespace UAssetEditor.Properties;

public enum ETextHistoryType : sbyte
{
    None = -1,
    Base = 0,
    NamedFormat,
    OrderedFormat,
    ArgumentFormat,
    AsNumber,
    AsPercent,
    AsCurrency,
    AsDate,
    AsTime,
    AsDateTime,
    Transform,
    StringTableEntry,
    TextGenerator,

    // Add new enum types at the end only! They are serialized by index.
}

public struct FTextHistory
{
    public string Namespace;
    public string Key;
    public string SourceString;

    public override string ToString() => SourceString;
}

public class TextProperty : AbstractProperty
{
    public override void Read(Reader reader, UsmapPropertyData? data, UAsset? asset = null)
    {
        var type = reader.Read<ETextHistoryType>();
        Value = type switch
        {
            ETextHistoryType.Base => new FTextHistory
            {
                Namespace = FString.Read(reader),
                Key = FString.Read(reader),
                SourceString = FString.Read(reader),
            },
            _ => new FTextHistory()
        };
    }
}