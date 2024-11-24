using UAssetEditor.Unreal.Containers;

namespace UAssetEditor.Unreal.Packages;

public abstract class UnrealFileEntry
{
    public string Path;
    public ContainerFile? Owner;

    public UnrealFileEntry(string path, ContainerFile container)
    {
        Owner = container;
        Path = path;
    }

    public abstract bool IsEncrypted { get; }
    public abstract string CompressionMethod { get; }
    
    public abstract byte[] Read();

    public override string ToString()
    {
        return Path;
    }
}