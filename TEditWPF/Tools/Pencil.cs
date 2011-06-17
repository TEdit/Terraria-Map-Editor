using System.ComponentModel.Composition;
using System.Windows.Controls;
using TEditWPF.TerrariaWorld;

namespace TEditWPF.Tools
{
    [Export(typeof(ITool))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class Pencil : ITool
    {
        [Import("World", typeof(World))]
        private World _world = null;

        public string Name
        {
            get { return "Pencil"; }
        }

        public ToolType Type
        {
            get { return ToolType.Pencil; }
        }

        private Image _icon = new Image();
        public Image Icon
        {
            get { return _icon; }
        }

        [Import]
        private TerrariaWorld.World _currentWorld;

        //public bool PreviewTool(Point[] location, World world, WriteableBitmap viewPortRegion)
        //{
        //    return false;
        //}

        //public bool UseTool(Point[] location, World world)
        //{
        //    return false;
        //}
    }
}
