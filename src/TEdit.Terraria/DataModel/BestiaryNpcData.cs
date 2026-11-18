using System.Text.Json.Serialization;

namespace TEdit.Terraria.DataModel;

public class BestiaryNpcData
{
    public int Id { get; set; }
    public int BannerId { get; set; }
    public string FullName { get; set; } = "";
    public string Name { get; set; } = "";

    private string? _localizedName;

    /// <summary>Localized display name; falls back to <see cref="Name"/>.</summary>
    [JsonIgnore]
    public string LocalizedName
    {
        get => _localizedName ?? Name;
        set => _localizedName = value;
    }

    public string BestiaryId { get; set; } = "";
    public bool CanTalk { get; set; }
    public bool IsCritter { get; set; }
    public bool IsTownNpc { get; set; }
    public bool IsKillCredit { get; set; }
    public int BestiaryDisplayIndex { get; set; }
    public int BestiaryStars { get; set; }
    public string Category { get; set; } = "";

    public override string ToString() => LocalizedName;
}
