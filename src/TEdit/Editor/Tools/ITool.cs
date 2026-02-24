using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEdit.Geometry;
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
    int PreviewOffsetX { get; }
    int PreviewOffsetY { get; }
    WriteableBitmap PreviewTool();

    /// <summary>Preview path tiles for CAD wire routing. Empty when no preview active.</summary>
    IReadOnlyList<Vector2Int32> CadPreviewPath { get; }

    /// <summary>Whether CAD wire preview is active and should be rendered.</summary>
    bool HasCadPreview { get; }

    /// <summary>Anchor point for shift+line preview. Only valid when HasLinePreviewAnchor is true.</summary>
    Vector2Int32 LinePreviewAnchor { get; }

    /// <summary>Whether a valid anchor exists for shift+line preview.</summary>
    bool HasLinePreviewAnchor { get; }
}
