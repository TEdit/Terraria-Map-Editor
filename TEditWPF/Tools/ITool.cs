using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using TEditWPF.Common;
using TEditWPF.TerrariaWorld.Structures;

namespace TEditWPF.Tools
{
    public interface ITool
    {
        string Name { get; }
        BitmapImage Image { get; }
        ToolType Type { get; }
        bool IsActive { get; set; }

        // Bool for can use tool
        //bool PreviewTool(Point[] location, World world, WriteableBitmap viewPortRegion);

        //// Bool for was tool used
        //bool UseTool(Point[] location, TerrariaWorld.Game.World world);

        bool PressTool(TileMouseEventArgs e);
        bool MoveTool(TileMouseEventArgs e);
        bool ReleaseTool(TileMouseEventArgs e);
        bool PreviewTool(TileMouseEventArgs e);
    }
}
