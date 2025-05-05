using System.Data;
using System.Diagnostics;
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
var system = new UnrealFileSystem(@"S:\Fortnite\FortniteGame\Content\Paks");

// Add aes keys
system.AesKeys.Add(new FGuid(), new FAesKey("0x17243B0E3E66DA90347F7C4787692505EC5E5285484633D71B09CD6ABB714E9B"));

// Start a stopwatch
var sw1 = Stopwatch.StartNew();

// Mount containers
system.Initialize();

// Write stats
sw1.Stop();
Console.WriteLine($"\nRead all in {sw1.ElapsedMilliseconds}ms.\n");

// Load mappings
system.LoadMappings("++Fortnite+Release-35.00-CL-41994699-Windows_oo.usmap", "oo2core_9_win64.dll");

// Extract the asset
if (!system.TryExtractAsset(
        "FortniteGame/Content/Balance/DefaultGameDataCosmetics.uasset",
        out var asset))
    throw new KeyNotFoundException("Unable to find asset.");

// Start a stopwatch
var sw = Stopwatch.StartNew();

// Read everything
asset!.ReadAll();

// Populate Imported Packages
((ZenAsset)asset).PopulateImportIds();

// Write stats
sw.Stop();
Console.WriteLine($"\nRead all in {sw.ElapsedMilliseconds}ms.\n");

// Get Hero Definition (Testing)
/*var heroDefinition = asset["CID_028_Athena_Commando_F"]?["HeroDefinition"].GetValue<ObjectProperty>();
var obj = heroDefinition?.Value?.ResolvedObject;
if (obj != null)
    Console.WriteLine($"HeroDefinition: '{obj.Name}'");*/

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