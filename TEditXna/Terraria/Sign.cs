using System;
using GalaSoft.MvvmLight;

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