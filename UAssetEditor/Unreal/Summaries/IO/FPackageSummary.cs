using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UAssetEditor.Summaries;
using UAssetEditor.Unreal.Names;

namespace UAssetEditor.Unreal.Summaries.IO;

[StructLayout(LayoutKind.Sequential)]
public struct FPackageSummary
{
    public static int Size => Unsafe.SizeOf<FPackageSummary>();
    
    public FMappedName Name;
    public FMappedName SourceName;
    public EPackageFlags PackageFlags;
    public uint CookedHeaderSize;
    public int NameMapNamesOffset;
    public int NameMapNamesSize;
    public int NameMapHashesOffset;
    public int NameMapHashesSize;
    public int ImportMapOffset;
    public int ExportMapOffset;
    public int ExportBundlesOffset;
    public int GraphDataOffset;
    public int GraphDataSize;
    private int _pad;
}