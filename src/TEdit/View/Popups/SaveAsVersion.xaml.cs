using System;
using System.Linq;
using System.Windows;
using TEdit.Terraria;
using TEdit.Configuration;
using System.Windows.Controls;
using ReactiveUI.SourceGenerators;

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

        [ReactiveCommand]
        private void SaveAsVersion(string gameVersion)
        {
            try
            {
                var data = WorldConfiguration.SaveConfiguration.GetDataForGameVersion(gameVersion);
                WorldVersion = (uint)data.SaveVersion;
                this.DialogResult = true;
                this.Close();
            }
            catch
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
                var versions = WorldConfiguration.SaveConfiguration.GameVersionToSaveVersion.Keys
                    .Select(v => new
                    {
                        Raw = v,
                        Parsed = Version.TryParse(v, out var ver) ? ver : new Version(0, 0)
                    })
                    .OrderByDescending(x => x.Parsed)
                    .Select(x => x.Raw)
                    .ToList();

                // Iterate over the SaveVersions values in reverse order.
                foreach (var gv in versions)
                {
                    // Create a new Button for each version.
                    Button button = new()
                    {
                        // Set the button content to the game version, removing the leading "v" character.
                        Content = gv,
                        Width = 50,  // Set the width.
                        Height = 20, // Set the height.
                        Margin = new Thickness(5), // Add margin around the button for spacing.
                                                   // Set up the command and command parameter for each button.
                        Command = SaveAsVersionCommand
                    };
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
