using System.Data;
using UAssetEditor.Binary;
using UAssetEditor.Unreal.Exports;
using UsmapDotNet;

namespace UAssetEditor.Unreal.Properties.Unversioned;

public static class UnversionedPropertyHandler
{
	public static UsmapSchema? FindSchema(this Usmap mappings, string type)
	{
		return mappings.Schemas.FirstOrDefault(x => x.Name == type);
	}
	
	// https://github.com/EpicGames/UnrealEngine/blob/a3cb3d8fdec1fc32f071ae7d22250f33f80b21c4/Engine/Source/Runtime/CoreUObject/Private/Serialization/UnversionedPropertySerialization.cpp#L528
    public static List<UProperty> DeserializeProperties(Asset asset, UStruct struc)
    {
	    asset.CheckMappings();
	    
	    // Get fragments
	    var header = new FUnversionedHeader(asset);

	    // Add struct for possible later use
	    asset.DefinedStructures.Add(struc);
	    
	    var properties = new List<UProperty>();
	    var schemaIndex = 0;
	    var zeroMaskIndex = 0;
	    
	    do
	    {
		    var frag = header.Fragments.Current;

		    if (header.HasNonZeroValues)
			    schemaIndex += frag.SkipNum;

		    var remainingValues = frag.ValueNum;
		    
		    while (remainingValues > 0)
		    {
			    if (struc.Properties.Count < schemaIndex)
					throw new KeyNotFoundException($"Could not find property with the schema index {schemaIndex}");

			    var property = struc.Properties.ElementAt(schemaIndex);
			    var propertyType = property.Data?.Type ?? throw new NoNullAllowedException($"'{property.Name}' has no data?");

			    var mode = ESerializationMode.Normal;
			    if (header.ZeroMask != null && frag.bHasAnyZeroes)
			    {
				    if (header.ZeroMask.Get(zeroMaskIndex))
					    mode = ESerializationMode.Zero;

				    zeroMaskIndex++;
			    }

			    var propertyValue = PropertyUtils.ReadProperty(propertyType, asset, property.Data, asset, mode);
			    var uProperty = new UProperty(property.Data, property.Name, propertyValue, property.ArraySize,
				    property.SchemaIdx, mode == ESerializationMode.Zero);
			    
			    properties.Add(uProperty);
			    remainingValues--;
			    schemaIndex++;
		    }

	    } while (header.Fragments.MoveNext());

	    return properties;
    }
    
    public static void SerializeProperties(ZenAsset asset, Writer writer, UStruct struc, List<UProperty> properties)
    {
	    var sorted = SortProperties(properties, struc);
	    
	    // Serialize header
	    FUnversionedHeader.Serialize(writer, struc, sorted);
	    
	    // Serialize properties
	    foreach (var prop in sorted)
	    {
		    // We write everything atm
		    /*if (prop.IsZero)
			    continue;*/
		    
		    PropertyUtils.WriteProperty(writer, prop, asset);
	    }
    }
    
    private static List<UProperty> SortProperties(List<UProperty> properties, UStruct struc)
    {
	    var result = new List<UProperty>();

	    foreach (var property in struc.Properties)
	    {
		    var correspondingProperty = properties.FirstOrDefault(x => x.Name == property.Name);
		    
		    if (correspondingProperty != null)
			    result.Add(correspondingProperty);
	    }

	    return result;
    }
}