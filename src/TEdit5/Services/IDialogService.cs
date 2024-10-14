using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TEdit5.ViewModels;
using TEdit.Editor;
using TEdit.Terraria;

namespace TEdit5.Services;

public interface IDialogService
{
    Task<string> OpenFileDialogAsync();
}

public class DialogService : IDialogService
{
    public async Task<string> OpenFileDialogAsync() => await Task.FromResult("test");
}

public interface IDocumentService
{
    ObservableCollection<DocumentViewModel> Documents { get; }
    Task LoadWorldAsync(IStorageFile file, IProgress<ProgressChangedEventArgs>? progress = null);
}


public partial class DocumentService : ReactiveObject, IDocumentService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ObservableCollection<DocumentViewModel> Documents { get; } = new();

    public DocumentService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }


    public async Task LoadWorldAsync(IStorageFile file, IProgress<ProgressChangedEventArgs>? progress = null)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var toolSelection = scope.ServiceProvider.GetRequiredService<ToolSelectionViewModel>();
            var tilePicker = scope.ServiceProvider.GetRequiredService<TilePicker>();

            (var world, var errors) = await World.LoadWorldAsync(file.TryGetLocalPath(), progress: progress);

            if (world != null)
            {
                var document = new DocumentViewModel(world, toolSelection, tilePicker);

                Documents.Add(document);
            }

            if (errors != null)
            {
                Debug.WriteLine("Error loading world: " + errors);
            }
        }
    }
}
