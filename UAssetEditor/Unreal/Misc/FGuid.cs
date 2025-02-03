using System.Runtime.InteropServices;
using UAssetEditor.Classes;

namespace UAssetEditor.Unreal.Misc;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct FGuid : IUnrealType
{
    [UField] public uint A;
    [UField] public uint B;
    [UField] public uint C;
    [UField] public uint D;
}