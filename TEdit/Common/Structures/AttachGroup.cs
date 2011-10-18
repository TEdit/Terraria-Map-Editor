using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using TEdit.RenderWorld;
using System.Linq;
using System.Text.RegularExpressions;

namespace TEdit.Common.Structures
{
    [Serializable]
    public class AttachGroup : ObservableObject
    {
        private string _name;
        private FramePlacement? _placement = null;
        private readonly ObservableCollection<TileFrame> _attachesTo = new ObservableCollection<TileFrame>();

        public AttachGroup()
        {
            _name = string.Empty;
        }

        public AttachGroup(string n, string s)
        {
            _name = n;
            AttachGroup result = new AttachGroup();
            TryParse(s, ref result);
            _placement = result.Placement;
            _attachesTo = result.AttachesTo;
        }

        public AttachGroup(string n, string p, string s)
        {
            _name = n;
            _placement = (new FramePlacement()).Convert<FramePlacement>(p);
            TryParse(s, ref _attachesTo);
        }

        public AttachGroup(string n, FramePlacement? p, string s)
        {
            _name = n;
            _placement = p;
            TryParse(s, ref _attachesTo);
        }

        public AttachGroup(string n, FramePlacement? p, ObservableCollection<TileFrame> s)
        {
            _name = n;
            _placement = p;
            _attachesTo.ReplaceRange(s);
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public FramePlacement? Placement
        {
            get { return _placement; }
            set { _placement = value; }
        }

        public ObservableCollection<TileFrame> AttachesTo
        {
            get { return _attachesTo; }
            set { _attachesTo.ReplaceRange(value); }
        }

        public override string ToString()
        {
            return String.Format("{0} ({1}) = {2}", Name, Placement, String.Join<TileFrame>(",", AttachesTo));
        }

        public static bool TryParse(string aStr, ref AttachGroup attach)
        {
            string raw = Regex.Replace(aStr, @"\s", String.Empty);
            
            var newAttach       = new AttachGroup();
            newAttach.Name      = attach.Name ?? String.Empty;
            newAttach.Placement = attach.Placement;

            if (string.IsNullOrWhiteSpace(raw))
            {
                attach = newAttach;
                return false;
            }

            // first split - colons
            string[] ptf = raw.Split(':');
            if (ptf.Length == 2)
            {
                newAttach.Placement = (new FramePlacement()).Convert<FramePlacement>(ptf[0]);
                raw = ptf[1];
            }
            else if (ptf.Length > 2)
            {
                attach = newAttach;
                return false;
            }

            // second split - commas
            string[] tiles = raw.Split(',');
            foreach (string rawTF in tiles)
            {
                byte t;
                byte? f;
                // third split - periods (not using TileFrame for this, as there may be multiples)
                string[] tf = rawTF.Split('.');
                
                if     (tf.Length == 2)
                {
                    t = byte.Parse(tf[0]);
                    // fourth split - dashes
                    string[] fRng = tf[1].Split('-');

                    if (fRng.Length == 2)
                    {
                        int s = int.Parse(fRng[0]);
                        int c = int.Parse(fRng[1]) - s + 1;

                        var rng = Enumerable.Range(s, c);
                        foreach (int frm in rng) { newAttach.AttachesTo.Add(new TileFrame(t, (byte)frm)); }
                    }
                    else if (fRng.Length > 2)
                    {
                        attach = newAttach;
                        return false;
                    }
                    else
                    {
                        f = byte.Parse(tf[1]);
                        newAttach.AttachesTo.Add(new TileFrame(t, f));
                    }
                }
                else if (tf.Length == 1)
                {
                    // fourth split - dashes
                    string[] tRng = rawTF.Split('-');

                    if (tRng.Length == 2)
                    {
                        int s = int.Parse(tRng[0]);
                        int c = int.Parse(tRng[1]) - s + 1;

                        var rng = Enumerable.Range(s, c);
                        foreach (int tile in rng) { newAttach.AttachesTo.Add(new TileFrame((byte)tile)); }
                    }
                    else if (tRng.Length > 2)
                    {
                        attach = newAttach;
                        return false;
                    }
                    else
                    {
                        t = byte.Parse(rawTF);
                        newAttach.AttachesTo.Add(new TileFrame(t));
                    }
                }
            }

            attach = newAttach;
            return true;
        }

        public static bool TryParse(string aStr, ref ObservableCollection<TileFrame> attach)
        {
            AttachGroup result = new AttachGroup();
            bool valid = TryParse(aStr, ref result);
            attach = result.AttachesTo;
            return valid;
        }

        public static AttachGroup TryParseInline(string aStr)
        {
            AttachGroup result = new AttachGroup();
            TryParse(aStr, ref result);
            return result;
        }

        #region Operator Overrides

        private static bool MatchFields(AttachGroup a, AttachGroup m)
        {
            return (a.AttachesTo == m.AttachesTo && a.Name == m.Name && a.Placement == m.Placement);
        }

        private static bool MatchFields(AttachGroup a, ObservableCollection<TileFrame> m)
        {
            return (a.AttachesTo == m);
        }

        private static bool MatchFields(AttachGroup a, FramePlacement? m)
        {
            return (a.Placement == m);
        }

        private static bool MatchFields(AttachGroup a, FramePlacement m)
        {
            return (a.Placement == m);
        }

        private static bool MatchFields(AttachGroup a, TileFrame m)
        {
            return (a.AttachesTo.Count == 1 && a.AttachesTo.Contains(m));
        }

        private static bool MatchFields(AttachGroup a, string m)
        {
            return (a.Name == m);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is string)
                return MatchFields(this, (string)obj);

            if (obj is ObservableCollection<TileFrame>)
                return MatchFields(this, (ObservableCollection<TileFrame>)obj);

            if (obj is FramePlacement?)
                return MatchFields(this, (FramePlacement?)obj);

            if (obj is FramePlacement)
                return MatchFields(this, (FramePlacement)obj);

            if (obj is AttachGroup)
                return MatchFields(this, (AttachGroup)obj);

            return false;
        }

