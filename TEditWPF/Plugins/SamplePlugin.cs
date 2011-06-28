using System;
using System.ComponentModel.Composition;
using TEdit.TerrariaWorld;

namespace TEdit.Plugins
{
    [Export(typeof (IPlugin))]
    public class SamplePlugin : IPlugin
    {
        [Import("World", typeof (World))]
        public World World { get; set; }

        #region IPlugin Members

        public void ExecutePlugin()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}