using System;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using TEditXNA.Terraria.Objects;
using TEditXna.ViewModel;

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
        private int _chestId = -1;

        public int ChestId
        {
            get
            {
                WorldViewModel wvm = ViewModelLocator.WorldViewModel;
                World world = wvm.CurrentWorld;
                var uvX = world.Tiles[X, Y].U;
                var uvY = world.Tiles[X, Y].V;
                var type = world.Tiles[X, Y].Type;
                foreach (ChestProperty prop in World.ChestProperties)
                {
                    if (prop.TileType == type && prop.UV.X == uvX && prop.UV.Y == uvY)
                    {
                        _chestId = prop.ChestId;
                        break;
                    }
                }
                return _chestId;
            }
            set
            {
                foreach (ChestProperty prop in World.ChestProperties)
                {
                    if (prop.ChestId == value)
                    {
                        WorldViewModel wvm = ViewModelLocator.WorldViewModel;
                        World world = wvm.CurrentWorld;
                        int rowNum = 2, colNum = 2;
                        // Chests are 2 * 2, dressers are 2 * 3.
                        if (prop.TileType == 88)
                        {
                            colNum = 3;
                        }
                        for (int i = 0; i < colNum; ++i)
                        {
                            for (int j = 0; j < rowNum; ++j)
                            {
                                world.Tiles[X + i, Y + j].U = (short)(prop.UV.X + 18 * i);
                                world.Tiles[X + i, Y + j].V = (short)(prop.UV.Y + 18 * j);
                                world.Tiles[X + i, Y + j].Type = prop.TileType;
                            }
                        }
                        Set("ChestId", ref _chestId, value);
                        break;
                    }
                }
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
            for (int i = 0; i < MaxItems; i++)
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
            return $"[Chest: ({X},{Y})]";
        }
    }
}
