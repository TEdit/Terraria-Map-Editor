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
    private static readonly ConcurrentDictionary<(int NpcId, int Variant), WriteableBitmap> _variantPreviews = new();

    /// <summary>
    /// Gets whether the cache has been populated.
    /// </summary>
    public static bool IsPopulated { get; private set; }

    /// <summary>
    /// Gets a preview bitmap for an NPC by ID.
    /// </summary>
    public static WriteableBitmap GetPreview(int npcId)
    {
        _previews.TryGetValue(npcId, out var preview);
        return preview;
    }

    /// <summary>
    /// Sets a preview bitmap for an NPC.
    /// </summary>
    public static void SetPreview(int npcId, WriteableBitmap preview)
    {
        _previews[npcId] = preview;
    }

    /// <summary>
    /// Gets a preview bitmap for a specific NPC variant.
    /// </summary>
    public static WriteableBitmap GetVariantPreview(int npcId, int variantIndex)
    {
        _variantPreviews.TryGetValue((npcId, variantIndex), out var preview);
        return preview;
    }

    /// <summary>
    /// Sets a preview bitmap for a specific NPC variant.
    /// </summary>
    public static void SetVariantPreview(int npcId, int variantIndex, WriteableBitmap preview)
    {
        _variantPreviews[(npcId, variantIndex)] = preview;
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
        _variantPreviews.Clear();
        IsPopulated = false;
    }
}
