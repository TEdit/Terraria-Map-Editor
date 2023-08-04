using System;
using System.Windows;
using System.Windows.Controls;

namespace TEdit.Editor.Plugins;

/// <summary>
/// Interaction logic for ReplaceAllPlugin.xaml
/// </summary>
public partial class FindTileWithPluginView : Window
{
    public string BlockToFind { get; private set; }
    public string WallToFind { get; private set; }
    public string SpriteToFind { get; private set; }
    public int MaxVolumeLimit { get; private set; }
    public FindTileWithPluginView()
    {
        InitializeComponent();
        NUDTextBox.Text = startvalue.ToString();
    }

    private void CancelButtonClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void SearchButtonClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        BlockToFind = BlockLookup.Text;
        WallToFind = WallLookup.Text;
        MaxVolumeLimit = int.Parse(NUDTextBox.Text);
        Close();
    }

    int minvalue = 1,
    maxvalue = 80640000,
    startvalue = 500;

    private void NUDButtonUP_Click(object sender, RoutedEventArgs e)
    {
        int number;
        if (NUDTextBox.Text != "") number = Convert.ToInt32(NUDTextBox.Text);
        else number = 0;
        if (number < maxvalue)
            NUDTextBox.Text = Convert.ToString(number + 1);
    }

    private void NUDButtonDown_Click(object sender, RoutedEventArgs e)
    {
        int number;
        if (NUDTextBox.Text != "") number = Convert.ToInt32(NUDTextBox.Text);
        else number = 0;
        if (number > minvalue)
            NUDTextBox.Text = Convert.ToString(number - 1);
    }

    private void NUDTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        int number = 0;
        if (NUDTextBox.Text != "")
            if (!int.TryParse(NUDTextBox.Text, out number)) NUDTextBox.Text = startvalue.ToString();
        if (number > maxvalue) NUDTextBox.Text = maxvalue.ToString();
        if (number < minvalue) NUDTextBox.Text = minvalue.ToString();
        NUDTextBox.SelectionStart = NUDTextBox.Text.Length;

    }
}
