using System.Data;
using UAssetEditor.Unreal.Assets;
using UAssetEditor.Unreal.Names;
using UAssetEditor.Unreal.Readers.IoStore;
using UAssetEditor.Utils;

namespace UAssetEditor.Unreal.Exports;

public class ResolvedObject
{
    public Asset Package;
    public int ExportIndex;

    public virtual FName Name => new("None");
    public virtual ResolvedObject? Outer => null;
    public virtual ResolvedObject? Class => null;
    
    public ResolvedObject(Asset package, int exportIndex = -1)
    {
        Package = package;
        ExportIndex = exportIndex;
    }
}

public class ResolvedExportObject : ResolvedObject
{
    public virtual UObject Object { get; set; }

    public ResolvedExportObject(Asset asset, int exportIndex) : base(asset, exportIndex)
    {
        Object = asset.Exports[exportIndex];
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