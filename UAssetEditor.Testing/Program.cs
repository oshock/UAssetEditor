using System.Diagnostics;
using Newtonsoft.Json;
using UAssetEditor;
using UAssetEditor.Properties;
using Usmap.NET;

var stopwatch = Stopwatch.StartNew();
var uasset = new UAsset(@"C:\Users\oshock\Documents\FModel\Output\Exports\CP_028_Athena_Body.uasset");
uasset.Initialize(@"C:\Program Files\Epic Games\Fortnite\FortniteGame\Content\Paks\global.utoc");
uasset.LoadMappings(@"C:\Users\oshock\Documents\FModel\Output\.data\++Fortnite+Release-27.00-CL-29072303_oo.usmap");
uasset.ReadAll();
stopwatch.Stop();
Console.WriteLine($"Read in {stopwatch.ElapsedMilliseconds}ms!");
//Console.WriteLine(JsonConvert.SerializeObject(uasset.Properties, Formatting.Indented));

var writer = new Writer(File.Open("CP_028_Athena_Body EDITED.uasset", FileMode.OpenOrCreate, FileAccess.Write));
uasset.NameMap.Strings[0] = "WOAHHHHH";
uasset.WriteAll(writer);
writer.Close();

var testRead = new UAsset("CP_028_Athena_Body EDITED.uasset");
testRead.Initialize(@"C:\Program Files\Epic Games\Fortnite\FortniteGame\Content\Paks\global.utoc");
testRead.LoadMappings(@"C:\Users\oshock\Documents\FModel\Output\.data\++Fortnite+Release-27.00-CL-29072303_oo.usmap");
testRead.ReadAll();
Console.WriteLine(testRead);
