using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Reflection;
using System.Xml.Linq;
using System.Linq;
using System.Text;
using Terraria.ID;
using System.Xml;
using System.IO;
using Terraria;
using System;

namespace SettingsFileUpdater.TerrariaHost
{
    /// <summary>
    /// Builds MapColorsUpdated.xml using Terraria's internal map palette:
    /// MapHelper.colorLookup + tileLookup/tileOptionCounts + wallLookup/wallOptionCounts.
    /// No changes required to Terraria.Map.MapHelper.
    /// </summary>
    public static class MapColorsExporter
    {
        #region XML Header Comment

        /// <summary>
        /// Header comment written at the top of the XML (between declaration and root).
        /// </summary>
        private const string HeaderComment =
            @"

Terraria MapColors.XML Specifications
====================================
Current Game Ver: Terraria 1.4.5.0
Current File Ver: 0.5
Primary URL: https://github.com/TEdit/Terraria-Map-Editor/tree/main/src/TEdit/MapColors.xml
====================================

PLEASE KEEP FORMATTING IF SUBMITTING PULL REQUEST FOR THIS FILE. THANKS!

";

        /// <summary>
        /// Writer settings to keep formatting stable (UTF-8, indented, Windows newlines, includes declaration).
        /// </summary>
        private static XmlWriterSettings CreateXmlWriterSettings() => new()
        {
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            Indent = true,
            NewLineChars = "\r\n",
            NewLineHandling = NewLineHandling.Replace,
            OmitXmlDeclaration = false
        };

        /// <summary>
        /// Saves an XDocument to a string using the writer settings (includes declaration + comment).
        /// </summary>
        private static string SaveDocToString(XDocument doc)
        {
            using var sw = new StringWriter();
            using var xw = XmlWriter.Create(sw, CreateXmlWriterSettings());
            doc.Save(xw);
            return sw.ToString();
        }

        /// <summary>
        /// Saves an XDocument to disk using the writer settings (includes declaration + comment).
        /// </summary>
        private static void SaveDocToFile(XDocument doc, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
            using var xw = XmlWriter.Create(path, CreateXmlWriterSettings());
            doc.Save(xw);
        }
        #endregion

        #region Reflection Handles (MapHelper Internals)

        /// <summary>
        /// Reflection flags for accessing MapHelper internals (public + nonpublic static).
        /// </summary>
        private static readonly BindingFlags _bf = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// Target type: Terraria.Map.MapHelper (holds colorLookup + lookup tables).
        /// </summary>
        private static readonly Type _mapHelperType = typeof(Terraria.Map.MapHelper);

        /// <summary>
        /// Internal palette + lookup tables (built by Lang.BuildMapAtlas / MapHelper.Initialize).
        /// </summary>
        private static readonly FieldInfo _fiColorLookup      = _mapHelperType.GetField("colorLookup", _bf);
        private static readonly FieldInfo _fiTileLookup       = _mapHelperType.GetField("tileLookup", _bf);
        private static readonly FieldInfo _fiTileOptionCounts = _mapHelperType.GetField("tileOptionCounts", _bf);

        private static readonly FieldInfo _fiWallLookup       = _mapHelperType.GetField("wallLookup", _bf);
        private static readonly FieldInfo _fiWallOptionCounts = _mapHelperType.GetField("wallOptionCounts", _bf);

        /// <summary>
        /// Terraria's internal "apply paint to map color" method.
        /// (MapColor(ushort type, ref Color color, byte paint) is usually private/internal.)
        /// </summary>
        private static readonly MethodInfo _miMapColor =
            _mapHelperType.GetMethod("MapColor", _bf, null,
                [typeof(ushort), typeof(Color).MakeByRefType(), typeof(byte)], null);

        #endregion

        #region Public Entry Points

        /// <summary>
        /// Builds the full MapColors XML as a string (includes XML declaration + header comment).
        /// </summary>
        public static string BuildMapColorsXmlString(string optionalOriginalPath = null)
        {
            var doc = BuildMapColorsUpdatedXDocument(optionalOriginalPath);
            return SaveDocToString(doc); // includes XML declaration + comment
        }

        /// <summary>
        /// Writes the MapColors XML to disk (includes XML declaration + header comment).
        /// </summary>
        public static void WriteMapColorsXml(string outputPath, string optionalOriginalPath = null)
        {
            var doc = BuildMapColorsUpdatedXDocument(optionalOriginalPath);
            SaveDocToFile(doc, outputPath); // includes XML declaration + comment
        }
        #endregion

        #region Core Build

