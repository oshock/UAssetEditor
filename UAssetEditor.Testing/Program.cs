using System.Diagnostics;
using Newtonsoft.Json;
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

// Load mappings
system.LoadMappings("++Fortnite+Release-32.11-CL-38202817-Windows_oo.usmap");

// Initialize Oodle
Oodle.Initialize("oo2core_9_win64.dll");

// Extract the asset
if (!system.TryExtractAsset("FortniteGame/Content/Balance/DefaultGameDataCosmetics.uasset", out var asset))
    throw new KeyNotFoundException("Unable to find asset.");

var uasset = (ZenAsset)asset!;

// Global .utoc path (required for class identification)
var globalToc = system.GetGlobalReader();
uasset.Initialize(globalToc!);

// Start a stopwatch
var sw = Stopwatch.StartNew();

// Read everything
uasset.ReadAll();

// Write stats
sw.Stop();
Console.WriteLine($"\nRead all in {sw.ElapsedMilliseconds}ms.\n");

if (File.Exists("Character_BadBear.uasset"))
    File.Delete("Character_BadBear.uasset");

var materialArray = (ArrayProperty)uasset["DefaultGameDataCosmetics"]["DefaultCharacterParts"].Value;
materialArray.AddItem(SoftObjectProperty.Create("/Game/Owen.Owen"));

var writer = new Writer("Character_BadBear.uasset");
uasset.WriteAll(writer);
writer.Close();

var testAsset = new ZenAsset("Character_BadBear.uasset");
testAsset.Initialize(globalToc!);
testAsset.Mappings = system.Mappings;
testAsset.ReadAll();

Console.ReadKey();
Console.ReadKey();