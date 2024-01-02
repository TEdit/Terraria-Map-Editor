using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using TEdit.Desktop.ViewModels;
using TEdit.Terraria;

namespace TEdit.Desktop.Views;

public partial class MainWindow : Window
{
    protected MainWindowViewModel MainWindowViewModel => (MainWindowViewModel)this.DataContext!;

    public MainWindow()
    {
        InitializeComponent();
        this.DataContext = this.CreateInstance<MainWindowViewModel>();
    }

    public async void LoadWorldButton_Clicked(object sender, RoutedEventArgs args)
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

            (var world, var errors) = World.LoadWorld(file.TryGetLocalPath());

            if (world != null)
            {
                MainWindowViewModel.SelectedDocument.World = world;
            }
        }
    }
}
