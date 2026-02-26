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
        // tModLoader writes the runtime type ID as "value" (ushort stored as short in NBT).
        // If absent (old format), fall back to 1-based index convention.
        ushort saveType = tag.ContainsKey("value")
            ? tag.Get<ushort>("value")
            : (ushort)(index + 1);

        return new ModTileEntry
        {
            SaveType = saveType,
            ModName = tag.GetString("mod"),
            Name = tag.GetString("name"),
            FrameImportant = tag.GetBool("framed"),
        };
    }

    public TagCompound ToTag()
    {
        var tag = new TagCompound();
        tag.Set("value", (short)SaveType); // NBT stores as short; tModLoader deserializes as ushort
        tag.Set("mod", ModName);
        tag.Set("name", Name);
        tag.Set("framed", (byte)(FrameImportant ? 1 : 0));
        return tag;
    }
}
