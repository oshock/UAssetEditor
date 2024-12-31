using System.Data;
using UAssetEditor.Unreal.Names;
using UAssetEditor.Binary;
using UAssetEditor.Unreal.Exports;
using UAssetEditor.Utils;


namespace UAssetEditor.Unreal.Properties.Types;

// I did NOT feel like grabbing this from unrealengine repo - owen
// https://github.com/FabianFG/CUE4Parse/blob/0cd21c2d96068d29b812c9d0538de5654d8fbab5/CUE4Parse/UE4/Objects/Core/i18N/FText.cs

public enum EFormatArgumentType : sbyte
{
    Int,
    UInt,
    Float,
    Double,
    Text,
    Gender,

    // Add new enum types at the end only! They are serialized by index.
}

public class FFormatArgumentValue : UStruct
{
    public EFormatArgumentType Type;
    public object Value;

    public FFormatArgumentValue()
    { }

    public override void Read(Reader reader, PropertyData? data, Asset? asset = null, ESerializationMode mode = ESerializationMode.Normal)
    {
        Type = reader.Read<EFormatArgumentType>();
        switch (Type)
        {
            case EFormatArgumentType.Text:
                var text = new FText();
                text.Read(reader, data);
                Value = text;
                break;
            case EFormatArgumentType.Int:
                // TODO maybe versioning
                Value = reader.Read<long>();
                break;
            case EFormatArgumentType.UInt:
                Value = reader.Read<ulong>();
                break;
            case EFormatArgumentType.Double:
                Value = reader.Read<double>();
                break;
            case EFormatArgumentType.Float:
                Value = reader.Read<float>();
                break;
            default:
                throw new KeyNotFoundException();
        }
    }

    public override void Write(Writer writer, Asset? asset = null)
    {
        writer.Write(Type);

        if (Type == EFormatArgumentType.Text)
        {
            Value.As<FText>().Write(writer, asset);
            return;
        }
        
        writer.Write(Value);
    }
}

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

public class FText : UStruct
{
    public uint Flags;
    public ETextHistoryType Type;
    public FTextHistory History;

    public FText()
    { }

    public override string ToString()
    {
        return History.ToString();
    }

    public override void Read(Reader reader, PropertyData? data, Asset? asset = null, ESerializationMode mode = ESerializationMode.Normal)
    {
        Flags = reader.Read<uint>();
        Type = reader.Read<ETextHistoryType>();
        History = Type switch
        {
            ETextHistoryType.Base => new FTextHistory.Base(reader),
            _ => new FTextHistory.None(reader)
        };
    }

    public override void Write(Writer writer, Asset? asset = null)
    {
        writer.Write(Flags);
        writer.Write(Type);
        History.Serialize(writer);
    }

    public abstract class FTextHistory
    {
        public abstract string Text { get; }

        public override string ToString()
        {
            return Text;
        }

        public abstract void Serialize(Writer writer);
        
        public class None : FTextHistory
        {
            public string? CultureInvariantString;
            public override string Text => CultureInvariantString ?? "";
        
            public None(Reader reader)
            {
                var bHasCultureInvariantString = reader.ReadBool();
                if (bHasCultureInvariantString)
                    CultureInvariantString = FString.Read(reader);
            }

            public override void Serialize(Writer writer)
            {
                if (CultureInvariantString != null)
                {
                    writer.Write(1); // bHasCultureInvariantString = True
                    FString.Write(writer, CultureInvariantString);

                    return;
                }
            
                writer.Write(0); // bHasCultureInvariantString = False
            }
        }

        public class Base : FTextHistory
        {
            public string Namespace;
            public string Key;
            public string SourceString;
            // public string LocalizedString;

            public override string Text => SourceString;
        
            public Base(Reader reader)
            {
                Namespace = FString.Read(reader);
                Key = FString.Read(reader);
                SourceString = FString.Read(reader);
            }

            public override void Serialize(Writer writer)
            {
                FString.Write(writer, Namespace);
                FString.Write(writer, Key);
                FString.Write(writer, SourceString);
            }
        }

        public class NamedFormat : FTextHistory
        {
            public FText SourceFmt;

            public Dictionary<string, FFormatArgumentValue> Arguments;
        
            public override string Text { get; }

            public NamedFormat(Reader reader)
            {
                SourceFmt = new FText();
                SourceFmt.Read(reader, null);
                var argCount = reader.Read<int>();
                Arguments = new Dictionary<string, FFormatArgumentValue>(argCount);

                for (int i = 0; i < argCount; i++)
                {
                    var key = FString.Read(reader);
                    var value = new FFormatArgumentValue();
                    value.Read(reader, null);
                    Arguments[key] = value;
                }
            }
            
            public override void Serialize(Writer writer)
            {
                
            }
        }
    }
}

public class TextProperty : AbstractProperty<FText>
{
    public TextProperty()
    { }
    
    public TextProperty(FText value)
    {
        Value = value;
    }
    
    public override void Read(Reader reader, PropertyData? data, Asset? asset = null,
        ESerializationMode mode = ESerializationMode.Normal)
    {
        Value = new FText();
        Value.Read(reader, data, asset, mode);
    }

    public override void Write(Writer writer, UProperty property, Asset? asset = null, ESerializationMode mode = ESerializationMode.Normal)
    {
        if (Value is null)
            throw new NoNullAllowedException("Value of FText cannot be null.");
        
        Value.Write(writer, asset);
    }
}