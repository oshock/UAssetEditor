using System.Data;
using System.Diagnostics;
using System.IO.Enumeration;
using UAssetEditor;
using UAssetEditor.Unreal.Properties.Types;
using UAssetEditor.Binary;
using UAssetEditor.Compression;
using UAssetEditor.Encryption.Aes;
using UAssetEditor.Unreal.Assets;
using UAssetEditor.Unreal.Misc;
using UAssetEditor.Unreal.Names;
using UAssetEditor.Unreal.Readers.IoStore;
using UAssetEditor.Unreal.Versioning;
using UAssetEditor.Utils;
using UsmapDotNet;

Logger.StartLogger();

var globalData =
    IoGlobalReader.InitializeGlobalData(@"B:\FN Versions\14.40\FortniteGame\Content\Paks\global.utoc", EGame.GAME_UE4_26);

var zen = new ZenAsset(
    @"C:\Users\Owens\Documents\FModel\Output\Exports\WID_Sniper_BoltAction_Scope_Athena_VR_Ore_T03.uasset");
zen.Game = EGame.GAME_UE4_26;
zen.GlobalData = globalData;
zen.Mappings = Usmap.Parse(@"C:\Users\Owens\Documents\FModel\Output\.data\++Fortnite+Release-14.40-CL-14550713-Windows_oo.usmap", new UsmapOptions
{
    Oodle = new OodleDotNet.Oodle("oo2core_9_win64.dll"),
    SaveNames = false
});
zen.ReadAll();

var latestGlobalToc = IoGlobalReader.InitializeGlobalData(@"S:\Fortnite\FortniteGame\Content\Paks\global.utoc", EGame.GAME_UE5_LATEST);
var latestMappings = Usmap.Parse("++Fortnite+Release-39.51-CL-51287198_zs.usmap", new UsmapOptions
{
    Oodle = new OodleDotNet.Oodle("oo2core_9_win64.dll"),
    SaveNames = false
});

zen.GlobalData = latestGlobalToc;
zen.Mappings = latestMappings;
zen.Game = EGame.GAME_UE5_LATEST;

// Create a writer with the file "CID_028_Athena_Commando_F.uasset"
var writer = new Writer("WID_Sniper_BoltAction_Scope_Athena_VR_Ore_T03.uasset");

// Serialize the asset and dispose the writer
zen.WriteAll(writer);
writer.Close();

// Create a new ZenAsset with the asset with just serialized
var testAsset = new ZenAsset("WID_Sniper_BoltAction_Scope_Athena_VR_Ore_T03.uasset");

// Set the GlobalReader instance
testAsset.Initialize(latestGlobalToc);

// Set mappings
testAsset.Game = EGame.GAME_UE5_LATEST;
testAsset.Mappings = latestMappings;

// Test if it reads our asset properly
testAsset.ReadAll();
return;
/*
// Initialize Oodle (FIRST)
Oodle.Initialize("oo2core_9_win64.dll");

// Create system
var system = new UnrealFileSystem(@"S:\Fortnite\FortniteGame\Content\Paks");

// Add aes keys
system.AesKeys.Add(new FGuid(), new FAesKey("0x69385B0781311449AC9FD56B70C8EE9FD0EF062FD55FF8E28E0AE45C22AE2A1A"));

// Start a stopwatch
var sw1 = Stopwatch.StartNew();

// Mount containers
system.Initialize();

// Write stats
sw1.Stop();
Console.WriteLine($"\nMounted {system.Containers.Count} containers(s) in {sw1.ElapsedMilliseconds}ms.\n");

// Load mappings
system.LoadMappings("++Fortnite+Release-39.51-CL-51287198_zs.usmap", "oo2core_9_win64.dll");

// Extract the asset
if (!system.TryExtractAsset(
        "FortniteGame/Content/Balance/DefaultGameDataCosmetics.uasset",
        out var asset))
    throw new KeyNotFoundException("Unable to find asset.");

// Start a stopwatch
var sw = Stopwatch.StartNew();

// Read everything
asset!.ReadAll();

// Write stats
sw.Stop();
Console.WriteLine($"\nRead all in {sw.ElapsedMilliseconds}ms.\n");

var json = asset.ToString(); // Convert to Json String
File.WriteAllText("CID_028_Athena_Commando_F.json", json);

// Get ItemName Property
var export = asset["DefaultGameDataCosmetics"]?["RandomCharacters"]?.GetValue<ArrayProperty>();

if (export?.Value == null)
    throw new NoNullAllowedException("Could not get ItemName property!");

// Set new FText value
var name = export.Value[0].As<StructProperty>().Holder.GetPropertyValue<NameProperty>("PrimaryAssetName");
name.Value = new FName("CID_028");

// Create a writer with the file "CID_028_Athena_Commando_F.uasset"
var writer = new Writer("DefaultGameDataCosmetics.uasset");

// Serialize the asset and dispose the writer
asset.WriteAll(writer);
writer.Close();

// Create a new ZenAsset with the asset with just serialized
var testAsset = new ZenAsset("DefaultGameDataCosmetics.uasset");

// Set the GlobalReader instance
var globalToc = system.GetGlobalReader();
testAsset.Initialize(globalToc!);

// Set mappings
testAsset.Mappings = system.Mappings;

// Test if it reads our asset properly
testAsset.ReadAll();
*/

Console.ReadKey();