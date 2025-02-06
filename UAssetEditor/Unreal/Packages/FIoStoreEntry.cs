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

    public override string CompressionMethod
    {
        get
        {
            var file = GetIoFile();
            var methods = file.Resource.CompressionMethods;
            var firstBlock = GetCompressionBlocks(file.ReaderAsIoReader, GetOffsetAndLength()).First();

            return methods[firstBlock.CompressionMethodIndex];
        }
    }

    public FIoChunkId GetChunkId() => GetIoFile().Resource.ChunkIds[TocEntryIndex];

    private FIoOffsetAndLength GetOffsetAndLength() => GetIoFile().Resource.OffsetAndLengths[TocEntryIndex];
    
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
        var ioReader = GetIoFile().ReaderAsIoReader;
        var blocks = GetCompressionBlocks(ioReader, offsetLength);
        
        var data = new byte[offsetLength.Length];
        ioReader.ReadBlocks(blocks, data);

        return data;
    }
}