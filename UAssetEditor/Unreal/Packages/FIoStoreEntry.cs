using UAssetEditor.Unreal.Containers;
using UAssetEditor.Unreal.Readers.IoStore;

namespace UAssetEditor.Unreal.Packages;

public class FIoStoreEntry : UnrealFileEntry
{
    private uint TocEntryIndex;

    public FIoStoreEntry(string path, ContainerFile container, uint tocEntryIndex) : base(path, container)
    {
        TocEntryIndex = tocEntryIndex;
    }

    public IoFile GetIoFile() => (IoFile)Owner!;

    public override bool IsEncrypted { get; }
    public override string CompressionMethod { get; }

    private FIoOffsetAndLength GetOffsetAndLength()
    {
        var container = (IoFile)Owner!;
        return container.Resource.OffsetAndLengths[TocEntryIndex];
    }
    
    public static FIoStoreTocCompressedBlockEntry[] GetCompressionBlocks(IoStoreReader reader, FIoOffsetAndLength offsetAndLength)
    {
        var compressionBlockSize = reader.Resource.Header.CompressionBlockSize;
        var blockIndex = (int)(offsetAndLength.Offset / compressionBlockSize);
        var blockCount = (offsetAndLength.Length - 1) / compressionBlockSize + 1;

        var blocks = new FIoStoreTocCompressedBlockEntry[blockCount];
        Array.ConstrainedCopy(reader.Resource.CompressionBlocks, blockIndex, blocks, 0, blocks.Length);
        
        return blocks;
    }

    public override byte[] Read()
    {
        var offsetLength = GetOffsetAndLength();
        var blocks = GetCompressionBlocks((IoStoreReader)GetIoFile().Reader, offsetLength);
        var reader = (IoStoreReader)GetIoFile().Reader;
        
        var data = new byte[offsetLength.Length];
        reader.ReadBlocks(blocks, data);

        return data;
    }
}