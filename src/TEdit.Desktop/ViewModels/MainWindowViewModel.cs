using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Threading.Tasks;
using TEdit.Desktop.Controls.WorldRenderEngine;
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

    public MainWindowViewModel()
    {
        _selectedDocument = new DocumentViewModel();
    }
}
