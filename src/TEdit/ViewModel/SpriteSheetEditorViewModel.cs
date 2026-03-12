using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TEdit.Configuration;
using TEdit.Geometry;
using TEdit.Terraria;
using TEdit.Terraria.Objects;
using TEdit.Terraria.TModLoader;

namespace TEdit.ViewModel;

/// <summary>
/// Editable frame entry in the sprite sheet editor.
/// Wraps FrameProperty data for two-way binding.
/// </summary>
public partial class EditableFrame : ReactiveObject
{
    [Reactive] private string _name = "Default";
    [Reactive] private string? _variety;
    [Reactive] private short _uvX;
    [Reactive] private short _uvY;
    [Reactive] private short _sizeX = 1;
    [Reactive] private short _sizeY = 1;
    [Reactive] private FrameAnchor _anchor = FrameAnchor.None;
    [Reactive] private bool _isSelected;

    /// <summary>Pixel X position on texture.</summary>
    public int PixelX { get; set; }
    /// <summary>Pixel Y position on texture.</summary>
    public int PixelY { get; set; }
    /// <summary>Pixel width on texture.</summary>
    public int PixelWidth { get; set; }
    /// <summary>Pixel height on texture.</summary>
    public int PixelHeight { get; set; }

    public static EditableFrame FromFrameProperty(FrameProperty fp, Vector2Short textureGrid, Vector2Short frameGap, Vector2Short defaultSize)
    {
        var size = (fp.Size.X > 0 && fp.Size.Y > 0) ? fp.Size : defaultSize;
        var ef = new EditableFrame
        {
            Name = fp.Name,
            Variety = fp.Variety,
            UvX = fp.UV.X,
            UvY = fp.UV.Y,
            SizeX = size.X,
            SizeY = size.Y,
            Anchor = fp.Anchor,
        };

        // Calculate pixel position from UV and grid
        ef.PixelX = fp.UV.X;
        ef.PixelY = fp.UV.Y;
        ef.PixelWidth = size.X * textureGrid.X + (size.X - 1) * frameGap.X;
        ef.PixelHeight = size.Y * textureGrid.Y + (size.Y - 1) * frameGap.Y;

        return ef;
    }

    public FrameProperty ToFrameProperty() => new()
    {
        Name = Name,
        Variety = Variety,
        UV = new Vector2Short(UvX, UvY),
        Size = new Vector2Short(SizeX, SizeY),
        Anchor = Anchor,
    };
}

/// <summary>
/// ViewModel for the Sprite Sheet Editor dialog.
/// Manages frame editing, grid settings, and save/reset operations.
/// </summary>
public partial class SpriteSheetEditorViewModel : ReactiveObject
{
    private readonly ModTileConfigStore _configStore;
    private readonly TileProperty _tileProperty;

    // Grid settings
    [Reactive] private short _textureGridX = 16;
    [Reactive] private short _textureGridY = 16;
    [Reactive] private short _frameGapX = 2;
    [Reactive] private short _frameGapY = 2;
    [Reactive] private short _frameSizeX = 1;
    [Reactive] private short _frameSizeY = 1;
    [Reactive] private bool _isAnimated;

    /// <summary>
    /// Comma-separated per-row pixel heights (e.g. "16, 18" for chests).
    /// Blank = uniform TextureGridY for all rows.
    /// </summary>
    [Reactive] private string _coordinateHeightsText = "";

    // UI state
    [Reactive] private EditableFrame? _selectedFrame;
    [Reactive] private double _zoomLevel = 2.0;
    [Reactive] private string _statusText = "Ready";

    /// <summary>The full tile texture as a WPF bitmap.</summary>
    public WriteableBitmap? TileTexture { get; }

    /// <summary>Tile name displayed in title bar.</summary>
    public string TileName => _tileProperty.Name;

    /// <summary>Tile ID.</summary>
    public int TileId => _tileProperty.Id;

    /// <summary>Whether this is a mod tile (has ':' in name).</summary>
    public bool IsModTile => _tileProperty.Name.Contains(':');

    /// <summary>Mod name extracted from tile name.</summary>
    public string ModName => _tileProperty.ModName;

    /// <summary>Short tile name without mod prefix.</summary>
    public string ShortTileName => _tileProperty.ShortName;

    /// <summary>All defined frames.</summary>
    public ObservableCollection<EditableFrame> Frames { get; } = new();

