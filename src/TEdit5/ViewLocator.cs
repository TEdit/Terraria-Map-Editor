using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Microsoft.Extensions.DependencyInjection;
using System;
using TEdit5.ViewModels;

namespace TEdit5;

public static class DependencyInjectionExtensions
{
    // https://www.reddit.com/r/AvaloniaUI/comments/ssplp9/comment/hx0e3zi/?utm_source=share&utm_medium=web2x&context=3
    // https://github.com/Revolutionary-Games/Thrive-Launcher/blob/270d7462e81267bae7019e68f642e5565c0b0014/ThriveLauncher/Program.cs
    public static IServiceProvider GetServiceProvider(this IResourceHost control)
    {
        var sp = (IServiceProvider?)control.FindResource(typeof(IServiceProvider));
        if (sp == null) { throw new ArgumentOutOfRangeException(nameof(control), "No service provider initialized."); }
        return sp;
    }

    public static T CreateInstance<T>(this IResourceHost control)
    {
        return ActivatorUtilities.CreateInstance<T>(control.GetServiceProvider());
    }
}

public class ViewLocator : IDataTemplate
{
    public Control Build(object data)
    {
        var name = data.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object data)
    {
        return data is ReactiveObject;
    }
}
