using System.Runtime.InteropServices;

namespace UAssetEditor.Unreal.Objects;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = Size)]
public struct FSerializedNameHeader
{
    public const int Size = 2;

    private byte _data0;
    private byte _data1;

    public FSerializedNameHeader(byte size)
    {
        _data0 = 0;
        _data1 = size;
    }

    public bool IsUtf16 => (_data0 & 0x80u) != 0;
    public uint Length => ((_data0 & 0x7Fu) << 8) + _data1;
}