        public bool Equals(AttachGroup ag)
        {
            return MatchFields(this, ag);
        }

        public static bool operator ==(AttachGroup a, AttachGroup b)                     { return MatchFields(a, b); }
        public static bool operator ==(AttachGroup a, ObservableCollection<TileFrame> b) { return MatchFields(a, b); }
        public static bool operator ==(ObservableCollection<TileFrame> a, AttachGroup b) { return MatchFields(b, a); }
        public static bool operator ==(AttachGroup a, FramePlacement? b)                 { return MatchFields(a, b); }
        public static bool operator ==(AttachGroup a, FramePlacement b)                  { return MatchFields(a, b); }
        public static bool operator ==(FramePlacement? a, AttachGroup b)                 { return MatchFields(b, a); }
        public static bool operator ==(FramePlacement a, AttachGroup b)                  { return MatchFields(b, a); }
        public static bool operator ==(AttachGroup a, TileFrame b)                       { return MatchFields(a, b); }
        public static bool operator ==(TileFrame a, AttachGroup b)                       { return MatchFields(b, a); }
        public static bool operator ==(AttachGroup a, string b)                          { return MatchFields(a, b); }
        public static bool operator ==(string a, AttachGroup b)                          { return MatchFields(b, a); }

        public static bool operator !=(AttachGroup a, AttachGroup b)                     { return !(a == b); }
        public static bool operator !=(AttachGroup a, ObservableCollection<TileFrame> b) { return !(a == b); }
        public static bool operator !=(ObservableCollection<TileFrame> a, AttachGroup b) { return !(a == b); }
        public static bool operator !=(AttachGroup a, FramePlacement? b)                 { return !(a == b); }
        public static bool operator !=(AttachGroup a, FramePlacement b)                  { return !(a == b); }
        public static bool operator !=(FramePlacement? a, AttachGroup b)                 { return !(a == b); }
        public static bool operator !=(FramePlacement a, AttachGroup b)                  { return !(a == b); }
        public static bool operator !=(AttachGroup a, TileFrame b)                       { return !(a == b); }
        public static bool operator !=(TileFrame a, AttachGroup b)                       { return !(a == b); }
        public static bool operator !=(AttachGroup a, string b)                          { return !(a == b); }
        public static bool operator !=(string a, AttachGroup b)                          { return !(a == b); }

        public static AttachGroup operator +(AttachGroup a, AttachGroup b)
        {
            var c = new AttachGroup(a.Name, a.Placement, a.AttachesTo);
            c.AttachesTo.AddRange(b.AttachesTo);
            c.AttachesTo = (ObservableCollection<TileFrame>)c.AttachesTo.Distinct();
            return c;
        }

        public static AttachGroup operator +(AttachGroup a, ObservableCollection<TileFrame> b)
        {
            var c = new AttachGroup(a.Name, a.Placement, a.AttachesTo);
            c.AttachesTo.AddRange(b);
            c.AttachesTo = (ObservableCollection<TileFrame>)c.AttachesTo.Distinct();
            return c;
        }

        public static AttachGroup operator +(AttachGroup a, TileFrame b)
        {
            if (a.AttachesTo.Contains(b)) return a;
            var c = new AttachGroup(a.Name, a.Placement, a.AttachesTo);
            c.AttachesTo.Add(b);
            return c;
        }
        public static AttachGroup operator +(ObservableCollection<TileFrame> a, AttachGroup b) { return b + a; }
        public static AttachGroup operator +(TileFrame a, AttachGroup b)                       { return b + a; }

        public static AttachGroup operator -(AttachGroup a, AttachGroup b)
        {
            var c = new AttachGroup(a.Name, a.Placement, a.AttachesTo);
            c.AttachesTo.RemoveRange(b.AttachesTo);
            return c;
        }

        public static AttachGroup operator -(AttachGroup a, ObservableCollection<TileFrame> b)
        {
            var c = new AttachGroup(a.Name, a.Placement, a.AttachesTo);
            c.AttachesTo.RemoveRange(b);
            return c;
        }

        public static AttachGroup operator -(AttachGroup a, TileFrame b)
        {
            var c = new AttachGroup(a.Name, a.Placement, a.AttachesTo);
            c.AttachesTo.Remove(b);
            return c;
        }
        public static AttachGroup operator -(ObservableCollection<TileFrame> a, AttachGroup b) { return b - a; }
        public static AttachGroup operator -(TileFrame a, AttachGroup b)                       { return b - a; }

        public override int GetHashCode()
        {
            int result = 13;
            result = result * 7 + Name.GetHashCode();
            result = result * 7 + Placement.GetHashCode();
            result = result * 7 + AttachesTo.GetHashCode();
            return result;
        }

        #endregion
    }
}