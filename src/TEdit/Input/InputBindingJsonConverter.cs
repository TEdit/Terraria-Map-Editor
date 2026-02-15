using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TEdit.Input;

/// <summary>
/// JSON converter for InputBinding - serializes as string like "Ctrl+C", "Shift+LeftClick".
/// </summary>
public class InputBindingJsonConverter : JsonConverter<InputBinding>
{
    public override InputBinding Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        return str == null ? default : InputBinding.Parse(str);
    }

    public override void Write(Utf8JsonWriter writer, InputBinding value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}

/// <summary>
/// JSON converter for List of InputBinding.
/// </summary>
public class InputBindingListJsonConverter : JsonConverter<List<InputBinding>>
{
    public override List<InputBinding>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            return null;

        var list = new List<InputBinding>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            if (reader.TokenType == JsonTokenType.String)
            {
                var str = reader.GetString();
                if (!string.IsNullOrEmpty(str))
                {
                    var binding = InputBinding.Parse(str);
                    if (binding.IsValid)
                        list.Add(binding);
                }
            }
        }

        return list;
    }

    public override void Write(Utf8JsonWriter writer, List<InputBinding> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var binding in value)
        {
            writer.WriteStringValue(binding.ToString());
        }
        writer.WriteEndArray();
    }
}

/// <summary>
/// JSON converter for Dictionary of action ID to List of InputBinding.
/// </summary>
public class InputBindingsDictionaryJsonConverter : JsonConverter<Dictionary<string, List<InputBinding>>>
{
    public override Dictionary<string, List<InputBinding>>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            return null;

        var dict = new Dictionary<string, List<InputBinding>>();
        var listConverter = new InputBindingListJsonConverter();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var key = reader.GetString();
                reader.Read();

                if (key != null)
                {
                    var list = listConverter.Read(ref reader, typeof(List<InputBinding>), options);
                    if (list != null)
                        dict[key] = list;
                }
            }
        }

        return dict;
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<string, List<InputBinding>> value, JsonSerializerOptions options)
    {
        var listConverter = new InputBindingListJsonConverter();

        writer.WriteStartObject();
        foreach (var (key, list) in value)
        {
            writer.WritePropertyName(key);
            listConverter.Write(writer, list, options);
        }
        writer.WriteEndObject();
    }
}
