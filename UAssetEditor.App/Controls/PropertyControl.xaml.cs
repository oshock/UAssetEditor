using System.Windows.Controls;
using System.Windows.Input;

namespace UAssetEditor.App.Controls;

public class Property
{
    public string Name;
    public List<Property> SubProperties;
    public UProperty PropertyReference;

    public void Show(StackPanel panel)
    {
        // TODO EditablePropertyControl handle
    }
}

public partial class PropertyControl : UserControl
{
    public PropertyControl()
    {
        InitializeComponent();
    }

    public Property Property;

    public string Text
    {
        get => TextBox.Text;
        set => TextBox.Text = value;
    }

    private void UIElement_OnMouseEnter(object sender, MouseEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void Edit(object sender, MouseButtonEventArgs e)
    {
        
    }
}