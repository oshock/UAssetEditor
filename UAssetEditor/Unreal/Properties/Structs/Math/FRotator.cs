using UAssetEditor.Binary;
using UsmapDotNet;

namespace UAssetEditor.Unreal.Properties.Structs.Math;

public class FRotator : UStruct
{
    public float Pitch;
    public float Yaw;
    public float Roll;

    public override void Read(Reader reader, UsmapPropertyData? data, Asset? asset = null, EReadMode mode = EReadMode.Normal)
    {
        if (mode == EReadMode.Zero)
        {
            Pitch = 0;
            Yaw = 0;
            Roll = 0;
            return;
        }
        
        Pitch = reader.Read<float>();
        Yaw = reader.Read<float>();
        Roll = reader.Read<float>();
    }

    public override void Write(Writer writer, Asset? asset = null)
    {
        writer.Write(Pitch);
        writer.Write(Yaw);
        writer.Write(Roll);
    }
}