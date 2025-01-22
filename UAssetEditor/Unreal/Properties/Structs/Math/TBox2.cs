using UAssetEditor.Binary;
using UAssetEditor.Classes;
using UAssetEditor.Unreal.Assets;
using UAssetEditor.Unreal.Exports;

namespace UAssetEditor.Unreal.Properties.Structs.Math;

public class TBox2<T> : UStruct, IUnrealType
{
    [UField] 
    public TIntVector2<T> Min;

    [UField]
    public TIntVector2<T> Max;

    [UField] 
    public byte bIsValid;

    public override void Read(Reader reader, PropertyData? data, Asset? asset = null,
        ESerializationMode mode = ESerializationMode.Normal)
    {
        if (mode == ESerializationMode.Zero)
        {
            return;
        }

        Min = new TIntVector2<T>();
        Min.Read(reader, null);
        Max = new TIntVector2<T>();
        Max.Read(reader, null);
        bIsValid = reader.ReadByte();
    }

    public override void Write(Writer writer, Asset? asset = null)
    {
        Min.Write(writer);
        Max.Write(writer);
        writer.WriteByte(bIsValid);
    }
}