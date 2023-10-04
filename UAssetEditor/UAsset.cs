using System.Collections;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text;
using UAssetEditor.IoStore;
using UAssetEditor.Properties;
using Usmap.NET;

namespace UAssetEditor;

public class UAsset : Reader
{
    public string Name { get; set; }
    public IoGlobalReader GlobalData;

    public NameMapContainer NameMap;
    public ulong[] ImportedPublicExportHashes;
    public ulong[] ImportMap;
    public FExportMapEntry[] ExportMap;
    public FExportBundleEntry[] ExportBundleEntries;

    public Dictionary<string, List<UProperty>> Properties = new();

    public void LoadMappings(string path)
    {
	    Mappings = new Usmap.NET.Usmap(path, new UsmapOptions
	    {
		    OodlePath = "oo2core_9_win64.dll",
		    SaveNames = false
	    });
    }
    
    public UAsset(byte[] data) : base(data, null)
    { }
    
    public UAsset(string path) : this(File.ReadAllBytes(path))
    { }

    /// <summary>
    /// Initialize the global objects required to read this package.
    /// </summary>
    /// <param name="globalContainerPath"></param>
    public void Initialize(string globalContainerPath)
    {
	    GlobalData = new IoGlobalReader(globalContainerPath);
    }

    /// <summary>
    /// Read the entirety of this asset
    /// </summary>
    public void ReadAll()
    {
	    var headerSize = Position = ReadHeader();
	    foreach (var entry in ExportBundleEntries)
	    {
		    if (entry.CommandType != EExportCommandType.ExportCommandType_Serialize)
			    continue;
		    
		    var export = ExportMap[entry.LocalExportIndex];
		    var name = NameMap[(int)export.ObjectName.NameIndex];
		    var @class = GlobalData.GlobalNameMap[(int)GlobalData.ScriptObjectEntriesMap[export.ClassIndex].ObjectName.NameIndex]; // TODO make this a method

		    Position = headerSize + (long)export.CookedSerialOffset;
		    Properties.Add(name, ReadProperties(@class));
	    }
    }

    public uint ReadHeader()
    {
        var summary = new FZenPackageSummary(this);
        NameMap = ReadNameMap(this);
        Name = NameMap[(int)summary.Name.NameIndex];

        // Not needed
        //Position += (long)Read<ulong>(); // Skip BulkDataMap
        Position = summary.ImportedPublicExportHashesOffset;
        ImportedPublicExportHashes =
            ReadArray<ulong>((summary.ImportMapOffset - summary.ImportedPublicExportHashesOffset) / sizeof(ulong));

        Position = summary.ImportMapOffset;
        ImportMap = ReadArray<ulong>((summary.ExportMapOffset - summary.ImportMapOffset) / sizeof(ulong));

        Position = summary.ExportMapOffset;
        ExportMap = ReadArray(() => new FExportMapEntry(this),
	        (summary.ExportBundleEntriesOffset - summary.ExportMapOffset) / FExportMapEntry.Size);
        ExportBundleEntries = ReadArray<FExportBundleEntry>(ExportMap.Length * (byte)EExportCommandType.ExportCommandType_Count);

        return summary.HeaderSize;
    }

    public static NameMapContainer ReadNameMap(Reader reader)
    {
        var count = reader.Read<int>();
        switch (count)
        {
            case < 0:
                throw new IndexOutOfRangeException($"Name map cannot have a length of {count}!");
            case 0:
                return default;
        }

        var numBytes = reader.Read<uint>();
        var hashVersion = reader.Read<ulong>();

        var hashes = new ulong[count];
        for (int i = 0; i < hashes.Length; i++) 
            hashes[i] = reader.Read<ulong>();
        
        var headers = new byte[count];
        for (int i = 0; i < headers.Length; i++)
        {
            reader.Position++; // skip utf-16 check
            headers[i] = reader.ReadByte();
        }

        var strings = headers.Select(t => Encoding.UTF8.GetString(reader.ReadBytes(t))).ToList();
        return new NameMapContainer
        {
            NumBytes = numBytes,
            HashVersion = hashVersion,
            Hashes = hashes,
            Strings = strings
        };
    }

