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

// Initialize Oodle (FIRST)
Oodle.Initialize("oo2core_9_win64.dll");

// Create system
var system = new UnrealFileSystem(@"C:\Program Files\Epic Games\Fortnite\FortniteGame\Content\Paks", EGame.GAME_UE5_LATEST);

// Add aes keys
system.AesKeys.Add(new FGuid(), new FAesKey("0x98F7261584C345F9B19402F20110A7A68A48C798FFCE86982F2E8C86F0725CDA"));

// Start a stopwatch
var sw1 = Stopwatch.StartNew();

// Mount containers
system.Initialize();

// Print stats
sw1.Stop();
Console.WriteLine($"\nMounted {system.Containers.Count} containers(s) in {sw1.ElapsedMilliseconds}ms.\n");

// Load mappings
system.LoadMappings("++Fortnite+Release-39.51-CL-51287198_zs.usmap", "oo2core_9_win64.dll");

// Extract the asset
if (!system.TryExtractAsset(
        "FortniteGame/Content/Athena/Items/Weapons/WID_Sniper_Cowboy_Athena_SR.uasset",
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
File.WriteAllText("WID_Sniper_Cowboy_Athena_SR.json", json);

asset.Game = EGame.GAME_UE4_26;
asset.Mappings =
    UnrealFileSystem.LoadMappingsStatic("++Fortnite+Release-14.60-CL-14786821-Windows_oo.usmap", "oo2core_9_win64.dll");

// Create a writer with the file "CID_028_Athena_Commando_F.uasset"
var writer = new Writer("WID_Sniper_Cowboy_Athena_SR.uasset");

// Serialize the asset and dispose the writer
asset.WriteAll(writer);
writer.Close();

// Create a new ZenAsset with the asset with just serialized
var testAsset = new ZenAsset("WID_Sniper_Cowboy_Athena_SR.uasset");

// Set the GlobalReader instance
var globalToc = system.GetGlobalReader();
testAsset.Initialize(globalToc!);

// Set mappings
testAsset.Mappings = asset.Mappings;

// Test if it reads our asset properly
testAsset.ReadAll();

Console.ReadKey();