using System.Runtime.InteropServices;
using UAssetEditor.Summaries;
using UAssetEditor.Unreal.Names;

namespace UAssetEditor.Unreal.Summaries.IO;

[StructLayout(LayoutKind.Sequential)]
public readonly struct FPackageSummaryIO
{
    public readonly FMappedName Name;
    public readonly FMappedName SourceName;
    public readonly EPackageFlags PackageFlags;
    public readonly uint CookedHeaderSize;
    public readonly int NameMapNamesOffset;
    public readonly int NameMapNamesSize;
    public readonly int NameMapHashesOffset;
    public readonly int NameMapHashesSize;
    public readonly int ImportMapOffset;
    public readonly int ExportMapOffset;
    public readonly int ExportBundlesOffset;
    public readonly int GraphDataOffset;
    public readonly int GraphDataSize;
    private readonly int _pad;
}