    // https://github.com/EpicGames/UnrealEngine/blob/a3cb3d8fdec1fc32f071ae7d22250f33f80b21c4/Engine/Source/Runtime/CoreUObject/Private/Serialization/UnversionedPropertySerialization.cpp#L528
    public List<UProperty> ReadProperties(string type)
    {
	    var props = new List<UProperty>();
	    bool bHasNonZeroValues;

	    var frags = new List<FFragment>();
	    var zeroMaskNum = 0U;
	    var unmaskedNum = 0U;
	    do
	    {
		    var packed = Read<ushort>();
		    frags.Add(new FFragment(packed));

		    var valueNum = frags.Last().ValueNum;
		    if (frags.Last().bHasAnyZeroes)
			    zeroMaskNum += valueNum;
		    else
			    unmaskedNum += valueNum;
	    } while (!frags.Last().bIsLast);

	    BitArray? zeroMask = null;
	    if (zeroMaskNum > 0)
	    {
		    if (zeroMaskNum <= 8)
		    {
			    var @int = ReadByte();
			    zeroMask = new BitArray(new[] { @int });
		    }
		    else if (zeroMaskNum <= 16)
		    {
			    var @int = Read<ushort>();
			    zeroMask = new BitArray(new[] { (int)@int });
		    }
		    else
		    {
			    var data = new int[(zeroMaskNum + 32 - 1) / 32];
			    for (var idx = 0; idx < data.Length; idx++)
				    data[idx] = Read<int>();
			    zeroMask = new BitArray(data);
		    }
	    }

	    var falseFound = false;
	    if (zeroMask != null)
	    {
		    foreach (var bit in zeroMask)
			    falseFound &= !(bool)bit;
	    }

	    bHasNonZeroValues = unmaskedNum > 0 || falseFound;

	    if (Mappings == null)
		    throw new NoNullAllowedException("Mappings cannot be null if properties are to be read!");
	    
	    var schema = Mappings?.Schemas.First(x => x.Name == type);
	    if (schema == null)
		    throw new NoNullAllowedException($"Cannot find '{type}' in mappings. Unable to parse data!");
	    
	    var schemaIndex = 0;
	    var zeroMaskIndex = 0;

	    if (bHasNonZeroValues)
		    schemaIndex += frags.First().SkipNum;
	    
	    foreach (var frag in frags)
	    {
		    var currentRemainingValues = frag.ValueNum;
		    if (frag.bHasAnyZeroes)
			    zeroMaskIndex++;

		    do
		    {
			    // TODO add default values if zero
			    if (!frag.bHasAnyZeroes || !zeroMask.Get(zeroMaskIndex))
			    {
				    var prop = schema.Value.Properties.ToList().Find(x => x.SchemaIdx == schemaIndex);
				    props.Add(new UProperty
				    {
					    Type = prop.Name,
					    Value =	AbstractProperty.ReadProperty(prop.Name, this, prop.Data, this)
				    });
			    }
                
			    schemaIndex++;
			    currentRemainingValues--;
		    } while (currentRemainingValues > 0);

		    schemaIndex += frag.SkipNum;
	    }

	    return props;
    }
}

public struct NameMapContainer
{
    public uint NumBytes;
    public ulong HashVersion;
    public ulong[] Hashes;
    public List<string> Strings;

    public string this[int nameIndex] => Strings[nameIndex];
}

public readonly struct FExportMapEntry
{
	public const int Size = 72;

	public readonly ulong CookedSerialOffset;
	public readonly ulong CookedSerialSize;
	public readonly FMappedName ObjectName;
	public readonly ulong OuterIndex;
	public readonly ulong ClassIndex;
	public readonly ulong SuperIndex;
	public readonly ulong TemplateIndex;
	public readonly ulong PublicExportHash;
	public readonly EObjectFlags ObjectFlags;
	public readonly byte FilterFlags; // EExportFilterFlags: client/server flags

	public FExportMapEntry(UAsset Ar)
	{
		var start = Ar.Position;
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
		Ar.Position = start + Size;
	}
}

[Flags]
public enum EObjectFlags
{
	// Do not add new flags unless they truly belong here. There are alternatives.
	// if you change any the bit of any of the RF_Load flags, then you will need legacy serialization
	RF_NoFlags = 0x00000000,

