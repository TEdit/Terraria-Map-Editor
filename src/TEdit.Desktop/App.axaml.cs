using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using System;
using TEdit.Desktop.Controls.WorldRenderEngine;
using TEdit.Desktop.Services;
using TEdit.Desktop.ViewModels;
using TEdit.Desktop.Views;

namespace TEdit.Desktop;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        var services = new ServiceCollection();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<IDialogService, DialogService>();

        //services.AddSingleton<IMyInterface, MyImplementation>()
        var serviceProvider = services.BuildServiceProvider();
        this.Resources[typeof(IServiceProvider)] = serviceProvider;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
