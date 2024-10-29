using System.Diagnostics;
using UAssetEditor;
using UAssetEditor.Binary;
using UAssetEditor.Logging;

Logger.StartLogger("output.log");
var stopwatch = Stopwatch.StartNew();

var uasset = new ZenAsset(@"DefaultGameDataCosmetics.uasset");
uasset.Initialize(@"C:\Fortnite\FortniteGame\Content\Paks\global.utoc");
uasset.LoadMappings(@"++Fortnite+Release-31.41-CL-37324991-Windows_oo.usmap");
uasset.ReadAll();

stopwatch.Stop();
Console.WriteLine($"\nRead all in {stopwatch.ElapsedMilliseconds}.\n");

File.WriteAllText("output.json", uasset.ToString());

if (File.Exists("output.uasset"))
    File.Delete("output.uasset");

var writer = new Writer(File.Open("output.uasset", FileMode.OpenOrCreate, FileAccess.ReadWrite));
uasset.WriteAll(writer);

var modifiedAsset = new ZenAsset(writer.ToArray());
modifiedAsset.Initialize(@"C:\Fortnite\FortniteGame\Content\Paks\global.utoc");
modifiedAsset.LoadMappings(@"++Fortnite+Release-31.41-CL-37324991-Windows_oo.usmap");
modifiedAsset.ReadAll();

writer.Close();


Console.ReadKey();