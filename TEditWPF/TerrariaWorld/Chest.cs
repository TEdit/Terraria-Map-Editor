using System;
using System.Collections.ObjectModel;
using TEditWPF.Common;
using TEditWPF.TerrariaWorld.Structures;

namespace TEditWPF.TerrariaWorld
{
    public class Chest : ObservableObject
    {
        public static int MaxItems = 20;

        public Chest()
        {

        }

        private PointInt32 _Location;
        public PointInt32 Location
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

        private ObservableCollection<Item> _Items = new ObservableCollection<Item>();
        public ObservableCollection<Item> Items
        {
            get { return _Items; }
        }

        
        public override string ToString()
        {
            return String.Format("[Chest: {0}]", this.Location);
        }
    }
}
