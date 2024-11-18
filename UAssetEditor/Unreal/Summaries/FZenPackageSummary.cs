using System.Runtime.CompilerServices;
using UAssetEditor.Names;
using UnrealExtractor.Binary;

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

    public FZenPackageSummary(ZenAsset Ar)
    {
        bHasVersioningInfo = Ar.Read<uint>();
        HeaderSize = Ar.Read<uint>();
        Name = Ar.Read<FMappedName>();
        PackageFlags = Ar.Read<EPackageFlags>();
        CookedHeaderSize = Ar.Read<uint>();
        ImportedPublicExportHashesOffset = Ar.Read<int>();
        ImportMapOffset = Ar.Read<int>();
        ExportMapOffset = Ar.Read<int>();
        ExportBundleEntriesOffset = Ar.Read<int>();
        DependencyBundleHeadersOffset = Ar.Read<int>();
        DependencyBundleEntriesOffset = Ar.Read<int>();
        ImportedPackageNamesOffset = Ar.Read<int>();
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