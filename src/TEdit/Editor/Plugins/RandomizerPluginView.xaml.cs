
using System;
using System.Windows;

namespace TEdit.Editor.Plugins;

/// <summary>
/// Interaction logic for RandomizerPluginView.xaml
/// </summary>
public partial class RandomizerPluginView : Window
{
    public int Seed { get; private set; }
    public bool OnlySelection { get; private set; }
    public bool EnableUndo { get; private set; }
    public bool EnableWallRandomize { get; private set; }
    public bool NoDisappearingBlocks { get; private set; }
    public bool SupportDependentBlocks { get; private set; }
    public bool SupportGravityBlocks { get; private set; }

    public RandomizerPluginView(bool activeSelection)
    {
        InitializeComponent();
        if (!activeSelection)
        {
            OnlySelectionCheckBox.IsEnabled = false;
        }
    }

    private void OkButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (SeedTextBox.Text == "")
            {
                Seed = (int)DateTime.Now.Ticks;
            }
            else
            {
                Seed = SeedTextBox.Text.GetHashCode();
            }
            OnlySelection = OnlySelectionCheckBox.IsChecked ?? false;
            EnableUndo = UndoCheckBox.IsChecked ?? false;
            EnableWallRandomize = RandomizeWallsCheckBox.IsChecked ?? false;
            NoDisappearingBlocks = NoDisappearingBlocksCheckBox.IsChecked ?? false;
            SupportDependentBlocks = SupportDependentBlocksCheckBox.IsChecked ?? false;
            SupportGravityBlocks = SupportGravityBlocksCheckBox.IsChecked ?? false;

            this.DialogResult = true;
            this.Close();
        }
        catch (System.Exception)
        {
            this.DialogResult = false;
            this.Close();
        }
    }

    private void CancelButtonClick(object sender, RoutedEventArgs e)
    {
        this.DialogResult = false;
        this.Close();
    }
}
