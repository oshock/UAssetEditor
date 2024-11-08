using UAssetEditor.Binary;
using UsmapDotNet;

namespace UAssetEditor;

public class PakAsset : Asset
{
    public PakAsset(byte[] data) : base(data)
    {
    }

    public PakAsset(string path) : base(path)
    {
    }

    public override void ReadAll()
    {
        throw new NotImplementedException();
    }

    public override uint ReadHeader()
    {
        throw new NotImplementedException();
    }

    // TODO when I actually implement pak assets
    public override List<UProperty> ReadProperties(UsmapSchema structure)
    {
        throw new NotImplementedException();
    }

    public override Writer WriteProperties(string type, int exportIndex, List<UProperty> properties)
    {
        throw new NotImplementedException();
    }

    public override void WriteAll(Writer writer)
    {
        throw new NotImplementedException();
    }

    public override void Fix()
    {
        throw new NotImplementedException();
    }

    public override void WriteHeader(Writer writer)
    {
        throw new NotImplementedException();
    }
}