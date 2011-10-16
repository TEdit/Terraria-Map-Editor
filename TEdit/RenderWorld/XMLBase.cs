using System;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using TEdit.Common.Structures;
using TEdit.Common;
using System.Windows.Media;
namespace TEdit.RenderWorld
{
    // Useful functions for porting XML properties to C# properties
    [Serializable]
    public class XMLBase : NamedBase
    {
        // XML conversions with defaults
        public bool       XMLConvert(bool       v, XAttribute attr) { return (bool?) attr ?? v;  }
        public string     XMLConvert(string     v, XAttribute attr) { return (string)attr ?? v; }
        public byte       XMLConvert(byte       v, XAttribute attr) { return (byte?)(uint?)attr ?? v; }
        public short      XMLConvert(short      v, XAttribute attr) { return (short?)attr ?? v; }
        public int        XMLConvert(int        v, XAttribute attr) { return (int?)  attr ?? v; }
        public float      XMLConvert(float      v, XAttribute attr) { return (float?)attr ?? v; }
        public Color      XMLConvert(Color      v, XAttribute attr) { if (attr == null) return v; return (Color)ColorConverter.ConvertFromString((string)attr); }
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
        public void XMLConvert(ObservableCollection<byte> v, XAttribute attr)
        {
            var oc = new ObservableCollection<byte>();
            if (string.IsNullOrWhiteSpace((string)attr)) return;

            string[] split = ((string)attr).Split(',');
            foreach (var s in split)
            {
                oc.Add((byte)Convert.ChangeType(s, typeof(byte)));
            }
            v.ReplaceRange(oc);
        }
    
    }
}