using System.Text.Json.Serialization;

namespace TEdit.Terraria.Objects;

public class SecretSeedProperty
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("code")]
    public string Code { get; set; } = "";
}
