using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using TEdit5.Controls.WorldRenderEngine;
using TEdit5.Controls.WorldRenderEngine.Layers;
using TEdit5.Services;

namespace TEdit5.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    public IDocumentService DocumentService { get; }
    [Reactive] public DocumentViewModel SelectedDocument { get; set; }
    [Reactive] public ToolSelectionViewModel ToolSelection { get; set; }
    [Reactive] public int ProgressPercentage { get; set; }
    [Reactive] public string ProgressText { get; set; } = string.Empty;
    [Reactive] public RenderLayerVisibility RenderLayerVisibility { get; set; } = new();

    public MainWindowViewModel(
        IDocumentService documentService,
        ToolSelectionViewModel toolSelection)
    {
        DocumentService = documentService;
        ToolSelection = toolSelection;
    }
}
