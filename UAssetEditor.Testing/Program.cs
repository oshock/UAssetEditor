using System.Diagnostics;
using UAssetEditor;
using UAssetEditor.Properties;

var stopwatch = Stopwatch.StartNew();
var uasset = new UAsset(@"DefaultGameDataCosmetics.uasset");
uasset.Initialize(@"C:\Program Files\Epic Games\Fortnite\FortniteGame\Content\Paks\global.utoc");
uasset.LoadMappings(@"++Fortnite+Release-28.01-CL-30106568-Android_oo.usmap");
uasset.ReadAll();
stopwatch.Stop();
Console.WriteLine($"Read in {stopwatch.ElapsedMilliseconds}ms!");
//Console.WriteLine(JsonConvert.SerializeObject(uasset.Properties, Formatting.Indented));

if (File.Exists("DefaultGameDataCosmetics EDITED.uasset"))
    File.Delete("DefaultGameDataCosmetics EDITED.uasset");

var writer = new Writer(File.Open("DefaultGameDataCosmetics EDITED.uasset", FileMode.Create, FileAccess.Write));
uasset.NameMap.Strings.Add("/Game/Characters/Player/Male/Medium/Bodies/M_MED_InstantGravelNoble/FX/NS_Skin_InstantGravelNoble");
uasset.NameMap.Strings.Add("NS_Skin_InstantGravelNoble");
uasset.Properties["DefaultGameDataCosmetics"][5] = new UProperty
{
    Name = "DefaultCharacterParts",
    Type = "ArrayProperty",
    InnerType = "SoftObjectProperty",
    Value = new List<SoftObjectProperty>
    {
        new()
        {
            AssetPathName = "/Game/Athena/Heroes/Meshes/Bodies/CP_Athena_Body_F_Fallback",
            PackageName = "CP_Athena_Body_F_Fallback",
            SubPathName = ""
        },
        new()
        {
            AssetPathName = "/Game/Characters/CharacterParts/FaceAccessories/CP_F_MED_Innovator_Ponytail",
            PackageName = "CP_F_MED_Innovator_Ponytail",
            SubPathName = ""
        },
        new()
        {
            AssetPathName = "/Game/Characters/CharacterParts/FaceAccessories/CP_F_MED_Innovator_Ponytail",
            PackageName = "CP_F_MED_Innovator_Ponytail",
            SubPathName = ""
        },
        new()
        {
            AssetPathName = "/Game/Characters/CharacterParts/FaceAccessories/CP_F_MED_Innovator_Ponytail",
            PackageName = "CP_F_MED_Innovator_Ponytail",
            SubPathName = ""
        },
        new()
        {
            AssetPathName = "/Game/Characters/CharacterParts/FaceAccessories/CP_F_MED_Innovator_Ponytail",
            PackageName = "CP_F_MED_Innovator_Ponytail",
            SubPathName = ""
        },
                 new()
                 {
                     AssetPathName = "/Game/Characters/CharacterParts/FaceAccessories/CP_F_MED_Innovator_Ponytail",
                     PackageName = "CP_F_MED_Innovator_Ponytail",
                     SubPathName = ""
                 },
                 new()
                 {
                     AssetPathName = "/Game/Characters/CharacterParts/FaceAccessories/CP_F_MED_Innovator_Ponytail",
                     PackageName = "CP_F_MED_Innovator_Ponytail",
                     SubPathName = ""
                 },
                          new()
                          {
                              AssetPathName = "/Game/Characters/CharacterParts/FaceAccessories/CP_F_MED_Innovator_Ponytail",
                              PackageName = "CP_F_MED_Innovator_Ponytail",
                              SubPathName = ""
                          },
                                   new()
                                   {
                                       AssetPathName = "/Game/Characters/CharacterParts/FaceAccessories/CP_F_MED_Innovator_Ponytail",
                                       PackageName = "CP_F_MED_Innovator_Ponytail",
                                       SubPathName = ""
                                   },
                                            new()
                                            {
                                                AssetPathName = "/Game/Characters/CharacterParts/FaceAccessories/CP_F_MED_Innovator_Ponytail",
                                                PackageName = "CP_F_MED_Innovator_Ponytail",
                                                SubPathName = ""
                                            },
                                                     new()
                                                     {
                                                         AssetPathName = "/Game/Characters/CharacterParts/FaceAccessories/CP_F_MED_Innovator_Ponytail",
                                                         PackageName = "CP_F_MED_Innovator_Ponytail",
                                                         SubPathName = ""
                                                     },
                                                              new()
                                                              {
                                                                  AssetPathName = "/Game/Characters/CharacterParts/FaceAccessories/CP_F_MED_Innovator_Ponytail",
                                                                  PackageName = "CP_F_MED_Innovator_Ponytail",
                                                                  SubPathName = ""
                                                              },
                                                                       new()
                                                                       {
                                                                           AssetPathName = "/Game/Characters/CharacterParts/FaceAccessories/CP_F_MED_Innovator_Ponytail",
                                                                           PackageName = "CP_F_MED_Innovator_Ponytail",
                                                                           SubPathName = ""
                                                                       },
                                                                                new()
                                                                                {
                                                                                    AssetPathName = "/Game/Characters/CharacterParts/FaceAccessories/CP_F_MED_Innovator_Ponytail",
                                                                                    PackageName = "CP_F_MED_Innovator_Ponytail",
                                                                                    SubPathName = ""
                                                                                },
                                                                                         new()
                                                                                         {
                                                                                             AssetPathName = "/Game/Characters/CharacterParts/FaceAccessories/CP_F_MED_Innovator_Ponytail",
                                                                                             PackageName = "CP_F_MED_Innovator_Ponytail",
                                                                                             SubPathName = ""
                                                                                         },
                                                                                                  new()
                                                                                                  {
                                                                                                      AssetPathName = "/Game/Characters/CharacterParts/FaceAccessories/CP_F_MED_Innovator_Ponytail",
                                                                                                      PackageName = "CP_F_MED_Innovator_Ponytail",
                                                                                                      SubPathName = ""
                                                                                                  },
                                                                                                           new()
                                                                                                           {
                                                                                                               AssetPathName = "/Game/Characters/CharacterParts/FaceAccessories/CP_F_MED_Innovator_Ponytail",
                                                                                                               PackageName = "CP_F_MED_Innovator_Ponytail",
                                                                                                               SubPathName = ""
                                                                                                           },
                                                                                                                    new()
                                                                                                                    {
                                                                                                                        AssetPathName = "/Game/Characters/CharacterParts/FaceAccessories/CP_F_MED_Innovator_Ponytail",
                                                                                                                        PackageName = "CP_F_MED_Innovator_Ponytail",
                                                                                                                        SubPathName = ""
                                                                                                                    },
                                                                                                                             new()
                                                                                                                             {
                                                                                                                                 AssetPathName = "/Game/Characters/CharacterParts/FaceAccessories/CP_F_MED_Innovator_Ponytail",
                                                                                                                                 PackageName = "CP_F_MED_Innovator_Ponytail",
                                                                                                                                 SubPathName = ""
                                                                                                                             },
                                                                                                                                      new()
                                                                                                                                      {
                                                                                                                                          AssetPathName = "/Game/Characters/CharacterParts/FaceAccessories/CP_F_MED_Innovator_Ponytail",
                                                                                                                                          PackageName = "CP_F_MED_Innovator_Ponytail",
                                                                                                                                          SubPathName = ""
                                                                                                                                      },
                                                                                                                                               new()
                                                                                                                                               {
                                                                                                                                                   AssetPathName = "/Game/Characters/CharacterParts/FaceAccessories/CP_F_MED_Innovator_Ponytail",
                                                                                                                                                   PackageName = "CP_F_MED_Innovator_Ponytail",
                                                                                                                                                   SubPathName = ""
                                                                                                                                               },
                                                                                                                                               new()
                                                                                                                                               {
                                                                                                                                                   AssetPathName = "/Game/Characters/CharacterParts/FaceAccessories/CP_F_MED_Innovator_Ponytail",
                                                                                                                                                   PackageName = "CP_F_MED_Innovator_Ponytail",
                                                                                                                                                   SubPathName = ""
                                                                                                                                               }
    }
};
uasset.WriteAll(writer);
writer.Close();

var testRead = new UAsset("DefaultGameDataCosmetics.uasset");
testRead.Initialize(@"C:\Program Files\Epic Games\Fortnite\FortniteGame\Content\Paks\global.utoc");
testRead.LoadMappings(@"++Fortnite+Release-28.01-CL-30106568-Android_oo.usmap");
testRead.ReadAll();
Console.WriteLine(testRead.ToJson());
