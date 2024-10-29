using System.Diagnostics;
using UAssetEditor;
using UAssetEditor.Binary;
using UAssetEditor.Logging;

Logger.StartLogger("output.log");
var stopwatch = Stopwatch.StartNew();

/*var uasset = new ZenAsset(@"C:\Users\Owen\Documents\FModel\Output\Exports\FortniteGame\Content\Athena\Heroes\Meshes\Bodies\CP_015_Athena_Body.uasset");
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
writer.Close();*/

/*var testAsset = new ZenAsset(@"WID_Harvest_Pickaxe_Athena_C_T01.uasset");
testAsset.Initialize(@"C:\Fortnite\FortniteGame\Content\Paks\global.utoc");
testAsset.LoadMappings(@"C:\Users\Owen\Documents\FModel\Output\.data\++Fortnite+Release-31.40-CL-36874825-Windows_oo.usmap");
testAsset.ReadAll();*/

var defaultPickAsset = new ZenAsset(@"C:\Users\Owen\Documents\FModel\Output\Exports\FortniteGame\Content\Athena\Items\Weapons\WID_Harvest_Pickaxe_Athena_C_T01.uasset");
defaultPickAsset.Initialize(@"C:\Fortnite\FortniteGame\Content\Paks\global.utoc");
//defaultPickAsset.LoadMappings(@"C:\Users\Owen\Documents\FModel\Output\.data\++Fortnite+Release-31.40-CL-36874825-Windows_oo.usmap");
defaultPickAsset.ReadHeader();

var stellarAxeAsset = new ZenAsset(@"C:\Users\Owen\Documents\FModel\Output\Exports\FortniteGame\Plugins\GameFeatures\BRCosmetics\Content\Athena\Items\Weapons\WID_Harvest_Pickaxe_Celestial.uasset");
stellarAxeAsset.Initialize(@"C:\Fortnite\FortniteGame\Content\Paks\global.utoc");
stellarAxeAsset.LoadMappings(@"C:\Users\Owen\Documents\FModel\Output\.data\++Fortnite+Release-31.40-CL-36874825-Windows_oo.usmap");
stellarAxeAsset.ReadHeader();

var rest = stellarAxeAsset.ReadBytes((int)(stellarAxeAsset.BaseStream.Length - stellarAxeAsset.Position));

// Set the package's path
stellarAxeAsset.Name = defaultPickAsset.Name;

// Rename the property container to match export name
//stellarAxeAsset.SetPropertyContainerKey("WID_Harvest_Pickaxe_Celestial", "WID_Harvest_Pickaxe_Athena_C_T01");

var writer = new Writer(File.OpenWrite("WID_Harvest_Pickaxe_Athena_C_T01.uasset"));
stellarAxeAsset.WriteHeader(writer);
writer.WriteBytes(rest);
writer.Close();

Console.ReadKey();