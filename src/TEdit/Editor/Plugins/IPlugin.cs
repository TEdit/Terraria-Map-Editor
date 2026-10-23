namespace TEdit.Editor.Plugins;

public interface IPlugin
{
    string Name { get; }
    string Title { get; }
    void Execute();
}