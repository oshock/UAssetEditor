using System.Text;
using UAssetEditor.Binary;
using UAssetEditor.Encryption.Aes;

namespace UAssetEditor.Unreal.Readers.IoStore;

// Might have taken a little from
// https://github.com/FabianFG/CUE4Parse/blob/master/CUE4Parse/UE4/IO/Objects/FIoStoreTocResource.cs
public class FIoStoreTocResource
{
    private readonly Reader Reader;
    
    public readonly FIoStoreTocHeader Header;

    public FIoChunkId[]? ChunkIds;
    public readonly FIoOffsetAndLength[]? OffsetAndLengths;
    public readonly FIoStoreTocCompressedBlockEntry[]? CompressionBlocks;
    public readonly int[]? ChunkPerfectHashSeeds;
    public readonly int[]? ChunkIndicesWithoutPerfectHash;
    public readonly string[] CompressionMethods;
    public readonly long DirectoryIndexPosition = -1;
    
    // Encryption
    private FAesKey? AesKey;
    public bool IsEncrypted => Header.ContainerFlags.HasFlag(EIoContainerFlags.Encrypted);

    public void SetAesKey(FAesKey key)
    {
        AesKey = key;
    }
    
    public FIoStoreTocResource(Reader reader)
    {
        Reader = reader;
        Header = new FIoStoreTocHeader(reader);

        if (Header.Version < EIoStoreTocVersion.PartitionSize)
        {
            Header.PartitionCount = 1;
            Header.PartitionSize = ulong.MaxValue;
        }

        ChunkIdPosition = Reader.Position;
        if (Globals.OptimizeMemory)
            Reader.Position += Header.TocEntryCount * 12;
        else
            ChunkIds = reader.ReadArray<FIoChunkId>((int)Header.TocEntryCount);

        OffsetAndLengthPosition = Reader.Position;
        if (Globals.OptimizeMemory)
            Reader.Position += Header.TocEntryCount * 10;
        else
        {
            OffsetAndLengths = new FIoOffsetAndLength[Header.TocEntryCount];
            for (int i = 0; i < OffsetAndLengths.Length; i++)
                OffsetAndLengths[i] = new FIoOffsetAndLength(reader);
        }

        uint perfectHashSeedsCount = 0;
        uint chunksWithoutPerfectHashCount = 0;
        if (Header.Version > EIoStoreTocVersion.PerfectHashWithOverflow)
        {
            perfectHashSeedsCount = Header.TocChunkPerfectHashSeedsCount;
            chunksWithoutPerfectHashCount = Header.TocChunksWithoutPerfectHashCount;
        }
        else if (Header.Version >= EIoStoreTocVersion.PerfectHash)
        {
            perfectHashSeedsCount = Header.TocChunkPerfectHashSeedsCount;
        }
        if (perfectHashSeedsCount > 0)
        {
            ChunkPerfectHashSeeds = reader.ReadArray<int>((int)perfectHashSeedsCount);
        }
        if (chunksWithoutPerfectHashCount > 0)
        {
            ChunkIndicesWithoutPerfectHash = reader.ReadArray<int>((int)chunksWithoutPerfectHashCount);
        }

        CompressionBlockPosition = Reader.Position;
        if (Globals.OptimizeMemory)
            Reader.Position += Header.TocCompressedBlockEntryCount * 12;
        else
        {
            CompressionBlocks = new FIoStoreTocCompressedBlockEntry[Header.TocCompressedBlockEntryCount];
            for (int i = 0; i < CompressionBlocks.Length; i++)
                CompressionBlocks[i] = new FIoStoreTocCompressedBlockEntry(reader);
        }

        var length = (int)Header.CompressionMethodNameLength;
        
        CompressionMethods = new string[Header.CompressionMethodNameCount + 1];
        CompressionMethods[0] = "None";

        for (int i = 1; i < CompressionMethods.Length; i++)
        {
            var buffer = reader.ReadBytes(length);
            var str = Encoding.ASCII.GetString(buffer).TrimEnd('\0');

            CompressionMethods[i] = str;
        }

        if (Header.ContainerFlags.HasFlag(EIoContainerFlags.Signed))
        {
            var hashSize = reader.Read<int>();
            reader.Position += hashSize + hashSize + 20 * Header.TocCompressedBlockEntryCount; // 20 = sizeof(FSHAHash)
        }

        if (Header.Version >= EIoStoreTocVersion.DirectoryIndex
            && Header.ContainerFlags.HasFlag(EIoContainerFlags.Indexed)
            && Header.DirectoryIndexSize > 0)
        {
            DirectoryIndexPosition = reader.Position;
        }
    }
    
    public unsafe ulong HashChunkIdWithSeed(int seed, FIoChunkId chunk)
    {
        var ptr = (byte*)&chunk;
        var size = sizeof(FIoChunkId);
        var hash = seed != 0 ? (ulong)seed : 0xcbf29ce484222325;
       
        for (uint index = 0; index < size; index++)
        {
            hash = (hash * 0x00000100000001B3) ^ ptr[index];
        }

        return hash;
    }
    
    #region Memory Optimizations

    private readonly long ChunkIdPosition;
    private readonly long OffsetAndLengthPosition;
    private readonly long CompressionBlockPosition;
    
    public FIoChunkId GetChunkId(uint index)
    {
        if (!Globals.OptimizeMemory) 
            return ChunkIds[index];
        
        Reader.Position = ChunkIdPosition + 12 * index;
        return Reader.Read<FIoChunkId>();
    }

    public FIoChunkId GetChunkId(int index) => GetChunkId((uint)index);

    public void LoadChunkIds()
    {
        if (ChunkIds == null)
            return;

        Reader.Position = ChunkIdPosition;
        ChunkIds = Reader.ReadArray<FIoChunkId>((int)Header.TocEntryCount);
    }
    
    public FIoOffsetAndLength GetOffsetAndLength(uint index)
    {
        if (!Globals.OptimizeMemory) 
            return OffsetAndLengths[index];
        
        Reader.Position = OffsetAndLengthPosition + 10 * index;
        return new FIoOffsetAndLength(Reader);
    }
    
    public FIoOffsetAndLength GetOffsetAndLength(int index) => GetOffsetAndLength((uint)index);
    
    public FIoStoreTocCompressedBlockEntry GetBlock(int blockIndex)
    {
        /*if (!Globals.OptimizeMemory) 
            return CompressionBlocks[blockIndex];*/
        
        Reader.Position = CompressionBlockPosition + 12 * blockIndex;
        return new FIoStoreTocCompressedBlockEntry(Reader);
    }
    
    #endregion
}