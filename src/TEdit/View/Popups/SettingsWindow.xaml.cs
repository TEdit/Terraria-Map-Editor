using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TEdit.Input;
using TEdit.ViewModel;

namespace TEdit.View.Popups;

public partial class SettingsWindow : Window
{
    public SettingsWindow(WorldViewModel wvm)
    {
        InitializeComponent();
        DataContext = new SettingsViewModel(wvm);
    }

    private void BrowsePath_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: SettingItem setting }) return;

        var currentPath = setting.Value as string;
        if (string.IsNullOrWhiteSpace(currentPath))
            currentPath = DependencyChecker.PathToContent ?? "";

        using var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = setting.Description,
            SelectedPath = currentPath
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            setting.Value = dialog.SelectedPath;
        }
    }

    private void AddKeybinding_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: SettingItem setting }) return;

        var captureWindow = new InputCaptureWindow { Owner = this };
        if (captureWindow.ShowDialog() == true && captureWindow.CapturedInput.HasValue)
        {
            var binding = captureWindow.CapturedInput.Value;
            if (binding.IsValid && !setting.Bindings.Contains(binding))
            {
                setting.Bindings.Add(binding);
                App.Input.Registry.SetUserBindings(setting.ActionId, new List<InputBinding>(setting.Bindings));
                App.Input.SaveUserCustomizations();
            }
        }
    }

    private void RemoveKeybinding_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        if (button.Tag is not SettingItem setting) return;
        if (button.CommandParameter is not InputBinding binding) return;

        setting.Bindings.Remove(binding);
        App.Input.Registry.SetUserBindings(setting.ActionId, new List<InputBinding>(setting.Bindings));
        App.Input.SaveUserCustomizations();
    }

    private void ResetKeybinding_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: SettingItem setting }) return;

        App.Input.Registry.ResetToDefaults(setting.ActionId);
        App.Input.SaveUserCustomizations();

        // Refresh the bindings in the UI
        setting.Bindings.Clear();
        foreach (var binding in App.Input.Registry.GetBindings(setting.ActionId))
        {
            setting.Bindings.Add(binding);
        }
    }

    private void CategoryExpander_Expanded(object sender, RoutedEventArgs e)
    {
        if (sender is Expander { DataContext: CollectionViewGroup group } &&
            DataContext is SettingsViewModel vm)
        {
            vm.SetCategoryExpanded(group.Name?.ToString() ?? "", true);
        }
    }

    private void CategoryExpander_Collapsed(object sender, RoutedEventArgs e)
    {
        if (sender is Expander { DataContext: CollectionViewGroup group } &&
            DataContext is SettingsViewModel vm)
        {
            vm.SetCategoryExpanded(group.Name?.ToString() ?? "", false);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