    /// <summary>Whether save was performed (dialog result).</summary>
    public bool WasSaved { get; private set; }

    public SpriteSheetEditorViewModel(TileProperty tileProperty, WriteableBitmap? texture)
    {
        _tileProperty = tileProperty;
        TileTexture = texture;
        _configStore = new ModTileConfigStore(AppDataPaths.DataDir);

        // Load grid settings from tile property
        TextureGridX = tileProperty.TextureGrid.X;
        TextureGridY = tileProperty.TextureGrid.Y;
        FrameGapX = tileProperty.FrameGap.X;
        FrameGapY = tileProperty.FrameGap.Y;
        IsAnimated = tileProperty.IsAnimated;

        if (tileProperty.FrameSize?.Length > 0)
        {
            FrameSizeX = tileProperty.FrameSize[0].X;
            FrameSizeY = tileProperty.FrameSize[0].Y;
        }

        // Try loading user override first, then fall back to tile's frame data
        LoadFromOverrideOrTile();

        SaveCommand = ReactiveCommand.Create(ExecuteSave);
        ResetCommand = ReactiveCommand.Create(ExecuteReset);
        AutoDetectCommand = ReactiveCommand.Create(ExecuteAutoDetect);
        AddFrameCommand = ReactiveCommand.Create(ExecuteAddFrame);
        RemoveFrameCommand = ReactiveCommand.Create(ExecuteRemoveFrame);
        ZoomInCommand = ReactiveCommand.Create(() => ZoomLevel = Math.Min(ZoomLevel * 1.5, 16.0));
        ZoomOutCommand = ReactiveCommand.Create(() => ZoomLevel = Math.Max(ZoomLevel / 1.5, 0.25));
    }

    /// <summary>Available FrameAnchor values for ComboBox binding.</summary>
    public static FrameAnchor[] AnchorValues { get; } = Enum.GetValues<FrameAnchor>();

    public ICommand SaveCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand AutoDetectCommand { get; }
    public ICommand AddFrameCommand { get; }
    public ICommand RemoveFrameCommand { get; }
    public ICommand ZoomInCommand { get; }
    public ICommand ZoomOutCommand { get; }

    private void LoadFromOverrideOrTile()
    {
        // Check for user override
        if (IsModTile)
        {
            var over = _configStore.GetTileOverride(ModName, ShortTileName);
            if (over != null)
            {
                ApplyOverride(over);
                StatusText = "Loaded from user override";
                return;
            }
        }

        // Fall back to tile's existing frame data
        LoadFromTileProperty();
    }

    private void ApplyOverride(ModTileOverride over)
    {
        TextureGridX = over.TextureGrid.X;
        TextureGridY = over.TextureGrid.Y;
        FrameGapX = over.FrameGap.X;
        FrameGapY = over.FrameGap.Y;
        IsAnimated = over.IsAnimated;
        CoordinateHeightsText = over.CoordinateHeights != null
            ? string.Join(", ", over.CoordinateHeights)
            : "";

        if (over.FrameSize?.Length > 0)
        {
            FrameSizeX = over.FrameSize[0].X;
            FrameSizeY = over.FrameSize[0].Y;
        }

        Frames.Clear();
        if (over.Frames != null)
        {
            var grid = new Vector2Short(TextureGridX, TextureGridY);
            var gap = new Vector2Short(FrameGapX, FrameGapY);
            var defaultSize = new Vector2Short(FrameSizeX, FrameSizeY);
            foreach (var fp in over.Frames)
            {
                Frames.Add(EditableFrame.FromFrameProperty(fp, grid, gap, defaultSize));
            }
        }
    }

    private void LoadFromTileProperty()
    {
        Frames.Clear();
        if (_tileProperty.Frames != null)
        {
            var grid = new Vector2Short(TextureGridX, TextureGridY);
            var gap = new Vector2Short(FrameGapX, FrameGapY);
            var defaultSize = new Vector2Short(FrameSizeX, FrameSizeY);
            foreach (var fp in _tileProperty.Frames)
            {
                Frames.Add(EditableFrame.FromFrameProperty(fp, grid, gap, defaultSize));
            }
        }
        StatusText = $"Loaded {Frames.Count} frames from tile data";
    }

