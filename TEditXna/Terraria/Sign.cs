using System;
using GalaSoft.MvvmLight;
using TEdit.Geometry.Primitives;
using TEditXna.ViewModel;

namespace TEditXNA.Terraria
{
    [Serializable]
    public class Sign : ObservableObject
    {
        public Sign()
        {
            _text = string.Empty;
        }

        public Sign(int x, int y, string text)
        {
            _text = text;
            _x = x;
            _y = y;
        }

        private string _name = string.Empty;
        private Vector2Short _uV = new Vector2Short(-1, -1);
        private ushort _tileType = 55;

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
                    _tileType = world.Tiles[X, Y].Type;
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

        private string _text;
        public string Text
        {
            get { return _text; }
            set { Set("Text", ref _text, value); }
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


        public override string ToString()
        {
            return String.Format("[Sign: {0}[{1}], ({2},{3})]", Text.Substring(0, Math.Max(25, Text.Length)), Text.Length, X, Y);
        }


        public Sign Copy()
        {
            return new Sign(_x, _y, _text);
        }
    }
}