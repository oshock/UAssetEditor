using System.Data;
using System.Diagnostics;
using System.IO.Enumeration;
using UAssetEditor;
using UAssetEditor.Unreal.Properties.Types;
using UAssetEditor.Binary;
using UAssetEditor.Compression;
using UAssetEditor.Encryption.Aes;
using UAssetEditor.Unreal.Assets;
using UAssetEditor.Unreal.Containers;
using UAssetEditor.Unreal.Misc;
using UAssetEditor.Unreal.Names;
using UAssetEditor.Unreal.Objects;
using UAssetEditor.Unreal.Objects.IO;
using UAssetEditor.Unreal.Readers.IoStore;
using UAssetEditor.Unreal.Versioning;
using UAssetEditor.Utils;
using UsmapDotNet;

Logger.StartLogger();

UnrealFileSystem.InitializeOodle("oo2core_9_win64.dll");

var system = new UnrealFileSystem(@"S:\Fortnite\FortniteGame\Content\Paks", EGame.GAME_UE5_LATEST);
system.AesKeys.Add(new FGuid(), new FAesKey("0x98F7261584C345F9B19402F20110A7A68A48C798FFCE86982F2E8C86F0725CDA"));
system.Initialize();
system.LoadMappings("++Fortnite+Release-40.00-CL-51746096_zs.usmap");

if (!system.TryExtractAsset(
        "FortniteGame/Content/Athena/Items/Weapons/WID_Sniper_Cowboy_Athena_SR.uasset",
        out var asset))
    throw new KeyNotFoundException("Unable to find asset.");

asset!.ForceLoadImportedObjects = true;
asset.ReadAll();

var system2 = new UnrealFileSystem(@"B:\FN Versions\14.40\FortniteGame\Content\Paks", EGame.GAME_UE4_26);
system2.AesKeys.Add(new FGuid(), new FAesKey("0xAB32BAB083F7D923A33AA768BC64B64BF62488948BD49FE61D95343492252558"));
system2.Initialize();
system2.LoadMappings("++Fortnite+Release-14.40-CL-14550713-Windows_oo.usmap");

// Change version
asset.Game = EGame.GAME_UE4_26;
asset.Mappings = system2.Mappings;
asset.System = system2;

var writer = new Writer("WID_Sniper_Cowboy_Athena_SR.uasset");
asset.WriteAll(writer);
writer.Close();

var testAsset = new ZenAsset("WID_Sniper_Cowboy_Athena_SR.uasset");
var globalToc = system2.GetGlobalReader();
testAsset.Initialize(globalToc!);
testAsset.ForceLoadImportedObjects = true;
testAsset.System = system2;
testAsset.Mappings = system2.Mappings;
testAsset.Game = system2.Game;
testAsset.ReadAll();

Console.ReadKey();