    private void ExecuteSave()
    {
        var over = BuildOverride();

        if (IsModTile)
        {
            _configStore.SaveTile(ModName, ShortTileName, over);
            StatusText = $"Saved to {ModName} override file";
        }
        else
        {
            // For vanilla tiles, save under a "Vanilla" override file
            _configStore.SaveTile("Vanilla", _tileProperty.Name, over);
            StatusText = $"Saved vanilla override for {_tileProperty.Name}";
        }

        // Update in-memory TileProperty so RebuildSpriteForTile sees the new data
        _tileProperty.TextureGrid = over.TextureGrid;
        _tileProperty.FrameGap = over.FrameGap;
        _tileProperty.FrameSize = over.FrameSize;
        _tileProperty.IsAnimated = over.IsAnimated;
        _tileProperty.Frames = over.Frames;

        WasSaved = true;
    }

    private void ExecuteReset()
    {
        if (IsModTile)
        {
            _configStore.RemoveTile(ModName, ShortTileName);
        }
        else
        {
            _configStore.RemoveTile("Vanilla", _tileProperty.Name);
        }

        LoadFromTileProperty();
        StatusText = "Reset to default tile data";
    }

    private void ExecuteAutoDetect()
    {
        if (TileTexture == null)
        {
            StatusText = "No texture loaded — cannot auto-detect";
            return;
        }

        int texW = TileTexture.PixelWidth;
        int texH = TileTexture.PixelHeight;
        int intervalX = TextureGridX + FrameGapX;
        int intervalY = TextureGridY + FrameGapY;

        int cols = intervalX > 0 ? (texW + FrameGapX) / intervalX : 1;
        int rows = intervalY > 0 ? (texH + FrameGapY) / intervalY : 1;
        if (cols < 1) cols = 1;
        if (rows < 1) rows = 1;

        short fw, fh;
        string reason;

        // Pixel-based detection from WriteableBitmap when possible
        if (TileTexture.Format == System.Windows.Media.PixelFormats.Bgra32 ||
            TileTexture.Format == System.Windows.Media.PixelFormats.Pbgra32)
        {
            int stride = texW * 4;
            byte[] pixels = new byte[texH * stride];
            TileTexture.CopyPixels(pixels, stride, 0);

            // Convert BGRA to RGBA for DetectFrameSize (which expects RGBA order)
            for (int i = 0; i < pixels.Length; i += 4)
            {
                (pixels[i], pixels[i + 2]) = (pixels[i + 2], pixels[i]); // swap B and R
            }

            (fw, fh) = TmodTextureExtractor.DetectFrameSizeFromRgba(
                pixels, texW, texH, TextureGridX, TextureGridY, FrameGapX, FrameGapY, cols, rows);
            reason = $"pixel-detected {fw}x{fh}";
        }
        else
        {
            AutoFrameHeuristic(cols, rows, out fw, out fh, out reason);
        }

        FrameSizeX = fw;
        FrameSizeY = fh;

        RegenerateFramesFromGrid();
        StatusText = $"Auto: {fw}x{fh} frame, {Frames.Count} frames ({reason})";
    }

