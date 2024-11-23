using UAssetEditor.Unreal.Exports;
using UAssetEditor.Unreal.Objects;
using UAssetEditor.Unreal.Properties.Reflection;
using UAssetEditor.Unreal.Properties.Structs;
using UnrealExtractor.Binary;
using UsmapDotNet;

namespace UAssetEditor.Unreal.Properties.Types;

// TODO
public class FInstancedStruct : UStruct
{
    public FPackageIndex Index;
    public byte[] Buffer;

    public override void Read(Reader reader, PropertyData? data, Asset? asset = null,
        ESerializationMode mode = ESerializationMode.Normal)
    {
        Index = new FPackageIndex(asset, reader.Read<int>());
        var serialSize = reader.Read<int>();

        /*if (TryGetStructByIndexOrWhatever(index, out var @struct))
        {

        }
        else*/
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