        /// <summary>
        /// Core document builder:
        /// - Pulls base colors from MapHelper.colorLookup via tileLookup/wallLookup + option counts
        /// - Applies paint colors using MapHelper.MapColor (if available)
        /// - Optionally overrides BuildSafe from an existing MapColors file
        /// - Emits a single <Colors> root with <Tiles> and <Walls>
        /// </summary>
        private static XDocument BuildMapColorsUpdatedXDocument(string optionalOriginalPath)
        {
            // Pull MapHelper palette arrays (must be initialized already).
            Color[]  colorLookup      = (Color[]) _fiColorLookup.GetValue(null);
            ushort[] tileLookup       = (ushort[])_fiTileLookup.GetValue(null);
            int[]    tileOptionCounts = (int[])   _fiTileOptionCounts.GetValue(null);

            ushort[] wallLookup       = (ushort[])_fiWallLookup.GetValue(null);
            int[]    wallOptionCounts = (int[])   _fiWallOptionCounts.GetValue(null);

            if (colorLookup == null || tileLookup == null || tileOptionCounts == null ||
                wallLookup == null || wallOptionCounts == null)
            {
                throw new InvalidOperationException(
                    "MapHelper palette is not initialized. Call Lang.BuildMapAtlas() / MapHelper.Initialize() first.");
            }

            int maxPaintID = GetMaxPaintIdExclusive();

            // Optional overrides from MapColorsOriginal.xml.
            var origTile = new Dictionary<string, string>();
            var origWall = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(optionalOriginalPath) && File.Exists(optionalOriginalPath))
            {
                var xml = new XmlDocument();
                xml.Load(optionalOriginalPath);

                origTile = BuildSafeDict(xml, "/Colors/Tiles/Tile");
                origWall = BuildSafeDict(xml, "/Colors/Walls/Wall");
            }

            // Detect if solidity arrays look initialized (prevents the "everything is non-solid" timing bug).
            bool solidReady = Main.tileSolid != null &&
                              Main.tileSolid.Length > TileID.Dirt &&
                              Main.tileSolid[TileID.Dirt];

            // -------------------------
            // Tiles
            // -------------------------
            var tilesEl = new XElement("Tiles");
            for (int tileId = 0; tileId < TileID.Count; tileId++)
            {
                int subCount = tileOptionCounts[tileId];
                if (subCount <= 0) continue;

                bool ruleSafe = ComputeTileBuildSafe(tileId, solidReady);

                int baseIndex = tileLookup[tileId];
                if (baseIndex == 0) continue;

                for (int subId = 0; subId < subCount; subId++)
                {
                    Color baseColor = colorLookup[baseIndex + subId];

                    for (int paintType = 0; paintType < maxPaintID; paintType++)
                    {
                        Color c = baseColor;

                        if (paintType > 0)
                            ApplyPaint((ushort)tileId, ref c, (byte)paintType);

                        string hex = $"{c.R:X2}{c.G:X2}{c.B:X2}";

                        string key = $"{tileId}-{paintType}";
                        string buildSafe =
                            (origTile.TryGetValue(key, out var bs) && !string.IsNullOrWhiteSpace(bs))
                                ? bs
                                : (ruleSafe ? "true" : "false");

                        if (string.IsNullOrWhiteSpace(buildSafe))
                            buildSafe = ruleSafe ? "true" : "false";

                        tilesEl.Add(new XElement("Tile",
                            new XAttribute("Id", tileId),
                            new XAttribute("SubID", subId),
                            new XAttribute("Name", IdNameLookup.Tile(tileId)),
                            new XAttribute("Paint", paintType),
                            new XAttribute("Color", $"#FF{hex}"),
                            new XAttribute("BuildSafe", buildSafe)
                        ));
                    }
                }
            }

            // -------------------------
            // Walls
            // -------------------------
            var wallsEl = new XElement("Walls");
            for (int wallId = 0; wallId < WallID.Count; wallId++)
            {
                int subCount = wallOptionCounts[wallId];
                if (subCount <= 0) continue;

                // Default wall rule.
                bool ruleSafe = true;

                int baseIndex = wallLookup[wallId];
                if (baseIndex == 0) continue;

                for (int subId = 0; subId < subCount; subId++)
                {
                    Color baseColor = colorLookup[baseIndex + subId];

                    for (int paintType = 0; paintType < maxPaintID; paintType++)
                    {
                        Color c = baseColor;

                        if (paintType > 0)
                            ApplyPaint((ushort)(wallId + TileID.Count), ref c, (byte)paintType);

                        string hex = $"{c.R:X2}{c.G:X2}{c.B:X2}";

                        string key = $"{wallId}-{paintType}";
                        string buildSafe =
                            (origWall.TryGetValue(key, out var bs) && !string.IsNullOrWhiteSpace(bs))
                                ? bs
                                : (ruleSafe ? "true" : "false");

                        if (string.IsNullOrWhiteSpace(buildSafe))
                            buildSafe = ruleSafe ? "true" : "false";

                        wallsEl.Add(new XElement("Wall",
                            new XAttribute("Id", wallId),
                            new XAttribute("SubID", subId),
                            new XAttribute("Name", IdNameLookup.Wall(wallId)),
                            new XAttribute("Paint", paintType),
                            new XAttribute("Color", $"#FF{hex}"),
                            new XAttribute("BuildSafe", buildSafe)
                        ));
                    }
                }
            }

