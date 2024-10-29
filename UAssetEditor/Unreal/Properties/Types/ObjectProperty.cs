

using UAssetEditor.Binary;
using UsmapDotNet;

namespace UAssetEditor.Unreal.Properties.Types;

public class ObjectProperty : AbstractProperty<int>
{
    // TODO find asset reference (import)
    public string Text = "None";

    public bool IsExport => (int)Value > 0;
    public bool IsImport => (int)Value < 0;
    public bool IsNull => (int)Value == 0;
    
    public override void Read(Reader reader, UsmapPropertyData? data, BaseAsset? asset = null, EReadMode mode = EReadMode.Normal)
    {
        Value = mode == EReadMode.Zero ? 0 : reader.Read<int>();

        if (asset is not ZenAsset zen) return;
        
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
    
    public override void Write(Writer writer, UProperty property, BaseAsset? asset = null)
    {
        writer.Write(Value);
    }
}