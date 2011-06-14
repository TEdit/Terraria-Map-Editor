using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using TerrariaMapEditor.Common;
using System.Windows.Media;

namespace TEditWPF.RenderWorld
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
                        if (lineproperty != null)
                        {
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
            return String.Format("{0}|{1}|#{2}{3}{4}{5}", item.ID, item.Name, item.Color.A, item.Color.R, item.Color.G, item.Color.B);
        }

        public static TileProperties ParseColorFileLine(string line)
        {
            var splitline = line.Split(new char[] { ',', '|' });
            if (splitline.Length == 3)
            {
                byte id = 0;
                byte.TryParse(splitline[0], out id);

                string name = splitline[1];
                Color color = (Color) ColorConverter.ConvertFromString("#" + splitline[2]);

                return new TileProperties(id, color, name);
            }
            return null;
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
