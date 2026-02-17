using System;
using System.Windows;
using System.Windows.Media;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TEdit.Common;

namespace TEdit.ViewModel;

[IReactiveObject]
public partial class HslColorViewModel
{
    [Reactive] private double _hue;           // 0.0 - 360.0
    [Reactive] private double _saturation;    // 0.0 - 1.0
    [Reactive] private double _lightness;     // 0.0 - 1.0
    [Reactive] private string _hexText = "000000";
    [Reactive] private string _label = "Color";

    private bool _isSyncing;

    public SolidColorBrush PreviewBrush { get; } = new(Colors.Black);
    public LinearGradientBrush HueTrackBrush { get; private set; }
    public LinearGradientBrush SaturationTrackBrush { get; private set; }
    public LinearGradientBrush LightnessTrackBrush { get; private set; }

    public HslColorViewModel()
    {
        HueTrackBrush = CreateHueGradient();
        SaturationTrackBrush = CreateSaturationGradient();
        LightnessTrackBrush = CreateLightnessGradient();

        this.WhenAnyValue(x => x.Hue, x => x.Saturation, x => x.Lightness)
            .Subscribe(_ => OnHslChanged());

        this.WhenAnyValue(x => x.HexText)
            .Subscribe(_ => OnHexChanged());
    }

    public HslColorViewModel(string label) : this()
    {
        _label = label;
    }

    public TEditColor GetColor()
    {
        var (r, g, b) = HslToRgb(Hue, Saturation, Lightness);
        return new TEditColor((byte)r, (byte)g, (byte)b, (byte)255);
    }

    public void SetColor(TEditColor color)
    {
        _isSyncing = true;
        try
        {
            var (h, s, l) = RgbToHsl(color.R, color.G, color.B);
            Hue = h;
            Saturation = s;
            Lightness = l;
            HexText = $"{color.R:X2}{color.G:X2}{color.B:X2}";
            UpdatePreview();
            UpdateGradients();
        }
        finally
        {
            _isSyncing = false;
        }
    }

    private void OnHslChanged()
    {
        if (_isSyncing) return;
        _isSyncing = true;
        try
        {
            var (r, g, b) = HslToRgb(Hue, Saturation, Lightness);
            HexText = $"{r:X2}{g:X2}{b:X2}";
            UpdatePreview();
            UpdateGradients();
        }
        finally
        {
            _isSyncing = false;
        }
    }

    private void OnHexChanged()
    {
        if (_isSyncing) return;
        if (HexText == null || HexText.Length != 6) return;

        if (uint.TryParse(HexText, System.Globalization.NumberStyles.HexNumber, null, out uint val))
        {
            _isSyncing = true;
            try
            {
                byte r = (byte)((val >> 16) & 0xFF);
                byte g = (byte)((val >> 8) & 0xFF);
                byte b = (byte)(val & 0xFF);
                var (h, s, l) = RgbToHsl(r, g, b);
                Hue = h;
                Saturation = s;
                Lightness = l;
                UpdatePreview();
                UpdateGradients();
            }
            finally
            {
                _isSyncing = false;
            }
        }
    }

    private void UpdatePreview()
    {
        var (r, g, b) = HslToRgb(Hue, Saturation, Lightness);
        PreviewBrush.Color = Color.FromRgb((byte)r, (byte)g, (byte)b);
    }

    private void UpdateGradients()
    {
        // Saturation gradient: gray to fully saturated at current H/L
        var (rLow, gLow, bLow) = HslToRgb(Hue, 0, Lightness);
        var (rHigh, gHigh, bHigh) = HslToRgb(Hue, 1, Lightness);
        SaturationTrackBrush.GradientStops[0].Color = Color.FromRgb((byte)rLow, (byte)gLow, (byte)bLow);
        SaturationTrackBrush.GradientStops[1].Color = Color.FromRgb((byte)rHigh, (byte)gHigh, (byte)bHigh);

        // Lightness gradient: black -> hue -> white
        var (rMid, gMid, bMid) = HslToRgb(Hue, Saturation, 0.5);
        LightnessTrackBrush.GradientStops[0].Color = Colors.Black;
        LightnessTrackBrush.GradientStops[1].Color = Color.FromRgb((byte)rMid, (byte)gMid, (byte)bMid);
        LightnessTrackBrush.GradientStops[2].Color = Colors.White;

        // Hue gradient: rainbow at current S/L
        for (int i = 0; i < 7; i++)
        {
            double h = i * 60.0;
            var (rh, gh, bh) = HslToRgb(h, Saturation, Lightness);
            HueTrackBrush.GradientStops[i].Color = Color.FromRgb((byte)rh, (byte)gh, (byte)bh);
        }
    }

