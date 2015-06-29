using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight;
using TEditXNA.Terraria.Objects;
using TEditXna.ViewModel;
using TEdit.Geometry.Primitives;

namespace TEditXNA.Terraria
{
    [Serializable]
    public class Chest : ObservableObject
    {
        public static int MaxItems = 40; 

        public Chest()
        {
            for (int i = 0; i < MaxItems; i++)
            {
                _items.Add(new Item());
            }
        }
        public Chest(int x, int y)
            : this()
        {
            _x = x;
            _y = y;

        }

        public Chest(int x, int y, string name)
            : this()
        {
            _x = x;
            _y = y;
            _name = name;
        }


        private int _x;
        private int _y;

        private string _name = string.Empty;
        private Vector2Short _uV = new Vector2Short(-1, -1);

        public Vector2Short UV
        {
            get 
            {
                if (_uV.X == -1 && _uV.Y == -1)
                {
                    WorldViewModel wvm = ViewModelLocator.WorldViewModel;
                    World world = wvm.CurrentWorld;
                    _uV.X = world.Tiles[X, Y].U;
                    _uV.Y = world.Tiles[X, Y].V;
                }
                return _uV; 
            }
            set
            {
                WorldViewModel wvm = ViewModelLocator.WorldViewModel;
                World world = wvm.CurrentWorld;
                for (int i = 0; i < 2; ++i)
                {
                    for (int j = 0; j < 2; ++j)
                    {
                        world.Tiles[X + i, Y + j].U = (short)(value.X + 18 * i);
                        world.Tiles[X + i, Y + j].V = (short)(value.Y + 18 * j);
                    }
                }
                Set("UV", ref _uV, value); 
            }
        } 

        public string Name
        {
            get { return _name; }
            set { Set("Name", ref _name, value); }
        }
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
            chest.Name = Name;
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
