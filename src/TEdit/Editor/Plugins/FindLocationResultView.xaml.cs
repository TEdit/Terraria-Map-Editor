using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xna.Framework;

namespace TEdit.Editor.Plugins;

/// <summary>
/// Interaction logic for ReplaceAllPlugin.xaml
/// </summary>
public partial class FindLocationResultView : Window
{
    private char[] splitters = new char[] { ',' };
    public FindLocationResultView(List<Tuple<Vector2, string>> locations)
    {
        InitializeComponent();

        foreach (var location in locations)
        {
            LocationList.Items.Add($"{location.Item1.X}, {location.Item1.Y}{location.Item2.ToString()}");
        }
    }

    public void CloseButtonClick(object sender, RoutedEventArgs routedEventArgs)
    {
        Close();
    }

    private void ListBoxMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is TextBlock)
        {
            TextBlock item = e.OriginalSource as TextBlock;
            if (!string.IsNullOrEmpty(item.Text))
            {
                string[] positions = item.Text.Split(splitters);
                int x = 0;
                int y = 0;

                if (int.TryParse(positions[0].Trim(), out x) && int.TryParse(positions[1].Trim(), out y))
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