	///< No flags, used to avoid a cast

	// This first group of flags mostly has to do with what kind of object it is. Other than transient, these are the persistent object flags.
	// The garbage collector also tends to look at these.
	RF_Public = 0x00000001,

	///< Object is visible outside its package.
	RF_Standalone = 0x00000002,

	///< Keep object around for editing even if unreferenced.
	RF_MarkAsNative = 0x00000004,

	///< Object (UField) will be marked as native on construction (DO NOT USE THIS FLAG in HasAnyFlags() etc)
	RF_Transactional = 0x00000008,

	///< Object is transactional.
	RF_ClassDefaultObject = 0x00000010,

	///< This object is its class's default object
	RF_ArchetypeObject = 0x00000020,

	///< This object is a template for another object - treat like a class default object
	RF_Transient = 0x00000040,

	///< Don't save object.

	// This group of flags is primarily concerned with garbage collection.
	RF_MarkAsRootSet = 0x00000080,

	///< Object will be marked as root set on construction and not be garbage collected, even if unreferenced (DO NOT USE THIS FLAG in HasAnyFlags() etc)
	RF_TagGarbageTemp = 0x00000100,

	///< This is a temp user flag for various utilities that need to use the garbage collector. The garbage collector itself does not interpret it.

	// The group of flags tracks the stages of the lifetime of a uobject
	RF_NeedInitialization = 0x00000200,

	///< This object has not completed its initialization process. Cleared when ~FObjectInitializer completes
	RF_NeedLoad = 0x00000400,

	///< During load, indicates object needs loading.
	RF_KeepForCooker = 0x00000800,

	///< Keep this object during garbage collection because it's still being used by the cooker
	RF_NeedPostLoad = 0x00001000,

	///< Object needs to be postloaded.
	RF_NeedPostLoadSubobjects = 0x00002000,

	///< During load, indicates that the object still needs to instance subobjects and fixup serialized component references
	RF_NewerVersionExists = 0x00004000,

	///< Object has been consigned to oblivion due to its owner package being reloaded, and a newer version currently exists
	RF_BeginDestroyed = 0x00008000,

	///< BeginDestroy has been called on the object.
	RF_FinishDestroyed = 0x00010000,

	///< FinishDestroy has been called on the object.

	// Misc. Flags
	RF_BeingRegenerated = 0x00020000,

	///< Flagged on UObjects that are used to create UClasses (e.g. Blueprints) while they are regenerating their UClass on load (See FLinkerLoad::CreateExport())
	RF_DefaultSubObject = 0x00040000,

	///< Flagged on subobjects that are defaults
	RF_WasLoaded = 0x00080000,

	///< Flagged on UObjects that were loaded
	RF_TextExportTransient = 0x00100000,

	///< Do not export object to text form (e.g. copy/paste). Generally used for sub-objects that can be regenerated from data in their parent object.
	RF_LoadCompleted = 0x00200000,

	///< Object has been completely serialized by linkerload at least once. DO NOT USE THIS FLAG, It should be replaced with RF_WasLoaded.
	RF_InheritableComponentTemplate = 0x00400000,

	///< Archetype of the object can be in its super class
	RF_DuplicateTransient = 0x00800000,

	///< Object should not be included in any type of duplication (copy/paste, binary duplication, etc.)
	RF_StrongRefOnFrame = 0x01000000,

	///< References to this object from persistent function frame are handled as strong ones.
	RF_NonPIEDuplicateTransient = 0x02000000,

	///< Object should not be included for duplication unless it's being duplicated for a PIE session
	RF_Dynamic =
		0x04000000, // Field Only. Dynamic field - doesn't get constructed during static initialization, can be constructed multiple times
	RF_WillBeLoaded = 0x08000000, // This object was constructed during load and will be loaded shortly
}

public enum EExportCommandType : uint
{
	ExportCommandType_Create,
	ExportCommandType_Serialize,
	ExportCommandType_Count
};
    
public readonly struct FExportBundleEntry
{
	public readonly uint LocalExportIndex;
	public readonly EExportCommandType CommandType;
}