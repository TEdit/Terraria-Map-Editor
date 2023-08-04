using System;
using System.Text.Json.Serialization;
using System.Text.Json;
using TEdit.Geometry;

namespace TEdit.Common.Serialization;

public class Vector4ShortJsonConverter : JsonConverter<Vector4Short>
{
    public override Vector4Short Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray) { throw new JsonException(); }

        Vector4Short value = default;

        int ix = 0;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray) { return value; }

            if (ix == 0) value.X = reader.GetInt16();
            if (ix == 1) value.Y = reader.GetInt16();
            if (ix == 2) value.Z = reader.GetInt16();
            if (ix == 3) value.W = reader.GetInt16();

            ix++;
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, Vector4Short value, JsonSerializerOptions options) =>
        writer.WriteRawValue($"[{value.X:0},{value.Y:0},{value.Z:0},{value.W:0}]", true);
}
