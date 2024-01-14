using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Threading.Tasks;
using TEdit.Desktop.Controls.WorldRenderEngine;
using TEdit.Desktop.Controls.WorldRenderEngine.Layers;
using TEdit.Desktop.Services;

namespace TEdit.Desktop.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private DocumentViewModel _selectedDocument;

    [ObservableProperty]
    private int _progressPercentage;

    [ObservableProperty]
    private string _progressText = string.Empty;

    [ObservableProperty]
    private RenderLayerVisibility _renderLayerVisibility = new();

    public MainWindowViewModel()
    {
        _selectedDocument = new DocumentViewModel();
    }
}
