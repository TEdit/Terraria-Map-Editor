﻿using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace TEdit.Editor;

public partial class ToolDefaultData
{
    private static PaintMode _paintMode = PaintMode.TileAndWall;

    private static int _brushWidth = 20;
    private static int _brushHeight = 20;
    private static int _brushOutline = 1;
    private static BrushShape _brushShape = BrushShape.Square;

    private static int _paintTile = 0;
    private static int _paintTileMask = 0;
    private static bool _paintTileActive = true;
    private static MaskMode _paintTileMaskMode = MaskMode.Off;

    private static int _paintWall = 0;
    private static int _paintWallMask = 0;
    private static bool _paintWallActive = true;
    private static MaskMode _paintWallMaskMode = MaskMode.Off;

    private static bool _redWire = false;
    private static bool _greenWire = false;
    private static bool _blueWire = false;
    private static bool _yellowWire = false;

    //  Invoked from World.Settings
    public static void LoadSettings(IEnumerable<XElement> xmlToolSettings)
    {
        foreach (var xElement in xmlToolSettings.Elements("Tool"))
        {
            string toolName = (string)xElement.Attribute("Name");

            switch (toolName)
            {
                case "Paint":
                    _paintMode = (PaintMode)ToEnum(typeof(PaintMode), (string)xElement.Attribute("Mode") ?? PaintMode.TileAndWall.ToString());
                    break;
                case "Brush":
                    _brushWidth = (int?)xElement.Attribute("Width") ?? 20;
                    _brushHeight = (int?)xElement.Attribute("Height") ?? 20;
                    _brushOutline = (int?)xElement.Attribute("Outline") ?? 1;
                    _brushShape = (BrushShape)ToEnum(typeof(BrushShape), (string)xElement.Attribute("Shape") ?? BrushShape.Square.ToString());
                    break;
                case "Tile":
                    _paintTile = (int?)xElement.Attribute("Tile") ?? 0;
                    _paintTileMask = (int?)xElement.Attribute("Mask") ?? 0;
                    _paintTileActive = (bool)xElement.Attribute("Active");
                    _paintTileMaskMode = (MaskMode)ToEnum(typeof(MaskMode), (string)xElement.Attribute("Mode") ?? MaskMode.Off.ToString());
                    break;
                case "Wall":
                    _paintWall = (int?)xElement.Attribute("Wall") ?? 0;
                    _paintWallMask = (int?)xElement.Attribute("Mask") ?? 0;
                    _paintWallActive = (bool)xElement.Attribute("Active");
                    _paintWallMaskMode = (MaskMode)ToEnum(typeof(MaskMode), (string)xElement.Attribute("Mode") ?? MaskMode.Off.ToString());
                    break;
                case "Wire":
                    _redWire = (bool)xElement.Attribute("Red");
                    _blueWire = (bool)xElement.Attribute("Blue");
                    _greenWire = (bool)xElement.Attribute("Green");
                    _yellowWire = (bool)xElement.Attribute("Yellow");
                    break;
            }
        }
    }

    private static Enum ToEnum(Type type, string name)
    {
        return (Enum)Enum.Parse(type, name, true);
    }

    public static PaintMode PaintMode
    {
        get { return _paintMode; }
    }

    public static int BrushWidth
    {
        get { return _brushWidth; }
    }

    public static int BrushHeight
    {
        get { return _brushHeight; }
    }

    public static int BrushOutline
    {
        get { return _brushOutline; }
    }

    public static BrushShape BrushShape
    {
        get { return _brushShape; }
    }

    public static int PaintTile
    {
        get { return _paintTile; }
    }

    public static int PaintTileMask
    {
        get { return _paintTileMask; }
    }

    public static bool PaintTileActive
    {
        get { return _paintTileActive; }
    }

    public static MaskMode PaintTileMaskMode
    {
        get { return _paintTileMaskMode; }
    }

    public static int PaintWall
    {
        get { return _paintWall; }
    }

    public static int PaintWallMask
    {
        get { return _paintWallMask; }
    }

    public static bool PaintWallActive
    {
        get { return _paintWallActive; }
    }

    public static MaskMode PaintWallMaskMode
    {
        get { return _paintWallMaskMode; }
    }

    public static bool RedWire
    {
        get { return _redWire; }
    }

    public static bool GreenWire
    {
        get { return _greenWire; }
    }

    public static bool BlueWire
    {
        get { return _blueWire; }
    }

    public static bool YellowWire
    {
        get { return _yellowWire; }
    }
}
