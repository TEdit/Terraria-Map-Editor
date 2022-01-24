
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TEdit.Editor.Plugins
{
    /// <summary>
    /// Interaction logic for RandomizerPluginView.xaml
    /// </summary>
    public partial class RandomizerPluginView : Window
    {
        public int Seed { get; private set; }
        public bool OnlySelection { get; private set; }
        public bool EnableUndo { get; private set; }
        public bool EnableWallRandomize { get; private set; }

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
}
