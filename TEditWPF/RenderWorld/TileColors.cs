using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;

namespace TEdit.RenderWorld
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

        private static TileColor[] _tiles = new TileColor[byte.MaxValue];
        private static TileColor[] _walls = new TileColor[byte.MaxValue];

        public static TileColor[] Tiles
        {
            get { return _tiles; }
        }

        public static TileColor[] Walls
        {
            get { return _walls; }
        }

        public static Color Water { get; set; }
        public static Color Lava { get; set; }
        

        public static void Load(string filename)
        {
            Water = Color.FromArgb(128, 0, 64, 255);
            Lava = Color.FromArgb(255, 255, 96, 0);

            for (byte i = 0; i < byte.MaxValue; i++)
            {
                _tiles[i] = new TileColor() { Color = Colors.Magenta, ID = i, Name = "Unknown" };
                _walls[i] = new TileColor() { Color = Colors.Magenta, ID = i, Name = "Unknown" };
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
                        var lineproperty = TileColor.FromString(line);
                        if (lineproperty != null)
                        {
                            switch (section)
                            {
                                case FileSection.WALLCOLORS:
                                    _walls[lineproperty.ID] = lineproperty;
                                    break;
                                case FileSection.TILECOLORS:
                                    _tiles[lineproperty.ID] = lineproperty;
                                    break;
                                case FileSection.LIQUIDCOLORS:
                                    if (string.Equals(lineproperty.Name,"Water",StringComparison.InvariantCultureIgnoreCase))
                                        Water = lineproperty.Color;
                                    else if (string.Equals(lineproperty.Name, "Lava", StringComparison.InvariantCultureIgnoreCase))
                                        Lava = lineproperty.Color;
                                    break;
                            }
                        }
                    }
                }
            }
        }

        public static void Save(string filename)
        {
            using (TextWriter sr = new StreamWriter(filename))
            {
                sr.WriteLine("# File structure is");
                sr.WriteLine("# BlockID|BlockName|Color");
                sr.WriteLine("# Color is in ARGB HEX, e.g. AARRGGBB");

                sr.WriteLine(FileSection.LIQUIDCOLORS.ToString());
                sr.WriteLine((new TileColor(1, Water, "Water")).ToString());
                sr.WriteLine((new TileColor(2, Lava, "Lava")).ToString());

                sr.WriteLine(FileSection.WALLCOLORS.ToString());
                for (byte i = 0; i < byte.MaxValue; i++)
                {
                    if (!string.Equals(_walls[i].Name,"Unknown",StringComparison.InvariantCultureIgnoreCase))
                        sr.WriteLine(_walls[i].ToString());
                }


                sr.WriteLine(FileSection.TILECOLORS.ToString());
                for (byte i = 0; i < byte.MaxValue; i++)
                {
                    if (!string.Equals(_tiles[i].Name, "Unknown", StringComparison.InvariantCultureIgnoreCase))
                        sr.WriteLine(_tiles[i].ToString());
                }
            }
        }

    }
}