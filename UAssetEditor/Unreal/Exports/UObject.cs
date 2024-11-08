namespace UAssetEditor.Unreal.Exports;

public class UObject
{
    public string? Name;
    public UObject? Outer;
    public UObject? Class;
    public UObject? Super;
    public UObject? Template;

    public Lazy<List<UProperty>>? Properties;

    public Asset? Owner;
}