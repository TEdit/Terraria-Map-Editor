using System.ComponentModel;
using GalaSoft.MvvmLight;
using TEdit.Terraria;
using System.Windows.Input;

namespace TEdit.Editor
{
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

    public class TilePicker : ObservableObject
    {
        private PaintMode _paintMode = ToolDefaultData.PaintMode;
        private MaskMode _tileMaskMode = ToolDefaultData.PaintTileMaskMode;
        private MaskMode _wallMaskMode = ToolDefaultData.PaintWallMaskMode;
        private int _wall = ToolDefaultData.PaintWall;
        private int _tile = ToolDefaultData.PaintTile;
        private int _wallMask = ToolDefaultData.PaintWallMask;
        private int _tileMask = ToolDefaultData.PaintTileMask;
        private bool _blueWireActive = ToolDefaultData.BlueWire;
        private bool _redWireActive = ToolDefaultData.RedWire;
        private bool _greenWireActive = ToolDefaultData.GreenWire;
        private bool _yellowWireActive = ToolDefaultData.YellowWire;
        private bool _tileStyleActive = ToolDefaultData.PaintTileActive;
        private bool _wallStyleActive = ToolDefaultData.PaintWallActive;
        private TrackMode _trackMode = TrackMode.Track;
        private JunctionBoxMode _junctionboxMode = JunctionBoxMode.None;

        private WireReplaceMode _wireReplaceModeRed = WireReplaceMode.Red;
        private WireReplaceMode _wireReplaceModeBlue = WireReplaceMode.Blue;
        private WireReplaceMode _wireReplaceModeGreen = WireReplaceMode.Green;
        private WireReplaceMode _wireReplaceModeYellow = WireReplaceMode.Yellow;

        private bool _wireReplaceRed = false;
        private bool _wireReplaceBlue = false;
        private bool _wireReplaceGreen = false;
        private bool _wireReplaceYellow = false;

        private BrickStyle _brickStyle = BrickStyle.Full;
        public BrickStyle BrickStyle
        {
            get { return _brickStyle; }
            set { Set(nameof(BrickStyle), ref _brickStyle, value); }
        }

        public TrackMode TrackMode
        {
            get { return _trackMode; }
            set { Set(nameof(TrackMode), ref _trackMode, value); }
        }

        public JunctionBoxMode JunctionBoxMode
        {
            get { return _junctionboxMode; }
            set { Set(nameof(JunctionBoxMode), ref _junctionboxMode, value); }
        }

        public WireReplaceMode WireReplaceModeRed
        {
            get { return _wireReplaceModeRed; }
            set { Set(nameof(WireReplaceModeRed), ref _wireReplaceModeRed, value); }
        }

        public WireReplaceMode WireReplaceModeBlue
        {
            get { return _wireReplaceModeBlue; }
            set { Set(nameof(WireReplaceModeBlue), ref _wireReplaceModeBlue, value); }
        }

        public WireReplaceMode WireReplaceModeGreen
        {
            get { return _wireReplaceModeGreen; }
            set { Set(nameof(WireReplaceModeGreen), ref _wireReplaceModeGreen, value); }
        }

        public WireReplaceMode WireReplaceModeYellow
        {
            get { return _wireReplaceModeYellow; }
            set { Set(nameof(WireReplaceModeYellow), ref _wireReplaceModeYellow, value); }
        }

        public bool WireReplaceRed
        {
            get { return _wireReplaceRed; }
            set { Set(nameof(WireReplaceRed), ref _wireReplaceRed, value); }
        }

        public bool WireReplaceGreen
        {
            get { return _wireReplaceGreen; }
            set { Set(nameof(WireReplaceGreen), ref _wireReplaceGreen, value); }
        }

        public bool WireReplaceBlue
        {
            get { return _wireReplaceBlue; }
            set { Set(nameof(WireReplaceBlue), ref _wireReplaceBlue, value); }
        }

        public bool WireReplaceYellow
        {
            get { return _wireReplaceYellow; }
            set { Set(nameof(WireReplaceYellow), ref _wireReplaceYellow, value); }
        }

        public bool WireReplaceActive => WireReplaceRed || WireReplaceBlue || WireReplaceGreen || WireReplaceYellow;

