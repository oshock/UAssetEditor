using UnrealExtractor.Unreal.Packages;
using UnrealExtractor.Unreal.Readers;

namespace UnrealExtractor.Unreal.Containers;

public abstract class ContainerFile
{
    public UnrealFileReader Reader { get; protected set; }
    public abstract bool IsEncrypted { get; }
    
    public string Path;
    public UnrealFileSystem? System;

    public ContainerFile(string path, UnrealFileSystem? system = null)
    {
        Path = path;
        System = system;
    }

    public IReadOnlyDictionary<string, UnrealFileEntry> PackagesByPath => Reader.PackagesByPath;

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
    public bool TryFindPackage(string path, out UnrealFileEntry? pkg)
    {
        return PackagesByPath.TryGetValue(path, out pkg);
    }
}