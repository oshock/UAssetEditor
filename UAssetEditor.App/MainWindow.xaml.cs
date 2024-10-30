using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UAssetEditor.App.Classes;
using UAssetEditor.App.Controls;
using UAssetEditor.Unreal.Properties.Types;

namespace UAssetEditor.App;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public ObservableCollection<TableItem> Items { get; set; }
    
    public MainWindow()
    {
        InitializeComponent();
        Test();
    }

    public ZenAsset? Asset;
    
    public void Test()
    {
        Asset = new ZenAsset("C:\\Users\\Owen\\Documents\\FModel\\Output\\Exports\\DefaultGameDataCosmetics.uasset");
        Asset.Initialize("C:\\Fortnite\\FortniteGame\\Content\\Paks\\global.utoc");
        Asset.LoadMappings("++Fortnite+Release-31.41-CL-37324991-Windows_oo.usmap");
        Asset.ReadAll();

        Populate();
    }

    public void Populate()
    {
        if (Asset is null)
            throw new NoNullAllowedException("Cannot populate without an asset loaded.");

        TableItem MakeTableItem(UProperty property)
        {
            var item = new TableItem
            {
                Name = property.Name,
                Property = property
            };

            if (property.Value is ArrayProperty array)
            {
                var properties = (List<AbstractProperty>)array.ValueAsObject;
                foreach (var elm in properties)
                {
                    MakeTableItem(elm);
                }
            }

            return item;
        }
        
        foreach (var container in Asset.Properties)
        {
            foreach (var property in container.Value)
            {
                var item = MakeTableItem(property);
            }
        }
    }
}