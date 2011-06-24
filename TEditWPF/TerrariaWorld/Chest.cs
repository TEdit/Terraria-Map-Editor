using System;
using System.Collections.ObjectModel;
using TEditWPF.Common;
using TileMouseEventArgs = TEditWPF.TerrariaWorld.Structures.PointInt32;

namespace TEditWPF.TerrariaWorld
{
    public class Chest : ObservableObject
    {
        public static int MaxItems = 20;
        private readonly ObservableCollection<Item> _Items = new ObservableCollection<Item>();

        private TileMouseEventArgs _Location;

        public TileMouseEventArgs Location
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

        public ObservableCollection<Item> Items
        {
            get { return _Items; }
        }


        public override string ToString()
        {
            return String.Format("[Chest: {0}]", Location);
        }
    }
}