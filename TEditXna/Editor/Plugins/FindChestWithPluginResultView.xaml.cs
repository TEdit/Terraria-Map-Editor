using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Xna.Framework;

namespace TEditXna.Editor.Plugins
{
    /// <summary>
    /// Interaction logic for ReplaceAllPlugin.xaml
    /// </summary>
    public partial class FindChestWithPluginResultView : Window
    {
        public FindChestWithPluginResultView(IEnumerable<Vector2> locations)
        {
            InitializeComponent();
            foreach (Vector2 location in locations)
            {
                // Was to lazy to do it with Bindings (sorry)
                LocationList.Items.Add(String.Format("{0}, {1}", location.X, location.Y));
            }
        }

        public void CloseButtonClick(object sender, RoutedEventArgs routedEventArgs)
        {
            this.Close();
        }
    }
}
