using OodleDotNet;
using Serilog;
using UAssetEditor.Encryption.Aes;
using UAssetEditor.Unreal.Containers;
using UAssetEditor.Unreal.Misc;
using UAssetEditor.Unreal.Packages;
using UAssetEditor.Unreal.Readers.IoStore;
using UAssetEditor.Utils;
using UsmapDotNet;

namespace UAssetEditor;

public class UnrealFileSystem
{
    private List<ContainerFile> _containers;
    public IReadOnlyList<ContainerFile> Containers => _containers;
    public Dictionary<FGuid, FAesKey> AesKeys = new();
    
    private string _directory;

    public Usmap? Mappings { get; private set; }

    public IoGlobalReader? GetGlobalReader() => Containers.FirstOrDefault(x => x.Reader is IoGlobalReader)?.Reader as IoGlobalReader;
    
    public UnrealFileSystem(string directory)
    {
        _directory = directory;
    }

    public void LoadMappings(string path)
    {
        Mappings = new Usmap(path, new UsmapOptions
        {
            Oodle = new Oodle("oo2core_9_win64.dll"),
            SaveNames = false
        });
    }
    
    public void Initialize(bool loadInParallel = true, int maxDegreeOfParallelism = 6)
    {
        _containers = new List<ContainerFile>();
        var files = Directory.EnumerateFiles(_directory);

        if (loadInParallel)
        {
            Parallel.ForEach(files, new ParallelOptions
                { MaxDegreeOfParallelism = maxDegreeOfParallelism }, HandleContainer);
        }
        else
        {
            foreach (var file in files)
                HandleContainer(file);
        }
    }

    public void HandleContainer(string file)
    {
        Log.Information($"Mounting '{file}'");

        ContainerFile? container = null;
        
        if (file.EndsWith(".utoc"))
        {
            if (file.EndsWith(".o.utoc")) // Skip Fortnite's UEFN containers
                return;

            if (file.EndsWith("global.utoc"))
            {
                container = new IoFile(IoGlobalReader.InitializeGlobalData(file), this);
            }
            else
            {
                container = new IoFile(file, this);
                container.Mount();
            }

            _containers.Add(container);
        }
        else if (file.EndsWith(".pak"))
        {
            // TODO paks
        }
        
        Log.Information($"Mounted '{file}' with {container?.FileCount ?? 0} files.");
    }

    public bool TryGetPackage(string packagePath, out UnrealFileEntry? pkg)
    {
        foreach (var container in Containers)
        {
            if (!container.TryFindPackage(packagePath.ToLower(), out pkg))
                continue;

            return true;
        }

        pkg = null;
        return false;
    }
    
    public bool TryRead(string packagePath, out byte[] data)
    {
        if (!TryGetPackage(packagePath, out var pkg))
        {
            data = [];
            return false;
        }

        data = pkg!.Read();
        return true;
    }

    public bool TryExtractAsset(string packagePath, out Asset? asset)
    {
        if (!TryGetPackage(packagePath, out var pkg))
        {
            asset = null;
            return false;
        }

        switch (pkg)
        {
            case FIoStoreEntry ioEntry:
            {
                var data = ioEntry.Read();
                asset = new ZenAsset(data);

                var globalToc = GetGlobalReader();
                asset.As<ZenAsset>().Initialize(globalToc!);

                asset.Mappings = Mappings;
                
                break;
            }
            case FPakEntry pakEntry:
                throw new NotImplementedException();
        }

        asset = null;
        return false;
    }
}