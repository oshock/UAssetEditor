using System.Data;
using UAssetEditor.Binary;
using Usmap.NET;

namespace UAssetEditor.Properties.Unversioned;

public class UnversionedWriter(ZenAsset asset)
{
	public readonly ZenAsset Asset = asset;

	public Writer WriteProperties(string type, int exportIndex, List<UProperty> properties)
    {
	    if (Asset.Mappings == null)
		    throw new NoNullAllowedException("Mappings cannot be null!");
	    
	    var writer = new Writer();
	    var frags = new List<FFragment>();
	    var zeroMask = new List<bool>();

	    AddFrag();
	    using var enumerator = properties.GetEnumerator();
	    enumerator.MoveNext();
	    var schema = Asset.Mappings?.Schemas.FirstOrDefault(x => x.Name == type);
	    var allProps = new List<UsmapProperty>();

	    while (!string.IsNullOrEmpty(schema?.Name))
	    {
		    allProps.AddRange(schema.Value.Properties);
		    schema = Asset.Mappings?.Schemas.FirstOrDefault(x => x.Name == schema.Value.SuperType);
	    }
	    
	    foreach (var prop in allProps)
	    {
		    var isZero = enumerator.Current.IsZero;

		    if (prop.Name == enumerator.Current.Name)
		    {
			    IncludeProperty(isZero);
			    
			    if (!enumerator.MoveNext())
			    {
				    MakeLast();
				    break;
			    }
		    }
		    else
			    ExcludeProperty();
	    }

	    foreach (var frag in frags)
		    writer.Write(frag.Pack());

	    if (zeroMask.Count > 0)
	    {
		    if (zeroMask.Any(x => x))
		    {
			    var result = new byte[(zeroMask.Count - 1) / 8 + 1];
			    var index = 0;

			    for (int i = 0; i < zeroMask.Count; i++)
			    {
				    result[index] += Convert.ToByte(zeroMask[i] ? 1 : 0 * Math.Pow(2, i));

				    if (i > 0 && i % 8 == 0)
					    index++;
			    }

			    writer.WriteBytes(result);
		    }
	    }

	    foreach (var prop in properties)
	    {
		    if (prop.IsZero)
			    continue;
		    
		    AbstractProperty.WriteProperty(writer, prop, Asset);
	    }

	    if (exportIndex > 0)
	    {
		    Asset.ExportMap[exportIndex].SetCookedSerialSize((ulong)writer.BaseStream.Length);
	    }

	    return writer;
	    
	    void AddFrag() => frags.Add(new FFragment());

	    void TrimZeroMask(FFragment frag)
	    {
		    if (!frag.bHasAnyZeroes)
		    {
			    zeroMask.RemoveRange(zeroMask.Count - frag.ValueNum, frag.ValueNum);
		    }
	    }
	    
	    void IncludeProperty(bool isZero)
	    {
		    if (GetLast().ValueNum == FFragment.ValueMax)
		    {
			    TrimZeroMask(GetLast());
			    AddFrag();
		    }
		    
		    zeroMask.Add(isZero);
		    frags[^1] = frags[^1] with
		    {
			    ValueNum = (byte)(frags[^1].ValueNum + 1),
			    bHasAnyZeroes = frags[^1].bHasAnyZeroes | isZero
		    };
	    }

	    void ExcludeProperty()
	    {
		    if (GetLast().ValueNum != 0 || GetLast().SkipNum == FFragment.SkipMax)
		    {
			    TrimZeroMask(GetLast());
			    AddFrag();
		    }

		    frags[^1] = frags[^1] with
		    {
			    SkipNum = (byte)(frags[^1].SkipNum + 1)
		    };
	    }

	    void MakeLast() => frags[^1] = frags[^1] with
	    {
		    bIsLast = true
	    };

	    FFragment GetLast() => frags[^1];
    }
}