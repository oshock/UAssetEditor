using System.Diagnostics;
using UAssetEditor;

var uasset = new UAsset(@"C:\Users\owen\Downloads\FModel\Output\Exports\FortniteGame\Content\Balance\DefaultGameDataCosmetics.uasset");
var stopwatch = Stopwatch.StartNew();
uasset.Initialize(@"C:\Program Files\Epic Games\Fortnite\FortniteGame\Content\Paks\global.utoc");
uasset.LoadMappings(@"C:\Users\owen\Downloads\FModel\Output\.data\++Fortnite+Release-26.20-CL-28096793-Android_oo.usmap");
uasset.ReadAll();
stopwatch.Stop();
Console.WriteLine($"Done in {stopwatch.ElapsedMilliseconds}ms!");