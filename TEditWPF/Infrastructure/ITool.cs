using System.Windows;

namespace TEditWPF.Infrastructure
{
    using System.Windows.Media.Imaging;

    public interface ITool
    {
        string Name { get; }
        ToolType Type { get; }

        // Bool for can use tool
        //bool PreviewTool(Point[] location, World world, WriteableBitmap viewPortRegion);

        //// Bool for was tool used
        //bool UseTool(Point[] location, TerrariaWorld.Game.World world);
    }
}
