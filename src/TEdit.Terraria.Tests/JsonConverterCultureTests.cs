using System.Globalization;
using System.Text.Json;
using TEdit.Common.Serialization;
using TEdit.Geometry;
using Shouldly;
using Xunit;

namespace TEdit.Terraria.Tests;

public class JsonConverterCultureTests
{
    [Fact]
    public void Vector2Int32_NegativeUnderSwedishCulture_WritesAsciiMinus()
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            // sv-SE formats negatives with U+2212, which is not valid JSON.
            CultureInfo.CurrentCulture = new CultureInfo("sv-SE");
            var options = new JsonSerializerOptions();
            options.Converters.Add(new Vector2Int32JsonConverter());

            string json = JsonSerializer.Serialize(new Vector2Int32(-5, 3), options);

            json.ShouldBe("[-5,3]");
            JsonSerializer.Deserialize<Vector2Int32>(json, options).X.ShouldBe(-5);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }
}
