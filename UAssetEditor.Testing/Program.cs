using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Serilog;
using UnrealExtractor;
using UnrealExtractor.Encryption.Aes;
using UnrealExtractor.Unreal.Misc;

/*// Read the asset from file path 
var uasset = new ZenAsset("DefaultGameDataCosmetics.uasset");

// Global .utoc path (required for class identification)
var globalToc = @"C:\Fortnite\FortniteGame\Content\Paks\global.utoc";
uasset.Initialize(globalToc);

// Loading mappings (required for unversioned assets)
uasset.LoadMappings("++Fortnite+Release-31.41-CL-37324991-Windows_oo.usmap");

// Start a stopwatch
var sw = Stopwatch.StartNew();

// Read everything
uasset.ReadAll();

// Write stats
sw.Stop();
Console.WriteLine($"\nRead all in {sw.ElapsedMilliseconds}ms.\n");

// Serialize the asset into json
File.WriteAllText("output.json", uasset.ToJsonString());
Console.ReadKey();*/

Logger.StartLogger();


// Create system
var system = new UnrealFileSystem(@"C:\Fortnite\FortniteGame\Content\Paks");

// Add aes keys
system.AesKeys.Add(new FGuid(), new FAesKey("0xEF7CC91D735CC2F5316477F780026CD7B2226600A001168B6CB062D7EA9D3121"));

// Mount containers
system.Initialize();

// Read and output to file
if (system.TryRead("FortniteGame/Content/Balance/DefaultDataCosmetics.uasset", out var data))
    File.WriteAllBytes("DefaultDataCosmetics.uasset", data);

Console.ReadKey();
Console.ReadKey();