using UAssetEditor.Unreal.Exports;
using UAssetEditor.Binary;
using UAssetEditor.Classes;
using UAssetEditor.Unreal.Assets;

namespace UAssetEditor.Unreal.Properties.Structs.Math;

public class FVector2D : UStruct, IUnrealType
{
    [UnrealField]
    public float X;
    
    [UnrealField]
    public float Y;

    public override void Read(Reader reader, PropertyData? data, Asset? asset = null,
        ESerializationMode mode = ESerializationMode.Normal)
    {
        if (mode == ESerializationMode.Zero)
        {
            X = 0;
            Y = 0;
            return;
        }

        X = reader.Read<float>();
        Y = reader.Read<float>();
    }

    public override void Write(Writer writer, Asset? asset = null)
    {
        writer.Write(X);
        writer.Write(Y);
    }
}