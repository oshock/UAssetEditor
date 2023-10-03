using System.Diagnostics;
using UAssetEditor;
using UAssetEditor.Properties;
using Usmap.NET;

var reader = new Reader(new byte[]
{
    0x04, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00
}, new Usmap.NET.Usmap(
    @"C:\Users\owen\Downloads\FModel\Output\.data\++Fortnite+Release-26.20-CL-28096793-Android_oo.usmap",
    new UsmapOptions
    {
        OodlePath = "C:\\Users\\owen\\Downloads\\FModel\\Output\\.data\\oo2core_9_win64.dll",
        SaveNames = false
    }));


var dummyType = reader.Mappings.Schemas.First(x => x.Name == "CustomCharacterPart").Properties.First( x=> x.Name == "bAttachToSocket").Data;
var arrayOfBooleans = AbstractProperty.ReadProperty("ArrayProperty", reader, dummyType);
foreach (var i in (List<object>)arrayOfBooleans)
    Console.WriteLine(i);

Console.WriteLine("funny");

/*var stopwatch = Stopwatch.StartNew();
var uasset = new UAsset(@"C:\Users\owen\Downloads\FModel\Output\Exports\FortniteGame\Content\Athena\Heroes\Meshes\Bodies\CP_028_Athena_Body.uasset");
uasset.Initialize(@"C:\Program Files\Epic Games\Fortnite\FortniteGame\Content\Paks\global.utoc");
uasset.LoadMappings(@"C:\Users\owen\Downloads\FModel\Output\.data\++Fortnite+Release-26.20-CL-28096793-Android_oo.usmap");
uasset.ReadAll();
stopwatch.Stop();
Console.WriteLine($"Done in {stopwatch.ElapsedMilliseconds}ms!");*/