using System.ComponentModel;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TEdit.Terraria;

namespace TEdit.Editor;

public enum HalfBlockMode
{
    [Description("No Change")]
    NoAction = -2,
    [Description("Set Solid")]
    Solid = -1,
    [Description("Half Block")]
    HalfBlock = 0,
    [Description("Ramp Left")]
    RampLeft = 1,
    [Description("Ramp Right")]
    RampRight = 2
}

public partial class TilePicker : ReactiveObject
{
    private PaintMode _paintMode = ToolDefaultData.PaintMode;
    [Reactive]
    private MaskMode _tileMaskMode = ToolDefaultData.PaintTileMaskMode;
    [Reactive]
    private MaskMode _wallMaskMode = ToolDefaultData.PaintWallMaskMode;
    [Reactive]
    private int _wall = ToolDefaultData.PaintWall;
    [Reactive]
    private int _tile = ToolDefaultData.PaintTile;
    [Reactive]
    private int _wallMask = ToolDefaultData.PaintWallMask;
    [Reactive]
    private int _tileMask = ToolDefaultData.PaintTileMask;

    [Reactive]
    private MaskMode _wallPaintMaskMode = MaskMode.Off;
    [Reactive]
    private MaskMode _tilePaintMaskMode = MaskMode.Off;
    [Reactive]
    private MaskMode _liquidMaskMode = MaskMode.Off;

    [Reactive]
    private bool _blueWireActive = ToolDefaultData.BlueWire;
    [Reactive]
    private bool _redWireActive = ToolDefaultData.RedWire;
    [Reactive]
    private bool _greenWireActive = ToolDefaultData.GreenWire;
    [Reactive]
    private bool _yellowWireActive = ToolDefaultData.YellowWire;
    [Reactive]
    private bool _tileStyleActive = ToolDefaultData.PaintTileActive;
    [Reactive]
    private bool _wallStyleActive = ToolDefaultData.PaintWallActive;
    [Reactive]
    private TrackMode _trackMode = TrackMode.Track;
    [Reactive]
    private JunctionBoxMode _junctionBoxMode = JunctionBoxMode.None;
    [Reactive]
    private LiquidAmountMode _liquidAmountMode = LiquidAmountMode.OneHundredPercent;

    [Reactive]
    private WireReplaceMode _wireReplaceModeRed = WireReplaceMode.Red;
    [Reactive]
    private WireReplaceMode _wireReplaceModeBlue = WireReplaceMode.Blue;
    [Reactive]
    private WireReplaceMode _wireReplaceModeGreen = WireReplaceMode.Green;
    [Reactive]
    private WireReplaceMode _wireReplaceModeYellow = WireReplaceMode.Yellow;

    [Reactive]
    private bool _wireReplaceRed = false;
    [Reactive]
    private bool _wireReplaceBlue = false;
    [Reactive]
    private bool _wireReplaceGreen = false;
    [Reactive]
    private bool _wireReplaceYellow = false;
    [Reactive]
    private bool _usePaintMasks = false;

    [Reactive]
    private bool _enableWallCoating = false;
    [Reactive]
    private bool _enableTileCoating = false;
    [Reactive]
    private bool _useCoatingMasks = false;

    [Reactive]
    private bool _wallCoatingIlluminant = false;
    [Reactive]
    private bool _wallCoatingEcho = false;
    [Reactive]
    private bool _tileCoatingIlluminant = false;
    [Reactive]
    private bool _tileCoatingEcho = false;

    [Reactive]
    private BrickStyle _brickStyle = BrickStyle.Full;

    public bool WireReplaceActive => WireReplaceRed || WireReplaceBlue || WireReplaceGreen || WireReplaceYellow;

    private bool _isEraser;

    public bool IsEraser
    {
        get { return _isEraser; }
        set
        {
            if (!value && PaintMode == PaintMode.Sprites) { return; } // the only allowed mode for sprite painting is erase
            this.RaiseAndSetIfChanged(ref _isEraser, value);
        }
    }

    [Reactive]
    private LiquidType _liquidType;

    [Reactive]
    private bool _actuator;

    [Reactive]
    private bool _tilePaintActive;

    [Reactive]
    private bool _wallPaintActive;

    [Reactive]
    private bool _brickStyleActive;

    [Reactive]
    private bool _extrasActive;

    [Reactive]
    private bool _actuatorInActive;

    [Reactive]
    private int _wallColor;

    [Reactive]
    private int _tileColor;

    public PaintMode PaintMode
    {
        get { return _paintMode; }
        set
        {
            this.RaiseAndSetIfChanged(ref _paintMode, value);
            if (value == PaintMode.Sprites)
            {
                IsEraser = true;
            }
        }
    }

    public void Swap(bool wall = false)
    {
        switch (PaintMode)
        {
            //                case PaintMode.Tile:
            //                    SwapTile();
            //                    break;
            //                case PaintMode.Wall:
            //                    SwapWall();
            //                    break;
            case PaintMode.TileAndWall:
                if (wall)
                    SwapWall();
                else
                    SwapTile();
                break;
            case PaintMode.Liquid:
                SwapLiquid();
                break;
        }
    }

    public void SwapTile()
    {
        int currentTile = Tile;
        Tile = TileMask;
        TileMask = currentTile;
    }

    public void SwapWall()
    {
        int currentWall = Wall;
        Wall = WallMask;
        WallMask = currentWall;
    }

    public void SwapLiquid()
    {
        switch (_liquidType)
        {
            case LiquidType.Lava: _liquidType = LiquidType.Honey; break;
            case LiquidType.Water: _liquidType = LiquidType.Lava; break;
            default: _liquidType = LiquidType.Water; break;
        }

        this.RaisePropertyChanged(nameof(LiquidType));
    }
}
