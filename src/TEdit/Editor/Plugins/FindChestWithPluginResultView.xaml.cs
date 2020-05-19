using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xna.Framework;

namespace TEditXna.Editor.Plugins
{
    /// <summary>
    /// Interaction logic for ReplaceAllPlugin.xaml
    /// </summary>
    public partial class FindChestWithPluginResultView : Window
    {
        private char[] splitters = new char[] { ',' };
        public FindChestWithPluginResultView(IEnumerable<Vector2> locations)
        {
            InitializeComponent();
            foreach (Vector2 location in locations)
            {
                // Was to lazy to do it with Bindings (sorry)
                LocationList.Items.Add($"{location.X}, {location.Y}");
            }
        }

        public void CloseButtonClick(object sender, RoutedEventArgs routedEventArgs)
        {
            Close();
        }

        private void ListBoxMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ( e.OriginalSource is TextBlock )
            {
                TextBlock item = e.OriginalSource as TextBlock;
                if ( !string.IsNullOrEmpty(item.Text) )
                {
                    string[] positions = item.Text.Split(splitters);
                    if ( positions.Length == 2 )
                    {
                        int x = 0;
                        int y = 0;

                        if ( int.TryParse(positions[0].Trim(), out x) && int.TryParse(positions[1].Trim(), out y) )
                        {
                            MainWindow mainwin = Application.Current.MainWindow as MainWindow;
                            if (mainwin != null)
                            {
                                mainwin.ZoomFocus(x, y);
                            }
                        }
                    }
                }
            }
        }
    }
}
