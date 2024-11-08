using UnrealExtractor.Binary;

namespace UnrealExtractor.Unreal.IoStore;

public class IoStoreReader : Reader
{
    private readonly uint TocEntryCount;
    private readonly uint TocCompressedBlockEntryCount;
    private readonly uint CompressionMethodNameCount;
    private readonly uint CompressionMethodNameLength;
    private readonly uint TocChunkPerfectHashSeedsCount;
    private readonly int[] TocChunkPerfectHashSeeds;
    
    public readonly ulong PartitionSize;
    private readonly uint TocChunksWithoutPerfectHashCount;
    private readonly int[] TocChunksWithoutPerfectHashes;
    
    private const uint COMPRESSION_BLOCK_SIZE = 65536;
    private const uint HASH_SIZE = 512;

    private readonly FIoChunkId[] ChunkIds;
    private readonly FIoOffsetAndLength[] OffsetAndLengths;

    private readonly long CompressionBlocksPosition;
    private readonly long DirectoryIndexPosition;
    
    public readonly string FilePath;

    private Dictionary<string, uint> Files = new();
    
    public IoStoreReader(string file) : base(file)
    {
        FilePath = file;
        Position = 24;
        
        TocEntryCount = Read<uint>();
        TocCompressedBlockEntryCount = Read<uint>();
        
        Position += 4;
        
        CompressionMethodNameCount = Read<uint>();
        CompressionMethodNameLength = Read<uint>();
        
        Position = 84;
        
        TocChunkPerfectHashSeedsCount = Read<uint>();
        PartitionSize = Read<ulong>();
        TocChunksWithoutPerfectHashCount = Read<uint>();
        
        Position = 144;

        ChunkIds = new FIoChunkId[TocEntryCount];
        for (int i = 0; i < ChunkIds.Length; i++)
            ChunkIds[i] = Read<FIoChunkId>();

        OffsetAndLengths = new FIoOffsetAndLength[TocEntryCount];
        for (int i = 0; i < OffsetAndLengths.Length; i++)
            OffsetAndLengths[i] = new FIoOffsetAndLength(this);
        
        TocChunkPerfectHashSeeds = ReadArray<int>((int)TocChunkPerfectHashSeedsCount);
        TocChunksWithoutPerfectHashes = ReadArray<int>((int)TocChunksWithoutPerfectHashCount);

        CompressionBlocksPosition = Position;
    }
    
    private struct FIoDirectoryIndexEntry
    {
        public uint Name;
        public uint FirstChildEntry;
        public uint NextSiblingEntry;
        public uint FirstFileEntry;
    }

    private struct FIoFileIndexEntry
    {
        public uint Name;
        public uint NextFileEntry;
        public uint UserData;
    }
}