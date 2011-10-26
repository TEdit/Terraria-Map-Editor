using System;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using TEdit.Common.Structures;
using TEdit.Common;
using System.Text.RegularExpressions;

namespace TEdit.RenderWorld
{
    // Useful functions for porting XML properties to C# properties
    [Serializable]
    public class XMLBase : NamedBase
    {
        private static readonly ObservableCollection<AttachGroup> _attachGroupsCache = new ObservableCollection<AttachGroup>();
        private static bool _hasReadCache = false;
        
        // XML conversions with defaults
        public bool       XMLConvert(bool       v, XAttribute attr) { return (bool?) attr ?? v;  }
        public string     XMLConvert(string     v, XAttribute attr) { return (string)attr ?? v; }
        public byte       XMLConvert(byte       v, XAttribute attr) { return (byte?)(uint?)attr ?? v; }
        public short      XMLConvert(short      v, XAttribute attr) { return (short?)attr ?? v; }
        public int        XMLConvert(int        v, XAttribute attr) { return (int?)  attr ?? v; }
        public float      XMLConvert(float      v, XAttribute attr) { return (float?)attr ?? v; }
        public Color      XMLConvert(Color      v, XAttribute attr) { if (attr == null) return v; return (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString((string)attr); }
        public PointShort XMLConvert(PointShort v, XAttribute attr)
        {
            if (attr == null) return v;
            var ps = PointShort.TryParseInline((string)attr);
            if (attr.Name == "size" && ps == new PointShort()) ps = new PointShort(1, 1);
            return ps;
        }
        public FrameDirection XMLConvert(FrameDirection v, XAttribute attr)
        {
            if (attr == null) return v;
            FrameDirection f = FrameDirection.None;
            f = f.Convert<FrameDirection>(attr);
            return f;
        }
        public FramePlacement XMLConvert(FramePlacement v, XAttribute attr)
        {
            if (attr == null) return v;
            FramePlacement f = FramePlacement.Any;
            f = f.Convert<FramePlacement>(attr);
            return f;
        }
        public void XMLConvert(ObservableCollection<TileFrame> v, XAttribute attr)
        {
            var oc = new ObservableCollection<TileFrame>();
            if (string.IsNullOrWhiteSpace((string)attr)) return;

            foreach (var s in ((string)attr).Split(','))
            {
                oc.Add(new TileFrame(s));
            }
            v.ReplaceRange(oc);
        }

        public AttachGroup XMLConvert(AttachGroup v, XElement el)
        {
            if (el == null) return v;
            return new AttachGroup((string)el.Attribute("name"), (string)el.Attribute("attachTiles"));
        }

        public AttachGroup XMLConvert(AttachGroup v, XAttribute attr)
        {
            if (attr == null) return v;
            return new AttachGroup((string)attr.Parent.Attribute("name") ?? String.Empty, (string)attr);
        }

        public void XMLConvert(ObservableCollection<AttachGroup> v, XAttribute attr)
        {
            var oc = new ObservableCollection<AttachGroup>();
            if (string.IsNullOrWhiteSpace((string)attr)) return;

            foreach (string ag in ((string)attr).Split(';'))
            {
                var newAG = new AttachGroup();
                newAG.Name = "Tile: " + this.Name;

                var raw = ag;
                string[] ptf = raw.Split(':');
                if (ptf.Length == 2)
                { 
                    newAG.Placement = (new FramePlacement()).Convert<FramePlacement>(ptf[0]);
                    raw = ptf[1];
                }

                // Check for named AttachGroups
                if (Regex.IsMatch(raw, @"[a-zA-Z]+"))
                {
                    if (!_hasReadCache) { _readAttachGroups(attr.Parent); }
                    raw = Regex.Replace(raw, @"\s", String.Empty);
                    foreach (var agc in _attachGroupsCache)
                    {
                        if (agc == raw)
                        {
                            if (agc.Placement != null && newAG.Placement == null) newAG.Placement = agc.Placement;
                            newAG.AttachesTo = agc.AttachesTo;
                            break;
                        }
                    }
                }
                else
                {
                    AttachGroup.TryParse(raw, ref newAG);
                }
                
                oc.Add(newAG);
            }
            v.ReplaceRange(oc);
        }

        private void _readAttachGroups(XElement el)
        {
            _hasReadCache = true;  // start it off here to prevent recursion
            while (el.Parent != null) { el = el.Parent; }

            foreach (var ag in el.Elements("AttachGroups").Elements("AttachGroup"))
            {
                var curAG = XMLConvert(new AttachGroup(), ag);
                _attachGroupsCache.Add(curAG);
            }
        }

    }
}