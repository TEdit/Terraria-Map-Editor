using System.Collections.Concurrent;
using System.Windows.Media.Imaging;

namespace TEdit.Render;

/// <summary>
/// Static cache for item preview bitmaps used in WPF UI.
/// Populated by WorldRenderXna when textures are loaded.
/// </summary>
public static class ItemPreviewCache
{
    private static readonly ConcurrentDictionary<int, WriteableBitmap> _previews = new();

    public static bool IsPopulated { get; private set; }

    public static WriteableBitmap GetPreview(int itemId)
    {
        _previews.TryGetValue(itemId, out var preview);
        return preview;
    }

    public static void SetPreview(int itemId, WriteableBitmap preview)
    {
        _previews[itemId] = preview;
    }

    public static void MarkPopulated()
    {
        IsPopulated = true;
    }

    public static void Clear()
    {
        _previews.Clear();
        IsPopulated = false;
    }
}
