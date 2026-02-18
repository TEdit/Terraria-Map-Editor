namespace TEdit.Terraria;

public partial class BannerItem : ReactiveObject
{
    /// <summary>Tally index (index into ClaimableBanners/KilledMobs).</summary>
    public int TallyIndex { get; set; }

    /// <summary>Banner item name (e.g. "Zombie Banner").</summary>
    public string Name { get; set; } = "";

    /// <summary>Biome/event category from bestiaryNpcs.json.</summary>
    public string Category { get; set; } = "";

    /// <summary>First NPC ID with this banner, used for icon preview.</summary>
    public int NpcId { get; set; }

    /// <summary>Kill count from KilledMobs (read-only display).</summary>
    [Reactive]
    private int _kills;

    /// <summary>Claimable banner count (editable, clamped to 0-9999).</summary>
    private ushort _count;
    public ushort Count
    {
        get => _count;
        set
        {
            // Clamp to valid range (matching Terraria's max of 9999)
            ushort clamped = value > 9999 ? (ushort)9999 : value;
            this.RaiseAndSetIfChanged(ref _count, clamped);
        }
    }
}
