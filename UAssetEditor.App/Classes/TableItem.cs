using System.Collections.ObjectModel;

namespace UAssetEditor.App.Classes;

public class TableItem
{
    public string Name { get; set; }
    public ObservableCollection<TableItem> Children { get; set; } = new();

    public UProperty Property { get; set; }
}