using System.Data;
using System.Diagnostics;
using UAssetEditor;
using UAssetEditor.Unreal.Properties.Types;
using UAssetEditor.Binary;
using UAssetEditor.Compression;
using UAssetEditor.Encryption.Aes;
using UAssetEditor.Unreal.Assets;
using UAssetEditor.Unreal.Misc;

Logger.StartLogger();

// Create system
var system = new UnrealFileSystem(@"C:\Fortnite\FortniteGame\Content\Paks");

// Add aes keys
system.AesKeys.Add(new FGuid(), new FAesKey("0xEF7CC91D735CC2F5316477F780026CD7B2226600A001168B6CB062D7EA9D3121"));

// Start a stopwatch
var sw1 = Stopwatch.StartNew();

// Mount containers
system.Initialize();

// Write stats
sw1.Stop();
Console.WriteLine($"\nRead all in {sw1.ElapsedMilliseconds}ms.\n");

// Load mappings
system.LoadMappings("++Fortnite+Release-33.11-CL-38773622-Windows_oo.usmap", "oo2core_9_win64.dll");

// Initialize Oodle
Oodle.Initialize("oo2core_9_win64.dll");

// Extract the asset
if (!system.TryExtractAsset(
        "FortniteGame/Plugins/GameFeatures/BRCosmetics/Content/Athena/Items/Cosmetics/Characters/CID_028_Athena_Commando_F.uasset",
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
var text = asset["CID_028_Athena_Commando_F"]?["ItemName"]?.GetValue<TextProperty>();

if (text?.Value == null)
    throw new NoNullAllowedException("Could not get ItemName property!");

// Set new FText value
text.Value.Text = "Very cool item!";

// Create a writer with the file "CID_028_Athena_Commando_F.uasset"
var writer = new Writer("CID_028_Athena_Commando_F.uasset");

// Serialize the asset and dispose the writer
asset.WriteAll(writer);
writer.Close();

// Create a new ZenAsset with the asset with just serialized
var testAsset = new ZenAsset("CID_028_Athena_Commando_F.uasset");

// Set the GlobalReader instance
var globalToc = system.GetGlobalReader();
testAsset.Initialize(globalToc!);

// Set mappings
testAsset.Mappings = system.Mappings;

// Test if it reads our asset properly
testAsset.ReadAll();

Console.ReadKey();