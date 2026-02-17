using System;
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

    /// <summary>
    /// Raised on the UI thread when all item previews have been generated.
    /// Subscribers can use this to refresh bindings that depend on item previews.
    /// </summary>
    public static event Action Populated;

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
        Populated?.Invoke();
    }

    public static void Clear()
    {
        _previews.Clear();
        IsPopulated = false;
    }
}
