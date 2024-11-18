using UnrealExtractor.Binary;
using UnrealExtractor.Encryption.Aes;
using UnrealExtractor.Unreal.Containers;
using UnrealExtractor.Unreal.Readers;
using UnrealExtractor.Unreal.Readers.IoStore;
using UnrealExtractor.Utils;

namespace UnrealExtractor.Unreal.Containers;

public class IoFile : ContainerFile
{
    public IoStoreReader ReaderAsIoReader => Reader.As<IoStoreReader>();
    public FIoStoreTocHeader Header => Resource.Header;
    public FIoStoreTocResource Resource => ReaderAsIoReader.Resource;
    public uint CompressionBlockSize => Header.CompressionBlockSize;
    
    public override bool IsEncrypted => Resource.IsEncrypted;
    
    public IoFile(string path, UnrealFileSystem? system = null) : base(path, system)
    {
        Reader = new IoStoreReader(this, path);
        
        if (System?.AesKeys.TryGetValue(Header.EncryptionKeyGuid, out var key) ?? false)
            ReaderAsIoReader.SetAesKey(key);
    }
    
    public IoFile(Reader reader, UnrealFileSystem? system = null) : base("", system)
    {
        Reader = reader;
    }

    public override void Mount()
    {
        if (Reader is not IoStoreReader)
            throw new ApplicationException("Cannot mount a non I/O store container");
        
        ReaderAsIoReader.ProcessIndex();
    }
}