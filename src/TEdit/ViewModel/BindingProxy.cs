using System.Windows;

namespace TEdit.ViewModel;

/// <summary>
/// A proxy class that allows bindings in Styles and other non-visual-tree elements
/// to access the DataContext. This is necessary because elements like SolidColorBrush
/// inside Style setters cannot directly bind to the DataContext.
/// </summary>
public class BindingProxy : Freezable
{
    protected override Freezable CreateInstanceCore()
    {
        return new BindingProxy();
    }

    public object Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(
            nameof(Data),
            typeof(object),
            typeof(BindingProxy),
            new UIPropertyMetadata(null));
}
