using System.Diagnostics;
using UAssetEditor;
using UAssetEditor.Unreal.Properties.Types;
using UAssetEditor.Binary;
using UAssetEditor.Compression;
using UAssetEditor.Encryption.Aes;
using UAssetEditor.Unreal.Misc;

Logger.StartLogger();

// Create system
var system = new UnrealFileSystem(@"C:\Program Files\Epic Games\Fortnite\FortniteGame\Content\Paks");

// Add aes keys
system.AesKeys.Add(new FGuid(), new FAesKey("0xEF7CC91D735CC2F5316477F780026CD7B2226600A001168B6CB062D7EA9D3121"));

// Mount containers
system.Initialize();

// Initialize Oodle
Oodle.Initialize("oo2core_9_win64.dll");

// Read the package
if (!system.TryRead("FortniteGame/Plugins/GameFeatures/BRCosmetics/Content/Athena/Items/Cosmetics/Characters/CID_028_Athena_Commando_F.uasset", out var data))
    throw new KeyNotFoundException("Unable to read package");

// Read the asset from file path 
var uasset = new ZenAsset(data);

// Global .utoc path (required for class identification)
var globalToc = system.GetGlobalReader();
uasset.Initialize(globalToc!);

// Loading mappings (required for unversioned assets)
uasset.LoadMappings("++Fortnite+Release-32.11-CL-38202817-Windows_oo.usmap");

// Start a stopwatch
var sw = Stopwatch.StartNew();

// Read everything
uasset.ReadAll();

// Write stats
sw.Stop();
Console.WriteLine($"\nRead all in {sw.ElapsedMilliseconds}ms.\n");

// Serialize the asset into json
// Broken ATM
// File.WriteAllText("output.json", uasset.ToJsonString()); 

if (File.Exists("CP_Athena_Body_F_RenegadeRaiderFire.uasset"))
    File.Delete("CP_Athena_Body_F_RenegadeRaiderFire.uasset");

/*var materialArray = (ArrayProperty)uasset["CP_Athena_Body_F_RenegadeRaiderFire"]["MaterialOverrides"].Value;
var overrideMaterial = materialArray.GetItemAt<StructProperty>(0).Holder.GetPropertyValue<SoftObjectProperty>("OverrideMaterial");
overrideMaterial.Value = "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Skins/TV_21/Materials/F_MED_Commando_Body_TV21.F_MED_Commando_Body_TV21";*/

var writer = new Writer("CP_Athena_Body_F_RenegadeRaiderFire.uasset");
uasset.WriteAll(writer);
writer.Close();

var testAsset = new ZenAsset("CP_Athena_Body_F_RenegadeRaiderFire.uasset");
testAsset.Initialize(globalToc!);
testAsset.LoadMappings("++Fortnite+Release-32.11-CL-38202817-Windows_oo.usmap");
testAsset.ReadAll();

Console.ReadKey();
Console.ReadKey();