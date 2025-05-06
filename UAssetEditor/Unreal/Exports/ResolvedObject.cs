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
    public UObject? Object { get; set; }
    public override FName Name => new(Object?.Name ?? "None");
    
    public ResolvedExportObject(Asset asset, int exportIndex) : base(asset, exportIndex)
    {
        Load();
    }

    public void Load()
    {
        if (Package.Exports.Length > ExportIndex)
            Object = Package.Exports[ExportIndex];
        else
        {
            throw new ArgumentOutOfRangeException(
                $"Export {ExportIndex} in package '{Package.Name}' can not be referenced because package only contains {Package.Exports.Length} export(s)");
        }
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