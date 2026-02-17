using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using TEdit.ViewModel;
using Wpf.Ui.Controls;

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

    private void WorldList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (_vm.SelectedWorld != null && !_vm.SelectedWorld.IsMissing)
        {
            OpenAndClose(_vm.SelectedWorld.FilePath);
        }
    }

    private void BackupTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is BackupEntryViewModel backup)
        {
            _vm.SelectedBackup = backup;
        }
        else
        {
            _vm.SelectedBackup = null;
        }
    }

    private void BackupEntry_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2 && _vm.SelectedBackup != null)
        {
            OpenAndClose(_vm.SelectedBackup.FilePath);
        }
    }

    private void Open_Click(object sender, RoutedEventArgs e)
    {
        if (_vm.SelectedTabIndex == 0 && _vm.SelectedWorld != null && !_vm.SelectedWorld.IsMissing)
        {
            OpenAndClose(_vm.SelectedWorld.FilePath);
        }
        else if (_vm.SelectedTabIndex == 1 && _vm.SelectedBackup != null)
        {
            OpenAndClose(_vm.SelectedBackup.FilePath);
        }
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (_vm.SelectedBackup == null) return;

        var result = System.Windows.MessageBox.Show(
            $"Delete backup?\n{_vm.SelectedBackup.DisplayText}",
            "Confirm Delete",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);

        if (result == System.Windows.MessageBoxResult.Yes)
        {
            _vm.DeleteBackup(_vm.SelectedBackup);
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
        if (sender is System.Windows.Controls.Button { Tag: WorldEntryViewModel entry })
        {
            if (entry.IsPinned)
                _vm.UnpinWorld(entry.FilePath);
            else
                _vm.PinWorld(entry.FilePath);
        }
    }

    private void OpenAndClose(string path)
    {
        OpenedWorldPath = path;
        _vm.OpenWorld(path);
        DialogResult = true;
        Close();
    }
}
