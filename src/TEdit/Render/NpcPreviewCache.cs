using System.Collections.Concurrent;
using System.Windows.Media.Imaging;

namespace TEdit.Render;

/// <summary>
/// Static cache for NPC preview bitmaps used in WPF UI.
/// Populated by WorldRenderXna when textures are loaded.
/// </summary>
public static class NpcPreviewCache
{
    private static readonly ConcurrentDictionary<int, WriteableBitmap> _previews = new();

    /// <summary>
    /// Gets whether the cache has been populated.
    /// </summary>
    public static bool IsPopulated { get; private set; }

    /// <summary>
    /// Gets a preview bitmap for an NPC by ID.
    /// </summary>
    /// <param name="npcId">The NPC sprite ID.</param>
    /// <returns>The preview bitmap, or null if not available.</returns>
    public static WriteableBitmap GetPreview(int npcId)
    {
        _previews.TryGetValue(npcId, out var preview);
        return preview;
    }

    /// <summary>
    /// Sets a preview bitmap for an NPC.
    /// </summary>
    /// <param name="npcId">The NPC sprite ID.</param>
    /// <param name="preview">The preview bitmap.</param>
    public static void SetPreview(int npcId, WriteableBitmap preview)
    {
        _previews[npcId] = preview;
    }

    /// <summary>
    /// Marks the cache as populated.
    /// </summary>
    public static void MarkPopulated()
    {
        IsPopulated = true;
    }

    /// <summary>
    /// Clears all cached previews.
    /// </summary>
    public static void Clear()
    {
        _previews.Clear();
        IsPopulated = false;
    }
}
