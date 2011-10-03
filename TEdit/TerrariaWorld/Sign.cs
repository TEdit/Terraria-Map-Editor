using System;
using TEdit.Common;
using TEdit.Common.Structures;

namespace TEdit.TerrariaWorld
{
    [Serializable]
    public class Sign : ObservableObject
    {
        private PointInt32 _Location;
        private string _Text;

        public Sign()
        {
        }

        public Sign(string text, PointInt32 location)
        {
            _Text = text;
            _Location = location;
        }

        public string Text
        {
            get { return _Text; }
            set
            {
                if (_Text != value)
                {
                    _Text = value;
                    RaisePropertyChanged("Text");
                }
            }
        }

        public PointInt32 Location
        {
            get { return _Location; }
            set
            {
                if (_Location != value)
                {
                    _Location = value;
                    RaisePropertyChanged("Location");
                }
            }
        }

        public override string ToString()
        {
            return String.Format("[Sign: {0}, {1}]", Text, Location);
        }
    }
}