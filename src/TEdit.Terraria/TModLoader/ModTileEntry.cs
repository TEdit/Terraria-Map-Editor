using TEdit.Common.IO;

namespace TEdit.Terraria.TModLoader;

/// <summary>
/// Represents a modded tile type mapping from the .twld tileMap.
/// Maps a save-time ID to a mod name + tile name.
/// </summary>
public class ModTileEntry
{
    /// <summary>Save-time tile type ID (index into the tileMap list).</summary>
    public ushort SaveType { get; set; }

    /// <summary>Mod internal name (e.g., "CalamityMod").</summary>
    public string ModName { get; set; } = string.Empty;

    /// <summary>Tile internal name (e.g., "AstralMonolith").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Whether this tile type uses frame coordinates (U/V).</summary>
    public bool FrameImportant { get; set; }

    public string FullName => $"{ModName}:{Name}";

    public static ModTileEntry FromTag(TagCompound tag, int index)
    {
        return new ModTileEntry
        {
            SaveType = (ushort)index,
            ModName = tag.GetString("mod"),
            Name = tag.GetString("name"),
            FrameImportant = tag.GetBool("framed"),
        };
    }

    public TagCompound ToTag()
    {
        var tag = new TagCompound();
        tag.Set("mod", ModName);
        tag.Set("name", Name);
        tag.Set("framed", (byte)(FrameImportant ? 1 : 0));
        return tag;
    }
}
