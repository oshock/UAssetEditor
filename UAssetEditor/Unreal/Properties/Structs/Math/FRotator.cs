using UAssetEditor.Unreal.Exports;
using UAssetEditor.Binary;
using UAssetEditor.Classes;

namespace UAssetEditor.Unreal.Properties.Structs.Math;

public class FRotator : UStruct, IUnrealType
{
    [UnrealField]
    public float Pitch;
    
    [UnrealField]
    public float Yaw;
    
    [UnrealField]
    public float Roll;

    public override void Read(Reader reader, PropertyData? data, Asset? asset = null,
        ESerializationMode mode = ESerializationMode.Normal)
    {
        if (mode == ESerializationMode.Zero)
        {
            Pitch = 0;
            Yaw = 0;
            Roll = 0;
            return;
        }

        //if (EUnrealEngineObjectUE5Version.LARGE_WORLD_COORDINATES)
        {
            Pitch = (float)reader.Read<double>();
            Yaw = (float)reader.Read<double>();
            Roll = (float)reader.Read<double>();
        }
    }

    public override void Write(Writer writer, Asset? asset = null)
    {
        //if (EUnrealEngineObjectUE5Version.LARGE_WORLD_COORDINATES)
        {
            writer.Write((double)Pitch);
            writer.Write((double)Yaw);
            writer.Write((double)Roll);
        }
    }
}