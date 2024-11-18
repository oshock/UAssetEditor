using System.Collections;
using System.Data;
using UsmapDotNet;

namespace UAssetEditor.Unreal.Properties.Unversioned;

public static class UnversionedReader
{
	public static UsmapSchema? FindSchema(this Usmap mappings, string type)
	{
		return mappings.Schemas.FirstOrDefault(x => x.Name == type);
	}
	
	// https://github.com/EpicGames/UnrealEngine/blob/a3cb3d8fdec1fc32f071ae7d22250f33f80b21c4/Engine/Source/Runtime/CoreUObject/Private/Serialization/UnversionedPropertySerialization.cpp#L528
    public static List<UProperty> ReadProperties(ZenAsset asset, UsmapSchema schema)
    {
	    var props = new List<UProperty>();

	    var frags = new List<FFragment>();
	    FFragment fragment; 
	    var zeroMaskNum = 0;
	    var unmaskedNum = 0U;
	    
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

	    asset.DefinedStructures.Add(schema);
	    
	    var totalSchemaIndex = 0;
	    var schemaIndex = 0;
	    var zeroMaskIndex = 0;
	    var schemaProperties = new List<UsmapProperty>();

	    foreach (var property in schema.Properties)
	    {
		    for (int i = 0; i < property.ArraySize; i++)
			    schemaProperties.Add(property);
	    }

	    while (frags.Count > 0)
	    {
		    var frag = frags.First();

		    if (bHasNonZeroValues)
			    schemaIndex += frag.SkipNum;

		    var currentRemainingValues = frag.ValueNum;

		    do
		    {
			    UsmapProperty? prop = null;

			    while (true)
			    {
				    if (schemaProperties.Count > schemaIndex)
					    prop = schemaProperties[schemaIndex];
				    else
					    schemaIndex -= schemaProperties.Count;
				    
				    if (prop is not null)
					    break;
			    }
			    
			    if (prop is null)
				    throw new KeyNotFoundException("Could not find property that matches current schema index.");

			    while (string.IsNullOrEmpty(prop!.Name))
			    {
				    totalSchemaIndex += schema.PropCount;
				    
				    schema = asset.Mappings.Schemas.First(x => x.Name == schema.SuperType);
				    prop = schemaProperties.ToList().Find(x => totalSchemaIndex + x.SchemaIdx == schemaIndex);
			    }
			    
			    var propType = prop.Data.Type.ToString();
			    var isNonZero = !frag.bHasAnyZeroes;
			    
			    if (zeroMask is not null)
				    isNonZero |= !zeroMask.Get(zeroMaskIndex);

			    var readMode = !isNonZero ? EReadMode.Zero : EReadMode.Normal;
			    var propertyType = prop.Data.Type.ToString();
			    var propertyValue = 
				    (AbstractProperty)PropertyUtils.ReadProperty(propertyType, asset, prop.Data, asset, readMode);
			    propertyValue.Name = prop.Name;
			    
			    props.Add(new UProperty
			    {
				    Type = propType,
				    Name = prop.Name,
				    Value = propertyValue,
				    StructType = prop.Data.StructType ?? prop.Data.InnerType?.StructType,
				    EnumName = prop.Data.EnumName,
				    InnerType = prop.Data.InnerType?.Type.ToString(),
				    IsZero = !isNonZero
			    });

			    if (frag.bHasAnyZeroes)
				    zeroMaskIndex++;
			    
			    schemaIndex++;
			    currentRemainingValues--;
		    } while (currentRemainingValues > 0);

		    frags.RemoveAt(0);
	    }

	    return props;
    }
}