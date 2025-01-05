using UAssetEditor.Unreal.Exports;
using UAssetEditor.Binary;
using UAssetEditor.Classes;
using UAssetEditor.Unreal.Assets;

namespace UAssetEditor.Unreal.Properties.Structs.Math;

public class FLinearColor : UStruct, IUnrealType
{
    [UnrealField]
    public float R;
    
    [UnrealField]
    public float G;
    
    [UnrealField]
    public float B;
    
    [UnrealField]
    public float A;

    public override void Read(Reader reader, PropertyData? data, Asset? asset = null,
        ESerializationMode mode = ESerializationMode.Normal)
    {
        if (mode == ESerializationMode.Zero)
        {
            R = 0;
            G = 0;
            B = 0;
            A = 0;
            return;
        }

        R = reader.Read<float>();
        G = reader.Read<float>();
        B = reader.Read<float>();
        A = reader.Read<float>();
    }

    public override void Write(Writer writer, Asset? asset = null)
    {
        writer.Write(R);
        writer.Write(G);
        writer.Write(B);
        writer.Write(A);
    }
}