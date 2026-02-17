using System.Windows;
using System.Windows.Controls;
using TEdit.Terraria.Player;

namespace TEdit.View.Sidebar.PlayerEditor;

public partial class PlayerBuffsView : UserControl
{
    public PlayerBuffsView()
    {
        InitializeComponent();
    }

    private void ClearBuff_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is PlayerBuff buff)
        {
            buff.Type = 0;
            buff.Time = 0;
        }
    }
}
