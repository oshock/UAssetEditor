﻿using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Serilog;
using UAssetEditor;
using UAssetEditor.Unreal.Properties.Types;
using UAssetEditor.Utils;
using UnrealExtractor;
using UnrealExtractor.Binary;
using UnrealExtractor.Compression;
using UnrealExtractor.Encryption.Aes;
using UnrealExtractor.Unreal.Misc;
using UnrealExtractor.Unreal.Readers.IoStore;
using UnrealExtractor.Utils;

Logger.StartLogger();

// Create system
var system = new UnrealFileSystem(@"C:\Fortnite\FortniteGame\Content\Paks");

// Add aes keys
system.AesKeys.Add(new FGuid(), new FAesKey("0xEF7CC91D735CC2F5316477F780026CD7B2226600A001168B6CB062D7EA9D3121"));

// Mount containers
system.Initialize();

// Initialize Oodle
Oodle.Initialize("oo2core_9_win64.dll");

// Read the package
if (!system.TryRead("FortniteGame/Content/Balance/DefaultGameDataCosmetics.uasset", out var data))
    throw new KeyNotFoundException("Unable to read package");
    
// Read the asset from file path 
var uasset = new ZenAsset(data);

// Global .utoc path (required for class identification)
var globalToc = system.Containers.First(x => x.Reader is IoGlobalReader);
uasset.Initialize((IoGlobalReader)globalToc.Reader);

// Loading mappings (required for unversioned assets)
uasset.LoadMappings("++Fortnite+Release-32.10-CL-37958378-Windows_oo.usmap");

// Start a stopwatch
var sw = Stopwatch.StartNew();

// Read everything
uasset.ReadAll();

// Write stats
sw.Stop();
Console.WriteLine($"\nRead all in {sw.ElapsedMilliseconds}ms.\n");

// Serialize the asset into json
File.WriteAllText("output.json", uasset.ToJsonString());

if (File.Exists("DefaultGameDataCosmetics.uasset"))
    File.Delete("DefaultGameDataCosmetics.uasset");


var softObject = SoftObjectProperty.Create("/Game/Owens_Cool_Path/Owens_Cool_Object.Owens_Cool_Object");

var characterParts = uasset.Properties["DefaultGameDataCosmetics"]["DefaultCharacterParts"]!;
characterParts.Value!.As<ArrayProperty>().AddItem(softObject);

var writer = new Writer("DefaultGameDataCosmetics.uasset");
uasset.WriteAll(writer);
writer.Close();

var testAsset = new ZenAsset("DefaultGameDataCosmetics.uasset");
testAsset.Initialize((IoGlobalReader)globalToc.Reader);
testAsset.LoadMappings("++Fortnite+Release-32.10-CL-37958378-Windows_oo.usmap");
testAsset.ReadAll();

Console.ReadKey();
Console.ReadKey();