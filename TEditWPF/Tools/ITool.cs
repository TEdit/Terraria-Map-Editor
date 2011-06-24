using System.Windows.Media.Imaging;
using TEditWPF.Common;

namespace TEditWPF.Tools
{
    public interface ITool
    {
        string Name { get; }
        BitmapImage Image { get; }
        ToolType Type { get; }
        bool IsActive { get; set; }

        bool PressTool(TileMouseEventArgs e);
        bool MoveTool(TileMouseEventArgs e);
        bool ReleaseTool(TileMouseEventArgs e);
        WriteableBitmap PreviewTool();
    }
}