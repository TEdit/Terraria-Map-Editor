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

    /// <summary>Preview tiles for track tunnel clearing (above the track path).</summary>
    IReadOnlyList<Vector2Int32> CadPreviewTunnelPath { get; }

    /// <summary>Whether CAD wire preview is active and should be rendered.</summary>
    bool HasCadPreview { get; }

    /// <summary>Anchor point for shift+line preview. Only valid when HasLinePreviewAnchor is true.</summary>
    Vector2Int32 LinePreviewAnchor { get; }

    /// <summary>Whether a valid anchor exists for shift+line preview.</summary>
    bool HasLinePreviewAnchor { get; }

    /// <summary>Whether a floating paste layer is active and should be rendered.</summary>
    bool IsFloatingPaste { get; }

    /// <summary>Top-left world tile coordinate of the floating paste layer.</summary>
    Vector2Int32 FloatingPasteAnchor { get; }

    /// <summary>Size of the floating paste layer in tiles.</summary>
    Vector2Int32 FloatingPasteSize { get; }

    /// <summary>Commits the floating paste layer to the world.</summary>
    void AcceptPaste();

    /// <summary>Discards the floating paste layer without modifying the world.</summary>
    void CancelPaste();

    /// <summary>Returns a cursor hint for the given tile position, or null for default.</summary>
    CursorHint GetCursorHint(Vector2Int32 tilePos);
}

public enum CursorHint
{
    Default,
    Move,
    SizeNS,
    SizeWE,
    SizeNWSE,
    SizeNESW,
}
