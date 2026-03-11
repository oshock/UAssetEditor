using System.Collections.Concurrent;
using System.Runtime.InteropServices.Marshalling;
using OodleDotNet;
using Serilog;
using UAssetEditor.Encryption.Aes;
using UAssetEditor.Unreal.Assets;
using UAssetEditor.Unreal.Containers;
using UAssetEditor.Unreal.Misc;
using UAssetEditor.Unreal.Objects.IO;
using UAssetEditor.Unreal.Packages;
using UAssetEditor.Unreal.Readers;
using UAssetEditor.Unreal.Readers.IoStore;
using UAssetEditor.Unreal.Versioning;
using UAssetEditor.Utils;
using UsmapDotNet;

namespace UAssetEditor;

public class UnrealFileSystem
{
    private List<ContainerFile> _containers;
    public IReadOnlyList<ContainerFile> Containers => _containers;
    public Dictionary<FGuid, FAesKey> AesKeys = new();
    
    private string _directory;
    public readonly IEnumerable<string> Files;
    public int MountedFiles { get; private set; }
    
    public ConcurrentDictionary<string, UnrealFileEntry> Packages { get; } = new();

    public EGame Game;
    public Usmap? Mappings { get; private set; }

    public IoGlobalReader? GetGlobalReader() => Containers.FirstOrDefault(x => x.Reader is IoGlobalReader)?.Reader as IoGlobalReader;
    
    public UnrealFileSystem(string directory, EGame gameVersion)
    {
        _directory = directory;
        Game = gameVersion;
        Files = Directory.EnumerateFiles(_directory);
        _containers = new List<ContainerFile>();
    }

    public static Usmap LoadMappingsStatic(string path, string? oodleDll = null)
    {
        return Usmap.Parse(path, new UsmapOptions
        {
            Oodle = string.IsNullOrEmpty(oodleDll) ? null : new Oodle(oodleDll),
            SaveNames = false
        });
    }
    
    public void LoadMappings(string path, string? oodleDll = null)
    {
        Mappings = LoadMappingsStatic(path, oodleDll);
    }

    
    public void LoadMappings(byte[] data, string? oodleDll = null)
    {
        Mappings = Usmap.Parse(data, new UsmapOptions
        {
            Oodle = string.IsNullOrEmpty(oodleDll) ? null : new Oodle(oodleDll),
            SaveNames = false
        });
    }
    
    public void Initialize(bool loadInParallel = true, int maxDegreeOfParallelism = 6, bool unloadContainersWithNoFiles = true)
    {
        var files = Directory.EnumerateFiles(_directory);

        if (loadInParallel)
        {
            Parallel.ForEach(files, new ParallelOptions
                { MaxDegreeOfParallelism = maxDegreeOfParallelism }, x => HandleContainer(x, unloadContainersWithNoFiles));
        }
        else
        {
            foreach (var file in files)
                HandleContainer(file, unloadContainersWithNoFiles);
        }
        
        if (unloadContainersWithNoFiles)
            GC.Collect();
    }

    public void HandleContainer(string file, bool unloadContainerIfNoFilesFound)
    {
        Information($"Mounting '{file}'");

        ContainerFile? container = null;
        
        if (file.EndsWith(".utoc"))
        {
            if (file.EndsWith(".o.utoc")) // Skip Fortnite's UEFN containers
                return;

            if (file.EndsWith("global.utoc"))
            {
                container = new IoFile(IoGlobalReader.InitializeGlobalData(file, Game), this);
            }
            else
            {
                container = new IoFile(file, this);
                container.Mount();
                MountedFiles++;

                if (unloadContainerIfNoFilesFound)
                {
                    if (container.FileCount == 0)
                    {
                        container.Unmount();
                        Information($"Unmounted '{file}' because no virtual files were found.");
                        return;
                    }
                }

                foreach (var pkg in container.PackagesByPath)
                    Packages.TryAdd(pkg.Key, pkg.Value);
            }

            _containers.Add(container);
        }
        else if (file.EndsWith(".pak"))
        {
            // TODO paks
        }
        
        Information($"Mounted '{file}' with {container?.FileCount ?? 0} files.");
    }

    public bool TryGetPackage(string packagePath, out UnrealFileEntry? pkg, out ContainerFile? file)
    {
        foreach (var container in Containers)
        {
            if (!container.TryFindPackage(packagePath.ToLower(), out pkg))
                continue;
            
            file = container;
            return true;
        }

        pkg = null;
        file = null;
        return false;
    }
    
    /// <summary>
    /// I/O Store only.
    /// </summary>
    /// <param name="packageId"></param>
    /// <param name="pkg"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    public bool TryGetPackage(FPackageId packageId, out UnrealFileEntry? pkg, out ContainerFile? file)
    {
        foreach (var ctn in Containers)
        {
            if (ctn is not IoFile ioFile)
                continue;

            if (ioFile.FilesById == null || !ioFile.FilesById.TryGetValue(packageId, out var _pkg)) 
                continue;
            
            pkg = _pkg;
            file = ioFile;
            return true;
        }

        pkg = null;
        file = null;
        return false;
    }
    
    public bool TryRead(string packagePath, out byte[] data)
    {
        if (!TryGetPackage(packagePath, out var pkg, out _))
        {
            data = [];
            return false;
        }

        data = pkg!.Read();
        return true;
    }

    public bool TryExtractAsset(UnrealFileEntry pkg, ContainerFile ctn, out Asset? asset)
    {
        switch (pkg)
        {
            case FIoStoreEntry ioEntry:
            {
                var data = ioEntry.Read();
                asset = new ZenAsset(data, this, ctn.Reader as UnrealFileReader);

                var globalToc = GetGlobalReader();
                asset.As<ZenAsset>().Initialize(globalToc!);

                asset.Game = Game;
                asset.Mappings = Mappings;
                asset.Entry = ioEntry;

                return true;
            }
            case FPakEntry pakEntry:
                throw new NotImplementedException();
        }

        asset = null;
        return false;
    }
    
    public bool TryExtractAsset(string packagePath, out Asset? asset)
    {
        if (!TryGetPackage(packagePath, out var pkg, out var ctn))
        {
            asset = null;
            return false;
        }

        return TryExtractAsset(pkg, ctn, out asset);
    }

    public bool TryExtractAndRead(FPackageId packageId, out ZenAsset? asset, bool onlyReadHeader = false)
    {
        if (!TryGetPackage(packageId, out var entry, out var ctn))
        {
            Log.Error("Could not find package!");
            asset = null;
            return false;
        }

        if (!TryExtractAsset(entry, ctn, out var _asset))
        {
            Log.Error("Found file via id, but system was unable to extract.");
            asset = null;
            return false;
        }

        if (_asset is ZenAsset pkg)
        {
            Information($"Reading package: '{entry.Path}'");
            
            if (onlyReadHeader)
                pkg.ReadHeader();
            else 
                pkg.ReadAll();
            
            asset = pkg;
            return true;
        }

        asset = null;
        return false;
    }
}