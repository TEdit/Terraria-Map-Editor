using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System;
using System.Linq;
using TEdit5.Controls.WorldRenderEngine.Layers;
using TEdit.Terraria;

namespace TEdit5.Controls;

public class SkiaMinimapRenderBox : Control
{
    private readonly GlyphRun _noSkia;

    static SkiaMinimapRenderBox()
    {
        FocusableProperty.OverrideDefaultValue(typeof(SkiaMinimapRenderBox), true);
        AffectsRender<SkiaMinimapRenderBox>(
            ViewPortProperty,
            WorldProperty);
    }

    public SkiaMinimapRenderBox()
    {
        RenderOptions.SetBitmapInterpolationMode(this, BitmapInterpolationMode.None);
        ClipToBounds = true;

        // "No Skia" text for unsupported platforms
        var text = "Current rendering API is not Skia";
        var glyphs = text.Select(ch => Typeface.Default.GlyphTypeface.GetGlyph(ch)).ToArray();
        _noSkia = new GlyphRun(Typeface.Default.GlyphTypeface, 12, text.AsMemory(), glyphs);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var viewport = Bounds;
        context.Custom(new NoSkiaCustomDrawOp(viewport, _noSkia));
        context.Custom(new BackgroundGridCustomDrawOp(viewport, 4));
        context.Custom(new MinimapPixelsCustomDrawOp(viewport, World));

        Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
    }

    /// <summary>
    /// Defines the <see cref="World"/> property.
    /// </summary>
    public static readonly StyledProperty<World?> WorldProperty =
        AvaloniaProperty.Register<SkiaMinimapRenderBox, World?>(nameof(World), null);

    /// <summary>
    /// Terraria World
    /// </summary>
    public World? World
    {
        get { return GetValue(WorldProperty); }
        set { SetValue(WorldProperty, value); }
    }

    /// <summary>
    /// Defines the <see cref="ViewPort"/> property.
    /// </summary>
    public static readonly StyledProperty<Rect> ViewPortProperty =
        AvaloniaProperty.Register<SkiaMinimapRenderBox, Rect>(nameof(ViewPort), new(0, 0, 0, 0));

    /// <summary>
    /// ViewPort Rectangle
    /// </summary>
    public Rect ViewPort
    {
        get { return GetValue(ViewPortProperty); }
        set { SetValue(ViewPortProperty, value); }
    }
}