    /// <summary>
    /// Heuristic autoframe rules based on cell dimensions.
    /// Produces a starting point — user overrides always take precedence.
    /// </summary>
    internal static void AutoFrameHeuristic(int cols, int rows, out short frameSizeX, out short frameSizeY, out string reason)
    {
        // 1x1 → single cell
        if (cols == 1 && rows == 1)
        {
            frameSizeX = 1;
            frameSizeY = 1;
            reason = "single cell";
            return;
        }

        // Small rectangular (both ≤ 3) → single frame
        if (cols <= 3 && rows <= 3)
        {
            frameSizeX = (short)cols;
            frameSizeY = (short)rows;
            reason = "small sprite";
            return;
        }

        // 5x3 → almost always a single frame
        if (cols == 5 && rows == 3)
        {
            frameSizeX = (short)cols;
            frameSizeY = (short)rows;
            reason = "5x3 single sprite";
            return;
        }

        // Very tall (h > 3*w), narrow (w ≤ 5) → try dividing into frames of height 3, then 2
        if (rows > 3 * cols && cols <= 5)
        {
            if (rows % 3 == 0)
            {
                frameSizeX = (short)cols;
                frameSizeY = 3;
                reason = $"tall animated, {rows / 3} frames of {cols}x3";
                return;
            }
            if (rows % 2 == 0)
            {
                frameSizeX = (short)cols;
                frameSizeY = 2;
                reason = $"tall animated, {rows / 2} frames of {cols}x2";
                return;
            }
        }

        // Width > 3: most likely 3 tall if divisible
        if (cols > 3 && rows >= 3 && rows % 3 == 0)
        {
            frameSizeX = (short)cols;
            frameSizeY = 3;
            reason = $"wide sprite, {rows / 3} rows of {cols}x3";
            return;
        }

        // Animation patterns: 3-5 wide, try frame height 3 then 2
        if (cols >= 3 && cols <= 5)
        {
            if (rows % 3 == 0 && rows > 3)
            {
                frameSizeX = (short)cols;
                frameSizeY = 3;
                reason = $"animated {cols}x3, {rows / 3} frames";
                return;
            }
            if (rows % 2 == 0 && rows > 3)
            {
                frameSizeX = (short)cols;
                frameSizeY = 2;
                reason = $"animated {cols}x2, {rows / 2} frames";
                return;
            }
            // Short enough to be single frame
            if (rows <= 5)
            {
                frameSizeX = (short)cols;
                frameSizeY = (short)rows;
                reason = "medium single sprite";
                return;
            }
        }

        // Horizontal strip where width is a multiple of height → square frames
        if (cols > rows && rows > 0 && cols % rows == 0)
        {
            frameSizeX = (short)rows;
            frameSizeY = (short)rows;
            reason = $"horizontal {cols / rows} frames";
            return;
        }

        // Vertical, narrow (w ≤ 2), short (h ≤ 5) → single frame
        if (rows > cols && cols <= 2 && rows <= 5)
        {
            frameSizeX = (short)cols;
            frameSizeY = (short)rows;
            reason = "tall narrow sprite";
            return;
        }

        // Larger sheets
        if (cols % 2 != 0 || rows % 2 != 0)
        {
            frameSizeX = 1;
            frameSizeY = 1;
            reason = "odd dimension, 1x1";
            return;
        }

        // Both even → 2x2
        frameSizeX = 2;
        frameSizeY = 2;
        reason = "even dimensions, 2x2";
    }

    public void SetSingleFrame()
    {
        if (TileTexture == null)
        {
            StatusText = "No texture loaded";
            return;
        }

        int texW = TileTexture.PixelWidth;
        int texH = TileTexture.PixelHeight;
        int intervalX = TextureGridX + FrameGapX;
        int intervalY = TextureGridY + FrameGapY;

        // Calculate frame size in tiles to cover the entire texture
        FrameSizeX = (short)(intervalX > 0 ? (texW + FrameGapX) / intervalX : 1);
        FrameSizeY = (short)(intervalY > 0 ? (texH + FrameGapY) / intervalY : 1);
        if (FrameSizeX < 1) FrameSizeX = 1;
        if (FrameSizeY < 1) FrameSizeY = 1;

        int frameW = FrameSizeX * TextureGridX + (FrameSizeX - 1) * FrameGapX;
        int frameH = FrameSizeY * TextureGridY + (FrameSizeY - 1) * FrameGapY;

        Frames.Clear();
        Frames.Add(new EditableFrame
        {
            Name = "Frame 0",
            UvX = 0,
            UvY = 0,
            SizeX = FrameSizeX,
            SizeY = FrameSizeY,
            PixelX = 0,
            PixelY = 0,
            PixelWidth = frameW,
            PixelHeight = frameH,
        });

        StatusText = $"Single frame: {FrameSizeX}x{FrameSizeY} tiles";
    }

