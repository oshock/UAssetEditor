using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Serilog;
using UAssetEditor;
using UAssetEditor.Utils;
using UnrealExtractor;
using UnrealExtractor.Compression;
using UnrealExtractor.Encryption.Aes;
using UnrealExtractor.Unreal.Misc;
using UnrealExtractor.Unreal.Readers.IoStore;

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






Console.ReadKey();
Console.ReadKey();