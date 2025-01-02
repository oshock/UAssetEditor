using System.Runtime.CompilerServices;
using UAssetEditor.Unreal.Names;
using UAssetEditor.Binary;
using UAssetEditor.Unreal.Assets;

namespace UAssetEditor.Summaries;

public struct FZenPackageSummary
{
    public static int Size => Unsafe.SizeOf<FZenPackageSummary>();
    
    public uint bHasVersioningInfo;
    public uint HeaderSize;
    public FMappedName Name;
    public EPackageFlags PackageFlags;
    public uint CookedHeaderSize;
    public int ImportedPublicExportHashesOffset;
    public int ImportMapOffset;
    public int ExportMapOffset;
    public int ExportBundleEntriesOffset;
    public int DependencyBundleHeadersOffset = 0;
    public int DependencyBundleEntriesOffset = 0;
    public int ImportedPackageNamesOffset = 0;

    public FZenPackageSummary(ZenAsset reader)
    {
        bHasVersioningInfo = reader.Read<uint>();
        HeaderSize = reader.Read<uint>();
        Name = reader.Read<FMappedName>();
        PackageFlags = reader.Read<EPackageFlags>();
        CookedHeaderSize = reader.Read<uint>();
        ImportedPublicExportHashesOffset = reader.Read<int>();
        ImportMapOffset = reader.Read<int>();
        ExportMapOffset = reader.Read<int>();
        ExportBundleEntriesOffset = reader.Read<int>();
        DependencyBundleHeadersOffset = reader.Read<int>();
        DependencyBundleEntriesOffset = reader.Read<int>();
        ImportedPackageNamesOffset = reader.Read<int>();
    }

    public void Serialize(Writer writer)
    {
        writer.Write(bHasVersioningInfo);
        writer.Write(HeaderSize);
        writer.Write(Name);
        writer.Write(PackageFlags);
        writer.Write(CookedHeaderSize);
        writer.Write(ImportedPublicExportHashesOffset);
        writer.Write(ImportMapOffset);
        writer.Write(ExportMapOffset);
        writer.Write(ExportBundleEntriesOffset);
        writer.Write(DependencyBundleHeadersOffset);
        writer.Write(DependencyBundleEntriesOffset);
        writer.Write(ImportedPackageNamesOffset);
    }
}