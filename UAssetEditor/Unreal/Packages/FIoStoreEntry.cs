using UAssetEditor.Unreal.Containers;
using UAssetEditor.Unreal.Readers.IoStore;

namespace UAssetEditor.Unreal.Packages;

public class FIoStoreEntry : UnrealFileEntry
{
    public uint TocEntryIndex;

    public FIoStoreEntry(string path, ContainerFile container, uint tocEntryIndex) : base(path, container)
    {
        TocEntryIndex = tocEntryIndex;
    }

    public IoFile GetIoFile() => (IoFile)Owner!;

    public override bool IsEncrypted => GetIoFile().IsEncrypted;

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

    public FIoChunkId GetChunkId()
    {
        return GetIoFile().Resource.GetChunkId(TocEntryIndex);
    }
    
    public FIoOffsetAndLength GetOffsetAndLength()
    {
        return GetIoFile().Resource.GetOffsetAndLength(TocEntryIndex);
    }

    public static FIoStoreTocCompressedBlockEntry[] GetCompressionBlocks(IoStoreReader reader, FIoOffsetAndLength offsetAndLength)
    {
        var compressionBlockSize = reader.Resource.Header.CompressionBlockSize;
        var blockIndex = (int)(offsetAndLength.Offset / compressionBlockSize);
        var blockCount = (offsetAndLength.Length - 1) / compressionBlockSize + 1;

        var blocks = new FIoStoreTocCompressedBlockEntry[blockCount];

        if (Globals.OptimizeMemory)
        {
            for (int i = 0; i < blocks.Length; i++)
                blocks[i] = reader.Resource.GetBlock(blockIndex + i);
        }
        else
        {
            Array.ConstrainedCopy(reader.Resource.CompressionBlocks, blockIndex, blocks, 0, blocks.Length);
        }
        
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