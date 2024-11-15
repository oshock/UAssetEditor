using UnrealExtractor.Encryption.Aes;
using UnrealExtractor.Unreal.Containers;
using UnrealExtractor.Unreal.Readers.IoStore;
using UnrealExtractor.Utils;

namespace UnrealExtractor.Unreal.Containers;

public class IoFile : ContainerFile
{
    public FIoStoreTocHeader Header => Resource.Header;
    public FIoStoreTocResource Resource => Reader!.As<IoStoreReader>().Resource;
    public uint CompressionBlockSize => Header.CompressionBlockSize;
    
    public override bool IsEncrypted => Resource.IsEncrypted;
    
    public IoFile(string path, UnrealFileSystem? system = null) : base(path, system)
    {
        Reader = new IoStoreReader(this, path);
        
        if (System?.AesKeys?.TryGetValue(Header.EncryptionKeyGuid, out var key) ?? false)
            Reader.SetAesKey(key);
    }

    public override void Mount()
    {
        Reader.ProcessIndex();
    }
}