using UAssetEditor.Classes;
using UAssetEditor.Unreal.Assets;
using UAssetEditor.Unreal.Exports;

namespace UAssetEditor.Unreal.Objects;

public class FPackageIndex : IUnrealType
{
    [UField]
    public int Index;

    public Asset? Owner;

    private ResolvedObject? _resolvedObject;

    public ResolvedObject? ResolvedObject
    {
        get
        {
            if (_resolvedObject == null)
                Resolve();
            
            return _resolvedObject;
        }
    }

    public void Resolve()
    {
        _resolvedObject = Owner?.ResolvePackageIndex(this);
    }

    public string Text = "None";

    public bool IsExport => Index > 0;
    public bool IsImport => Index < 0;
    public bool IsNull => Index == 0;

    public FPackageIndex(Asset? owner, int index)
    {
        Owner = owner;
        Index = index;
        
        if (IsNull)
            return;

        if (IsExport)
        {
            if (Owner is ZenAsset zen)
            {
                var i = Index - 1;
                if (i < zen.ExportMap.Length)
                {
                    var nameIndex = (int)zen.ExportMap[i].ObjectName.NameIndex;
                    if (nameIndex < zen.NameMap.Length)
                        Text = zen.NameMap[nameIndex];
                }
            }
            else if (Owner is PakAsset pak)
            {
                // TODO
            }
        }
        else if (IsImport)
        {
            // not implemented
        }
    }

    public static FPackageIndex Read(Asset asset)
    {
        var index = asset.Read<int>();
        return new FPackageIndex(asset, index);
    }
}