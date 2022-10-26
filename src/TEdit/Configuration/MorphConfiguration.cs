using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TEdit.Terraria;
using TEdit.Geometry.Primitives;
using TEdit.ViewModel;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using TEdit.Editor;

namespace TEdit.Configuration
{
    public class MorphConfiguration
    {
        public Dictionary<string, MorphBiomeData> Biomes { get; set; } = new();
        public Dictionary<string, int> MossTypes { get; set; } = new();

        private readonly HashSet<int> _mossTypes = new HashSet<int>();

        public bool IsMoss(ushort type) => _mossTypes.Contains(type);

        public void Save(string fileName)
        {
            using (StreamWriter file = File.CreateText(fileName))
            using (JsonTextWriter writer = new JsonTextWriter(file))
            {
                JsonSerializer serializer = new JsonSerializer()
                {
                    Formatting = Formatting.Indented,
                };
                serializer.Serialize(writer, this);
            }
        }

        public static MorphConfiguration LoadJson(string fileName)
        {
            using (StreamReader file = File.OpenText(fileName))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                JsonSerializer serializer = new JsonSerializer();
                var morphConfig = serializer.Deserialize<MorphConfiguration>(reader);
                morphConfig.InitCache();
                return morphConfig;
            }
        }

        public void InitCache()
        {
            foreach (var biome in Biomes)
            {
                biome.Value.InitCache();
            }

            _mossTypes.Clear();
            foreach (var id in MossTypes.Values)
            {
                _mossTypes.Add(id);
            }
        }
    }

    public class MorphBiomeData
    {
        // this could probably be move to settings file and loaded as global someday. this works for now
        static HashSet<int> notSolidTiles = new HashSet<int>(new int[] { 3, 4, 5, 11, 12, 13, 14, 15, 16, 17, 18, 20, 21, 24, 26, 27, 28, 29, 31, 32, 33, 34, 35, 36, 42, 49, 50, 51, 52, 55, 61, 62, 69, 71, 72, 73, 74, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 110, 113, 114, 115, 124, 125, 126, 128, 129, 131, 132, 133, 134, 135, 136, 139, 141, 142, 143, 144, 149, 165, 171, 172, 173, 174, 178, 184, 185, 186, 187, 201, 205, 207, 209, 210, 212, 213, 214, 215, 216, 217, 218, 219, 220, 227, 228, 231, 233, 236, 237, 238, 240, 241, 242, 243, 244, 245, 246, 247, 254, 269, 270, 271, 275, 276, 277, 278, 279, 280, 281, 282, 283, 285, 286, 287, 288, 289, 290, 291, 292, 293, 294, 295, 296, 297, 298, 299, 300, 301, 302, 303, 304, 305, 306, 307, 308, 309, 310, 314, 316, 317, 318, 319, 320, 323, 324, 330, 331, 332, 333, 334, 335, 336, 337, 338, 339, 340, 341, 342, 343, 344, 349, 351, 352, 353, 354, 355, 356, 358, 359, 360, 361, 362, 363, 364, 365, 366, 372, 373, 374, 375, 376, 377, 378, 379, 382, 386, 389, 390, 391, 392, 393, 394, 395, 405, 406, 410, 411, 412, 413, 414, 419, 420, 423, 424, 425, 428, 429, 440, 441, 442, 443, 444, 445, 449, 450, 451, 452, 453, 454, 455, 456, 457, 461, 462, 463, 464, 465, 466, 467, 468, 469, 470, 471, 475, 480, 485, 486, 487, 488, 489, 490, 491, 493, 494, 497, 499, 504, 505, 506, 509, 510, 511, 518, 519, 520, 521, 522, 523, 524, 525, 526, 527, 528, 529, 530, 531, 532, 533, 538, 542, 543, 544, 545, 547, 548, 549, 550, 551, 552, 553, 554, 555, 556, 558, 559, 560, 561, 564, 565, 567, 568, 569, 570, 571, 572, 573, 574, 575, 576, 577, 578, 579, 580, 581, 582, 583, 584, 585, 586, 587, 588, 589, 590, 591, 592, 593, 594, 595, 596, 597, 598, 599, 600, 601, 602, 603, 604, 605, 606, 607, 608, 609, 610, 611, 612, 613, 614, 615, 616, 617, 619, 620, 621, 622, 623, 624, 629, 630, 631, 632, 634, 636, 637, 638, 639, 640, 642, 643, 644, 645, 646, 647, 648, 649, 650, 651, 652, 653, 654, 655, 656, 657, 658, 660, 663, 665, });

        public string Name { get; set; }
        public List<MorphId> MorphTiles { get; set; } = new();
        public List<MorphId> MorphWalls { get; set; } = new();

        public bool ContainsWall(int id) => _wallCache.ContainsKey(id);
        public bool ContainsTile(int id) => _tileCache.ContainsKey(id);

        public void ApplyMorph(MorphToolOptions options, Tile source, MorphLevel level, Vector2Int32 location)
        {
            ApplyTileMorph(options, source, level, location);
            ApplyWallMorph(options, source, level, location);
        }

        private void ApplyWallMorph(MorphToolOptions options, Tile source, MorphLevel level, Vector2Int32 location)
        {
            if (source.Wall == 0) { return; }

            ushort sourceId = source.Wall;
            if (!_wallCache.TryGetValue(sourceId, out var morphId)) { return; }

            if (morphId.SourceIds.Contains(sourceId))
            {
                if (morphId.Delete)
                {
                    source.Wall = 0;
                    return;
                }

                // Check if tiles need gravity checks.
                if (morphId.Gravity != null && AirBelow(location.X, location.Y))
                {
                    source.Wall = morphId.Gravity.GetId(level) ?? 397;
                }
                else if (morphId.TouchingAir != null && TouchingAir(location.X, location.Y))
                {
                    var id = morphId.TouchingAir.GetId(level);
                    if (id != null)
                    {
                        source.Wall = id.Value;
                    }
                }
                else
                {
                    var id = morphId.Default.GetId(level);
                    if (id != null)
                    {
                        source.Wall = id.Value;
                    }
                }
            }
        }

        private void ApplyTileMorph(MorphToolOptions options, Tile source, MorphLevel level, Vector2Int32 location)
        {
            if (!source.IsActive) { return; }
            ushort sourceId = source.Type;

            if (!_tileCache.TryGetValue(sourceId, out var morphId)) { return; }

            // check skip base tiles
            if (!morphId.IsEvil && !options.EnableBaseTiles) { return; }

            // check skip evil tiles
            if (morphId.IsEvil && !options.EnableEvilTiles) { return; }

            if (morphId.SourceIds.Contains(sourceId))
            {
                if (morphId.Delete)
                {
                    source.Type = 0;
                    source.IsActive = false;
                    return;
                }

                // Check if tiles need gravity checks.
                if (morphId.Gravity != null && AirBelow(location.X, location.Y))
                {
                    source.Type = morphId.Gravity.GetId(level) ?? 397;
                }
                else if (morphId.TouchingAir != null && TouchingAir(location.X, location.Y))
                {
                    var id = morphId.TouchingAir.GetId(level);
                    if (id != null)
                    {
                        source.Type = id.Value;
                    }
                }
                else
                {
                    var id = morphId.Default.GetId(level) ?? source.Type;

                    // apply moss to stone blocks
                    if (morphId.UseMoss &&
                        options.EnableMoss &&
                        (World.MorphSettings.IsMoss(source.Type) ||
                         TouchingAir(location.X, location.Y)))
                    {
                        source.Type = (ushort)options.MossType;
                    }
                    else
                    {
                        source.Type = id;
                    }                    
                }

                // filter sprites
                if (options.EnableSprites &&
                    World.TileProperties[sourceId].IsFramed &&
                    morphId.SpriteOffsets.Count > 0)
                {
                    // filter and apply morph (offset or delete)
                    morphId.SpriteOffsets
                        .FirstOrDefault(uv => uv.FilterMatches(source))
                        ?.ApplyOffset(ref source);
                }
            }
        }

        public static bool AirBelow(int x, int y)
        {
            var world = ViewModelLocator.WorldViewModel.CurrentWorld;
            if (world == null) return false;

            if (y >= world.TilesHigh - 1) return false;

            Tile tile = world.Tiles[x, y + 1];
            return !tile.IsActive || notSolidTiles.Contains(tile.Type);
        }

        // copied from rendering
        const int e = 0, n = 1, w = 2, s = 3, ne = 4, nw = 5, sw = 6, se = 7;
        static Tile[] neighborTile = new Tile[8];

        public static bool TouchingAir(int x, int y)
        {
            var world = ViewModelLocator.WorldViewModel.CurrentWorld;
            if (world == null) return false;

            // copied from render code. this should probably be made a method so it can be reused
            neighborTile[e] = (x + 1) < world.TilesWide ? world.Tiles[x + 1, y] : null;
            neighborTile[n] = (y - 1) > 0 ? world.Tiles[x, y - 1] : null;
            neighborTile[w] = (x - 1) > 0 ? world.Tiles[x - 1, y] : null;
            neighborTile[s] = (y + 1) < world.TilesHigh ? world.Tiles[x, y + 1] : null;
            neighborTile[ne] = (x + 1) < world.TilesWide && (y - 1) > 0 ? world.Tiles[x + 1, y - 1] : null;
            neighborTile[nw] = (x - 1) > 0 && (y - 1) > 0 ? world.Tiles[x - 1, y - 1] : null;
            neighborTile[sw] = (x - 1) > 0 && (y + 1) < world.TilesHigh ? world.Tiles[x - 1, y + 1] : null;
            neighborTile[se] = (x + 1) < world.TilesWide && (y + 1) < world.TilesHigh ? world.Tiles[x + 1, y + 1] : null;

            // these loops are split out because checking isactive is orders of magnitude faster than checking the hashset
            // if a tile is empty, this lets the algorithm shortcut the hashcheck, making it faster
            for (int i = 0; i < neighborTile.Length; i++)
            {
                var t = neighborTile[i];
                if (t == null) { continue; }
                if (!t.IsActive) { return true; } // air
            }

            for (int i = 0; i < neighborTile.Length; i++)
            {
                var t = neighborTile[i];
                if (t == null) { continue; }
                if (notSolidTiles.Contains(t.Type)) { return true; } // non-solid
            }

            return false;
        }

        public void InitCache()
        {
            _tileCache.Clear();
            foreach (var item in MorphTiles)
            {
                foreach (var id in item.SourceIds)
                {
                    try
                    {
                        _tileCache.Add(id, item);
                    }
                    catch (Exception ex)
                    {
                        throw new IndexOutOfRangeException($"morphSetting tile entry is invalid or duplicate: {item.Name} [{id}]", ex);
                    }
                }
            }

            _wallCache.Clear();
            foreach (var item in MorphWalls)
            {
                foreach (var id in item.SourceIds)
                {
                    try
                    {
                        _wallCache.Add(id, item);
                    }
                    catch (Exception ex)
                    {
                        throw new IndexOutOfRangeException($"morphSetting wall entry is invalid or duplicate: {item.Name} [{id}]", ex);
                    }
                }
            }
        }

        private Dictionary<int, MorphId> _wallCache = new Dictionary<int, MorphId>();
        private Dictionary<int, MorphId> _tileCache = new Dictionary<int, MorphId>();
    }

    public class MorphIdLevels
    {
        public ushort? SkyId { get; set; }
        public ushort? DirtId { get; set; }
        public ushort? RockId { get; set; }
        public ushort? DeepRockId { get; set; }
        public ushort? HellId { get; set; }

        public ushort? GetId(MorphLevel level)
        {
            if (level == MorphLevel.Dirt)
            {
                return DirtId ?? SkyId;
            }

            if (level == MorphLevel.Rock)
            {
                return RockId ?? DirtId ?? SkyId;
            }

            if (level == MorphLevel.DeepRock)
            {
                return DeepRockId ?? RockId ?? DirtId ?? SkyId;
            }

            if (level == MorphLevel.Hell)
            {
                return HellId ?? DeepRockId ?? RockId ?? DirtId ?? SkyId;
            }

            // default to sky
            return SkyId;
        }
    }

    public class MorphId
    {
        public string Name { get; set; }
        public bool Delete { get; set; }

        public bool IsEvil { get; set; }
        public bool UseMoss { get; set; }

        public MorphIdLevels Default { get; set; } = new MorphIdLevels();
        public MorphIdLevels TouchingAir { get; set; }
        public MorphIdLevels Gravity { get; set; }

        public HashSet<ushort> SourceIds { get; set; } = new();
        public List<MorphSpriteUVOffset> SpriteOffsets { get; set; } = new();
    }

    public enum MorphLevel
    {
        Sky,
        Dirt,
        Rock,
        DeepRock,
        Hell
    }

    public class MorphSpriteUVOffset
    {
        public short MinU { get; set; }
        public short MaxU { get; set; }
        public short OffsetU { get; set; }
        public short MinV { get; set; }
        public short MaxV { get; set; }
        public short OffsetV { get; set; }
        public bool UseFilterV { get; set; }
        public bool Delete { get; set; }

        /// <summary>
        /// Check if a tile matches the filter
        /// </summary>
        public bool FilterMatches(Tile tile)
        {
            if (tile.U < MinU) return false;
            if (tile.U > MaxU) return false;

            if (UseFilterV)
            {
                if (tile.V < MinV) return false;
                if (tile.V > MaxV) return false;
            }

            return true;
        }

        /// <summary>
        /// This modifies the tile UV coordinates
        /// </summary>
        public void ApplyOffset(ref Tile tile)
        {
            if (Delete)
            {
                tile.Type = 0;
                tile.IsActive = false;
                return;
            }

            tile.U += OffsetU;

            if (UseFilterV)
            {
                tile.V += OffsetV;
            }
        }
    }
}
