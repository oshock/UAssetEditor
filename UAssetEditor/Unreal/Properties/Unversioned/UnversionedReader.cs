using System.Collections;
using System.Data;
using UAssetEditor.Unreal.Properties.Structs;
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
	    bool bHasNonZeroValues;

	    var frags = new List<FFragment>();
	    var zeroMaskNum = 0U;
	    var unmaskedNum = 0U;
	    
	    do
	    {
		    var packed = asset.Read<ushort>();
		    frags.Add(new FFragment(packed));

		    var valueNum = frags.Last().ValueNum;
		    if (frags.Last().bHasAnyZeroes)
			    zeroMaskNum += valueNum;
		    else
			    unmaskedNum += valueNum;
	    } while (!frags.Last().bIsLast);

	    BitArray? zeroMask = null;
	    if (zeroMaskNum > 0)
	    {
		    if (zeroMaskNum <= 8)
		    {
			    var @int = asset.ReadByte();
			    zeroMask = new BitArray(new[] { @int });
		    }
		    else if (zeroMaskNum <= 16)
		    {
			    var @int = asset.Read<ushort>();
			    zeroMask = new BitArray(new[] { (int)@int });
		    }
		    else
		    {
			    var data = new int[(zeroMaskNum + 32 - 1) / 32];
			    for (var idx = 0; idx < data.Length; idx++)
				    data[idx] = asset.Read<int>();
			    zeroMask = new BitArray(data);
		    }
	    }

	    var falseFound = false;
	    if (zeroMask != null)
	    {
		    foreach (var bit in zeroMask)
			    falseFound &= !(bool)bit;
	    }

	    bHasNonZeroValues = unmaskedNum > 0 || falseFound;

	    if (asset.Mappings is null)
		    throw new NoNullAllowedException("Mappings cannot be null if properties are to be read!");

	    asset.DefinedStructures.Add(schema);
	    
	    var totalSchemaIndex = 0;
	    var schemaIndex = 0;
	    var zeroMaskIndex = 0;

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
				    if (schema.Properties.Length > schemaIndex)
					    prop = schema.Properties[schemaIndex];
				    else
				    {
					    schemaIndex -= schema.Properties.Length;
					    
					    /*var super = schema.SuperType;
					    if (string.IsNullOrEmpty(super))
						    throw new NoNullAllowedException($"{nameof(super)} cannot be null.");
					    
					    schema = asset.Mappings.FindSchema(super) 
					             ?? throw new KeyNotFoundException("Cannot find super type '{type}' in mappings. Unable to parse data!");*/
				    }
				    
				    if (prop is not null)
					    break;
			    }
			    
			    if (prop is null)
				    throw new KeyNotFoundException("Could not find property that matches current schema index.");

			    while (string.IsNullOrEmpty(prop.Name))
			    {
				    totalSchemaIndex += schema.PropCount;
				    
				    schema = asset.Mappings.Schemas.First(x => x.Name == schema.SuperType);
				    prop = schema.Properties.ToList().Find(x => totalSchemaIndex + x.SchemaIdx == schemaIndex);
			    }
			    
			    var propType = prop.Data.Type.ToString();
			    var isNonZero = !frag.bHasAnyZeroes;
			    
			    if (zeroMask is not null)
				    isNonZero |= !zeroMask.Get(zeroMaskIndex);

			    var readMode = !isNonZero ? EReadMode.Zero : EReadMode.Normal;
			    var propertyType = prop.Data.Type.ToString();
			    var propertyValue = PropertyUtils.ReadProperty(propertyType, asset, prop.Data, asset, readMode);
			    
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