using Serilog;
using UAssetEditor.Encryption.Aes;
using UAssetEditor.Unreal.Containers;
using UAssetEditor.Unreal.Misc;
using UAssetEditor.Unreal.Readers.IoStore;

namespace UAssetEditor;

public class UnrealFileSystem
{
    private List<ContainerFile> _containers;
    public IReadOnlyList<ContainerFile> Containers => _containers;
    public Dictionary<FGuid, FAesKey> AesKeys = new();
    
    private string _directory;

    public IoGlobalReader? GetGlobalReader() => Containers.FirstOrDefault(x => x.Reader is IoGlobalReader)?.Reader as IoGlobalReader;
    
    public UnrealFileSystem(string directory)
    {
        _directory = directory;
    }
    
    public void Initialize(bool loadInParallel = true, int maxDegreeOfParallelism = 6)
    {
        _containers = new List<ContainerFile>();
        var files = Directory.EnumerateFiles(_directory);

        if (loadInParallel)
        {
            Parallel.ForEach(files, new ParallelOptions
                { MaxDegreeOfParallelism = 8 }, HandleContainer);
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

    public bool TryRead(string packagePath, out byte[] data)
    {
        foreach (var container in Containers)
        {
            if (!container.TryFindPackage(packagePath.ToLower(), out var pkg))
                continue;

            data = pkg!.Read();
            return true;
        }

        data = [];
        return false;
    }
}