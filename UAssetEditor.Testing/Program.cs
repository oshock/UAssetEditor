using System.Diagnostics;
using UAssetEditor;
using UAssetEditor.Properties;

var stopwatch = Stopwatch.StartNew();
var uasset = new UAsset(@"CP_028_Athena_Body.uasset");
uasset.Initialize(@"C:\Program Files\Epic Games\Fortnite\FortniteGame\Content\Paks\global.utoc");
uasset.LoadMappings(@"++Fortnite+Release-28.01-CL-30106568-Android_oo.usmap");
uasset.ReadAll();
stopwatch.Stop();
Console.WriteLine($"Read in {stopwatch.ElapsedMilliseconds}ms!");
//Console.WriteLine(JsonConvert.SerializeObject(uasset.Properties, Formatting.Indented));

if (File.Exists("CP_028_Athena_Body EDITED.uasset"))
    File.Delete("CP_028_Athena_Body EDITED.uasset");

var writer = new Writer(File.Open("CP_028_Athena_Body EDITED.uasset", FileMode.Create, FileAccess.Write));
uasset.NameMap.Strings.Add("/Game/Characters/Player/Male/Medium/Bodies/M_MED_InstantGravelNoble/FX/NS_Skin_InstantGravelNoble");
uasset.NameMap.Strings.Add("NS_Skin_InstantGravelNoble");
uasset.Properties["CP_028_Athena_Body"]["MasterSkeletalMeshes"] = new UProperty
{
    Type = "ArrayProperty",
    InnerType = "SoftObjectProperty",
    Value = new List<object>
    {
        new SoftObjectProperty()
        {
                AssetPathName =
                    "/Game/Characters/Player/Male/Medium/Bodies/M_MED_InstantGravelNoble/FX/NS_Skin_InstantGravelNoble",
                PackageName = "NS_Skin_InstantGravelNoble"
        },
        new SoftObjectProperty()
        {
            AssetPathName =
                "/Game/Characters/Player/Male/Medium/Bodies/M_MED_InstantGravelNoble/FX/NS_Skin_InstantGravelNoble",
            PackageName = "NS_Skin_InstantGravelNoble"
        },
        new SoftObjectProperty()
        {
            AssetPathName =
                "/Game/Characters/Player/Male/Medium/Bodies/M_MED_InstantGravelNoble/FX/NS_Skin_InstantGravelNoble",
            PackageName = "NS_Skin_InstantGravelNoble"
        }
    }
};
uasset.WriteAll(writer);
writer.Close();

var testRead = new UAsset("CP_028_Athena_Body EDITED.uasset");
testRead.Initialize(@"C:\Program Files\Epic Games\Fortnite\FortniteGame\Content\Paks\global.utoc");
testRead.LoadMappings(@"++Fortnite+Release-28.01-CL-30106568-Android_oo.usmap");
testRead.ReadAll();
Console.WriteLine(testRead.ToJson());
