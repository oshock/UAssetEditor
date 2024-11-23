using UAssetEditor.Names;
using UnrealExtractor.Binary;
using UsmapDotNet;


namespace UAssetEditor.Unreal.Properties.Types;

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

public class TextProperty : AbstractProperty<FTextHistory>
{
    public uint Flags;
    public ETextHistoryType Type;
    
    public override void Read(Reader reader, PropertyData? data, Asset? asset = null,
        ESerializationMode mode = ESerializationMode.Normal)
    {
        Flags = reader.Read<uint>();
        Type = reader.Read<ETextHistoryType>();
        Value = Type switch
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

    public override void Write(Writer writer, UProperty property, Asset? asset = null, ESerializationMode mode = ESerializationMode.Normal)
    {
        writer.Write(Flags);
        writer.Write(Type);
        var text = Value;
        
        switch (Type)
        {
            case ETextHistoryType.Base: 
                FString.Write(writer, text.Namespace);
                FString.Write(writer, text.Key);
                FString.Write(writer, text.SourceString);
                break;
            default:
                throw new NotImplementedException($"Type '{Type}' is not implemented.");
        }
    }
}