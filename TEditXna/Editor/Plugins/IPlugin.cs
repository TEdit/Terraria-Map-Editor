using System.Windows.Media.Imaging;

namespace TEditXna.Editor.Plugins
{
    public interface IPlugin
    {
        string Name { get; }
        void Execute();
    }
}