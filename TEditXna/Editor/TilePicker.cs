using BCCL.MvvmLight;

namespace TEditXna.Editor
{
    public class TilePicker : ObservableObject 
    {
        private PaintMode _paintMode = PaintMode.Tile;
        private MaskMode _tileMaskMode = MaskMode.Off;
        private MaskMode _wallMaskMode = MaskMode.Off;
        private int _wall;
        private int _tile;
        private bool _isLava;
        private bool _isEraiser; 

        public bool IsEraiser
        {
            get { return _isEraiser; }
            set { Set("IsEraiser", ref _isEraiser, value); }
        } 

        public bool IsLava
        {
            get { return _isLava; }
            set { Set("IsLava", ref _isLava, value); }
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