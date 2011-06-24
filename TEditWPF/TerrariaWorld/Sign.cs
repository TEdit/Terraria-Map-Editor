using TEditWPF.TerrariaWorld.Structures;

namespace TEditWPF.TerrariaWorld
{
    using System;
    using TEditWPF.Common;

    public class Sign : ObservableObject
    {
        public Sign()
        {
          
        }

        public Sign(string text, Structures.PointInt32 location)
        {
            this._Text = text;
            this._Location = location;
        }

        private string _Text;
        public string Text
        {
            get { return this._Text; }
            set
            {
                if (this._Text != value)
                {
                    this._Text = value;
                    this.RaisePropertyChanged("Text");
                }
            }
        }

        private Structures.PointInt32 _Location;
        public Structures.PointInt32 Location
        {
            get { return this._Location; }
            set
            {
                if (this._Location != value)
                {
                    this._Location = value;
                    this.RaisePropertyChanged("Location");
                }
            }
        }

        public override string ToString()
        {
            return String.Format("[Sign: {0}, {1}]", this.Text, this.Location);
        }
    }
}
