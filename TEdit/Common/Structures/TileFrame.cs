using System;
using TEdit.Common;

namespace TEdit.Common.Structures
{
    [Serializable]
    public struct TileFrame
    {
        private byte _tile;
        private byte? _frame;

        public TileFrame(TileFrame tf)    { _tile = tf.Tile; _frame = tf.Frame; }
        public TileFrame(byte t)          { _tile = t;       _frame = null;     }
        public TileFrame(byte t, byte f)  { _tile = t;       _frame = f;        }
        public TileFrame(byte t, byte? f) { _tile = t;       _frame = f;        }
        public TileFrame(string tf)
        {
            TileFrame TF;
            TryParse(tf, out TF);
            _tile  = TF.Tile;
            _frame = TF.Frame;
        }

        public byte Tile
        {
            get { return _tile; }
            set { _tile = value; }
        }

        public byte? Frame
        {
            get { return _frame; }
            set { _frame = value; }
        }

        public override string ToString()
        {
            return String.Format("{0}.{1}", Tile, Frame);
        }

        public static bool TryParse(string tf, out TileFrame newTF)
        {
            byte  tile = 0;
            byte? frame = null;

            if (string.IsNullOrWhiteSpace(tf))
            {
                newTF = new TileFrame(tile);
                return false;
            }
            
            string[] tileFrame = tf.Split('.');
            if      (tf.Length == 2)
            {
                tile  = byte.Parse(tileFrame[0]);
                frame = byte.Parse(tileFrame[1]);
                newTF = new TileFrame(tile, frame);
                return true;
            }
            else if (tf.Length == 1)
            {
                tile = byte.Parse(tileFrame[0]);
                newTF = new TileFrame(tile);
                return true;
            }

            newTF = new TileFrame(tile);
            return false;
        }

        public static TileFrame TryParseInline(string tf)
        {
            TileFrame result;
            TryParse(tf, out result);
            return result;
        }

        public static TileFrame Parse(string tf)
        {
            byte  tile = 0;
            byte? frame = null;

            if (string.IsNullOrWhiteSpace(tf))
            {
                throw new NullReferenceException("tile/frame cannot be null");
            }

            string[] tileFrame = tf.Split('.');
            if (tf.Length == 2)
            {
                tile = byte.Parse(tileFrame[0]);
                frame = byte.Parse(tileFrame[1]);
                return new TileFrame(tile, frame);
            }
            else if (tf.Length == 1)
            {
                tile = byte.Parse(tileFrame[0]);
                return new TileFrame(tile);
            }

            throw new ArgumentOutOfRangeException("tile", "Invalid tile/frame structure, must be in the form of t.f or t");
        }

        #region Operator Overrides

        private static bool MatchFields(TileFrame a, TileFrame m)
        {
            return (a.Tile == m.Tile && a.Frame == m.Frame);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is byte)
                return this.Tile == (byte)obj;

            if (obj is TileFrame)
                return MatchFields(this, (TileFrame) obj);

            return false;
        }

        public bool Equals(TileFrame tf)
        {
            return MatchFields(this, tf);
        }

        public static bool operator ==(TileFrame a, TileFrame b)
        {
            return MatchFields(a, b);
        }

        public static bool operator !=(TileFrame a, TileFrame b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            int result = 17;
            result = result*37 + Tile.GetHashCode();
            result = result*37 + Frame.GetHashCode();
            return result;
        }

        // this loses information, but maintains compatibility with existing code
        // (will refactor and convert to explict later on...)
        public static implicit operator byte(TileFrame tf)
        {
            return tf.Tile;
        }

        #endregion
    }
}