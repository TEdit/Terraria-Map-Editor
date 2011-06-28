using System.ComponentModel;

namespace TEdit.Plugins
{
    public interface IPluginMetaData
    {
        string Name { get; }

        [DefaultValue(0)]
        int Priority { get; }
    }
}