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

    public string FullName => $"{ModName}:{Name}";

    public static ModWallEntry FromTag(TagCompound tag, int index)
    {
        return new ModWallEntry
        {
            SaveType = (ushort)index,
            ModName = tag.GetString("mod"),
            Name = tag.GetString("name"),
        };
    }

    public TagCompound ToTag()
    {
        var tag = new TagCompound();
        tag.Set("mod", ModName);
        tag.Set("name", Name);
        return tag;
    }
}
