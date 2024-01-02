using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Threading.Tasks;
using TEdit.Desktop.Controls.WorldRenderEngine;
using TEdit.Desktop.Services;
using TEdit.Terraria;

namespace TEdit.Desktop.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private DocumentViewModel _selectedDocument;

    public MainWindowViewModel()
    {
        _selectedDocument = new DocumentViewModel();
    }
}

public partial class DocumentViewModel : ObservableObject
{
    [ObservableProperty]
    private World? _world;

    [ObservableProperty]
    private int _zoom = 100;

    [ObservableProperty]
    private int _minZoom = 7;

    [ObservableProperty]
    private int _maxZoom = 6400;

    [ObservableProperty]
    private Point _cursorTileCoordinate;

    public DocumentViewModel()
    {
    }
}
