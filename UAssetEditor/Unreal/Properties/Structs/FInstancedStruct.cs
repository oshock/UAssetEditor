using UAssetEditor.Unreal.Exports;
using UAssetEditor.Unreal.Objects;
using UAssetEditor.Binary;
using UAssetEditor.Unreal.Assets;
using UAssetEditor.Unreal.Properties.Unversioned;

namespace UAssetEditor.Unreal.Properties.Structs;

// TODO
public class FInstancedStruct : UStruct
{
    public FPackageIndex Index;
    public UStruct? Class;
    public byte[]? Buffer;
    public List<UProperty>? Properties;

    public override void Read(Reader reader, PropertyData? data, Asset? asset = null,
        ESerializationMode mode = ESerializationMode.Normal)
    {
        Index = new FPackageIndex(asset, reader.Read<int>());
        var serialSize = reader.Read<int>();

        var className = Index.ResolvedObject?.Name.ToString();
        var struc = asset?.Mappings?.FindSchema(className ?? "None");
        
        if (struc != null)
        {
            var start = reader.Position;
            Properties = asset?.ReadProperties(className);

            var actualSize = reader.Position - start;
            if (actualSize != serialSize)
                Warning($"FInstanedStruct ({className}) was expected to read {serialSize} bytes, but actually read {actualSize}");
        }
        else
        {
            Buffer = reader.ReadBytes(serialSize);
        }
    }

    public override void Write(Writer writer, Asset? asset = null)
    {
        writer.Write(Index.Index);
        writer.Write(Buffer.Length);
        writer.WriteBytes(Buffer);
    }
}