using System;
using TEdit.Common;
using TEdit.TerrariaWorld.Structures;

namespace TEdit.RenderWorld
{
    [Serializable]
    public class FrameProperty : ObservableObject
    {
        private PointShort _upperLeft;
        public PointShort UpperLeft
        {
            get { return this._upperLeft; }
            set
            {
                if (this._upperLeft != value)
                {
                    this._upperLeft = value;
                    this.RaisePropertyChanged("UpperLeft");
                }
            }
        }

        private PointShort _size;
        public PointShort Size
        {
            get { return this._size; }
            set
            {
                if (this._size != value)
                {
                    this._size = value;
                    this.RaisePropertyChanged("Size");
                }
            }
        }

        private string _name;
        public string Name
        {
            get { return this._name; }
            set
            {
                if (this._name != value)
                {
                    this._name = value;
                    this.RaisePropertyChanged("Name");
                }
            }
        }

        private string _direction;
        public string Direction
        {
            get { return this._direction; }
            set
            {
                if (this._direction != value)
                {
                    this._direction = value;
                    this.RaisePropertyChanged("Direction");
                }
            }
        }

        private string _variety;
        public string Variety
        {
            get { return this._variety; }
            set
            {
                if (this._variety != value)
                {
                    this._variety = value;
                    this.RaisePropertyChanged("Variety");
                }
            }
        }



    }
}