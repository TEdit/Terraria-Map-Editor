using System.Globalization;
using System.IO;
using System.Reflection;
using TEdit.Geometry;
using TEdit.Terraria;
using Xunit;

namespace TEdit.Tests;

public class LeafletExportTests
{
    [Theory]
    [InlineData("en-US")]
    [InlineData("de-DE")]
    [InlineData("fr-FR")]
    public void FractionalSelectionBounds_UseJavaScriptDecimalPoints(string locale)
    {
        var previousCulture = CultureInfo.CurrentCulture;
        string directory = Path.Combine(Path.GetTempPath(), $"tedit-leaflet-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(locale);
            var exporter = typeof(TEdit.ViewModel.WorldViewModel).Assembly.GetType("TEdit.Export.LeafletTileExporter", throwOnError: true)!;
            var writeHtml = exporter.GetMethod("WriteLeafletHtml", BindingFlags.NonPublic | BindingFlags.Static)!;
            writeHtml.Invoke(null, [directory, new RectangleInt32(0, 0, 4202, 1201), "Locale test", new World()]);

            string html = File.ReadAllText(Path.Combine(directory, "index.html"));
            Assert.Contains("L.latLng(-300.25, 1050.5)", html);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            foreach (string file in Directory.GetFiles(directory))
            {
                File.Delete(file);
            }
            Directory.Delete(directory);
        }
    }
}
