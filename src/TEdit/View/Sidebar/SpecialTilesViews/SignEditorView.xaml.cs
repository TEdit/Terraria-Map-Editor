using System.Windows.Controls;
using System.Windows.Input;

namespace TEdit.View.Sidebar.SpecialTilesViews;

public partial class SignEditorView : UserControl
{
    public SignEditorView()
    {
        InitializeComponent();
    }

    private void ValidateLines(object sender, KeyEventArgs e)
    {
        // Limit to 10 lines
        var tb = sender as TextBox;

        if (tb != null)
        {
            if (e.Key == Key.Enter)
            {
                if (tb.LineCount > 9)
                {
                    e.Handled = true;
                }
            }
        }
    }
}
