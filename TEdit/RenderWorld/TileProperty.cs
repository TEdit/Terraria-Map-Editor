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