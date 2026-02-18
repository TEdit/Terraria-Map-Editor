using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using TEdit.ViewModel;
using Wpf.Ui.Controls;
using Button = System.Windows.Controls.Button;
using MessageBox = Wpf.Ui.Controls.MessageBox;

namespace TEdit.View.Popups;

public partial class WorldExplorerWindow : FluentWindow
{
    private readonly WorldExplorerViewModel _vm;

    public WorldExplorerWindow(WorldViewModel wvm)
    {
        InitializeComponent();
        _vm = new WorldExplorerViewModel(wvm);
        DataContext = _vm;
    }

    /// <summary>
    /// Gets the path of the world that was opened, if any.
    /// </summary>
    public string OpenedWorldPath { get; private set; }

    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        var ofd = new OpenFileDialog
        {
            Filter = "Terraria World File|*.wld|TEdit Backup|*.TEdit|Terraria Backup|*.bak|All Files|*.*",
            Title = "Open World File",
            InitialDirectory = DependencyChecker.PathToWorlds
        };

        if (ofd.ShowDialog() == true)
        {
            OpenAndClose(ofd.FileName);
        }
    }

    private void WorldList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ClearBackupSelection();
    }

    private void WorldList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        // Don't handle if a backup was clicked (backup handles its own double-click)
        if (_vm.SelectedBackup != null) return;

        if (_vm.SelectedWorld != null && !_vm.SelectedWorld.IsMissing)
        {
            OpenAndClose(_vm.SelectedWorld.FilePath);
        }
    }

    private void ExpandToggle_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: WorldEntryViewModel entry } && entry.HasBackups)
        {
            entry.IsExpanded = !entry.IsExpanded;
            e.Handled = true;
        }
    }

    private void BackupEntry_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        // Let button clicks pass through — don't swallow their tunneling events
        if (e.OriginalSource is DependencyObject source && FindParent<Button>(source) != null)
            return;

        if (sender is FrameworkElement { DataContext: BackupEntryViewModel backup })
        {
            if (e.ClickCount == 2)
            {
                OpenAndClose(backup.FilePath);
            }
            else
            {
                _vm.SelectedBackup = backup;
                HighlightSelectedBackup(sender as FrameworkElement);
            }
            e.Handled = true;
        }
    }

    private static T FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        var current = child;
        while (current != null)
        {
            if (current is T found) return found;
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }

    private FrameworkElement _lastHighlightedBackup;

    private void HighlightSelectedBackup(FrameworkElement element)
    {
        // Clear previous highlight
        if (_lastHighlightedBackup is System.Windows.Controls.Border prevBorder)
        {
            prevBorder.Background = System.Windows.Media.Brushes.Transparent;
        }

        // Apply new highlight
        if (element is System.Windows.Controls.Border border)
        {
            border.Background = (System.Windows.Media.Brush)FindResource("ControlFillColorSecondaryBrush");
            _lastHighlightedBackup = border;
        }
    }

    private void ClearBackupSelection()
    {
        _vm.SelectedBackup = null;
        if (_lastHighlightedBackup is System.Windows.Controls.Border prevBorder)
        {
            prevBorder.Background = System.Windows.Media.Brushes.Transparent;
            _lastHighlightedBackup = null;
        }
    }

    private void Open_Click(object sender, RoutedEventArgs e)
    {
        if (_vm.SelectedBackup != null)
        {
            OpenAndClose(_vm.SelectedBackup.FilePath);
        }
        else if (_vm.SelectedWorld != null && !_vm.SelectedWorld.IsMissing)
        {
            OpenAndClose(_vm.SelectedWorld.FilePath);
        }
    }

    private void PreviewWorld_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: WorldEntryViewModel entry } && !entry.IsMissing)
        {
            _ = _vm.GeneratePreviewAsync(entry.FilePath);
        }
    }

    private void PreviewBackup_Click2(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: BackupEntryViewModel backup })
        {
            _ = _vm.GeneratePreviewAsync(backup.FilePath);
        }
    }

    private async void DeleteBackup_Click2(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: BackupEntryViewModel backup })
        {
            var messageBox = new MessageBox
            {
                Title = "Confirm Delete",
                Content = $"Delete backup?\n{backup.DisplayText}",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };

            var result = await messageBox.ShowDialogAsync();
            if (result == Wpf.Ui.Controls.MessageBoxResult.Primary)
            {
                _vm.DeleteBackup(backup);
            }
        }
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        _ = _vm.RefreshAsync();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void TogglePin_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: WorldEntryViewModel entry })
        {
            if (entry.IsPinned)
                _vm.UnpinWorld(entry.FilePath);
            else
                _vm.PinWorld(entry.FilePath);
        }
    }

    private void ToggleFavorite_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: WorldEntryViewModel entry })
        {
            _vm.ToggleFavorite(entry);
        }
    }

    private void CreateBackup_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: WorldEntryViewModel entry })
        {
            _vm.CreateBackupNow(entry);
        }
    }

    private void OpenFolder_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: WorldEntryViewModel entry } && !entry.IsMissing && File.Exists(entry.FilePath))
        {
            Process.Start("explorer.exe", $"/select,\"{entry.FilePath}\"");
        }
    }

    private void OpenBackupFolder_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: BackupEntryViewModel backup } && File.Exists(backup.FilePath))
        {
            Process.Start("explorer.exe", $"/select,\"{backup.FilePath}\"");
        }
    }

    private void ClosePreview_Click(object sender, RoutedEventArgs e)
    {
        _vm.ClearPreview();
    }

    private void OpenAndClose(string path)
    {
        OpenedWorldPath = path;
        _vm.OpenWorld(path);
        DialogResult = true;
        Close();
    }
}
