using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using UAssetEditor;
using UAssetEditor.Misc;

var stopwatch = Stopwatch.StartNew();
var uasset = new ZenAsset(@"C:\Users\Owen\Documents\FModel\Output\Exports\FortniteGame\Content\Balance\DefaultGameDataCosmetics.uasset");
uasset.Initialize(@"C:\Fortnite\FortniteGame\Content\Paks\global.utoc");
uasset.LoadMappings(@"C:\Users\Owen\Documents\FModel\Output\.data\++Fortnite+Release-31.40-CL-36874825-Windows_oo.usmap");
uasset.ReadAll();
stopwatch.Stop();
Console.WriteLine($"Read in {stopwatch.ElapsedMilliseconds}ms!");
Console.WriteLine(uasset.ToString());

uasset.Properties["CP_Athena_Body_F_GoodMood"].Add(PropertyUtils.CreateStruct(uasset.Mappings, "MaterialOverrides", new []
{
    new NameValuePair("OverrideIndex (IDK)", 1),
    new NameValuePair("MaterialPath (I forgot the names just for later)", "/Game/Path/To/MaterialOverride/Mat.Mat")
}));