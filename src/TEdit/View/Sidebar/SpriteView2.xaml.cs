using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace TEdit.View;

/// <summary>
/// Interaction logic for SpriteView2.xaml
/// </summary>
public partial class SpriteView2 : UserControl
{
    public SpriteView2()
    {
        InitializeComponent();
    }

    private void TextBox_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            BindingExpression binding = ((TextBox)sender).GetBindingExpression(TextBox.TextProperty);
            binding.UpdateSource();
        }
    }
}
