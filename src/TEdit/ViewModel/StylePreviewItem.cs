using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace TEdit.ViewModel;

/// <summary>
/// Represents a style option with a texture preview for display in comboboxes.
/// Used for tree styles, background options, and cave styles in WorldPropertiesView.
/// </summary>
public class StylePreviewItem : INotifyPropertyChanged
{
    private object _value;
    private string _displayName;
    private WriteableBitmap _preview;

    /// <summary>
    /// The value of this style (stored in world file).
    /// Type varies: byte for TreeStyle/CaveStyle, int for BgTree/etc.
    /// </summary>
    public object Value
    {
        get => _value;
        set
        {
            if (!Equals(_value, value))
            {
                _value = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Display name shown in the combobox alongside the preview.
    /// </summary>
    public string DisplayName
    {
        get => _displayName;
        set
        {
            if (_displayName != value)
            {
                _displayName = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Scaled texture preview (max 128px dimension).
    /// </summary>
    public WriteableBitmap Preview
    {
        get => _preview;
        set
        {
            if (_preview != value)
            {
                _preview = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
