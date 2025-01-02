using UAssetEditor.Unreal.Names;

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
    public UObject Object;

    public ResolvedExportObject(Asset asset, int exportIndex) : base(asset, exportIndex)
    {
        Object = asset.Exports[exportIndex];
    }
}

// TODO
/*public class ResolvedScriptObject : ResolvedObject
{
    public FScriptObjectEntry ScriptImport;

    public ResolvedScriptObject(FScriptObjectEntry scriptImport, IoPackage package) : base(package)
    {
        ScriptImport = scriptImport;
    }

    public override FName Name => ((IoPackage) Package).CreateFNameFromMappedName(ScriptImport.ObjectName);
    public override ResolvedObject? Outer => ((IoPackage) Package).ResolveObjectIndex(ScriptImport.OuterIndex);
    // This means we'll have UScriptStruct's shown as UClass which is wrong.
    // Unfortunately because the mappings format does not distinguish between classes and structs, there's no other way around :(
    public override ResolvedObject Class => new ResolvedLoadedObject(new UScriptClass("Class"));
    public override Lazy<UObject> Object => new(() => new UScriptClass(Name.Text));
}*/