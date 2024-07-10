using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using System;
using TEdit.Desktop.Controls.WorldRenderEngine;
using TEdit.Desktop.Editor;
using TEdit.Desktop.Services;
using TEdit.Desktop.ViewModels;
using TEdit.Desktop.Views;
using TEdit.Editor;

namespace TEdit.Desktop;

public partial class App : Application
{
    public IServiceProvider Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        var services = new ServiceCollection();

        // register view models
        services.AddSingleton<MainWindowViewModel>();

        services.AddTransient<DocumentViewModel>();
        services.AddTransient<FileManagerViewModel>();

        // register editing tools
        services.AddSingleton<ToolSelectionViewModel>();
        services.AddSingleton<TilePicker>();
        services.AddSingleton<IMouseTool, ArrowTool>();
        services.AddSingleton<IMouseTool, BrushTool>();
        services.AddSingleton<IMouseTool, PencilTool>();
        services.AddSingleton<IMouseTool, SelectTool>();

        // register services
        services.AddTransient<IDialogService, DialogService>();

        //services.AddSingleton<IMyInterface, MyImplementation>()
        var serviceProvider = services.BuildServiceProvider();
        this.Resources[typeof(IServiceProvider)] = serviceProvider;
        Services = serviceProvider;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = (IServiceProvider)this.Resources[typeof(IServiceProvider)];
        var vm = services.GetRequiredService<MainWindowViewModel>();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = vm
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainWindow
            {
                DataContext = vm
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
