using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TEdit.Common.Serialization;

/// <summary>
/// Converts numeric arrays to inline JSON format: [1, 2, 3] instead of multi-line.
/// Handles int[], short[], byte[], float[], double[] and their jagged array variants.
/// </summary>
public class InlineNumericArrayConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsArray) return false;

        var elementType = GetBaseElementType(typeToConvert);
        return IsNumericType(elementType);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(InlineNumericArrayConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    private static Type GetBaseElementType(Type arrayType)
    {
        var elementType = arrayType.GetElementType()!;
        while (elementType.IsArray)
            elementType = elementType.GetElementType()!;
        return elementType;
    }

    private static bool IsNumericType(Type type) =>
        type == typeof(int) || type == typeof(short) || type == typeof(byte) ||
        type == typeof(long) || type == typeof(float) || type == typeof(double) ||
        type == typeof(uint) || type == typeof(ushort) || type == typeof(sbyte) ||
        type == typeof(ulong) || type == typeof(decimal);
}

public class InlineNumericArrayConverter<T> : JsonConverter<T>
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Use default deserialization
        var tempOptions = new JsonSerializerOptions(options);
        tempOptions.Converters.Clear();
        foreach (var converter in options.Converters)
        {
            if (converter is not InlineNumericArrayConverterFactory)
                tempOptions.Converters.Add(converter);
        }
        return JsonSerializer.Deserialize<T>(ref reader, tempOptions);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        var json = WriteInlineArray((Array)(object)value);
        writer.WriteRawValue(json, true);
    }

    private static string WriteInlineArray(Array array)
    {
        var sb = new StringBuilder();
        WriteArrayRecursive(sb, array);
        return sb.ToString();
    }

    private static void WriteArrayRecursive(StringBuilder sb, Array array)
    {
        sb.Append('[');
        var elementType = array.GetType().GetElementType()!;

        for (int i = 0; i < array.Length; i++)
        {
            if (i > 0) sb.Append(", ");

            var element = array.GetValue(i);
            if (element == null)
            {
                sb.Append("null");
            }
            else if (elementType.IsArray)
            {
                WriteArrayRecursive(sb, (Array)element);
            }
            else
            {
                sb.Append(FormatNumber(element));
            }
        }
        sb.Append(']');
    }

    private static string FormatNumber(object value) => value switch
    {
        float f => f.ToString("G", System.Globalization.CultureInfo.InvariantCulture),
        double d => d.ToString("G", System.Globalization.CultureInfo.InvariantCulture),
        decimal m => m.ToString("G", System.Globalization.CultureInfo.InvariantCulture),
        _ => value.ToString()!
    };
}
