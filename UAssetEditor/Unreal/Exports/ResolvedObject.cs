using System.Data;
using Serilog;
using UAssetEditor.Unreal.Assets;
using UAssetEditor.Unreal.Names;
using UAssetEditor.Unreal.Readers.IoStore;
using UAssetEditor.Utils;

namespace UAssetEditor.Unreal.Exports;

public class ResolvedObject
{
    public Asset Package;
    public int ExportIndex;

    // Reload
    public void ReloadPackage(UnrealFileSystem? system)
    {
        if (system == null)
            throw new NoNullAllowedException("Cannot reload package because unreal file system is null.");

        if (!system.TryExtractAsset(Package.Entry.Path, out var pkg))
            throw new KeyNotFoundException($"Could not extract asset '{Package.Entry.Path}'");
        
        Package = pkg!;
        Package.ReadHeader();
    }

    public virtual FName Name => new();
    public virtual ResolvedObject? Outer => null;
    public virtual ResolvedObject? Class => null;
    
    public ResolvedObject(Asset package, int exportIndex = -1)
    {
        Package = package;
        ExportIndex = exportIndex;
    }
}

public sealed class ResolvedExportObject : ResolvedObject
{
    public FExportMapEntry? ExportMapEntry { get; set; }
    public UObject? Object { get; set; }
    public override FName Name => new(Object?.Name ?? "None");
    
    public ResolvedExportObject(Asset asset, int exportIndex) : base(asset, exportIndex)
    {
        Load();
    }

    public void Load()
    {
        var zen = (ZenAsset)Package;
        var map = zen.ExportMap;
        if (ExportIndex >= map.Length)
            return;
        
        ExportMapEntry = map[ExportIndex];
        Object = zen.Exports[ExportIndex];
    }

    public UObject? GetObject()
    {
        Load();
        return Object;
    }
}

public class ResolvedScriptObject : ResolvedObject
{
    public FScriptObjectEntry ScriptImport;

    public ResolvedScriptObject(FScriptObjectEntry scriptImport, ZenAsset package) : base(package)
    {
        ScriptImport = scriptImport;
    }

    public override FName Name => ScriptImport.GetObjectName(Package.As<ZenAsset>().GlobalData 
                                                             ?? throw new NoNullAllowedException("Global Data cannot be null."));
    public override ResolvedObject? Outer => Package.As<ZenAsset>().ResolveObjectIndex(ScriptImport.OuterIndex);
    public override ResolvedObject? Class => null;
}