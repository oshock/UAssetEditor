namespace UAssetEditor.Misc;

public class FGuid
{
    public uint A;
    public uint B;
    public uint C;
    public uint D;
    
    public FGuid(Reader reader)
    {
        A = reader.Read<uint>();
        B = reader.Read<uint>();
        C = reader.Read<uint>();
        D = reader.Read<uint>();
    }
}