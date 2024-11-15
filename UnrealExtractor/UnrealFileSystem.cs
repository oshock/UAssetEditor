using Serilog;
using Serilog.Core;
using UnrealExtractor.Encryption.Aes;
using UnrealExtractor.Unreal.Containers;
using UnrealExtractor.Unreal.Misc;

namespace UnrealExtractor;

public class UnrealFileSystem
{
    private List<ContainerFile> _containers;
    public IReadOnlyList<ContainerFile> Containers => _containers;

    public readonly Dictionary<FGuid, FAesKey>? AesKeys;
    
    public UnrealFileSystem(string directory, Dictionary<FGuid, FAesKey>? aesKeys = null, bool loadInParallel = true, int maxDegreeOfParallelism = 6)
    {
        AesKeys = aesKeys ?? new Dictionary<FGuid, FAesKey>();
        _containers = new List<ContainerFile>();
        var files = Directory.EnumerateFiles(directory);

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
            
            container = new IoFile(file, this);
            container.Mount();
                
            _containers.Add(container);
        }
        else if (file.EndsWith(".pak"))
        {
            // TODO paks
        }
        
        Log.Information($"Mounted '{file}' with {container?.FileCount ?? 0} files.");
    }
}