using System.Collections;
using UAssetEditor.Binary;

namespace UAssetEditor.Unreal.Properties.Unversioned;

public class FUnversionedHeader
{
    public readonly IEnumerator<FFragment> Fragments;
    public readonly BitArray? ZeroMask;
    public readonly int ZeroMaskNum;
    public readonly int UnmaskedNum;
    public readonly bool HasNonZeroValues;
    
    public FUnversionedHeader(Reader asset)
    {
        var frags = new List<FFragment>();
        FFragment fragment; 
	    
        do
        {
            fragment = new FFragment(asset.Read<ushort>());
            frags.Add(fragment);

            if (fragment.bHasAnyZeroes)
                ZeroMaskNum += fragment.ValueNum;
            else
                UnmaskedNum += fragment.ValueNum;
        } while (!fragment.bIsLast);

        Fragments = frags.GetEnumerator();
	    
        if (ZeroMaskNum > 0)
        {
            switch (ZeroMaskNum)
            {
                case <= 8:
                    ZeroMask = new BitArray(asset.ReadBytes(1));
                    break;
                case <= 16:
                    ZeroMask = new BitArray(asset.ReadBytes(2));
                    break;
                default:
                {
                    var num = (ZeroMaskNum + 32 - 1) / 32;
                    ZeroMask = new BitArray(asset.ReadArray<int>(num));
                    break;
                }
            }
		    
            ZeroMask.Length = ZeroMaskNum;
        }

        // Check if we have any non-zero values
        if (UnmaskedNum > 0)
        {
            HasNonZeroValues = true;
            return;
        }

        if (ZeroMask == null) 
            return;
        
        foreach (bool bit in ZeroMask)
        {
            if (bit)
                continue;

            HasNonZeroValues = true;
            break;
        }
    }
}