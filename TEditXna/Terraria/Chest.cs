using System;
using System.Collections.ObjectModel;
using BCCL.Geometry;
using BCCL.MvvmLight;

namespace TEditXNA.Terraria
{
    [Serializable]
    public class Chest : ObservableObject
    {
        public static int MaxItems = 40;
        public Chest()
        {

        }
        public Chest(int x, int y)
            : this()
        {
            _x = x;
            _y = y;

            for (int i = 0; i < MaxItems; i++)
            {
                _items.Add(new Item());
            }
        }

        private int _x;
        private int _y;


        public int Y
        {
            get { return _y; }
            set { Set("Y", ref _y, value); }
        }

        public int X
        {
            get { return _x; }
            set { Set("X", ref _x, value); }
        }


        private readonly ObservableCollection<Item> _items = new ObservableCollection<Item>();
        public ObservableCollection<Item> Items
        {
            get { return _items; }
        }

        public Chest Copy()
        {
            var chest = new Chest(_x, _y);
            //chest.Items.Clear();
            for (int i = 0; i < Chest.MaxItems; i++)
            {
                if (Items.Count > i)
                    chest.Items[i] = Items[i].Copy();
                else
                {
                    chest.Items[i] = new Item();
                }
            }

            return chest;
        }

        public override string ToString()
        {
            return String.Format("[Chest: ({0},{1})]", X, Y);
        }
    }
}