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
	    var props = new List<UProperty>();

	    var frags = new List<FFragment>();
	    FFragment fragment; 
	    var zeroMaskNum = 0;
	    var unmaskedNum = 0;
	    
	    do
	    {
		    fragment = new FFragment(asset.Read<ushort>());
		    frags.Add(fragment);

		    if (fragment.bHasAnyZeroes)
			    zeroMaskNum += fragment.ValueNum;
		    else
			    unmaskedNum += fragment.ValueNum;
	    } while (!fragment.bIsLast);
	    
	    BitArray? zeroMask = null;
	    if (zeroMaskNum > 0)
	    {
		    switch (zeroMaskNum)
		    {
			    case <= 8:
				    zeroMask = new BitArray(asset.ReadBytes(1));
				    break;
			    case <= 16:
				    zeroMask = new BitArray(asset.ReadBytes(2));
				    break;
			    default:
			    {
				    var num = (zeroMaskNum + 32 - 1) / 32;
				    zeroMask = new BitArray(asset.ReadArray<int>(num));
				    break;
			    }
		    }
		    
		    zeroMask.Length = zeroMaskNum;
	    }

	    var falseFound = false;
	    if (zeroMask != null)
	    {
		    foreach (var bit in zeroMask)
			    falseFound &= !(bool)bit;
	    }

	    var bHasNonZeroValues = unmaskedNum > 0 || falseFound;

	    if (asset.Mappings is null)
		    throw new NoNullAllowedException("Mappings cannot be null if properties are to be read!");

	    asset.DefinedStructures.Add(struc);
	    
	    var totalSchemaIndex = 0;
	    var schemaIndex = 0;
	    var zeroMaskIndex = 0;
	    var schemaProperties = new List<UProperty>();
	    gatherProperties();

	    while (frags.Count > 0)
	    {
		    var frag = frags.First();

		    if (bHasNonZeroValues)
			    schemaIndex += frag.SkipNum;

		    var currentRemainingValues = frag.ValueNum;

		    while (currentRemainingValues > 0)
		    {
			    UProperty? prop = null;

			    while (true)
			    {
				    if (schemaProperties.Count > schemaIndex)
					    prop = schemaProperties[schemaIndex];
				    else
				    {
					    schemaIndex -= schemaProperties.Count;
					    struc = new UStruct(asset.Mappings.Schemas.First(x => x.Name == struc.SuperType));
					    gatherProperties();
				    }

				    if (prop is not null)
					    break;
			    }
			    
			    if (prop is null)
				    throw new KeyNotFoundException("Could not find property that matches current schema index.");
			    
			    var isNonZero = !frag.bHasAnyZeroes;
			    
			    if (zeroMask != null && !isNonZero)
				    isNonZero |= !zeroMask.Get(zeroMaskIndex);

			    var readMode = !isNonZero ? ESerializationMode.Zero : ESerializationMode.Normal;
			    var propertyType = prop.Data?.Type;
			    var propertyValue = 
				    (AbstractProperty)PropertyUtils.ReadProperty(propertyType, asset, prop.Data, asset, readMode);
			    propertyValue.Name = prop.Name;
			    
			    props.Add(new UProperty(prop.Data, prop.Name, propertyValue, prop.ArraySize, prop.SchemaIdx, !isNonZero));

			    if (frag.bHasAnyZeroes)
				    zeroMaskIndex++;
			    
			    schemaIndex++;
			    currentRemainingValues--;
		    }

		    frags.RemoveAt(0);
	    }

	    void gatherProperties()
	    {
		    schemaProperties.Clear();
		    foreach (var property in struc.Properties)
		    {
			    for (int i = 0; i < property.ArraySize; i++)
				    schemaProperties.Add(property);
		    }
	    }

	    return props;
    }
}