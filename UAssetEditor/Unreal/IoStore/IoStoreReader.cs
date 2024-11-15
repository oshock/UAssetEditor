using System.Runtime.CompilerServices;
using UAssetEditor;
using UAssetEditor.IoStore;

namespace Astro.App.IoReader;

public struct VFileInfo
{
    public uint TocEntryIndex;
    public uint FirstBlockIndex;
    public uint BlockCount;
    public long Length;
}

public class IoStoreReader : Reader
{
    private readonly uint TocEntryCount;
    private readonly uint TocCompressedBlockEntryCount;
    private readonly uint CompressionMethodNameCount;
    private readonly uint CompressionMethodNameLength;
    private readonly uint TocChunkPerfectHashSeedsCount;
    private readonly int[] TocChunkPerfectHashSeeds;
    
    private readonly ulong PartitionSize;
    private readonly uint TocChunksWithoutPerfectHashCount;
    private readonly int[] TocChunksWithoutPerfectHashes;
    
    private const uint COMPRESSION_BLOCK_SIZE = 65536;
    private const uint HASH_SIZE = 512;

    private readonly Dictionary<ulong, EIoChunkType5> ChunkIds = new();
    private readonly Dictionary<ulong, ulong> ChunkOffsetLengths = new();

    private readonly long CompressionBlocksPosition;
    private readonly long DirectoryIndexPosition;
    
    public readonly string FilePath;

    private Dictionary<string, uint> Files = new();
    
    public IoStoreReader(string file) : base(File.ReadAllBytes(file))
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
        
        for (int i = 0; i < TocEntryCount; i++)
        {
            var id = Read<ulong>();
            Position += sizeof(ushort) + 1; // chunkIndex + padding
            ChunkIds.Add(id, Read<EIoChunkType5>());
        }
        
        for (int i = 0; i < TocEntryCount; i++)
        {
            var buffer = ReadBytes(10).Reverse().ToArray();
            var offsetBuffer = new byte[sizeof(ulong)];
            var lengthBuffer = new byte[sizeof(ulong)];
            Buffer.BlockCopy(buffer, 5, offsetBuffer, 0, 5);
            Buffer.BlockCopy(buffer, 0, lengthBuffer, 0, 5);

            var offset = BitConverter.ToUInt64(offsetBuffer);
            var length = BitConverter.ToUInt64(lengthBuffer);

            ChunkOffsetLengths.Add(offset, length);
        }
        
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
    
    public enum EIoChunkType5 : byte
    {
        Invalid = 0,
        ExportBundleData = 1,
        BulkData = 2,
        OptionalBulkData = 3,
        MemoryMappedBulkData = 4,
        ScriptObjects = 5,
        ContainerHeader = 6,
        ExternalFile = 7,
        ShaderCodeLibrary = 8,
        ShaderCode = 9,
        PackageStoreEntry = 10,
        DerivedData = 11,
        EditorDerivedData = 12
    }
    
    public VFileInfo ExtractChunk(ulong id, EIoChunkType5 type)
    {
        var result = new VFileInfo();

        var index = (uint)(HashWithSeed(id, type, 0) % TocChunkPerfectHashSeedsCount);
        var seed = TocChunkPerfectHashSeeds[index];

        var slot = (uint)(HashWithSeed(id, type, seed) % TocEntryCount);
        result.TocEntryIndex = slot;
        result.FirstBlockIndex = (uint)(ChunkOffsetLengths.ElementAt((int)slot).Key / COMPRESSION_BLOCK_SIZE);
        result.BlockCount = (uint)((ChunkOffsetLengths.ElementAt((int)slot).Value - 1) / COMPRESSION_BLOCK_SIZE) + 1;
        result.Length = (long)ChunkOffsetLengths.ElementAt((int)slot).Value;
        
        return result;
    }

    public ulong HashWithSeed(ulong id, EIoChunkType5 type, int seed)
    {
        var buffer = BitConverter.GetBytes(id);

        var hash = seed != 0 ? (ulong)seed : 0xcbf29ce484222325;
        for (var index = 0; index < sizeof(ulong); ++index)
        {
            hash = (hash * 0x00000100000001B3) ^ buffer[index];
        }

        return hash;
    }

    // TODO REMOVE
    public Reader Extract(VFileInfo info)
    {
        return new([]);
    }
    
    public T ReadUndersized<T>(int size)
    {
        var buffer = new byte[Unsafe.SizeOf<T>()];
        var bytes = ReadBytes(size);
        Buffer.BlockCopy(bytes, 0, buffer, 0, bytes.Length);

        return Unsafe.ReadUnaligned<T>(ref buffer[0]);
    }
}