using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;

namespace TEditWPF.RenderWorld
{
    public static class TileColors
    {
        #region FileSection enum

        public enum FileSection
        {
            WALLCOLORS,
            TILECOLORS,
            LIQUIDCOLORS
        }

        #endregion

        private static TileProperties[] tileColor = new TileProperties[byte.MaxValue];
        private static TileProperties[] wallColor = new TileProperties[byte.MaxValue];

        public static TileProperties[] TileColor
        {
            get { return tileColor; }
        }

        public static TileProperties[] WallColor
        {
            get { return wallColor; }
        }

        private static Color WaterColor { get; set; }
        private static Color LavaColor { get; set; }
        

        public static void Load(string filename)
        {
            WaterColor = Color.FromArgb(128, 0, 64, 255);
            LavaColor = Color.FromArgb(255, 255, 96, 0);

            for (byte i = 0; i < byte.MaxValue; i++)
            {
                tileColor[i] = new TileProperties() { Color = Colors.Magenta, ID = i, Name = "Unknown" };
                wallColor[i] = new TileProperties() { Color = Colors.Magenta, ID = i, Name = "Unknown" };
            }



            using (TextReader sr = new StreamReader(filename))
            {
                FileSection section = FileSection.LIQUIDCOLORS;
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
                        TileProperties lineproperty = ParseColorFileLine(line);
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
                                    if (string.Equals(lineproperty.Name,"Water",StringComparison.InvariantCultureIgnoreCase))
                                        WaterColor = lineproperty.Color;
                                    else if (string.Equals(lineproperty.Name, "Lava", StringComparison.InvariantCultureIgnoreCase))
                                        LavaColor = lineproperty.Color;
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
                foreach (var item in LiquidColor)
                {
                    sr.WriteLine(GetTilePropertyFileLine(item.Value));
                }

                sr.WriteLine(FileSection.WALLCOLORS.ToString());
                foreach (var item in WallColor)
                {
                    sr.WriteLine(GetTilePropertyFileLine(item.Value));
                }

                sr.WriteLine(FileSection.TILECOLORS.ToString());
                foreach (var item in TileColor)
                {
                    sr.WriteLine(GetTilePropertyFileLine(item.Value));
                }
            }
        }

        public static string GetTilePropertyFileLine(TileProperties item)
        {
            return String.Format("{0}|{1}|#{2}{3}{4}{5}", item.ID, item.Name, item.Color.A, item.Color.R, item.Color.G,
                                 item.Color.B);
        }

        public static TileProperties ParseColorFileLine(string line)
        {
            string[] splitline = line.Split(new[] {',', '|'});
            if (splitline.Length == 3)
            {
                byte id = 0;
                byte.TryParse(splitline[0], out id);

                string name = splitline[1];
                var color = (Color) ColorConverter.ConvertFromString("#" + splitline[2]);

                return new TileProperties(id, color, name);
            }
            return null;
        }
    }
}