using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace TEditWPF.Tools
{
    public interface ITool
    {
        string Name { get; }
        Image Icon { get; }
        ToolType Type { get; }

        // Bool for can use tool
        //bool PreviewTool(Point[] location, World world, WriteableBitmap viewPortRegion);

        //// Bool for was tool used
        //bool UseTool(Point[] location, TerrariaWorld.Game.World world);
    }
}