        //private bool _isLava;
        private bool _isEraser;

        public bool IsEraser
        {
            get { return _isEraser; }
            set
            {
                if (!value && PaintMode == PaintMode.Sprites) { return; } // the only allowed mode for sprite painting is erase
                Set(nameof(IsEraser), ref _isEraser, value);
            }
        }

        private LiquidType _liquidType;
        public LiquidType LiquidType
        {
            get { return _liquidType; }
            set { Set(nameof(LiquidType), ref _liquidType, value); }
        }

        private bool _actuator;
        public bool Actuator
        {
            get { return _actuator; }
            set { Set(nameof(Actuator), ref _actuator, value); }
        }

        private bool _tilepaintActive;
        public bool TilePaintActive
        {
            get { return _tilepaintActive; }
            set { Set(nameof(TilePaintActive), ref _tilepaintActive, value); }
        }

        private bool _wallpaintActive;
        public bool WallPaintActive
        {
            get { return _wallpaintActive; }
            set { Set(nameof(WallPaintActive), ref _wallpaintActive, value); }
        }

        private bool _brickStyleActive;
        public bool BrickStyleActive
        {
            get { return _brickStyleActive; }
            set { Set(nameof(BrickStyleActive), ref _brickStyleActive, value); }
        }

        public bool TileStyleActive
        {
            get { return _tileStyleActive; }
            set { Set(nameof(TileStyleActive), ref _tileStyleActive, value); }
        }

        public bool WallStyleActive
        {
            get { return _wallStyleActive; }
            set { Set(nameof(WallStyleActive), ref _wallStyleActive, value); }
        }

        private bool _extrasActive;
        public bool ExtrasActive
        {
            get { return _extrasActive; }
            set { Set(nameof(ExtrasActive), ref _extrasActive, value); }
        }

        public bool RedWireActive
        {
            get { return _redWireActive; }
            set { Set(nameof(RedWireActive), ref _redWireActive, value); }
        }

        public bool BlueWireActive
        {
            get { return _blueWireActive; }
            set { Set(nameof(BlueWireActive), ref _blueWireActive, value); }
        }

        public bool GreenWireActive
        {
            get { return _greenWireActive; }
            set { Set(nameof(GreenWireActive), ref _greenWireActive, value); }
        }

        public bool YellowWireActive
        {
            get { return _yellowWireActive; }
            set { Set(nameof(YellowWireActive), ref _yellowWireActive, value); }
        }

        private bool _actuatorInActive;
        public bool ActuatorInActive
        {
            get { return _actuatorInActive; }
            set { Set(nameof(ActuatorInActive), ref _actuatorInActive, value); }
        }

        public int TileMask
        {
            get { return _tileMask; }
            set { Set(nameof(TileMask), ref _tileMask, value); }
        }

        public int WallMask
        {
            get { return _wallMask; }
            set { Set(nameof(WallMask), ref _wallMask, value); }
        }

        private int _wallColor;
        public int WallColor
        {
            get { return _wallColor; }
            set { Set(nameof(WallColor), ref _wallColor, value); }
        }

        private int _tileColor;
        public int TileColor
        {
            get { return _tileColor; }
            set { Set(nameof(TileColor), ref _tileColor, value); }
        }

        public int Tile
        {
            get { return _tile; }
            set { Set(nameof(Tile), ref _tile, value); }
        }

        public int Wall
        {
            get { return _wall; }
            set { Set(nameof(Wall), ref _wall, value); }
        }

        public MaskMode WallMaskMode
        {
            get { return _wallMaskMode; }
            set { Set(nameof(WallMaskMode), ref _wallMaskMode, value); }
        }

        public MaskMode TileMaskMode
        {
            get { return _tileMaskMode; }
            set { Set(nameof(TileMaskMode), ref _tileMaskMode, value); }
        }

        public PaintMode PaintMode
        {
            get { return _paintMode; }
            set
            {
                Set(nameof(PaintMode), ref _paintMode, value);
                if (value == PaintMode.Sprites)
                {
                    IsEraser = true;
                }
            }
        }

        public void Swap(ModifierKeys modifier)
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
                    if (modifier.HasFlag(ModifierKeys.Shift))
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

            RaisePropertyChanged("LiquidType");
        }
    }
}
