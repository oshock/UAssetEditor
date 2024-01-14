using System.Diagnostics;
using Newtonsoft.Json;
using UAssetEditor;

var stopwatch = Stopwatch.StartNew();
var uasset = new ZenAsset(@"CP_Athena_Body_F_GoodMood.uasset");
uasset.Initialize(@"C:\Program Files\Epic Games\Fortnite\FortniteGame\Content\Paks\global.utoc");
uasset.LoadMappings(@"++Fortnite+Release-28.01-CL-30106568-Android_oo.usmap");
uasset.ReadAll();
stopwatch.Stop();
Console.WriteLine($"Read in {stopwatch.ElapsedMilliseconds}ms!");
Console.WriteLine(uasset.ToString());