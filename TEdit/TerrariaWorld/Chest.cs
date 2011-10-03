using System;
using System.Collections.ObjectModel;
using TEdit.Common;
using TEdit.Common.Structures;

namespace TEdit.TerrariaWorld
{
    [Serializable]
    public class Chest : ObservableObject
    {
        public Chest()
        {
            _Items = new ObservableCollection<Item>();
        }
        public Chest(PointInt32 location) : this()
        {
            _Location = location;
            for (int i = 0; i < MaxItems; i++)
            {
                _Items.Add(new Item(0,"[empty]"));
            }
        }

        public static int MaxItems = 20;
        private readonly ObservableCollection<Item> _Items;

        private PointInt32 _Location;
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