using System.ComponentModel.Composition;
using System.Windows.Media.Imaging;

namespace TEditWPF.Tools
{
    [Export(typeof(ITool))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class Pencil : ITool
    {
        public string Name
        {
            get { return "Pencil"; }
        }

        public ToolType Type
        {
            get { return ToolType.Pencil; }
        }

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