            // Formatting:
            // - Declaration.
            // - Comment.
            // - Blank line.
            // - Root.
            return new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XComment(HeaderComment),
                new XText("\r\n"),
                new XElement("Colors", tilesEl, wallsEl)
            );
        }
        #endregion

        #region BuildSafe Rules

        /// <summary>
        /// Simple BuildSafe heuristic (tile-focused):
        /// - falling tiles, cuttable tiles, frame-important tiles, breakable-on-place tiles => unsafe
        /// - optionally treat non-solid and non-solid-top tiles as unsafe (only if arrays look initialized)
        /// </summary>
        private static bool ComputeTileBuildSafe(int tileId, bool solidReady)
        {
            // Gravity / falling tiles => unsafe
            if (TileID.Sets.Falling[tileId])
                return false;

            // Cuttable by swords/Zenith (vines/plants/etc) => unsafe.
            if (Main.tileCut[tileId])
                return false;

            // Furniture / multi-tiles => unsafe (anchor-dependent).
            if (Main.tileFrameImportant[tileId])
                return false;

            // Fragile-on-place => unsafe.
            if (TileID.Sets.BreakableWhenPlacing[tileId])
                return false;

            // Optional: only apply solidity rule if arrays look initialized.
            if (solidReady)
            {
                if (!Main.tileSolid[tileId] && !Main.tileSolidTop[tileId])
                    return false;
            }

            return true;
        }
        #endregion

        #region Paint + MaxPaint

        /// <summary>
        /// Applies Terraria’s internal paint logic to a base map color (if MapColor exists).
        /// </summary>
        private static void ApplyPaint(ushort typeOrWallType, ref Color color, byte paint)
        {
            // If MapColor exists, use Terraria’s own paint logic (best).
            if (_miMapColor != null)
            {
                object[] args = [typeOrWallType, color, paint];
                _miMapColor.Invoke(null, args);
                color = (Color)args[1];
                return;
            }
        }

        /// <summary>
        /// Finds the highest PaintID constant value and returns max+1, so loops can use [0..maxPaintID-1].
        /// </summary>
        private static int GetMaxPaintIdExclusive()
        {
            return typeof(PaintID)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(byte))
                .Select(f => (byte)f.GetRawConstantValue())
                .DefaultIfEmpty((byte)0)
                .Max() + 1;
        }
        #endregion

        #region Original BuildSafe Dict

        /// <summary>
        /// Loads BuildSafe values from an existing MapColors file, keyed by "Id-Paint".
        /// </summary>
        private static Dictionary<string, string> BuildSafeDict(XmlDocument doc, string xpath)
        {
            var dict = new Dictionary<string, string>();

            foreach (XmlNode n in doc.SelectNodes(xpath))
            {
                var id    = n.Attributes["Id"]?.Value;
                var paint = n.Attributes["Paint"]?.Value;
                var bs    = n.Attributes["BuildSafe"]?.Value;

                if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(paint))
                    continue;

                if (!string.IsNullOrWhiteSpace(bs))
                    bs = bs.Trim().ToLowerInvariant();

                dict[$"{id}-{paint}"] = bs ?? "";
            }

            return dict;
        }
        #endregion

        #region ID Name Lookup (reflection-based)

        /// <summary>
        /// Reflection-based "ID -> constant name" lookup for TileID and WallID.
        /// </summary>
        public static class IdNameLookup
        {
            private static readonly Lazy<Dictionary<int, string>> _tileNames =
                new(() => Build(typeof(TileID)));

            private static readonly Lazy<Dictionary<int, string>> _wallNames =
                new(() => Build(typeof(WallID)));

            public static string Tile(int id)
                => _tileNames.Value.TryGetValue(id, out var name) ? name : $"Tile_{id}";

            public static string Wall(int id)
                => _wallNames.Value.TryGetValue(id, out var name) ? name : $"Wall_{id}";

            private static Dictionary<int, string> Build(Type t)
            {
                var dict = new Dictionary<int, string>();

                foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    if (!f.IsLiteral || f.IsInitOnly) continue;

                    var ft = f.FieldType;
                    if (ft != typeof(int) && ft != typeof(ushort) && ft != typeof(short) && ft != typeof(byte))
                        continue;

                    int value = Convert.ToInt32(f.GetRawConstantValue());
                    string name = f.Name;

                    if (!dict.ContainsKey(value))
                        dict[value] = name;
                }

                return dict;
            }
        }
        #endregion
    }
}
