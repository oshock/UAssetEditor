﻿using System.Diagnostics;
using System.Net;
using Newtonsoft.Json.Linq;
using RestSharp;
using Spectre.Console;
using UAssetEditor;
using UAssetEditor.Compression;
using UAssetEditor.Encryption.Aes;
using UAssetEditor.Unreal.Misc;
using UAssetEditor.Unreal.Properties;
using UAssetEditor.Unreal.Properties.Types;

// Construct USystem
Oodle.Initialize("oo2core_9_win64.dll");
var paks = AnsiConsole.Prompt(new TextPrompt<string>("[blue]What is the path to your paks folder?[/]"));
var uSystem = new UnrealFileSystem(paks);

// Use GenxGames.gg for Aes Keys
var useAesEndpoint = AnsiConsole.Prompt(
    new TextPrompt<bool>("[aqua]Should we use '[white]https://fortnitecentral.genxgames.gg/api/v1/aes[/]' for the latest AES keys?[/]")
        .AddChoice(true)
        .AddChoice(false)
        .DefaultValue(true)
        .WithConverter(choice => choice ? "y" : "n"));

if (useAesEndpoint)
{
    var response = new RestClient().ExecuteGet(new RestRequest("https://fortnitecentral.genxgames.gg/api/v1/aes"));
    var json = JObject.Parse(response.Content);
    var keys = new Dictionary<FGuid, FAesKey> { { new FGuid(), new FAesKey(json["mainKey"].ToString()) } };

    foreach (var dynamic in json["dynamicKeys"])
    {
        keys.Add(new FGuid(dynamic["guid"].ToString()), new FAesKey(dynamic["key"].ToString()));
    }

    uSystem.AesKeys = keys;
}
else
{
    var aesKey = AnsiConsole.Prompt(new TextPrompt<string>("Enter the main aes key (0x0000...) "));
    uSystem.AesKeys.Add(new FGuid(), new FAesKey(aesKey));
}

var mountingTask = Task.Run(() =>
{
    uSystem.Initialize(
        loadInParallel: true,
        maxDegreeOfParallelism: 6,
        unloadContainersWithNoFiles: true);
});

// Mount containers
await AnsiConsole.Progress()
    .StartAsync(async ctx =>
    {
        var task = ctx.AddTask("[aqua]Mounting containers...[/]");
        var totalFiles = Convert.ToDouble(uSystem.Files.Count(x => x.EndsWith(".utoc")));
        var last = 0.0;
        
        while (!mountingTask.IsCompleted)
        {
            var progress = Convert.ToDouble(uSystem.MountedFiles) / totalFiles * 100;
            task.Increment(progress - last);
            last = progress;
        }
        
        task.Increment(100.0);
    });
    
// Use GenxGames.gg for Mappings
var useMappingsEndpoint = AnsiConsole.Prompt(
    new TextPrompt<bool>("[aqua]Should we use '[white]https://fortnitecentral.genxgames.gg/api/v1/mappings[/]' for the latest Mappings?[/]")
        .AddChoice(true)
        .AddChoice(false)
        .DefaultValue(true)
        .WithConverter(choice => choice ? "y" : "n"));

if (useMappingsEndpoint)
{
    var response = new RestClient().ExecuteGet(new RestRequest("https://fortnitecentral.genxgames.gg/api/v1/mappings"));
    var json = JObject.Parse(response.Content);
    var mappingsUrl = json[0]["url"].ToString();

    var data = new HttpClient().GetByteArrayAsync(mappingsUrl).GetAwaiter().GetResult();
    uSystem.LoadMappings(data, "oo2core_9_win64.dll");
    
    AnsiConsole.Markup("\n[blue]Loaded mappings.");
}
else
{
    AnsiConsole.MarkupLine("[blue]Enter the path to your mappings file[/] (.usmap)");
    var mappingsPath = AnsiConsole.Prompt(new TextPrompt<string>("[aqua]>>>[/]")).Replace("\"", "").Replace("/", "\\").Trim();
    uSystem.LoadMappings(mappingsPath, "oo2core_9_win64.dll");
}

extractAsset:
AnsiConsole.MarkupLine("[blue]Enter a path to an asset [/](ex: 'FortniteGame/Content/Asset.uasset')");
var assetPath = AnsiConsole.Prompt(new TextPrompt<string>(">>>")).Replace("\\", "/").Trim();
AnsiConsole.MarkupLine($"[blue]Extracting '[white]{assetPath}[/]'...[/]");

if (!uSystem.TryExtractAsset(assetPath, out var asset))
{
    AnsiConsole.MarkupLine($"[red]Unable to extract '[white]{asset}[/]'[/]");
    AnsiConsole.MarkupLine("[red]Press any key to try another asset...[/]");
    Console.ReadKey();
    goto extractAsset;
}

AnsiConsole.Clear();
AnsiConsole.MarkupLine($"[blue]Reading '[white]{assetPath}[/]'...[/]");

// Read asset
var sw = Stopwatch.StartNew();
asset!.ReadAll();
sw.Stop();

AnsiConsole.MarkupLine($"[blue]Read [white]{asset.Exports.Length}[/] export(s) in [white]{sw.ElapsedMilliseconds}ms[/][/]");
Thread.Sleep(1500);

openExport:
AnsiConsole.Clear();

var exportName = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("[blue]Pick an export to open[/]:")
        .PageSize(10)
        .MoreChoicesText("[grey](Move up and down to show more exports)[/]")
        .AddChoices(asset.Exports.Select(x => x.Name)));
var export = asset.Exports[exportName];

var properties = export!.Properties.Select(x => Markup.Escape($"{x.Name} ({x.Data?.Type ?? "None"})")).ToList();
properties.Insert(0, "..");

openProperty:
var propertyName = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("[blue]Pick a property to modify[/]:")
        .PageSize(25)
        .MoreChoicesText("[grey](Move up and down to show more properties)[/]")
        .AddChoices(properties));

if (propertyName == "..")
    goto openExport;

var property = export.Properties[properties.ToList().FindIndex(x => x == propertyName)];
PrintPropertyInfo(property);

switch (property.Data?.Type)
{
    case "ArrayProperty":
    {
        var array = property.GetValue<ArrayProperty>();
        foreach (var elm in array.Value)
        {

        }

        break;
    }
    case "SoftObjectProperty":
    {
        var value = property.GetValue<SoftObjectProperty>();
        AnsiConsole.MarkupLine($"[blue]\"[white]{value?.Value ?? "None"}[/]\"[/]");
        AnsiConsole.MarkupLine("[blue]Enter a new value [white](ex: /Game/Path.Name)[/] or type '.' to exit...[/]");
        var newValue = AnsiConsole.Prompt(new TextPrompt<string>("[aqua]>>>[/]"));
        if (newValue != ".")
            value!.Value = newValue;
        
        goto openProperty;
    }
}

Console.ReadKey();

void PrintPropertyInfo(UProperty prop)
{
    AnsiConsole.MarkupLine($"[blue]{prop.Name} ([white]{prop.Data?.Type}[/])[/]");
}
