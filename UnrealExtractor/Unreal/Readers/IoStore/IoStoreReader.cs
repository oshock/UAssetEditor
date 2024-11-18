using System.Data;
using UnrealExtractor.Binary;
using UnrealExtractor.Compression;
using UnrealExtractor.Unreal.Containers;
using UnrealExtractor.Unreal.Misc;
using UnrealExtractor.Unreal.Names;
using UnrealExtractor.Unreal.Packages;

namespace UnrealExtractor.Unreal.Readers.IoStore;

public class IoStoreReader : UnrealFileReader
{
    public FIoStoreTocResource Resource;
    
    public override bool IsEncrypted => Resource.IsEncrypted;
    public override string[] CompressionMethods => Resource.CompressionMethods;
    public bool HasDirectoryIndex => Resource.DirectoryIndexPosition > 0;

    private List<Reader> Archives = new();
    public Reader GetArchive(int partitionIndex) => Archives.ElementAt(partitionIndex);
    
    public IoStoreReader(ContainerFile? owner, string file) : base(owner, file)
    {
        var reader = new Reader(file);
        Resource = new FIoStoreTocResource(reader);

        for (int i = 0; i < Resource.Header.PartitionCount; i++)
        {
            var path = Name.Replace(".utoc", i > 0 ? $"_s{i}.ucas" : ".ucas");
            Archives.Add(new Reader(path));
        }
    }
    
    public byte[] ExtractChunk(ulong id, EIoChunkType5 type)
    {
        var index = (uint)(HashWithSeed(id, type, 0) % Resource.Header.TocChunkPerfectHashSeedsCount);
        var seed = Resource.ChunkPerfectHashSeeds?[index] ?? throw new NoNullAllowedException("ChunkPerfectHashSeeds cannot be null to extract chunk from id.");

        var slot = (uint)(HashWithSeed(id, type, seed) % Resource.Header.TocEntryCount);
        var offsetLength = Resource.OffsetAndLengths[slot];
        var blocks = FIoStoreEntry.GetCompressionBlocks(this, offsetLength);

        var data = new byte[offsetLength.Length];
        ReadBlocks(blocks, data);
        
        return data;
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

    public void ReadBlocks(FIoStoreTocCompressedBlockEntry[] blocks, byte[] data)
    {
        var offset = 0;
        
        foreach (var block in blocks)
        {
            var indexAndOffset = block.GetContainerIndexAndOffset(this);
            var archive = GetArchive(indexAndOffset.index);

            archive.Position = indexAndOffset.offset;
            var compressed = archive.ReadBytes((int)block.CompressedSize);
            var decompressed = CompressionHandler.HandleDecompression(this, block.CompressionMethodIndex, compressed, (int)block.UncompressedSize);

            if (offset + decompressed.Length > data.Length)
                throw new OutOfMemoryException("The buffer given is too small to receive all of these blocks");
            
            Buffer.BlockCopy(decompressed, 0, data, offset, decompressed.Length);
            offset += decompressed.Length;
        }
    }
    
    public override void ProcessIndex()
    {
        if (!HasDirectoryIndex || Resource.Header.DirectoryIndexSize == 0 || !IsAssignedAes)
            return;
        
        Position = Resource.DirectoryIndexPosition;

        var buffer = ReadBytes((int)Resource.Header.DirectoryIndexSize);
        var decrypted = DecryptIfEncrypted(buffer);
        var indexReader = new Reader(Name + " - Directory Index", decrypted);

        try
        {
            MountPoint = FString.Read(indexReader).Text;
        }
        catch
        {
            throw new InvalidDataException("AES key is invalid, unable to decrypt.");
        }
        
        var directoryEntries = indexReader.ReadArray<FIoDirectoryIndexEntry>();
        var fileEntries = indexReader.ReadArray<FIoFileIndexEntry>();

        var stringTableSize = indexReader.Read<int>();
        var stringTable = indexReader.ReadArray(FString.Read, stringTableSize);

        _packagesByPath = new Dictionary<string, UnrealFileEntry>();
        ReadIndex(MountPoint, 0U);
        IsMounted = true;
        return;

        // https://github.com/FabianFG/CUE4Parse/blob/0b9616c806ba53c112cf2805ad3f9f823dbb35d6/CUE4Parse/UE4/IO/IoStoreReader.cs#L333
        void ReadIndex(string directoryName, uint dir)
        {
            const uint invalidHandle = uint.MaxValue;
            
            while (dir != invalidHandle)
            {
                ref var dirEntry = ref directoryEntries[dir];
                var subDirectoryName = dirEntry.Name == invalidHandle ? directoryName : $"{directoryName}{stringTable[dirEntry.Name].Text}/";

                var file = dirEntry.FirstFileEntry;
                while (file != invalidHandle)
                {
                    ref var fileEntry = ref fileEntries[file];

                    var path = string.Concat(subDirectoryName, stringTable[fileEntry.Name].Text);
                    var entry = new FIoStoreEntry(path, Owner, fileEntry.UserData);
                    
                    _packagesByPath[path.ToLower()] = entry;
                    file = fileEntry.NextFileEntry;
                }

                ReadIndex(subDirectoryName, dirEntry.FirstChildEntry);
                dir = dirEntry.NextSiblingEntry;
            }
        }
    }
}