using System;
using System.Linq;

namespace TEdit.TerrariaWorld.Structures
{
    [Serializable]
    public struct TileNumArray
    {
        private byte[] _tna;

        public TileNumArray(byte[] tna) { _tna = tna; }
        public byte[] Arr
        {
            get { return _tna; }
            set { _tna = value; }
        }

        public override string ToString()
        {
            return String.Join(", ", _tna);
        }

        public static bool TryParse(string tna, out TileNumArray tileNumArray)
        {
            byte[] newTNA = {};
            
            if (string.IsNullOrWhiteSpace(tna))
            {
                tileNumArray = new TileNumArray(newTNA);
                return false;
            }

            string[] split = tna.Split(',');
            try {
                newTNA = Array.ConvertAll(split, new Converter<string, byte>(byte.Parse));
            }
            catch {
                tileNumArray = new TileNumArray(new byte[] {});
                return false;
            }

            tileNumArray = new TileNumArray(newTNA);
            return true;
        }

        public static TileNumArray Parse(string tna)
        {
            TileNumArray result;
            TryParse(tna, out result);
            return result;
        }

        #region Operator Overrides

        private static bool MatchFields(TileNumArray a, TileNumArray m)
        {
            return Enumerable.SequenceEqual(a.Arr, m.Arr);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is TileNumArray)
                return MatchFields(this, (TileNumArray)obj);

            return false;
        }

        public bool Equals(TileNumArray p)
        {
            return MatchFields(this, p);
        }

        public static bool operator ==(TileNumArray a, TileNumArray b)
        {
            return MatchFields(a, b);
        }

        public static bool operator !=(TileNumArray a, TileNumArray b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return _tna.GetHashCode();
        }

        #endregion
    }
}