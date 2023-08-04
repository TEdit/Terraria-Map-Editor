using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TEdit.Geometry;

namespace TEdit.Editor.Plugins;

/// <summary>
/// Interaction logic for ReplaceAllPlugin.xaml
/// </summary>
public partial class FindTileLocationResultView : Window
{
    private char[] splitters = new char[] { ',' };
    public FindTileLocationResultView(List<Tuple<string, Vector2Float>> locations, string count)
    {
        InitializeComponent();
        LocationText.Text = count + " locations could be found.";
        foreach (var location in locations)
        {
            LocationList.Items.Add($"{location.Item1}{location.Item2.X}, {location.Item2.Y}");
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
                string[] split = item.Text.Split(':');
                string[] positions = split[split.Length - 1].Trim().Split(splitters);
                if (positions.Length == 2)
                {
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
}
