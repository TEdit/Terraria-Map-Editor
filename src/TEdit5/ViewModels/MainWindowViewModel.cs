using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ReactiveUI.SourceGenerators;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using TEdit5.Controls.WorldRenderEngine;
using TEdit5.Controls.WorldRenderEngine.Layers;
using TEdit5.Services;

namespace TEdit5.ViewModels;

public partial class MainWindowViewModel : ReactiveObject
{
    public IDocumentService DocumentService { get; }

    [Reactive] private DocumentViewModel _selectedDocument;
    [Reactive] private ToolSelectionViewModel _toolSelection;
    [Reactive] private int _progressPercentage;
    [Reactive] private string _progressText = string.Empty;
    [Reactive] private RenderLayerVisibility _renderLayerVisibility = new();

    public MainWindowViewModel(
        IDocumentService documentService,
        ToolSelectionViewModel toolSelection)
    {
        DocumentService = documentService;
        _toolSelection = toolSelection;
    }
}
