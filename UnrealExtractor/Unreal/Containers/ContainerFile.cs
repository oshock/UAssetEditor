using UnrealExtractor.Unreal.Packages;

namespace UnrealExtractor.Unreal.Containers;

public abstract class ContainerFile
{
    public string Path;

    public ContainerFile(string path)
    {
        Path = path;
    }

    private Dictionary<string, Package> _packagesByPath { get; set; } = new();
    public Dictionary<string, Package> PackagesByPath => _packagesByPath;

    public int FileCount => PackagesByPath.Count;
    
    /// <summary>
    /// Loads the container and all of its data
    /// </summary>
    public abstract void Mount();

    /// <summary>
    /// Attempts to find a package via its path.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="pkg"></param>
    /// <returns></returns>
    public bool TryFindPackage(string path, out Package? pkg)
    {
        return PackagesByPath.TryGetValue(path, out pkg);
    }
}