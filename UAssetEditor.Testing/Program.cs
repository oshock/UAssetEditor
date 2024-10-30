using System.Diagnostics;
using UAssetEditor;

// Read the asset from file path 
var uasset = new ZenAsset("DefaultGameDataCosmetics.uasset");

// Global .utoc path (required form class identification)
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
File.WriteAllText("output.json", uasset.ToString());