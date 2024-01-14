using UAssetEditor.Binary;

namespace UAssetEditor.Unreal.Names;


public class FName
{
    public string Name = "None";
    public int NameIndex;
    public int ExtraIndex;

    public FName(Reader reader, NameMapContainer nameMap)
    {
        NameIndex = reader.Read<int>();
        ExtraIndex = reader.Read<int>();
        if (nameMap.Length > NameIndex && NameIndex >= 0)
            Name = nameMap[NameIndex];
    }

    public FName(NameMapContainer nameMapContainer, string name, int extraIndex)
    {
        Name = name;
        NameIndex = nameMapContainer.GetIndex(name);
        ExtraIndex = extraIndex;
    }

    public void Serialize(Writer writer, NameMapContainer nameMapContainer)
    {
        writer.Write(nameMapContainer.GetIndex(Name));
        writer.Write(ExtraIndex);
    }

    public void Serialize(Writer writer)
    {
        writer.Write(NameIndex);
        writer.Write(ExtraIndex);
    }
}