using UnrealExtractor.Binary;

namespace UnrealExtractor.Unreal.IoStore;

public enum EIoStoreTocVersion : byte
{
    Invalid = 0,
    Initial,
    DirectoryIndex,
    PartitionSize,
    PerfectHash,
    PerfectHashWithOverflow,
    OnDemandMetaData,
    RemovedOnDemandMetaData,
    ReplaceIoChunkHashWithIoHash,
    LatestPlusOne,
    Latest = LatestPlusOne - 1
}

public enum EIoContainerFlags : byte
{
    None,
    Compressed	= (1 << 0),
    Encrypted	= (1 << 1),
    Signed		= (1 << 2),
    Indexed		= (1 << 3),
    OnDemand	= (1 << 4),
}

public class FIoStoreTocHeader
{
    public static byte[] TOC_MAGIC = {0x2D, 0x3D, 0x3D, 0x2D, 0x2D, 0x3D, 0x3D, 0x2D, 0x2D, 0x3D, 0x3D, 0x2D, 0x2D, 0x3D, 0x3D, 0x2D}; // -==--==--==--==-
    
    // public readonly byte[] TocMagic; 
    public readonly EIoStoreTocVersion Version;
    // private readonly byte _reserved0;
    // private readonly ushort _reserved1;
    // public readonly uint TocHeaderSize; Should always be 144
    public readonly uint TocEntryCount;
    public readonly uint TocCompressedBlockEntryCount;
    public readonly uint TocCompressedBlockEntrySize;
    public readonly uint CompressionMethodNameCount;
    public readonly uint CompressionMethodNameLength;
    public readonly uint CompressionBlockSize;
    public readonly uint DirectoryIndexSize;
    public uint PartitionCount;
    // public readonly FIoContainerId ContainerId;
    // public readonly FGuid EncryptionKeyGuid;
    public readonly EIoContainerFlags ContainerFlags;
    // private readonly byte _reserved3;
    // private readonly ushort _reserved4;
    public readonly uint TocChunkPerfectHashSeedsCount;
    public ulong PartitionSize;
    public readonly uint TocChunksWithoutPerfectHashCount;
    // private readonly uint _reserved7;
    // public readonly ulong[] _reserved8;
    
    public FIoStoreTocHeader(Reader reader)
    {
        if (!reader.ReadBytes(TOC_MAGIC.Length).SequenceEqual(TOC_MAGIC))
            throw new InvalidDataException("Invalid TOC magic");

        Version = reader.Read<EIoStoreTocVersion>();

        reader.Position += 1 + 2 + sizeof(uint); // Padding + TocHeaderSize

        TocEntryCount = reader.Read<uint>();
        TocCompressedBlockEntryCount = reader.Read<uint>();
        TocCompressedBlockEntrySize = reader.Read<uint>();
        CompressionMethodNameCount = reader.Read<uint>();
        CompressionMethodNameLength = reader.Read<uint>();
        CompressionBlockSize = reader.Read<uint>();
        DirectoryIndexSize = reader.Read<uint>();
        PartitionCount = reader.Read<uint>();

        reader.Position += sizeof(ulong); // sizeof(FIoContainerId)
        reader.Position += sizeof(uint) * 4; // sizeof(FGuid)

        ContainerFlags = reader.Read<EIoContainerFlags>();

        reader.Position += 1 + 3; // Padding

        TocChunkPerfectHashSeedsCount = reader.Read<uint>();
        PartitionSize = reader.Read<uint>();
        TocChunksWithoutPerfectHashCount = reader.Read<uint>();

        reader.Position = 144; // End of header
    }
}