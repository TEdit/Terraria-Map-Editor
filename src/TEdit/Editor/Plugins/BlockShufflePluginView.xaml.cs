using System;
using System.Windows;

namespace TEdit.Editor.Plugins
{
    public partial class BlockShufflePluginView : Window
    {
        public int Seed { get; private set; }
        public bool SensitivePlatform { get; private set; }
        public bool OnlySelection { get; private set; }
        public bool EnableUndo { get; private set; }
        public int ReplaceEmptyPercentage { get; private set; }
        public bool ConsiderWallEmpty { get; private set; }
        public bool ConsiderLiquidEmpty { get; private set; }
        public bool ConsiderEverything { get; private set; }

        public BlockShufflePluginView(bool activeSelection)
        {
            InitializeComponent();
            if (activeSelection)
                SelectionRadio.IsChecked = true;
            else
            {
                SelectionRadio.IsEnabled = false;
                WorldRadio.IsChecked = true; //Doesn't update when set in .xaml
            }
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SeedTextBox.Text == "") Seed = (int)DateTime.Now.Ticks;
                else Seed = SeedTextBox.Text.GetHashCode();
                SensitivePlatform = SensitivePlatformCheckBox.IsChecked ?? false;
                OnlySelection = SelectionRadio.IsChecked ?? false;
                EnableUndo = UndoCheckBox.IsChecked ?? false;
                ReplaceEmptyPercentage = (int)ReplaceEmptySlider.Value;
                if (ReplaceEmptyPercentage > 100) ReplaceEmptyPercentage = 100;
                if (ReplaceEmptyPercentage < 0) ReplaceEmptyPercentage = 0;
                ConsiderWallEmpty = ReplaceWallCheckBox.IsChecked ?? false;
                ConsiderLiquidEmpty = ReplaceLiquidCheckBox.IsChecked ?? false;
                ConsiderEverything = ReplaceEverythingCheckBox.IsChecked ?? false;
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

        private void ReplaceEverythingCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ReplaceWallCheckBox.IsEnabled = false;
            ReplaceLiquidCheckBox.IsEnabled = false;
        }
        private void ReplaceEverythingCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ReplaceWallCheckBox.IsEnabled = true;
            ReplaceLiquidCheckBox.IsEnabled = true;
        }
    }
}
