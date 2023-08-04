using System;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace TEdit.Common.Serialization;

public class TEditColorJsonConverter : JsonConverter<TEditColor>
{
    public override TEditColor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        TEditColor.FromString(reader.GetString()!);

    public override void Write(Utf8JsonWriter writer, TEditColor value, JsonSerializerOptions options) =>
        writer.WriteStringValue(TEditColor.ToHexString(value));
}

public class TEditJsonSerializer
{
    static TEditJsonSerializer()
    {
        DefaultOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        DefaultOptions.Converters.Add(new Vector2ShortJsonConverter());
        DefaultOptions.Converters.Add(new TEditColorJsonConverter());
    }

    public static JsonSerializerOptions DefaultOptions { get; }
}
