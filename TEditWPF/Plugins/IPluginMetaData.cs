using System.ComponentModel;

namespace TEditWPF.Plugins
{
    public interface IPluginMetaData
    {
        string Name { get; }

        [DefaultValue(0)]
        int Priority { get; }
    }
}