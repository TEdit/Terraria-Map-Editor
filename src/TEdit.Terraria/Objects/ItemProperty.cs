using System.Text.Json.Serialization;
using TEdit.Common;
using TEdit.Geometry;

namespace TEdit.Terraria.Objects;

public class ItemProperty : ITile
{
    [JsonPropertyOrder(0)]
    public int Id { get; set; }

    [JsonPropertyOrder(1)]
    public string Name { get; set; } = "UNKNOWN";

    [JsonPropertyOrder(2)]
    public string? Key { get; set; }

    public float Scale { get; set; } = 1f;
    public int MaxStackSize { get; set; }

    // Category flags
    public bool IsFood { get; set; }
    public bool IsKite { get; set; }
    public bool IsCritter { get; set; }
    public bool IsAccessory { get; set; }
    public bool IsRackable { get; set; }
    public bool IsMount { get; set; }

    // Armor slot indexes (null = not an armor piece)
    public int? Head { get; set; }
    public int? Body { get; set; }
    public int? Legs { get; set; }

    // Head armor hair visibility flags (from ArmorIDs.Head.Sets)
    // null = not a head item. When both are false/null, helmet fully covers hair.
    public bool? DrawFullHair { get; set; }
    public bool? DrawHatHair { get; set; }

    // Accessory slot indexes (null = not this accessory type)
    public int? WingSlot { get; set; }
    public int? BackSlot { get; set; }
    public int? BalloonSlot { get; set; }
    public int? ShoeSlot { get; set; }
    public int? WaistSlot { get; set; }
    public int? NeckSlot { get; set; }
    public int? FaceSlot { get; set; }
    public int? ShieldSlot { get; set; }
    public int? HandOnSlot { get; set; }
    public int? HandOffSlot { get; set; }
    public int? FrontSlot { get; set; }

    // Texture alias: when set, use Item_{TextureId}.xnb instead of Item_{Id}.xnb
    // From ItemID.Sets.TextureCopyLoad — trapped chests, etc.
    public int? TextureId { get; set; }

    // Kill tally index (0 = none)
    public int Tally { get; set; }

    // Item rarity name (Master, Expert, Quest, Gray, White, Blue, Green, etc.)
    public string Rarity { get; set; } = "White";

    // Resolved rarity color (set after data load from GlobalColors)
    [JsonIgnore]
    public TEditColor RarityColor { get; set; } = TEditColor.White;

    // Legacy properties (for compatibility)
    [JsonIgnore]
    public Vector2Short UV { get; set; }
    [JsonIgnore]
    public Vector2Short Size { get; set; }
    [JsonIgnore]
    public TEditColor Color => TEditColor.Transparent;

    public override string ToString() => Name;
}
