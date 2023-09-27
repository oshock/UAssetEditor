using System.Diagnostics;
using UAssetEditor;

var stopwatch = Stopwatch.StartNew();
var uasset = new UAsset(@"C:\Users\owen\Downloads\FModel\Output\Exports\FortniteGame\Content\Athena\Heroes\Meshes\Bodies\CP_028_Athena_Body.uasset");
uasset.Initialize(@"C:\Program Files\Epic Games\Fortnite\FortniteGame\Content\Paks\global.utoc");
uasset.LoadMappings(@"C:\Users\owen\Downloads\FModel\Output\.data\++Fortnite+Release-26.20-CL-28096793-Android_oo.usmap");
uasset.ReadAll();
stopwatch.Stop();
Console.WriteLine($"Done in {stopwatch.ElapsedMilliseconds}ms!");