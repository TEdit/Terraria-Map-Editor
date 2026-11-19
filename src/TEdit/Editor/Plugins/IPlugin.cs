namespace TEdit.Editor.Plugins;

public interface IPlugin
{
    string Name { get; }
    string Title { get; }
    System.Threading.Tasks.Task ExecuteAsync();
}