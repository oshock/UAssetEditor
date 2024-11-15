using System.Runtime.InteropServices;

namespace UnrealExtractor.Unreal.Misc;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct FGuid // I have no idea why it wants this as "record" but cool
{
    public uint A;
    public uint B;
    public uint C;
    public uint D;
}