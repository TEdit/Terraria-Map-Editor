using System;
using System.Windows.Media;
using System.Collections.ObjectModel;
using TEdit.Common.Structures;

namespace TEdit.RenderWorld
{
    [Serializable]
    public class TileProperty : FrameProperty
    {

        public TileProperty()
        {
        }

        public TileProperty(byte id)
        {
            // ID + Default values
            ID = id;
            Name = "UNKNOWN";
            Color = Colors.Magenta;
            Size = new PointShort(1, 1);
        }

        private bool _isFramed;
        public bool IsFramed
        {
            get { return _isFramed; }
            set { SetProperty(ref _isFramed, ref value, "IsFramed"); }
        }

        private bool _blocksLight;
        public bool BlocksLight
        {
            get { return _blocksLight; }
            set { SetProperty(ref _blocksLight, ref value, "BlocksLight"); }
        }

        private bool _canBeCut;
        public bool CanBeCut
        {
            get { return _canBeCut; }
            set { SetProperty(ref _canBeCut, ref value, "CanBeCut"); }
        }

        private LiquidType _destroyedBy;
        public LiquidType DestroyedBy
        {
            get { return _destroyedBy; }
            set { SetProperty(ref _destroyedBy, ref value, "DestroyedBy"); }
        }

        private bool _oneHit;
        public bool OneHit
        {
            get { return _oneHit; }
            set { SetProperty(ref _oneHit, ref value, "OneHit"); }
        }

        private float _sparkle;
        public float Sparkle
        {
            get { return _sparkle; }
            set { SetProperty(ref _sparkle, ref value, "Sparkle"); }
        }

        private bool _canMixFrames;
        public bool CanMixFrames
        {
            get { return _canMixFrames; }
            set { SetProperty(ref _canMixFrames, ref value, "CanMixFrames"); }
        }

        private readonly ObservableCollection<FrameProperty> _frames = new ObservableCollection<FrameProperty>();
        public ObservableCollection<FrameProperty> Frames
        {
            get { return _frames; }
        }
    }
}