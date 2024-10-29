using System.Windows.Controls;
using System.Windows.Input;

namespace UAssetEditor.App.Controls;

public class Property
{
    public string Name;
    //public List<Property> SubProperties;
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

    public static PropertyControl Create(UProperty property)
    {
        var prop = new Property
        {
            Name = property.Name,
            PropertyReference = property
        };

        return new PropertyControl
        {
            Property = prop
        };
    }

    public Property Property;

    public void Refresh()
    {
        PropertyName.Text = Property.Name;
        TextBox.Text = Property.PropertyReference.Value?.ToString() ?? "None";
    }

    private void UIElement_OnMouseEnter(object sender, MouseEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void Edit(object sender, MouseButtonEventArgs e)
    {
        
    }
}