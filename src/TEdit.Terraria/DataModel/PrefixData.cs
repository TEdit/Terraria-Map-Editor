using System.Text.Json.Serialization;

namespace TEdit.Terraria.DataModel;

public class PrefixData
{
    [JsonPropertyOrder(0)]
    public int Id { get; set; }

    [JsonPropertyOrder(1)]
    public string Name { get; set; } = "";

    /// <summary>Runtime-localized name; Name remains the original source name.</summary>
    [JsonIgnore]
    public string? LocalizedName { get; set; }

    [JsonIgnore]
    public string DisplayName => string.IsNullOrWhiteSpace(LocalizedName) ? Name : LocalizedName;

    [JsonIgnore]
    public byte PrefixId => (byte)Id;

    [JsonPropertyOrder(2)]
    public string? Key { get; set; }

    public override string ToString() => Name;
}
