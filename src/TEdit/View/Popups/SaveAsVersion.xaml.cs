using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TEdit.Terraria;

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
                    .OrderByDescending(v => v, DottedVersionComparer.Instance)
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

    // Compares version strings like "1.4.4.8.1" and "1.4.5.0" correctly,
    // even when they have MORE than 4 dot parts (System.Version can't parse those).
    sealed class DottedVersionComparer : IComparer<string>
    {
        // Reuse one instance (no need to allocate a comparer each time).
        public static readonly DottedVersionComparer Instance = new();

        public int Compare(string x, string y)
        {
            // Fast paths / null handling (keeps sorting stable).
            if (ReferenceEquals(x, y)) return 0;
            if (x is null) return -1;
            if (y is null) return 1;

            // Normalize whitespace.
            x = x.Trim();
            y = y.Trim();

            // Allow inputs like "v1.4.5.0" by stripping leading "v".
            if (x.StartsWith("v", StringComparison.OrdinalIgnoreCase)) x = x.Substring(1);
            if (y.StartsWith("v", StringComparison.OrdinalIgnoreCase)) y = y.Substring(1);

            // Split into numeric segments: "1.4.4.8.1" -> ["1","4","4","8","1"].
            var xa = x.Split('.');
            var ya = y.Split('.');

            // Compare each numeric segment left-to-right.
            // Missing segments are treated as 0, so "1.4" == "1.4.0" and "1.4.4.8" < "1.4.4.8.1".
            int n = Math.Max(xa.Length, ya.Length);
            for (int i = 0; i < n; i++)
            {
                int xi = (i < xa.Length && int.TryParse(xa[i], out var a)) ? a : 0;
                int yi = (i < ya.Length && int.TryParse(ya[i], out var b)) ? b : 0;

                int cmp = xi.CompareTo(yi);
                if (cmp != 0) return cmp; // First difference decides ordering.
            }

            // All segments equal.
            return 0;
        }
    }
}
