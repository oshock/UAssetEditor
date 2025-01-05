using UAssetEditor.Unreal.Exports;
using UAssetEditor.Binary;
using UAssetEditor.Classes;
using UAssetEditor.Unreal.Assets;

namespace UAssetEditor.Unreal.Properties.Structs.Math;

public class FQuat : UStruct, IUnrealType
{
    [UnrealField]
    public float X;
    
    [UnrealField]
    public float Y;
    
    [UnrealField]
    public float Z;
    
    [UnrealField]
    public float W;

    public override void Read(Reader reader, PropertyData? data, Asset? asset = null,
        ESerializationMode mode = ESerializationMode.Normal)
    {
        if (mode == ESerializationMode.Zero)
        {
            X = 0;
            Y = 0;
            Z = 0;
            W = 0;
            return;
        }
        
        //if (EUnrealEngineObjectUE5Version.LARGE_WORLD_COORDINATES)
        {
            X = (float)reader.Read<double>();
            Y = (float)reader.Read<double>();
            Z = (float)reader.Read<double>();
            W = (float)reader.Read<double>();
        }
    }

    public override void Write(Writer writer, Asset? asset = null)
    {
        //if (EUnrealEngineObjectUE5Version.LARGE_WORLD_COORDINATES)
        {
            writer.Write((double)X);
            writer.Write((double)Y);
            writer.Write((double)Z);
            writer.Write((double)W);
        }
    }
}