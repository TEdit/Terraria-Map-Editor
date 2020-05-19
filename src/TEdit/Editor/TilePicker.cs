using System.ComponentModel;
using GalaSoft.MvvmLight;
using TEditXNA.Terraria;
using System.Windows.Input;

namespace TEditXna.Editor
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

        private BrickStyle _brickStyle = BrickStyle.Full;
        public BrickStyle BrickStyle
        {
            get { return _brickStyle; }
            set { Set("BrickStyle", ref _brickStyle, value); }
        }

        public TrackMode TrackMode
        {
            get { return _trackMode; }
            set { Set("TrackMode", ref _trackMode, value); }
        }
        
        //private bool _isLava;
        private bool _isEraser;

        public bool IsEraser
        {
            get { return _isEraser; }
            set { Set("IsEraser", ref _isEraser, value); }
        }

        private LiquidType _liquidType;
        public LiquidType LiquidType
        {
            get { return _liquidType; }
            set { Set("LiquidType", ref _liquidType, value);}
        }

        private bool _actuator;
        public bool Actuator
        {
            get { return _actuator; }
            set { Set("Actuator", ref _actuator, value); }
        }

        private bool _tilepaintActive;
        public bool TilePaintActive
        {
            get { return _tilepaintActive; }
            set { Set("TilePaintActive", ref _tilepaintActive, value); }
        }

        private bool _wallpaintActive;
        public bool WallPaintActive
        {
            get { return _wallpaintActive; }
            set { Set("WallPaintActive", ref _wallpaintActive, value); }
        }


        private bool _brickStyleActive;
        public bool BrickStyleActive
        {
            get { return _brickStyleActive; }
            set { Set("BrickStyleActive", ref _brickStyleActive, value); }
        }

        public bool TileStyleActive
        {
            get { return _tileStyleActive; }
            set { Set("TileStyleActive", ref _tileStyleActive, value); }
        }

        public bool WallStyleActive
        {
            get { return _wallStyleActive; }
            set { Set("WallStyleActive", ref _wallStyleActive, value); }
        }

        private bool _extrasActive;
        public bool ExtrasActive
        {
            get { return _extrasActive; }
            set { Set("ExtrasActive", ref _extrasActive, value); }
        }

        public bool RedWireActive
        {
            get { return _redWireActive; }
            set { Set("RedWireActive", ref _redWireActive, value); }
        }

        public bool BlueWireActive
        {
            get { return _blueWireActive; }
            set { Set("BlueWireActive", ref _blueWireActive, value); }
        }

        public bool GreenWireActive
        {
            get { return _greenWireActive; }
            set { Set("GreenWireActive", ref _greenWireActive, value); }
        }

        public bool YellowWireActive
        {
            get { return _yellowWireActive; }
            set { Set("YellowWireActive", ref _yellowWireActive, value); }
        }

        private bool _actuatorInActive;
        public bool ActuatorInActive
        {
            get { return _actuatorInActive; }
            set { Set("ActuatorInActive", ref _actuatorInActive, value); }
        }   

        public int TileMask
        {
            get { return _tileMask; }
            set { Set("TileMask", ref _tileMask, value); }
        }

        public int WallMask
        {
            get { return _wallMask; }
            set { Set("WallMask", ref _wallMask, value); }
        }

        private int _wallColor;
        public int WallColor
        {
            get { return _wallColor; }
            set { Set("WallColor", ref _wallColor, value); }
        }

        private int _tileColor;
        public int TileColor
        {
            get { return _tileColor; }
            set { Set("TileColor", ref _tileColor, value); }
        }

        public int Tile
        {
            get { return _tile; }
            set { Set("Tile", ref _tile, value); }
        }

        public int Wall
        {
            get { return _wall; }
            set { Set("Wall", ref _wall, value); }
        }

        public MaskMode WallMaskMode
        {
            get { return _wallMaskMode; }
            set { Set("WallMaskMode", ref _wallMaskMode, value); }
        }

        public MaskMode TileMaskMode
        {
            get { return _tileMaskMode; }
            set { Set("TileMaskMode", ref _tileMaskMode, value); }
        }

        public PaintMode PaintMode
        {
            get { return _paintMode; }
            set { Set("PaintMode", ref _paintMode, value); }
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