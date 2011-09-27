using System;
using System.Windows.Media;
using TEdit.TerrariaWorld.Structures;
using System.Collections.ObjectModel;

namespace TEdit.RenderWorld
{
    [Serializable]
    public class TileProperty : TileFrameProperty
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
            IsSolid = false;
            IsSolidTop = false;
            Size = new PointShort(1, 1);
            Direction = null;
            Variety = "";
        }

        private bool _isFramed;
        public bool IsFramed
        {
            get { return _isFramed; }
            set { StandardSet(ref _isFramed, ref value, "IsFramed"); }
        }

        private bool _canMixFrames;
        public bool CanMixFrames
        {
            get { return _canMixFrames; }
            set { StandardSet(ref _canMixFrames, ref value, "CanMixFrames"); }
        }

        private readonly ObservableCollection<FrameProperty> _Frames = new ObservableCollection<FrameProperty>();
        public ObservableCollection<FrameProperty> Frames
        {
            get { return _Frames; }
        }

    }
}