using UAssetEditor.Unreal.Exports;
using UAssetEditor.Binary;
using UAssetEditor.Classes;

namespace UAssetEditor.Unreal.Properties.Structs.Math;

public class FVector : UStruct, IUnrealType
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
        
        //if (EUnrealEngineObjectUE5Version.LARGE_WORLD_COORDINATES)
        {
            A = (float)reader.Read<double>();
            B = (float)reader.Read<double>();
            C = (float)reader.Read<double>();
        }
    }

    public override void Write(Writer writer, Asset? asset = null)
    {
        //if (EUnrealEngineObjectUE5Version.LARGE_WORLD_COORDINATES)
        {
            writer.Write((double)A);
            writer.Write((double)B);
            writer.Write((double)C);
        }
    }
}