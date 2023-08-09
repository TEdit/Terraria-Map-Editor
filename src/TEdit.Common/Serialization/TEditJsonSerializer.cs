using System.Text.Json.Serialization;
using System.Text.Json;

namespace TEdit.Common.Serialization;

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
        DefaultOptions.Converters.Add(new Vector2ByteJsonConverter());
        DefaultOptions.Converters.Add(new Vector2Int32JsonConverter());
        DefaultOptions.Converters.Add(new Vector2FloatJsonConverter());

        DefaultOptions.Converters.Add(new Vector3ShortJsonConverter());
        DefaultOptions.Converters.Add(new Vector3ByteJsonConverter());
        DefaultOptions.Converters.Add(new Vector3Int32JsonConverter());
        DefaultOptions.Converters.Add(new Vector3FloatJsonConverter());

        DefaultOptions.Converters.Add(new Vector4ShortJsonConverter());
        DefaultOptions.Converters.Add(new Vector4ByteJsonConverter());
        DefaultOptions.Converters.Add(new Vector4Int32JsonConverter());
        DefaultOptions.Converters.Add(new Vector4FloatJsonConverter());

        DefaultOptions.Converters.Add(new RectangleInt32JsonConverter());
        DefaultOptions.Converters.Add(new RectangleFloatJsonConverter());

        DefaultOptions.Converters.Add(new TEditColorJsonConverter());
    }

    public static JsonSerializerOptions DefaultOptions { get; }
}
