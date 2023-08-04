using System;
using System.Text.Json.Serialization;
using System.Text.Json;
using TEdit.Geometry;

namespace TEdit.Common.Serialization;

public class RectangleFloatJsonConverter : JsonConverter<RectangleFloat>
{
    public override RectangleFloat Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray) { throw new JsonException(); }

        RectangleFloat value = default;

        int ix = 0;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray) { return value; }

            if (ix == 0) value.X = reader.GetSingle();
            if (ix == 1) value.Y = reader.GetSingle();
            if (ix == 2) value.Width = reader.GetSingle();
            if (ix == 3) value.Height = reader.GetSingle();

            ix++;
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, RectangleFloat value, JsonSerializerOptions options) =>
        writer.WriteRawValue($"[{value.X:0},{value.Y:0},{value.Width:0},{value.Height:0}]", true);
}
