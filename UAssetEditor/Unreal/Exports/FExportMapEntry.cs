using UAssetEditor.Binary;
using UAssetEditor.Names;
using UAssetEditor.Summaries;

namespace UAssetEditor.Unreal.Exports;

public struct FExportMapEntry
{
    public const int Size = 72;

    public ulong CookedSerialOffset;
    public ulong CookedSerialSize;
    public FMappedName ObjectName;
    public ulong OuterIndex;
    public ulong ClassIndex;
    public ulong SuperIndex;
    public ulong TemplateIndex;
    public ulong PublicExportHash;
    public EObjectFlags ObjectFlags;
    public byte FilterFlags; // EExportFilterFlags: client/server flags

    public ZenAsset Asset;
    public string Name;
    public string Class;
    
    public FExportMapEntry(ZenAsset Ar)
    {
        Asset = Ar;
        CookedSerialOffset = Ar.Read<ulong>();
        CookedSerialSize = Ar.Read<ulong>();
        ObjectName = Ar.Read<FMappedName>();
        OuterIndex = Ar.Read<ulong>();
        ClassIndex = Ar.Read<ulong>();
        SuperIndex = Ar.Read<ulong>();
        TemplateIndex = Ar.Read<ulong>();
        PublicExportHash = Ar.Read<ulong>();
        ObjectFlags = Ar.Read<EObjectFlags>();
        FilterFlags = Ar.Read<byte>();
        Ar.Position += 3;
        
        Name = Ar.NameMap[(int)ObjectName.NameIndex];
        Class = Ar.GlobalData.GetScriptName(ClassIndex);
    }

    public bool TryGetProperties(out PropertyContainer? container)
    {
        if (!Asset.Properties.TryGetValue(Name, out var ctn))
        {
            container = null;
            return false;
        }

        container = ctn;
        return true;
    }

    public void Serialize(Writer writer)
    {
        writer.Write(CookedSerialOffset);
        writer.Write(CookedSerialSize);
        writer.Write(ObjectName);
        writer.Write(OuterIndex);
        writer.Write(ClassIndex);
        writer.Write(SuperIndex);
        writer.Write(TemplateIndex);
        writer.Write(PublicExportHash);
        writer.Write(ObjectFlags);
        writer.WriteByte(FilterFlags);
        writer.Position += 3;
    }

    public void SetCookedSerialOffset(ulong offset) => CookedSerialOffset = offset;
    public void SetCookedSerialSize(ulong size) => CookedSerialSize = size;

    public void SetObjectName(string name)
    {
        var i = BaseAsset.ReferenceOrAddString(Asset, name);
        ObjectName = new FMappedName((uint)i, 0);
    }
}