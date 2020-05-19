using System.Windows.Media.Imaging;

namespace TEditXna.Editor.Tools
{
    public interface ITool
    {
        ToolType ToolType { get; }
        BitmapImage Icon { get; }
        bool IsActive { get; set; }
        string Name { get; }
        bool PreviewIsTexture { get; }
        void MouseDown(TileMouseState e);
        void MouseMove(TileMouseState e);
        void MouseUp(TileMouseState e);
        void MouseWheel(TileMouseState e);
        double PreviewScale { get; }
        WriteableBitmap PreviewTool();
    }
}