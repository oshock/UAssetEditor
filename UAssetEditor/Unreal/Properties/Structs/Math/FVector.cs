using UnrealExtractor.Binary;
using UsmapDotNet;

namespace UAssetEditor.Unreal.Properties.Structs.Math;

public class FVector : UStruct
{
    public float A;
    public float B;
    public float C;

    public override void Read(Reader reader, UsmapPropertyData? data, Asset? asset = null, EReadMode mode = EReadMode.Normal)
    {
        if (mode == EReadMode.Zero)
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