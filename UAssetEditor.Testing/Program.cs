using System.Diagnostics;
using UAssetEditor;
using UAssetEditor.Binary;
using UAssetEditor.Logging;

Logger.StartLogger("output.log");
var stopwatch = Stopwatch.StartNew();

var uasset = new ZenAsset(@"C:\Users\Owen\Documents\FModel\Output\Exports\FortniteGame\Content\Athena\Heroes\Meshes\Bodies\CP_015_Athena_Body.uasset");
//var uasset = new ZenAsset(@"C:\Users\Owen\Documents\GitHub\UAssetEditor\UAssetEditor.Testing\bin\Debug\net8.0\output.uasset");
uasset.Initialize(@"C:\Fortnite\FortniteGame\Content\Paks\global.utoc");
uasset.LoadMappings(@"C:\Users\Owen\Documents\FModel\Output\.data\++Fortnite+Release-31.40-CL-36874825-Windows_oo.usmap");
uasset.ReadAll();

File.WriteAllText("output.json", uasset.ToString());

var uassetToModify = new ZenAsset(@"C:\Users\Owen\Documents\FModel\Output\Exports\FortniteGame\Content\Athena\Heroes\Meshes\Bodies\CP_136_Athena_Body_M_StreetBasketball.uasset");
uassetToModify.Initialize(@"C:\Fortnite\FortniteGame\Content\Paks\global.utoc");
uassetToModify.LoadMappings(@"C:\Users\Owen\Documents\FModel\Output\.data\++Fortnite+Release-31.40-CL-36874825-Windows_oo.usmap");
uassetToModify.ReadAll();

if (File.Exists("output.uasset"))
    File.Delete("output.uasset");

// Get the 'MaterialOverrides' property in the 'CP_015_Athena_Body' export
var uProperty = uasset.Properties["CP_015_Athena_Body"]["MaterialOverrides"];

// Add to property to the other UAsset
uassetToModify.Properties["CP_136_Athena_Body_M_StreetBasketball"].Add(uProperty!);

var writer = new Writer(File.OpenWrite("output.uasset"));
uassetToModify.WriteAll(writer);
writer.Close();

Console.ReadKey();