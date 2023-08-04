using System;
using System.Text.Json.Serialization;
using System.Text.Json;
using TEdit.Geometry;

namespace TEdit.Common.Serialization;

public class Vector3ShortJsonConverter : JsonConverter<Vector3Short>
{
    public override Vector3Short Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray) { throw new JsonException(); }

        Vector3Short value = default;

        int ix = 0;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray) { return value; }

            if (ix == 0) value.X = reader.GetInt16();
            if (ix == 1) value.Y = reader.GetInt16();
            if (ix == 2) value.Z = reader.GetInt16();

            ix++;
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, Vector3Short value, JsonSerializerOptions options) =>
        writer.WriteRawValue($"[{value.X:0},{value.Y:0},{value.Z:0}]", true);
}
