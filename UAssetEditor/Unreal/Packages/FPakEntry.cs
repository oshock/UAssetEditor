using UAssetEditor.Unreal.Containers;

namespace UAssetEditor.Unreal.Packages;

public class FPakEntry : UnrealFileEntry
{
    public FPakEntry(string path, ContainerFile container) : base(path, container)
    {
    }

    public override bool IsEncrypted { get; }
    public override string CompressionMethod { get; }
    
    public override byte[] Read()
    {
        throw new NotImplementedException();
    }
}