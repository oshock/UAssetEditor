﻿using System.Diagnostics;
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

var json = asset.ToString();
Console.WriteLine(json);
File.WriteAllText("output.json", json);

// Write stats
sw.Stop();
Console.WriteLine($"\nRead all in {sw.ElapsedMilliseconds}ms.\n");

uasset.ToString();

if (File.Exists("DefaultGameDataCosmetics.uasset"))
    File.Delete("DefaultGameDataCosmetics.uasset");

var text = (TextProperty)uasset["CID_028_Athena_Commando_F"]["ItemName"].Value;
text.Value.Text = "Very cool item!";

var writer = new Writer("CID_028_Athena_Commando_F.uasset");
uasset.WriteAll(writer);
writer.Close();

var testAsset = new ZenAsset("CID_028_Athena_Commando_F.uasset");
testAsset.Initialize(globalToc!);
testAsset.Mappings = system.Mappings;
testAsset.ReadAll();

Console.ReadKey();
Console.ReadKey();