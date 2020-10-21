using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEdit.Geometry.Primitives;
using GalaSoft.MvvmLight;
using System.Linq;
using TEdit.View;

namespace TEdit.Terraria.Objects
{
    public class TileProperty : ObservableObject, ITile
    {
        private Color _color = Colors.Magenta;
        private int _id = -1;
        private string _name = "UNKNOWN";
        private bool _isFramed;
        private Vector2Short[] _frameSize = new Vector2Short[] { new Vector2Short(1, 1) };
        private Vector2Short _frameGap = new Vector2Short(0, 0);
        private readonly ObservableCollection<FrameProperty> _frames = new ObservableCollection<FrameProperty>();
        private bool _isSolid;
        private bool _saveSlope;
        private bool _isSolidTop;
        private bool _isLight;
        private FramePlacement _placement;
        private Vector2Short _textureGrid;

        private bool _isAnimated;
        private bool _isGrass;
        private bool _isPlatform;
        private bool _isCactus;
        private bool _isStone;
        private bool _canBlend;
        private int? _mergeWith;


        public TileProperty()
        {
        }

        public Vector2Short TextureGrid
        {
            get { return _textureGrid; }
            set { Set(nameof(TextureGrid), ref _textureGrid, value); }
        }

        public Vector2Short FrameGap
        {
            get { return _frameGap; }
            set { Set(nameof(FrameGap), ref _frameGap, value); }
        }
        public FramePlacement Placement
        {
            get { return _placement; }
            set { Set(nameof(Placement), ref _placement, value); }
        }

        public bool IsAnimated
        {
            get { return _isAnimated; }
            set { Set(nameof(IsAnimated), ref _isAnimated, value); }
        }

        public bool IsLight
        {
            get { return _isLight; }
            set { Set(nameof(IsLight), ref _isLight, value); }
        }

        public bool IsOrigin(Vector2Short uv)
        {
            if (uv == Vector2Short.Zero) return true;

            var renderUV = WorldRenderXna.GetRenderUV((ushort)Id, uv.X, uv.Y);
            var frameSizeIx = GetFrameSizeIndex((short)renderUV.Y);
            var frameSize = FrameSize[frameSizeIx];

            if (frameSizeIx == 0)
            {
                return (renderUV.X % ((TextureGrid.X + FrameGap.X) * frameSize.X) == 0 &&
                        renderUV.Y % ((TextureGrid.Y + FrameGap.Y) * frameSize.Y) == 0);
            }
            else
            {
                int y = 0;
                for (int i = 0; i < frameSizeIx; i++)
                {
                    y += FrameSize[i].Y * (TextureGrid.Y + FrameGap.Y);
                }
                return (renderUV.X % ((TextureGrid.X + FrameGap.X) * frameSize.X) == 0 && renderUV.Y == y);
            }
        }


        public bool IsOrigin(Vector2Short uv, out FrameProperty frame)
        {
            frame = Frames.FirstOrDefault(f => f.UV == uv);

            return frame != null;
        }

        public bool IsSolidTop
        {
            get { return _isSolidTop; }
            set { Set(nameof(IsSolidTop), ref _isSolidTop, value); }
        }

        public bool IsSolid
        {
            get { return _isSolid; }
            set { Set(nameof(IsSolid), ref _isSolid, value); }
        }

        public bool SaveSlope
        {
            get { return _saveSlope; }
            set { Set(nameof(SaveSlope), ref _saveSlope, value); }
        }

        public bool HasSlopes => IsSolid || SaveSlope;

        public ObservableCollection<FrameProperty> Frames
        {
            get { return _frames; }
        }

        public TileProperty(int id, string name, Color color, bool isFramed = false)
        {
            _color = color;
            _id = id;
            _name = name;
            _isFramed = isFramed;
        }

        public Color Color
        {
            get { return _color; }
            set { Set(nameof(Color), ref _color, value); }
        }

        public int Id
        {
            get { return _id; }
            set { Set(nameof(Id), ref _id, value); }
        }

        public string Name
        {
            get { return _name; }
            set { Set(nameof(Name), ref _name, value); }
        }

        public Vector2Short[] FrameSize
        {
            get { return _frameSize; }
            set { Set(nameof(FrameSize), ref _frameSize, value); }
        }

        public int GetFrameSizeIndex(short v)
        {
            if (FrameSize.Length > 1)
            {
                int row = v / TextureGrid.Y;
                int rowTest = 0;

                for (int pos = 0; pos < FrameSize.Length; pos++)
                {
                    if (row == rowTest)
                    {
                        return pos;
                    }

                    rowTest += FrameSize[pos].Y;
                }
                return FrameSize.Length - 1;
            }

            return 0;
        }

        public Vector2Short GetFrameSize(short v) => FrameSize[GetFrameSizeIndex(v)];

        public bool IsFramed
        {
            get { return _isFramed; }
            set { Set(nameof(IsFramed), ref _isFramed, value); }
        }

        private WriteableBitmap _image;
        public WriteableBitmap Image
        {
            get { return _image; }
            set { Set(nameof(Image), ref _image, value); }
        }


        public bool IsGrass
        {
            get { return _isGrass; }
            set { Set(nameof(IsGrass), ref _isGrass, value); }
        }


        public bool IsPlatform
        {
            get { return _isPlatform; }
            set { Set(nameof(IsPlatform), ref _isPlatform, value); }
        }


        public bool IsCactus
        {
            get { return _isCactus; }
            set { Set(nameof(IsCactus), ref _isCactus, value); }
        }

        public bool IsStone
        {
            get { return _isStone; }
            set { Set(nameof(IsStone), ref _isStone, value); }
        }


        public bool CanBlend
        {
            get { return _canBlend; }
            set { Set(nameof(CanBlend), ref _canBlend, value); }
        }


        public int? MergeWith
        {
            get { return _mergeWith; }
            set { Set(nameof(MergeWith), ref _mergeWith, value); }
        }

        public bool Merges(int other)
        {
            if (other == this.Id) return true;

            if (!MergeWith.HasValue) return false;

            return MergeWith.Value == other;
        }

        public bool Merges(TileProperty other)
        {
            if (other.MergeWith.HasValue && other.MergeWith.Value == Id) return true;
            if (MergeWith.HasValue && MergeWith.Value == other.Id) return true;
            if (MergeWith.HasValue && other.MergeWith.HasValue && MergeWith.Value == other.MergeWith.Value) return true;

            return false;
        }
    }
}