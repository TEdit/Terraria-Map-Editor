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
