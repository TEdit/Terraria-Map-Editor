using System.ComponentModel.Composition;
using TEditWPF.TerrariaWorld;

namespace TEditWPF.Plugins
{
    [Export(typeof(IPlugin))]
    public class SamplePlugin : IPlugin
    {
        [Import("World", typeof(World))]
        public World World { get; set; }

        public void ExecutePlugin()
        {
            throw new System.NotImplementedException();
        }
    }
}