using System.Collections.Generic;
using System.Linq;

namespace TEdit.Configuration
{
    public class BestiaryData
    {
        public List<string> BestiaryTalkedIDs { get; set; } = new List<string>();
        public List<string> BestiaryNearIDs { get; set; } = new List<string>();
        public List<string> BestiaryKilledIDs { get; set; } = new List<string>();
    }

    public class SaveConfiguration
    {
        public Dictionary<string, uint> GameVersionToSaveVersion { get; set; }
        public Dictionary<int, SaveVersionData> SaveVersions { get; set; }
    }

    public class SaveVersionData
    {
        public int SaveVersion { get; set; }
        public string GameVersion { get; set; }
        public int MaxTileId { get; set; }
        public int MaxWallId { get; set; }
        public int MaxItemId { get; set; }
        public int MaxMoonId { get; set; }
        public HashSet<int> FramedTileIds { get; set; }
        public bool[] GetFrames()
        {
            var max = FramedTileIds.Max();
            var frames = new bool[max + 1];

            for (int i = 0; i <= max; i++)
            {
                frames[i] = FramedTileIds.Contains(i);
            }

            return frames;
        }
    }
}
