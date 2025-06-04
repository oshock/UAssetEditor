using System.Runtime.InteropServices;
using System.Text;
using UAssetEditor.Classes;

namespace UAssetEditor.Unreal.Misc;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct FGuid : IUnrealType
{
    [UField] public uint A;
    [UField] public uint B;
    [UField] public uint C;
    [UField] public uint D;

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(A.ToString("X8"));
        sb.Append(B.ToString("X8"));
        sb.Append(C.ToString("X8"));
        sb.Append(D.ToString("X8"));
        
        return sb.ToString();
    }   
}