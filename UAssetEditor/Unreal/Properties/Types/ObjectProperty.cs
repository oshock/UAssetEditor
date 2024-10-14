

using UsmapDotNet;

namespace UAssetEditor.Properties;

public class ObjectProperty : AbstractProperty
{
    // TODO find asset reference (import)
    public string Text = "None";

    public bool IsExport => (int)Value > 0;
    public bool IsImport => (int)Value < 0;
    public bool IsNull => (int)Value == 0;
    
    public override void Read(Reader reader, UsmapPropertyData? data, BaseAsset asset = null)
    {
        Value = reader.Read<int>();
        
        if (asset is ZenAsset zen)
        {
            if (IsNull)
                return;

            if (IsExport)
            {
                var index = (int)Value - 1;
                if (index < zen.ExportMap.Length)
                {
                    var nameIndex = (int)zen.ExportMap[index].ObjectName.NameIndex;
                    if (nameIndex < zen.NameMap.Length)
                        Text = zen.NameMap[nameIndex];
                }
            }
            else if (IsImport)
            {
                // not implemented
            }
        }
    }
}