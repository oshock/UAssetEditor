using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using UAssetEditor;
using UAssetEditor.Misc;

var stopwatch = Stopwatch.StartNew();
var uasset = new ZenAsset(@"CP_Athena_Body_F_GoodMood.uasset");
uasset.Initialize(@"C:\Program Files\Epic Games\Fortnite\FortniteGame\Content\Paks\global.utoc");
uasset.LoadMappings(@"++Fortnite+Release-28.30-CL-31511038-Android_oo.usmap");
uasset.ReadAll();
stopwatch.Stop();
Console.WriteLine($"Read in {stopwatch.ElapsedMilliseconds}ms!");
Console.WriteLine(uasset.ToString());

uasset.Properties["CP_Athena_Body_F_GoodMood"].Add(PropertyUtils.CreateStruct(uasset.Mappings, "MaterialOverrides", new []
{
    new NameValuePair("OverrideIndex (IDK)", 1),
    new NameValuePair("MaterialPath (I forgot the names just for later)", "/Game/Path/To/MaterialOverride/Mat.Mat")
}));