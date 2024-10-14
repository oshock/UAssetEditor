using System.Collections;
using System.Data;

namespace UAssetEditor.Properties.Unversioned;

public class UnversionedReader(ZenAsset asset)
{
	public readonly ZenAsset Asset = asset;

	// https://github.com/EpicGames/UnrealEngine/blob/a3cb3d8fdec1fc32f071ae7d22250f33f80b21c4/Engine/Source/Runtime/CoreUObject/Private/Serialization/UnversionedPropertySerialization.cpp#L528
    public List<UProperty> ReadProperties(string type)
    {
	    var props = new List<UProperty>();
	    bool bHasNonZeroValues;

	    var frags = new List<FFragment>();
	    var zeroMaskNum = 0U;
	    var unmaskedNum = 0U;
	    do
	    {
		    var packed = Asset.Read<ushort>();
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
			    var @int = Asset.ReadByte();
			    zeroMask = new BitArray(new[] { @int });
		    }
		    else if (zeroMaskNum <= 16)
		    {
			    var @int = Asset.Read<ushort>();
			    zeroMask = new BitArray(new[] { (int)@int });
		    }
		    else
		    {
			    var data = new int[(zeroMaskNum + 32 - 1) / 32];
			    for (var idx = 0; idx < data.Length; idx++)
				    data[idx] = Asset.Read<int>();
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

	    if (Asset.Mappings == null)
		    throw new NoNullAllowedException("Mappings cannot be null if properties are to be read!");

	    var schema = Asset.Mappings?.Schemas.First(x => x.Name == type);
	    if (schema == null)
		    throw new NoNullAllowedException($"Cannot find '{type}' in mappings. Unable to parse data!");

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
			    var prop = schema.Properties.ToList().Find(x => totalSchemaIndex + x.SchemaIdx == schemaIndex);
			    while (string.IsNullOrEmpty(prop.Name))
			    {
				    totalSchemaIndex += schema.PropCount;
				    schema = Asset.Mappings?.Schemas.First(x => x.Name == schema.SuperType);
				    prop = schema!.Properties.ToList().Find(x => totalSchemaIndex + x.SchemaIdx == schemaIndex);
			    }
			    
			    var propType = prop.Data.Type.ToString();
			    var isNonZero = !frag.bHasAnyZeroes || !zeroMask.Get(zeroMaskIndex);
			    
			    props.Add(new UProperty
			    {
				    Type = propType,
				    Name = prop.Name,
				    Value = AbstractProperty.ReadProperty(prop.Data.Type.ToString(), Asset, prop, Asset, !isNonZero),
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