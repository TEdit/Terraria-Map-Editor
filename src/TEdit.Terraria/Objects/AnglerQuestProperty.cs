using System.Text.Json.Serialization;

namespace TEdit.Terraria.Objects;

public class AnglerQuestProperty
{
    public int QuestId { get; set; }
    public int ItemId { get; set; }
    public ItemProperty? Item { get; set; }

    /// <summary>Stable English identity name (item name, or numeric id when unresolved).</summary>
    public string Name => Item?.Name ?? ItemId.ToString();

    /// <summary>Localized display name.</summary>
    [JsonIgnore]
    public string LocalizedName => Item?.LocalizedName ?? Name;

    public override string ToString() => LocalizedName;
}
