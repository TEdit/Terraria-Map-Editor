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

    // Accessory slot indexes (null = not this accessory type)
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? WingSlot { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? BackSlot { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? BalloonSlot { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? ShoeSlot { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? WaistSlot { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? NeckSlot { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? FaceSlot { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? ShieldSlot { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? HandOnSlot { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? HandOffSlot { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? FrontSlot { get; set; }

    // Head armor hair visibility flags (indexed by headSlot in Terraria's ArmorIDs.Head.Sets)
    // null = not a head item. When both are false/null, helmet fully covers hair.
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? DrawFullHair { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? DrawHatHair { get; set; }

    // Texture alias: when set, use Item_{TextureId}.xnb instead of Item_{Id}.xnb
    // Populated from ItemID.Sets.TextureCopyLoad (resolved transitively)
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? TextureId { get; set; }

    // Kill tally index (0 = none)
    public int Tally { get; set; }

    // Item rarity name (null = White, otherwise Master, Expert, Quest, Gray, Blue, Green, etc.)
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Rarity { get; set; }
}
