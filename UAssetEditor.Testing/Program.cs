using System.Diagnostics;
using UAssetEditor;
using UAssetEditor.Binary;
using UAssetEditor.Properties;

var stopwatch = Stopwatch.StartNew();
var uasset = new ZenAsset(@"CP_Athena_Body_F_GoodMood.uasset");
uasset.Initialize(@"C:\Program Files\Epic Games\Fortnite\FortniteGame\Content\Paks\global.utoc");
uasset.LoadMappings(@"++Fortnite+Release-28.01-CL-30106568-Android_oo.usmap");
uasset.ReadAll();
stopwatch.Stop();
Console.WriteLine($"Read in {stopwatch.ElapsedMilliseconds}ms!");
//Console.WriteLine(JsonConvert.SerializeObject(uasset.Properties, Formatting.Indented));


if (File.Exists("CP_Athena_Body_F_GoodMood EDITED.uasset"))
    File.Delete("CP_Athena_Body_F_GoodMood EDITED.uasset");

var writer = new Writer(File.Open("CP_Athena_Body_F_GoodMood EDITED.uasset", FileMode.Create, FileAccess.Write));
uasset.NameMap.Strings.Add("/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Meshes/F_Med_Soldier_01");
uasset.NameMap.Strings.Add("/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Meshes/F_Med_Soldier_01_Skeleton_AnimBP");
uasset.NameMap.Strings.Add("F_Med_Soldier_01");
uasset.NameMap.Strings.Add("F_Med_Soldier_01_Skeleton_AnimBP_C");
uasset.Properties["CP_Athena_Body_F_GoodMood"]["SkeletalMesh"] = new UProperty
{
    Type = "SoftObjectProperty",
    Value = new SoftObjectProperty
    {
        AssetPathName =
            "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Meshes/F_Med_Soldier_01",
        PackageName = "F_Med_Soldier_01"
    }
};

uasset.Properties["CustomCharacterBodyPartData"]["AnimClass"] = new UProperty
{
    Type = "SoftObjectProperty",
    Value = new SoftObjectProperty
    {
        AssetPathName =
            "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Meshes/F_Med_Soldier_01_Skeleton_AnimBP",
        PackageName = "F_Med_Soldier_01_Skeleton_AnimBP_C"
    }
};
uasset.WriteAll(writer);
writer.Close();

var testRead = new ZenAsset("CP_Athena_Body_F_GoodMood EDITED.uasset");
testRead.Initialize(@"C:\Program Files\Epic Games\Fortnite\FortniteGame\Content\Paks\global.utoc");
testRead.LoadMappings(@"++Fortnite+Release-28.01-CL-30106568-Android_oo.usmap");
testRead.ReadAll();
Console.WriteLine(testRead.ToJson());

// Dont touch just so I can eventually add it to my UAssetEditor

var props = new Dictionary<string, PropertyContainer>
{
    {
        "CustomCharacterBodyPartData",
        asset2.Properties["CustomCharacterBodyPartData"]
    },
    {
        "CP_028_Athena_Body",
        asset2.Properties["CP_028_Athena_Body"]
    },
    {
        "CustomCharacterHeadData",
        asset3.Properties["CustomCharacterHeadData"]
    },
    {
        "F_MED_ASN_Sarah_Head_02_ATH",
        asset3.Properties["F_MED_ASN_Sarah_Head_02_ATH"]
    },
    {
        "CustomCharacterHatData",
        asset4.Properties["CustomCharacterHatData"]
    },
    {
        "Hat_F_Commando_08_V01",
        asset4.Properties["Hat_F_Commando_08_V01"]
    }
};
var exportMap = new[]
{
    asset2.ExportMap[0], // 0 = Body Anim
    asset2.ExportMap[1], // 1 = Body Data
    asset3.ExportMap[0], // 2 = Head Anim
    asset3.ExportMap[1], // 3 = Head Data
    asset4.ExportMap[0], // 4 = Hat Anim
    asset4.ExportMap[1], // 5 = Hat Data
};

for (int i = 0; i < exportMap.Length; i++)
{
    var export = exportMap[i];
    if (!asset.NameMap.Strings.Contains(export.Name))
        asset.NameMap.Strings.Add(export.Name);

    exportMap[i].ObjectName = new FMappedName((uint)asset.NameMap.Strings.FindIndex(x => x == export.Name), 0);
}

var exportBundles = new FExportBundleEntry[]
{
    new()
    {
        CommandType = EExportCommandType.ExportCommandType_Create,
        LocalExportIndex = 0,
    },
    new()
    {
        CommandType = EExportCommandType.ExportCommandType_Create,
        LocalExportIndex = 1,
    },
    new()
    {
        CommandType = EExportCommandType.ExportCommandType_Create,
        LocalExportIndex = 2,
    },
    new()
    {
        CommandType = EExportCommandType.ExportCommandType_Create,
        LocalExportIndex = 3,
    },
    new()
    {
        CommandType = EExportCommandType.ExportCommandType_Create,
        LocalExportIndex = 4,
    },
    new()
    {
        CommandType = EExportCommandType.ExportCommandType_Create,
        LocalExportIndex = 5,
    },
    new()
    {
        CommandType = EExportCommandType.ExportCommandType_Serialize,
        LocalExportIndex = 0,
    },
    new()
    {
        CommandType = EExportCommandType.ExportCommandType_Serialize,
        LocalExportIndex = 1,
    },
    new()
    {
        CommandType = EExportCommandType.ExportCommandType_Serialize,
        LocalExportIndex = 2,
    },
    new()
    {
        CommandType = EExportCommandType.ExportCommandType_Serialize,
        LocalExportIndex = 3,
    },
    new()
    {
        CommandType = EExportCommandType.ExportCommandType_Serialize,
        LocalExportIndex = 4,
    },
    new()
    {
        CommandType = EExportCommandType.ExportCommandType_Serialize,
        LocalExportIndex = 5,
    }
};

var importMap = asset2.ImportMap.ToList();
importMap.AddRange(asset3.ImportMap);
importMap.AddRange(asset4.ImportMap);

var hashes = asset2.ImportedPublicExportHashes.ToList();
hashes.AddRange(asset3.ImportedPublicExportHashes);
hashes.AddRange(asset4.ImportedPublicExportHashes);

asset.ExportMap = exportMap;
asset.ExportBundleEntries = exportBundles;

foreach (var prop in props)
{
    HandleProperties(prop.Value.GetProperties());
}

asset.Properties = props;
asset.ImportMap = importMap.ToArray();
asset.ImportedPublicExportHashes = hashes.ToArray();
    
var writer = new Writer(File.OpenWrite("CP_Athena_Body_F_Fallback.uasset"));
asset.WriteAll(writer);
writer.Close();