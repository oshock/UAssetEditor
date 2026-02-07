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
using UAssetEditor.Utils;

Logger.StartLogger();

// Initialize Oodle (FIRST)
Oodle.Initialize("oo2core_9_win64.dll");

// Create system
var system = new UnrealFileSystem(@"C:\Program Files\Epic Games\Fortnite\FortniteGame\Content\Paks");

// Add aes keys
system.AesKeys.Add(new FGuid(), new FAesKey("0x69385B0781311449AC9FD56B70C8EE9FD0EF062FD55FF8E28E0AE45C22AE2A1A"));

// Start a stopwatch
var sw1 = Stopwatch.StartNew();

// Mount containers
system.Initialize();

// Print stats
sw1.Stop();
Console.WriteLine($"\nMounted {system.Containers.Count} containers(s) in {sw1.ElapsedMilliseconds}ms.\n");

// Load mappings
system.LoadMappings("++Fortnite+Release-36.00-CL-43214806-Windows_oo.usmap", "oo2core_9_win64.dll");

// Extract the asset
if (!system.TryExtractAsset(
        "FortniteGame/Plugins/GameFeatures/BRCosmetics/Content/Characters/Player/Male/Medium/Bodies/M_MED_WeaveHarbor/Textures/T_WeaveHarbor_Body_D.uasset",
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

Console.ReadKey();