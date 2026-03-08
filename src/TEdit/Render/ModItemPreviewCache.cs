using System;
using System.Collections.Concurrent;
using System.Windows.Media.Imaging;

namespace TEdit.Render;

/// <summary>
/// Static cache for mod item preview bitmaps, keyed by "ModName:ItemName".
/// Populated during LoadModTextures when .tmod archives contain item textures.
/// </summary>
public static class ModItemPreviewCache
{
    private static readonly ConcurrentDictionary<string, WriteableBitmap> _previews = new(StringComparer.OrdinalIgnoreCase);

    public static WriteableBitmap GetPreview(string modName, string itemName)
    {
        if (string.IsNullOrEmpty(modName) || string.IsNullOrEmpty(itemName))
            return null;

        _previews.TryGetValue($"{modName}:{itemName}", out var preview);
        return preview;
    }

    public static void SetPreview(string modName, string itemName, WriteableBitmap preview)
    {
        _previews[$"{modName}:{itemName}"] = preview;
    }

    public static void Clear()
    {
        _previews.Clear();
    }
}
