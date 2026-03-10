using System.Runtime.InteropServices;

namespace UAssetEditor.Unreal.Objects.IO;

[StructLayout(LayoutKind.Sequential)]
public struct FArc
{
    public uint FromNodeIndex;
    public uint ToNodeIndex;
};