    [ReactiveCommand]
    private void CopyHex()
    {
        try { Clipboard.SetText(HexText); } catch { }
    }

    [ReactiveCommand]
    private void PasteHex()
    {
        try
        {
            string text = Clipboard.GetText()?.Trim() ?? "";
            if (text.StartsWith("#")) text = text[1..];
            if (text.Length == 6 && uint.TryParse(text, System.Globalization.NumberStyles.HexNumber, null, out _))
                HexText = text.ToUpperInvariant();
        }
        catch { }
    }

    [ReactiveCommand]
    private void RandomColor()
    {
        var rng = Random.Shared;
        Hue = rng.NextDouble() * 360.0;
        Saturation = 0.3 + rng.NextDouble() * 0.7;
        Lightness = 0.3 + rng.NextDouble() * 0.4;
    }

    private static LinearGradientBrush CreateHueGradient()
    {
        var brush = new LinearGradientBrush { StartPoint = new Point(0, 0.5), EndPoint = new Point(1, 0.5) };
        for (int i = 0; i < 7; i++)
        {
            var (r, g, b) = HslToRgb(i * 60.0, 1, 0.5);
            brush.GradientStops.Add(new GradientStop(Color.FromRgb((byte)r, (byte)g, (byte)b), i / 6.0));
        }
        return brush;
    }

    private static LinearGradientBrush CreateSaturationGradient()
    {
        var brush = new LinearGradientBrush { StartPoint = new Point(0, 0.5), EndPoint = new Point(1, 0.5) };
        brush.GradientStops.Add(new GradientStop(Colors.Gray, 0));
        brush.GradientStops.Add(new GradientStop(Colors.Red, 1));
        return brush;
    }

    private static LinearGradientBrush CreateLightnessGradient()
    {
        var brush = new LinearGradientBrush { StartPoint = new Point(0, 0.5), EndPoint = new Point(1, 0.5) };
        brush.GradientStops.Add(new GradientStop(Colors.Black, 0));
        brush.GradientStops.Add(new GradientStop(Colors.Red, 0.5));
        brush.GradientStops.Add(new GradientStop(Colors.White, 1));
        return brush;
    }

    // Standard HSL -> RGB conversion
    public static (int R, int G, int B) HslToRgb(double h, double s, double l)
    {
        h = ((h % 360) + 360) % 360;
        double c = (1 - Math.Abs(2 * l - 1)) * s;
        double x = c * (1 - Math.Abs((h / 60.0) % 2 - 1));
        double m = l - c / 2;

        double r1, g1, b1;
        if (h < 60) { r1 = c; g1 = x; b1 = 0; }
        else if (h < 120) { r1 = x; g1 = c; b1 = 0; }
        else if (h < 180) { r1 = 0; g1 = c; b1 = x; }
        else if (h < 240) { r1 = 0; g1 = x; b1 = c; }
        else if (h < 300) { r1 = x; g1 = 0; b1 = c; }
        else { r1 = c; g1 = 0; b1 = x; }

        return (
            (int)Math.Round((r1 + m) * 255),
            (int)Math.Round((g1 + m) * 255),
            (int)Math.Round((b1 + m) * 255)
        );
    }

    // Standard RGB -> HSL conversion
    public static (double H, double S, double L) RgbToHsl(byte r, byte g, byte b)
    {
        double rd = r / 255.0, gd = g / 255.0, bd = b / 255.0;
        double max = Math.Max(rd, Math.Max(gd, bd));
        double min = Math.Min(rd, Math.Min(gd, bd));
        double l = (max + min) / 2;

        if (Math.Abs(max - min) < 0.0001)
            return (0, 0, l);

        double d = max - min;
        double s = l > 0.5 ? d / (2 - max - min) : d / (max + min);

        double h;
        if (Math.Abs(max - rd) < 0.0001)
            h = ((gd - bd) / d + (gd < bd ? 6 : 0)) * 60;
        else if (Math.Abs(max - gd) < 0.0001)
            h = ((bd - rd) / d + 2) * 60;
        else
            h = ((rd - gd) / d + 4) * 60;

        return (h, s, l);
    }
}
