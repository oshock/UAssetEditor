using System.Diagnostics;
using UAssetEditor;

var uasset = new UAsset(@"C:\Users\owens\Documents\Output\Exports\FortniteGame\Content\Balance\DefaultGameDataCosmetics.uasset");
var stopwatch = Stopwatch.StartNew();
uasset.ReadAll();
stopwatch.Stop();
Console.WriteLine($"Done in {stopwatch.ElapsedMilliseconds}ms!");