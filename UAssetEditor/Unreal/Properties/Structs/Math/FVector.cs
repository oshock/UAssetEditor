using UAssetEditor.Unreal.Exports;
using UnrealExtractor.Binary;
using UsmapDotNet;

namespace UAssetEditor.Unreal.Properties.Structs.Math;

public class FVector : UStruct
{
    public float A;
    public float B;
    public float C;

    public override void Read(Reader reader, PropertyData? data, Asset? asset = null,
        ESerializationMode mode = ESerializationMode.Normal)
    {
        if (mode == ESerializationMode.Zero)
        {
            A = 0;
            B = 0;
            C = 0;
            return;
        }
        
        A = reader.Read<float>();
        B = reader.Read<float>();
        C = reader.Read<float>();
    }

    public override void Write(Writer writer, Asset? asset = null)
    {
        writer.Write(A);
        writer.Write(B);
        writer.Write(C);
    }
}