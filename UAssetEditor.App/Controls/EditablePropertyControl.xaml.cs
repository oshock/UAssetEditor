using System.Windows.Controls;

namespace UAssetEditor.App.Controls;

public enum PropertyType
{
    String,
    Int,
    Float,
    Enum
}

public partial class EditablePropertyControl : UserControl
{
    public PropertyType Type { get; }
    private string _backendType { get; }

    private UProperty _ref { get; }

    public object Value;
    public bool IsNull => ReferenceEquals(Value, "None");

    public string Text
    {
        get => TextBox.Text;
        set => TextBox.Text = value;
    }
    
    // TODO
    public EditablePropertyControl(UProperty reference)
    {
        /*_ref = reference;
        _backendType = _ref.Type;
        Value = _ref.Value ?? "None";
        
        switch (reference.Type)
        {
            case "SoftObjectProperty":
            case "NameProperty":
            case "StrProperty":
            case "TextProperty":
                Text = Value.ToString()!;
                break;
            case "Int8Property":
            case "Int16Property":
            case "IntProperty":
            case "Int64Property":
            case "UInt16Property":
            case "UInt64Property":
                Text = Convert.ToInt64(Value).ToString();
                break;
        }

        InitializeComponent();*/
    }
}