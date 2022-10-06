using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TEdit.Terraria;

namespace TEdit.Configuration
{
    public class MorphConfiguration
    {
        public Dictionary<string, MorphBiomeData> Biomes { get; set; } = new();

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
        }
    }

    public class MorphBiomeData
    {
        public string Name { get; set; }
        public List<MorphId> MorphTiles { get; set; } = new();
        public List<MorphId> MorphWalls { get; set; } = new();

        public bool ContainsWall(int id) => _wallCache.ContainsKey(id);
        public bool ContainsTile(int id) => _tileCache.ContainsKey(id);

        public void ApplyMorph(Tile source, MorphLevel level)
        {
            ApplyTileMorph(source, level);
            ApplyWallMorph(source, level);
        }

        private void ApplyWallMorph(Tile source, MorphLevel level)
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

                source.Wall = level switch
                {
                    MorphLevel.Sky => morphId.SkyLevelTargetId,
                    MorphLevel.Dirt => morphId.DirtLevelTargetId,
                    MorphLevel.Rock => morphId.RockLevelTargetId,
                    _ => source.Wall,
                };
            }
        }

        private void ApplyTileMorph(Tile source, MorphLevel level)
        {
            if (!source.IsActive) { return; }
            ushort sourceId = source.Type;
            
            if (!_tileCache.TryGetValue(sourceId, out var morphId)) { return; }

            if (morphId.SourceIds.Contains(sourceId))
            {
                if (morphId.Delete)
                {
                    source.Type = 0;
                    source.IsActive = false;
                    return;
                }

                source.Type = level switch
                {
                    MorphLevel.Sky =>  morphId.SkyLevelTargetId,
                    MorphLevel.Dirt => morphId.DirtLevelTargetId,
                    MorphLevel.Rock => morphId.RockLevelTargetId,
                    _ => source.Type,
                };


                // hopefully this works...kinda hacky and probably slooow
                if (World.TileProperties[sourceId].IsFramed &&
                    morphId.SpriteOffsets.Count > 0)
                {
                    var offset = morphId.SpriteOffsets.FirstOrDefault(uv => uv.MinU <= source.U && source.U <= uv.MaxU);
                    if (offset != null)
                    {
                        source.U += offset.OffsetU;
                    }
                }
            }
        }

        public void InitCache()
        {
            _tileCache.Clear();
            foreach (var item in MorphTiles)
            {
                foreach (var id in item.SourceIds)
                {
                    _tileCache.Add(id, item);
                }
            }

            _wallCache.Clear();
            foreach (var item in MorphWalls)
            {
                foreach (var id in item.SourceIds)
                {
                    _wallCache.Add(id, item);
                }
            }
        }

        private Dictionary<int, MorphId> _wallCache   = new Dictionary<int, MorphId>();
        private Dictionary<int, MorphId> _tileCache   = new Dictionary<int, MorphId>();
    }

    public class MorphId
    {
        public string Name { get; set; }
        public bool Delete { get; set; }
        public ushort SkyLevelTargetId { get; set; }
        public ushort DirtLevelTargetId { get; set; }
        public ushort RockLevelTargetId { get; set; }
        public HashSet<ushort> SourceIds { get; set; } = new();
        public List<MorphSpriteUVOffset> SpriteOffsets { get; set; } = new();
    }

    public enum MorphLevel
    {
        Sky,
        Dirt,
        Rock
    }

    public class MorphSpriteUVOffset
    {
        public short MinU { get; set; }
        public short MaxU { get; set; }
        public short OffsetU { get; set; }
    }
}
