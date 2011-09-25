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
            set { StandardSet<bool>(ref _isFramed, ref value, "IsFramed"); }
        }

        private bool _canMixFrames;
        public bool CanMixFrames
        {
            get { return _canMixFrames; }
            set { StandardSet<bool>(ref _canMixFrames, ref value, "CanMixFrames"); }
        }

        private readonly ObservableCollection<FrameProperty> _Frames = new ObservableCollection<FrameProperty>();
        public ObservableCollection<FrameProperty> Frames
        {
            get { return _Frames; }
        }

        public override string ToString()
        {
            return String.Format("{0}|{1}|#{2:x2}{3:x2}{4:x2}{5:x2}", ID, this.Name, Color.A, Color.R, Color.G, Color.B);
        }

    }
}