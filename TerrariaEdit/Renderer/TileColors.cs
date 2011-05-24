using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Text;
using System.IO;
using System.Xml;
using TerrariaMapEditor.Common;

namespace TerrariaMapEditor.Renderer
{
    public class TileColors
    {
        public TileColors()
        {

        }

        public static TileColors Load(string filename)
        {
            TileColors tc = new TileColors();


            using (TextReader sr = new StreamReader(filename))
            {
                var section = FileSection.LIQUIDCOLORS;
                string line = string.Empty;
                while ((line = sr.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line) || line.StartsWith("#") || line.StartsWith("//"))
                        continue;

                    if (string.Equals(line, FileSection.WALLCOLORS.ToString()))
                        section = FileSection.WALLCOLORS;
                    else if (string.Equals(line, FileSection.TILECOLORS.ToString()))
                        section = FileSection.TILECOLORS;
                    else if (string.Equals(line, FileSection.LIQUIDCOLORS.ToString()))
                        section = FileSection.LIQUIDCOLORS;
                    else
                    {
                        var lineproperty = ParseColorFileLine(line);
                        switch (section)
                        {
                            case FileSection.WALLCOLORS:
                                tc.WallColor.Add(lineproperty.ID, lineproperty);
                                break;
                            case FileSection.TILECOLORS:
                                tc.TileColor.Add(lineproperty.ID, lineproperty);
                                break;
                            case FileSection.LIQUIDCOLORS:
                                tc.LiquidColor.Add(lineproperty.ID, lineproperty);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            return tc;
        }

        public void Save(string filename)
        {
            using (TextWriter sr = new StreamWriter(filename))
            {
                sr.WriteLine("# File structure is");
                sr.WriteLine("# BlockID|BlockName|Color");
                sr.WriteLine("# Color is in ARGB HEX, e.g. AARRGGBB");

                sr.WriteLine(FileSection.LIQUIDCOLORS.ToString());
                foreach (var item in this.LiquidColor)
                {
                    sr.WriteLine(GetTilePropertyFileLine(item.Value));
                }

                sr.WriteLine(FileSection.WALLCOLORS.ToString());
                foreach (var item in this.WallColor)
                {
                    sr.WriteLine(GetTilePropertyFileLine(item.Value));
                }

                sr.WriteLine(FileSection.TILECOLORS.ToString());
                foreach (var item in this.TileColor)
                {
                    sr.WriteLine(GetTilePropertyFileLine(item.Value));
                }
            }
        }

        public static string GetTilePropertyFileLine(TileProperties item)
        {
            return String.Format("{0}|{1}|{2}", item.ID, item.Name, item.Color.Name);
        }

        public static TileProperties ParseColorFileLine(string line)
        {
            var splitline = line.Split(new char[] { ',', '|' });
            byte id = 0;
            byte.TryParse(splitline[0], out id);

            string name = splitline[1];

            Color color = Color.FromName(splitline[2]);
            if (Utility.IsNumeric(splitline[2], System.Globalization.NumberStyles.HexNumber))
            {
                Int32 argb;
                Int32.TryParse(splitline[2], System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.CurrentCulture, out argb);
                color = Color.FromArgb(argb);
            }

            return new TileProperties(id, color, name);
        }

        public enum FileSection
        {
            WALLCOLORS,
            TILECOLORS,
            LIQUIDCOLORS
        }

        private Dictionary<byte, TileProperties> tileColor = new Dictionary<byte, TileProperties>();
        public Dictionary<byte, TileProperties> TileColor
        {
            get { return tileColor; }
        }

        private Dictionary<byte, TileProperties> liquidColor = new Dictionary<byte, TileProperties>();
        public Dictionary<byte, TileProperties> LiquidColor
        {
            get { return liquidColor; }
        }

        private Dictionary<byte, TileProperties> wallColor = new Dictionary<byte, TileProperties>();
        public Dictionary<byte, TileProperties> WallColor
        {
            get { return wallColor; }
        }
    }
}
