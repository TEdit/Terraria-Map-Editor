using System;
using GalaSoft.MvvmLight;
using TEditXna.ViewModel;
using TEditXNA.Terraria.Objects;

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
        private int _signId = -1;

        public int SignId
        {
            get
            {
                if (_signId == -1)
                {
                    WorldViewModel wvm = ViewModelLocator.WorldViewModel;
                    World world = wvm.CurrentWorld;
                    var uvX = world.Tiles[X, Y].U;
                    var uvY = world.Tiles[X, Y].V;
                    var type = world.Tiles[X, Y].Type;
                    foreach (SignProperty prop in World.SignProperties)
                    {
                        if (prop.TileType == type && prop.UV.X == uvX && prop.UV.Y == uvY)
                        {
                            _signId = prop.SignId;
                            break;
                        }
                    }
                }
                return _signId;
            }
            set
            {
                foreach (SignProperty prop in World.SignProperties)
                {
                    if (prop.SignId == value)
                    {
                        WorldViewModel wvm = ViewModelLocator.WorldViewModel;
                        World world = wvm.CurrentWorld;
                        for (int i = 0; i < 2; ++i)
                        {
                            for (int j = 0; j < 2; ++j)
                            {
                                world.Tiles[X + i, Y + j].U = (short)(prop.UV.X + 18 * i);
                                world.Tiles[X + i, Y + j].V = (short)(prop.UV.Y + 18 * j);
                                world.Tiles[X + i, Y + j].Type = prop.TileType;
                            }
                        }
                        Set("SignId", ref _signId, value);
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
            return $"[Sign: {Text.Substring(0, Math.Max(25, Text.Length))}[{Text.Length}], ({X},{Y})]";
        }


        public Sign Copy()
        {
            return new Sign(_x, _y, _text);
        }
    }
}