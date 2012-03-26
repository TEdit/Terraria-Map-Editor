using BCCL.MvvmLight;
using TEditXNA.Terraria;

namespace TEditXna.Editor
{
    public class TilePicker : ObservableObject
    {
        private PaintMode _paintMode = ToolDefaultData.PaintMode;
        private MaskMode _tileMaskMode = ToolDefaultData.PaintTileMaskMode;
        private MaskMode _wallMaskMode = ToolDefaultData.PaintWallMaskMode;
        private int _wall = ToolDefaultData.PaintWall;
        private int _tile = ToolDefaultData.PaintTile;
        private int _wallMask = ToolDefaultData.PaintWallMask;
        private int _tileMask = ToolDefaultData.PaintTileMask;
        private bool _isLava;
        private bool _isEraser;

        public bool IsEraser
        {
            get { return _isEraser; }
            set { Set("IsEraser", ref _isEraser, value); }
        }

        public bool IsLava
        {
            get { return _isLava; }
            set
            {
                Set("IsLava", ref _isLava, value);
                RaisePropertyChanged("IsWater");
            }
        }

        public bool IsWater
        {
            get { return !_isLava; }
            set
            {
                Set("IsLava", ref _isLava, !value);
                RaisePropertyChanged("IsWater");
            }
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
    }
}