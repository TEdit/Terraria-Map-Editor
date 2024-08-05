using System;
using System.Linq;
using System.Windows;
using TEdit.Terraria;
using TEdit.Configuration;
using System.Windows.Input;
using System.Windows.Controls;
using TEdit.Common.Reactive.Command; // TE4: using GalaSoft.MvvmLight.Command;

namespace TEdit.UI.Xaml
{
    /// <summary>
    /// Interaction logic for SaveAsVersionGUI.xaml
    /// </summary>
    /// 
    public partial class SaveAsVersionGUI : Window
    {
        // Using a DependencyProperty as the backing store for WorldVersion. This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WorldVersionProperty =
            DependencyProperty.Register("WorldVersion", typeof(uint), typeof(SaveAsVersionGUI), new PropertyMetadata((uint)0));

        private ICommand _saveAsCommand;

        public SaveAsVersionGUI()
        {
            InitializeComponent();
            DataContext = this;

            // Call the method to load and display version buttons.
            LoadVersions();
        }

        public uint WorldVersion
        {
            get { return (uint)GetValue(WorldVersionProperty); }
            set { SetValue(WorldVersionProperty, value); }
        }

        public ICommand SaveAsVersionCommand
        {
            get { return _saveAsCommand ??= new RelayCommand<string>(SaveAsVersionCommandAction); }
        }

        private void SaveAsVersionCommandAction(string gameVersion)
        {
            if (WorldConfiguration.SaveConfiguration.GameVersionToSaveVersion.TryGetValue(gameVersion, out uint worldVersion))
            {
                WorldVersion = worldVersion;
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                WorldVersion = WorldConfiguration.CompatibleVersion;
                this.DialogResult = false;
                this.Close();
            }
        }

        private void LoadVersions()
        {
            try
            {
                // Iterate over the SaveVersions values in reverse order.
                foreach (var version in WorldConfiguration.SaveConfiguration.SaveVersions.Values.Reverse())
                {
                    // Create a new Button for each version.
                    Button button = new()
                    {
                        // Set the button content to the game version, removing the leading "v" character.
                        Content = version.GameVersion.Substring(1), // e.g., "v1.2" becomes "1.2".
                        Width = 50,  // Set the width.
                        Height = 20, // Set the height.
                        Margin = new Thickness(5) // Add margin around the button for spacing.
                    };

                    // Set up the command and command parameter for each button.
                    button.Command = SaveAsVersionCommand;
                    button.CommandParameter = button.Content;

                    // Add the newly created button to the WrapPanel (ButtonPanel).
                    ButtonPanel.Children.Add(button);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the version loading process.
                MessageBox.Show("An error occurred while loading versions: " + ex.Message);
            }
        }
    }
}
