using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEdit.UI;
using Wpf.Ui.Controls;

namespace TEdit.Editor.Tools;

public interface ITool
{
    ToolType ToolType { get; }
    BitmapImage Icon { get; }
    SymbolRegular SymbolIcon { get; }
    ImageSource? VectorIcon { get; }
    bool IsActive { get; set; }
    string Name { get; }
    string Title { get; }
    bool PreviewIsTexture { get; }
    void MouseDown(TileMouseState e);
    void MouseMove(TileMouseState e);
    void MouseUp(TileMouseState e);
    void MouseWheel(TileMouseState e);
    double PreviewScale { get; }
    WriteableBitmap PreviewTool();
}
