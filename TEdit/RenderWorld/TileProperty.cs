using System;
using System.Collections.ObjectModel;

namespace TEdit.RenderWorld
{
    [Serializable]
    public class TileProperty : ColorProperty
    {
        private bool _isFramed;
        public bool IsFramed
        {
            get { return _isFramed; }
            set
            {
                if (_isFramed != value)
                {
                    _isFramed = value;
                    RaisePropertyChanged("IsFramed");
                }
            }
        }

        private bool _isSolid;
        public bool IsSolid
        {
            get { return _isSolid; }
            set
            {
                if (_isSolid != value)
                {
                    _isSolid = value;
                    RaisePropertyChanged("IsSolid");
                }
            }
        }

        private bool _IsSolidTop;
        public bool IsSolidTop
        {
            get { return this._IsSolidTop; }
            set
            {
                if (this._IsSolidTop != value)
                {
                    this._IsSolidTop = value;
                    this.RaisePropertyChanged("IsSolidTop");
                }
            }
        }

        private readonly ObservableCollection<FrameProperty> _Frames = new ObservableCollection<FrameProperty>();
        public ObservableCollection<FrameProperty> Frames
        {
            get { return _Frames; }
        }
    }
}