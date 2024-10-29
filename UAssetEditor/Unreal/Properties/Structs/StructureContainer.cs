using System.Xml.Linq;
using UAssetEditor.Classes;
using UsmapDotNet;

namespace UAssetEditor.Unreal.Properties.Structs;

public class LoadedStructure(StructureContainer container, UsmapSchema schema)
{
    public readonly UsmapSchema Schema = schema;
    public string Name => Schema.Name;
    public UsmapProperty[] Properties => Schema.Properties;
    
    public void Release()
    {
        container.Remove(this);
    }
}

public class StructureContainer() : Container<LoadedStructure>(new List<LoadedStructure>())
{
    public UsmapSchema? this[string name]
    {
        get
        {
            foreach (var structure in Items)
            {
                if (structure.Name != name)
                    continue;

                return structure.Schema;
            }

            return null;
        }
    }


    public bool Contains(string name)
    {
        return Items.Any(structure => structure.Name == name);
    }

    public void Add(UsmapSchema schema)
    {
        if (!Contains(schema.Name))
            Items.Add(new LoadedStructure(this, schema));
    }
}