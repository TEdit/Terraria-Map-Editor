using SkiaSharp;
using TEdit.Common;

namespace TEdit5.Controls.WorldRenderEngine;

public static class SkiaTEditExtensions
{
    public static SKColor ToSKColor(this TEditColor color)
    {
        return new SKColor(color.R, color.G, color.B, color.A);
    }
}
