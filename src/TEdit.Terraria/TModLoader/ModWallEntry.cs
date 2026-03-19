using TEdit.Common.IO;

namespace TEdit.Terraria.TModLoader;

/// <summary>
/// Represents a modded wall type mapping from the .twld wallMap.
/// Maps a save-time ID to a mod name + wall name.
/// </summary>
public class ModWallEntry
{
    /// <summary>Save-time wall type ID (index into the wallMap list).</summary>
    public ushort SaveType { get; set; }

    /// <summary>Mod internal name (e.g., "CalamityMod").</summary>
    public string ModName { get; set; } = string.Empty;

    /// <summary>Wall internal name (e.g., "AstralMonolithWall").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Original TagCompound from the .twld file. Preserved during save to retain
    /// tModLoader-required fields that TEdit doesn't parse.
    /// </summary>
    public TagCompound RawTag { get; set; }

    public string FullName => $"{ModName}:{Name}";

    public static ModWallEntry FromTag(TagCompound tag, int index)
    {
        // tModLoader writes the runtime type ID as "value" (ushort stored as short in NBT).
        // If absent (old format), fall back to 1-based index convention.
        ushort saveType = tag.ContainsKey("value")
            ? tag.Get<ushort>("value")
            : (ushort)(index + 1);

        return new ModWallEntry
        {
            SaveType = saveType,
            ModName = tag.GetString("mod"),
            Name = tag.GetString("name"),
            RawTag = tag,
        };
    }

    public TagCompound ToTag()
    {
        // If we have the original tag, clone it and update fields we may have changed.
        // This preserves tModLoader-required fields.
        if (RawTag != null)
        {
            var tag = RawTag.Clone();
            tag.Set("value", (short)SaveType);
            tag.Set("mod", ModName);
            tag.Set("name", Name);
            return tag;
        }

        // New entry — create minimal tag
        var newTag = new TagCompound();
        newTag.Set("value", (short)SaveType);
        newTag.Set("mod", ModName);
        newTag.Set("name", Name);
        return newTag;
    }
}
