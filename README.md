# UAssetEditor
An Unreal Engine UAsset API for Fortnite

## Features
- Mount I/O containers (.utoc) and read packages compressed with Oodle
- Deserialize unversioned uassets with the provided mappings
- Modify uasset data/ properties
- Serialize the modified asset back into a uasset file
- Switching engine version and mappings to allow porting assets to different Fortnite versions (WIP documentation)

## How to use
```csharp

// Create system
var system = new UnrealFileSystem(@"C:\Program Files\Epic Games\Fortnite\FortniteGame\Content\Paks", EGame.GAME_UE5_LATEST);

// Add aes keys
system.AesKeys.Add(new FGuid(), new FAesKey("0x0000...")); // Replace with your game's AesKey

// Mount containers
system.Initialize();

// Initialize Oodle
UnrealFileSystem.InitializeOodle("oo2core_9_win64.dll");

// Load mappings
system.LoadMappings("path/to/usmap/++Fortnite+Release-xx.xx.usmap");

// Extract the asset
if (!system.TryExtractAsset(
        "FortniteGame/Plugins/GameFeatures/BRCosmetics/Content/Athena/Items/Cosmetics/Characters/CID_028_Athena_Commando_F.uasset", out var asset))
    throw new KeyNotFoundException("Unable to find asset.");

// Read all properties
asset.ReadAll();

// Output JSON file
var json = asset.ToString();
File.WriteAllText("CID_028_Athena_Commando_F.json", json);

// Get ItemName Property
var text = asset["CID_028_Athena_Commando_F"]?["ItemName"]?.GetValue<TextProperty>();

// Set new FText value
text.Value.Text = "This is a new name for Renegade Raider";

// Create a writer with the file path: "CID_028_Athena_Commando_F.uasset"
var writer = new Writer("CID_028_Athena_Commando_F.uasset");

// Serialize the asset and dispose the writer
asset.WriteAll(writer);
writer.Close();

// Create a new ZenAsset with the asset we just serialized
var testAsset = new ZenAsset("CID_028_Athena_Commando_F.uasset");

// Set the GlobalReader instance
var globalToc = system.GetGlobalReader();
testAsset.Initialize(globalToc!);

// Set mappings
testAsset.Mappings = system.Mappings;

// Test if it reads our asset properly
testAsset.ReadAll();
```

## TODOs
- Export Types (Blueprints, etc.)
- Paks and versioned uassets
- Desktop app
- Documentation (wip)

### Special thanks to
This project was heavily based on [CUE4Parse](https://github.com/FabianFG/CUE4Parse) with the goal of recreating Unreal Engine's asset reading and writing functionality as an easy to use API
