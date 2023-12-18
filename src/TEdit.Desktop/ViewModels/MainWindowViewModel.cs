using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;
using TEdit.Desktop.Controls.WorldRenderEngine;
using TEdit.Desktop.Services;
using TEdit.Terraria;

namespace TEdit.Desktop.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IRasterTileCache _pixelTileCache;
    private World? _world;


    public MainWindowViewModel(IRasterTileCache pixelTileCache)
    {
        _pixelTileCache = pixelTileCache;
    }

    public IRasterTileCache PixelTileCache
    {
        get => _pixelTileCache;
    }

    public World? World
    {
        get => _world;
        set => this.RaiseAndSetIfChanged(ref _world, value);
    }
}