    public void RegenerateFramesFromGrid()
    {
        if (TileTexture == null) return;

        int texW = TileTexture.PixelWidth;
        int texH = TileTexture.PixelHeight;
        int intervalX = TextureGridX + FrameGapX;
        int strideX = intervalX * FrameSizeX;
        int frameW = FrameSizeX * TextureGridX + (FrameSizeX - 1) * FrameGapX;

        // Compute vertical stride and frame height using CoordinateHeights if available.
        var coordHeights = ParseCoordinateHeights();
        int strideY;
        int frameH;
        if (coordHeights != null && coordHeights.Length == FrameSizeY)
        {
            strideY = TmodTextureExtractor.ComputeStrideFromCoordinateHeights(coordHeights, FrameGapY);
            frameH = TmodTextureExtractor.ComputeFrameHeightFromCoordinateHeights(coordHeights, FrameGapY);
        }
        else
        {
            int intervalY = TextureGridY + FrameGapY;
            strideY = intervalY * FrameSizeY;
            frameH = FrameSizeY * TextureGridY + (FrameSizeY - 1) * FrameGapY;
        }

        Frames.Clear();
        int index = 0;

        // For animated tiles, only use the first row — animation frames are stacked
        // vertically below, but styles are only in the first row.
        int maxY = IsAnimated ? frameH : texH + FrameGapY;

        // Allow a tolerance of one gap width/height to handle textures that
        // don't include trailing gap after the last row/column.
        for (int y = 0; y + frameH <= maxY; y += strideY)
        {
            for (int x = 0; x + frameW <= texW + FrameGapX; x += strideX)
            {
                Frames.Add(new EditableFrame
                {
                    Name = $"Frame {index}",
                    UvX = (short)x,
                    UvY = (short)y,
                    SizeX = FrameSizeX,
                    SizeY = FrameSizeY,
                    PixelX = x,
                    PixelY = y,
                    PixelWidth = frameW,
                    PixelHeight = frameH,
                });
                index++;
            }
        }
    }

    private void ExecuteAddFrame()
    {
        var frame = new EditableFrame
        {
            Name = $"Frame {Frames.Count}",
            UvX = 0,
            UvY = 0,
            SizeX = FrameSizeX,
            SizeY = FrameSizeY,
            PixelX = 0,
            PixelY = 0,
            PixelWidth = FrameSizeX * TextureGridX + (FrameSizeX - 1) * FrameGapX,
            PixelHeight = FrameSizeY * TextureGridY + (FrameSizeY - 1) * FrameGapY,
        };
        Frames.Add(frame);
        SelectedFrame = frame;
        StatusText = $"Added frame ({Frames.Count} total)";
    }

    private void ExecuteRemoveFrame()
    {
        if (SelectedFrame == null) return;
        var frame = SelectedFrame;
        SelectedFrame = null;
        Frames.Remove(frame);
        StatusText = $"Removed frame ({Frames.Count} remaining)";
    }

    public ModTileOverride BuildOverride()
    {
        return new ModTileOverride
        {
            TextureGrid = new Vector2Short(TextureGridX, TextureGridY),
            FrameGap = new Vector2Short(FrameGapX, FrameGapY),
            FrameSize = [new Vector2Short(FrameSizeX, FrameSizeY)],
            IsAnimated = IsAnimated,
            CoordinateHeights = ParseCoordinateHeights(),
            Frames = Frames.Select(f => f.ToFrameProperty()).ToList(),
        };
    }

    /// <summary>
    /// Parses the CoordinateHeightsText into a short array.
    /// Returns null if blank or invalid.
    /// </summary>
    private short[]? ParseCoordinateHeights()
    {
        if (string.IsNullOrWhiteSpace(CoordinateHeightsText)) return null;
        try
        {
            var parts = CoordinateHeightsText.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return null;
            var heights = new short[parts.Length];
            for (int i = 0; i < parts.Length; i++)
                heights[i] = short.Parse(parts[i]);
            return heights;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Adds a frame from a drag-selected tile region.
    /// </summary>
    public void AddFrameFromSelection(short uvX, short uvY, short sizeX, short sizeY, int pixelW, int pixelH)
    {
        var frame = new EditableFrame
        {
            Name = $"Frame {Frames.Count}",
            UvX = uvX,
            UvY = uvY,
            SizeX = sizeX,
            SizeY = sizeY,
            PixelX = uvX,
            PixelY = uvY,
            PixelWidth = pixelW,
            PixelHeight = pixelH,
        };
        Frames.Add(frame);
        SelectedFrame = frame;
        StatusText = $"Added selection frame at ({uvX},{uvY}) {sizeX}x{sizeY} ({Frames.Count} total)";
    }

    /// <summary>
    /// Gets the frame at a pixel position on the texture.
    /// </summary>
    public EditableFrame? GetFrameAtPixel(double pixelX, double pixelY)
    {
        foreach (var frame in Frames)
        {
            if (pixelX >= frame.PixelX && pixelX < frame.PixelX + frame.PixelWidth &&
                pixelY >= frame.PixelY && pixelY < frame.PixelY + frame.PixelHeight)
            {
                return frame;
            }
        }
        return null;
    }
}
