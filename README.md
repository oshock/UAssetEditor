# UAssetEditor
 An Unreal I/O Asset Editor

## Features
- Mount I/O containers (.utoc) and read packages compressed with Oodle
- Deserialize unversioned uassets with provided mappings
- Modify uasset data/ properties easily
- Serialize the modified asset back into a file

## How to use
```csharp
// Create system
var system = new UnrealFileSystem(@"C:\Program Files\Epic Games\Fortnite\FortniteGame\Content\Paks");

// Add aes keys
system.AesKeys.Add(new FGuid(), new FAesKey("0x0000..."));

// Mount containers
system.Initialize();

// Load mappings
system.LoadMappings("path/to/usmap/++Fortnite+Release-33.11-CL-38773622-Windows_oo.usmap", "path/to/dll/oo2core_9_win64.dll (if needed)");

// Initialize Oodle
Oodle.Initialize("path/to/dll/oo2core_9_win64.dll");

// Extract the asset
if (!system.TryExtractAsset("FortniteGame/Plugins/GameFeatures/BRCosmetics/Content/Athena/Items/Cosmetics/Characters/CID_028_Athena_Commando_F.uasset", out var asset))
    throw new KeyNotFoundException("Unable to find asset.");

var uasset = (ZenAsset)asset!;

// Global .utoc path (required for class identification)
var globalToc = system.GetGlobalReader();
uasset.Initialize(globalToc!);

// Read everything
uasset.ReadAll();

// Output json
var json = asset.ToString();
File.WriteAllText("CID_028_Athena_Commando_F.json", json);

// Get the CID_028_Athena_Commando_F export
var CID_028_Athena_Commando_F = uasset["CID_028_Athena_Commando_F"];

// Get the ItemName property
var text = (TextProperty)CID_028_Athena_Commando_F["ItemName"].Value;

// Set the new value
text.Value.Text = "Very cool item!";

// Serialize the new data
var writer = new Writer("CID_028_Athena_Commando_F.uasset");
uasset.WriteAll(writer);
writer.Close();

// Open the serialized data to make sure it wrote correctly
var testAsset = new ZenAsset("CID_028_Athena_Commando_F.uasset");
testAsset.Initialize(globalToc!);
testAsset.Mappings = system.Mappings;
testAsset.ReadAll();

```

## TODOs
- Export Types (Blueprints, etc.)
- Optional I/O Container reading to populate imports
- Paks and versioned uassets
- Desktop app
- Documentation (wip)
