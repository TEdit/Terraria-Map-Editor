using System;
using System.Text.Json.Serialization;
using System.Text.Json;
using TEdit.Geometry;

namespace TEdit.Common.Serialization;

public class Vector3FloatJsonConverter : JsonConverter<Vector3Float>
{
    public override Vector3Float Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray) { throw new JsonException(); }

        Vector3Float value = default;

        int ix = 0;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray) { return value; }

            if (ix == 0) value.X = reader.GetSingle();
            if (ix == 1) value.Y = reader.GetSingle();
            if (ix == 2) value.Z = reader.GetSingle();

            ix++;
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, Vector3Float value, JsonSerializerOptions options) =>
        writer.WriteRawValue($"[{value.X:0},{value.Y:0},{value.Z:0}]", true);
}
