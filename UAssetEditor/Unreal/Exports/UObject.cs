using System.Data;
using UAssetEditor.Binary;
using UAssetEditor.Classes;
using UAssetEditor.Summaries;
using UAssetEditor.Unreal.Assets;
using UAssetEditor.Unreal.Exports.Engine;
using UAssetEditor.Unreal.Misc;
using UAssetEditor.Unreal.Names;
using UAssetEditor.Unreal.Properties;
using UAssetEditor.Unreal.Properties.Unversioned;
using UAssetEditor.Unreal.Versioning;

namespace UAssetEditor.Unreal.Exports;

public class UObject
{
    public string Name;
    public Lazy<UObject?> Outer;
    public UStruct? Class;
    public Lazy<ResolvedObject?> Super;
    public Lazy<ResolvedObject?> Template;
    public EObjectFlags Flags;
    public FGuid? ObjectGuid { get; private set; }

    public List<UProperty> Properties;

    public Asset? Owner;
    
    // Tagged
    public bool HasPropertyGuid;
    public FGuid PropertyGuid;
    public byte OverrideOperation;
    public bool bExperimentalOverridableLogic;

    public UObject()
    {
        Name = "None";
        Properties = new();
    }
    
    public UObject(Asset asset) : this()
    {
        Owner = asset;
    }

    // TODO other export types
    public static UObject ConstructObject(Asset asset, string className)
    {
        return className switch
        {
            "CurveTable" => new UCurveTable(asset),
            _ => new UObject(asset)
        };
    }

    public override string ToString()
    {
        return $"{Class?.Name ?? "None"}.{Name}";
    }

    public UProperty? this[string name]
    {
        get => Properties.FirstOrDefault(x => x.Name == name);
        set
        {
            var property = Properties.FirstOrDefault(x => x.Name == name);
            
            if (property != null)
            {
                if (value == null)
                {
                    Properties.Remove(property);
                    return;
                }
                
                property.Data = value.Data;
                property.Value = value.Value;
                property.ArraySize = value.ArraySize;
                property.IsZero = value.IsZero;
                property.SchemaIdx = value.SchemaIdx;
                return;
            }

            if (value == null)
                return;
            
            Properties.Add(value);
        }
    }

    public virtual void Deserialize(long position) 
    {
        if (Owner is null)
            throw new NoNullAllowedException("Cannot deserialize UObject without a valid asset to work with.");

        if (Class is null)
            throw new NoNullAllowedException($"Class cannot be null.");

        Owner.Position = position;

        Properties = Owner.HasUnversionedProperties
            ? UnversionedPropertyHandler.DeserializeProperties(Owner, Class)
            : DeserializePropertiesTagged();
        
        if (Owner!.Game >= EGame.GAME_UE4_0 && !Flags.HasFlag(EObjectFlags.RF_ClassDefaultObject) && Owner.Read<int>() == 1)
        {
            ObjectGuid = Owner.Read<FGuid>();
        }
    }

    // https://github.com/FabianFG/CUE4Parse/blob/ae9c85c8b6bae523b3194be29c72ea889f801a50/CUE4Parse/UE4/Assets/Objects/FPropertyTag.cs#L136
    public List<UProperty> DeserializePropertiesTagged()
    {
        var properties = new List<UProperty>();
        while (true)
        {
            var property = new UProperty { Name = new FName(Owner!, Owner!.NameMap).Name };
            if (property.Name == "None")
                break;

            if (Owner.FileVersion >= EUnrealEngineObjectUE5Version.PROPERTY_TAG_COMPLETE_TYPE_NAME)
            {
                throw new NotImplementedException("UE5 tagged properties are not implemented yet!");
            }
            else
            {
                var type = new FName(Owner!, Owner!.NameMap).Name;
                var size = Owner.Read<int>();
                var arrayIndex = Owner.Read<int>();
                var data = new PropertyData(Owner, type);
                
                if (Owner.FileVersion >= EUnrealEngineObjectUE4Version.PROPERTY_GUID_IN_PROPERTY_TAG)
                {
                    HasPropertyGuid = Owner.ReadFlag();
                    if (HasPropertyGuid)
                    {
                        PropertyGuid = Owner.Read<FGuid>();
                    }
                }
                
                if (Owner.FileVersion >= EUnrealEngineObjectUE5Version.PROPERTY_TAG_EXTENSION_AND_OVERRIDABLE_SERIALIZATION)
                {
                    var tagExtensions = Owner.Read<EPropertyTagExtension>();
                    if (tagExtensions.HasFlag(EPropertyTagExtension.OverridableInformation))
                    {
                        // TODO fix to be an inside UProperty
                        OverrideOperation = Owner.ReadByte(); // EOverriddenPropertyOperation
                        bExperimentalOverridableLogic = Owner.ReadBool();
                    }
                }
                
                var propertyValue = PropertyUtils.ReadProperty(type, Owner, data);
                var uProperty = new UProperty(data, property.Name, propertyValue, (byte)arrayIndex /*TODO*/, 0);
                properties.Add(uProperty);
                
                // TODO handle size checks
            }
        }

        return properties;
    }
    
    public virtual void Serialize(Writer writer)
    {
        if (Owner is null)
            throw new NoNullAllowedException("Cannot serialize UObject without a valid asset to work with.");

        if (Class is null)
            throw new NoNullAllowedException($"Class cannot be null.");
        
        if (Owner.HasUnversionedProperties)
        {
            UnversionedPropertyHandler.SerializeProperties((ZenAsset)Owner, writer, Class, Properties);
        }
        else
        {
            // TODO tagged properties
        }
        
        if (!Flags.HasFlag(EObjectFlags.RF_ClassDefaultObject))
        {
            var shouldWriteGuid = ObjectGuid != null;
            writer.Write(shouldWriteGuid ? 1 : 0); // Boolean as Int32
            
            if (shouldWriteGuid)
                writer.Write(ObjectGuid);
        }
    }
}