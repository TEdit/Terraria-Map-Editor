using System;
using System.Text.Json.Serialization;
using System.Text.Json;
using TEdit.Geometry;

namespace TEdit.Common.Serialization;

public class RectangleInt32JsonConverter : JsonConverter<RectangleInt32>
{
    public override RectangleInt32 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray) { throw new JsonException(); }

        RectangleInt32 value = default;

        int ix = 0;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray) { return value; }

            if (ix == 0) value.X = reader.GetInt32();
            if (ix == 1) value.Y = reader.GetInt32();
            if (ix == 2) value.Width = reader.GetInt32();
            if (ix == 3) value.Height = reader.GetInt32();

            ix++;
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, RectangleInt32 value, JsonSerializerOptions options) =>
        writer.WriteRawValue($"[{value.X:0},{value.Y:0},{value.Width:0},{value.Height:0}]", true);
}
