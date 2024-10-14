using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using TEdit5.ViewModels;
using TEdit.Terraria;

namespace TEdit5.Views;

public partial class MainWindow : Window
{
    protected MainWindowViewModel MainWindowViewModel => (MainWindowViewModel)this.DataContext!;

    public MainWindow()
    {
        InitializeComponent();
    }

    public async void LoadWorldButton_Clicked(object sender, RoutedEventArgs args)
    {
        await OpenWorldDialog();
    }

    private async Task OpenWorldDialog()
    {
        var fileTypes = new List<FilePickerFileType>
        {
            new FilePickerFileType("World File")
            {
                Patterns = new [] { "*.wld" },
                AppleUniformTypeIdentifiers = new [] { "wld" },
                MimeTypes = new []{ "application/octet-stream" }
            },
        };

        // Get top level from the current control. Alternatively, you can use Window reference instead.
        var topLevel = TopLevel.GetTopLevel(this)!;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Text File",
            AllowMultiple = false,
            FileTypeFilter = fileTypes
        });

        if (files.Count == 1)
        {
            var file = files[0];

            await LoadWorld(file);
        }
    }

    private async Task LoadWorld(IStorageFile file)
    {
        var progress = new Progress<ProgressChangedEventArgs>(ProgressChangedEventArgs =>
               {
                   MainWindowViewModel.ProgressPercentage = ProgressChangedEventArgs.ProgressPercentage;
                   MainWindowViewModel.ProgressText = ProgressChangedEventArgs.UserState?.ToString() ?? string.Empty;
               });

        await MainWindowViewModel.DocumentService.LoadWorldAsync(file, progress);

        ((IProgress<ProgressChangedEventArgs>)progress).Report(new ProgressChangedEventArgs(0, string.Empty));
    }
}
