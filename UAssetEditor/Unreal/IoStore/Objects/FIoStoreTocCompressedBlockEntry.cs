using UAssetEditor.Utils;

namespace UAssetEditor.Unreal.IoStore.Objects;

public class FIoStoreTocCompressedBlockEntry
{
    private const int SIZE = 5 + 3 + 3 + 1;
    
    public long Offset;
    public byte ContainerIndex;
    public uint CompressedSize;
    public uint UncompressedSize;
    
    public FIoStoreTocCompressedBlockEntry(Reader reader)
    {
        unsafe
        {
            var data = reader.ReadBytes(SIZE);
            var offset = data.Range(0, 5);
        }
    }
}