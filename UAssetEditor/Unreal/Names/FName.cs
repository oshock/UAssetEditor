
using UAssetEditor.Binary;

namespace UAssetEditor.Unreal.Names;


public class FName
{
    public string Name = "None";
    public int NameIndex;
    public int ExtraIndex;

    public FName(string name)
    {
        Name = name;
        NameIndex = -1;
        ExtraIndex = 0;
    }
    
    public FName() : this("None")
    { }
    
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
        writer.Write(nameMapContainer.GetIndexOrAdd(Name));
        writer.Write(ExtraIndex);
    }

    public void Serialize(Writer writer)
    {
        writer.Write(NameIndex);
        writer.Write(ExtraIndex);
    }

    public override string ToString()
    {
        return Name;
    }
}