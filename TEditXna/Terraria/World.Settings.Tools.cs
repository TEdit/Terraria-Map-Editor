using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TEditXna.Editor;

namespace TEditXNA.Terraria
{
    public enum DefaultSetting
    {
        PaintMode,
        BrushWidth, BrushHeight, BrushOutline, BrushShape,
        PaintTile, PaintTileMask, PaintTileMaskMode,
        PaintWall, PaintWallMask, PaintWallMaskMode,
    }

    public partial class ToolSettings
    {
        private static Dictionary<DefaultSetting, int> _intDefaults = new Dictionary<DefaultSetting, int>();
        private static Dictionary<DefaultSetting, string> _stringDefaults = new Dictionary<DefaultSetting, string>();

        //  Invoked from World.Settings
        internal static void LoadSettings(IEnumerable<XElement> xmlToolSettings)
        {
            foreach (var xElement in xmlToolSettings.Elements("Tool"))
            {
                string toolName = (string)xElement.Attribute("Name");

                switch (toolName)
                {
                    case "Paint":
                        _stringDefaults[DefaultSetting.PaintMode] = (string)xElement.Attribute("Mode") ?? "Tile";
                        break;
                    case "Brush":
                        _intDefaults[DefaultSetting.BrushWidth] = (int?)xElement.Attribute("Width") ?? 20;
                        _intDefaults[DefaultSetting.BrushHeight] = (int?)xElement.Attribute("Height") ?? 20;
                        _intDefaults[DefaultSetting.BrushOutline] = (int?)xElement.Attribute("Outline") ?? 1;
                        _stringDefaults[DefaultSetting.BrushShape] = (string)xElement.Attribute("Shape") ?? "Rectangle";
                        break;
                    case "Tile":
                        _intDefaults[DefaultSetting.PaintTile] = (int?)xElement.Attribute("Tile") ?? 0;
                        _intDefaults[DefaultSetting.PaintTileMask] = (int?)xElement.Attribute("Mask") ?? 0;
                        _stringDefaults[DefaultSetting.PaintTileMaskMode] = (string)xElement.Attribute("Mode") ?? "Off";
                        break;
                    case "Wall":
                        _intDefaults[DefaultSetting.PaintWall] = (int?)xElement.Attribute("Wall") ?? 0;
                        _intDefaults[DefaultSetting.PaintWallMask] = (int?)xElement.Attribute("Mask") ?? 0;
                        _stringDefaults[DefaultSetting.PaintWallMaskMode] = (string)xElement.Attribute("Mode") ?? "Off";
                        break;
                }
            }
        }

        public static Dictionary<DefaultSetting, int> Int
        {
            get { return _intDefaults; }
        }

        public static Dictionary<DefaultSetting, string> String
        {
            get { return _stringDefaults; }
        }

        public static Enum Mode(Type type, DefaultSetting defaultSetting)
        {
            string value = _stringDefaults[defaultSetting];

            return (Enum)System.Enum.Parse(type, value, true);
        }
    }
}