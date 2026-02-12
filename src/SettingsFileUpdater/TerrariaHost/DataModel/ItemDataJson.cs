using System.Text.Json.Serialization;

namespace SettingsFileUpdater.TerrariaHost.DataModel;

public class ItemDataJson
{
    [JsonPropertyOrder(0)]
    public int Id { get; set; }

    [JsonPropertyOrder(1)]
    public string Name { get; set; } = "UNKNOWN";

    public float Scale { get; set; } = 1f;
    public int MaxStackSize { get; set; }

    public bool IsFood { get; set; }
    public bool IsKite { get; set; }
    public bool IsCritter { get; set; }
    public bool IsAccessory { get; set; }
    public bool IsRackable { get; set; }

    // Armor slot indexes (null = not an armor piece)
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Head { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Body { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Legs { get; set; }

    // Kill tally index (0 = none)
    public int Tally { get; set; }

    // Item rarity name (null = White, otherwise Master, Expert, Quest, Gray, Blue, Green, etc.)
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Rarity { get; set; }
}
