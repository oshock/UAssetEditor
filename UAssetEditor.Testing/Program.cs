using System.Diagnostics;
using UAssetEditor;
using UAssetEditor.Properties;
using Usmap.NET;

var stopwatch = Stopwatch.StartNew();
var uasset = new UAsset(@"C:\Users\owen\Downloads\FModel\Output\Exports\FortniteGame\Content\Athena\Items\Cosmetics\Characters\CID_028_Athena_Commando_F.uasset");
uasset.Initialize(@"C:\Program Files\Epic Games\Fortnite\FortniteGame\Content\Paks\global.utoc");
uasset.LoadMappings(@"C:\Users\owen\Downloads\FModel\Output\.data\++Fortnite+Release-26.20-CL-28096793-Android_oo.usmap");
uasset.ReadAll();
stopwatch.Stop();
Console.WriteLine($"Done in {stopwatch.ElapsedMilliseconds}ms!");