using System.Collections;
using System.Data;
using UAssetEditor.Unreal.Exports;
using UsmapDotNet;

namespace UAssetEditor.Unreal.Properties.Unversioned;

public static class UnversionedReader
{
	public static UsmapSchema? FindSchema(this Usmap mappings, string type)
	{
		return mappings.Schemas.FirstOrDefault(x => x.Name == type);
	}
	
	// https://github.com/EpicGames/UnrealEngine/blob/a3cb3d8fdec1fc32f071ae7d22250f33f80b21c4/Engine/Source/Runtime/CoreUObject/Private/Serialization/UnversionedPropertySerialization.cpp#L528
    public static List<UProperty> ReadProperties(Asset asset, UStruct struc)
    {
	    if (asset.Mappings is null)
		    throw new NoNullAllowedException("Mappings cannot be null if properties are to be read!");
	    
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
}