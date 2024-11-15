using UnrealExtractor.Binary;
using UnrealExtractor.Unreal.Containers;
using UnrealExtractor.Unreal.Misc;
using UnrealExtractor.Unreal.Packages;

namespace UnrealExtractor.Unreal.Readers.IoStore;

public class IoStoreReader : AbstractFileReader
{
    public FIoStoreTocResource Resource;

    public string? MountPoint;
    
    public override bool IsEncrypted => Resource.IsEncrypted;
    public bool HasDirectoryIndex => Resource.DirectoryIndexPosition > 0;
    
    public IoStoreReader(ContainerFile owner, string file) : base(owner, file)
    {
        var reader = new Reader(file);
        Resource = new FIoStoreTocResource(reader);
    }
    
    public override void ProcessIndex()
    {
        if (!HasDirectoryIndex || Resource.Header.DirectoryIndexSize == 0 || !IsAssignedAes)
            return;
        
        Position = Resource.DirectoryIndexPosition;

        var buffer = ReadBytes((int)Resource.Header.DirectoryIndexSize);
        var decrypted = DecryptIfEncrypted(buffer);
        var indexReader = new Reader(decrypted);

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

        _packagesByPath = new Dictionary<string, Package>();
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