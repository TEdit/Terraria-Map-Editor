using System;
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

        /// <summary>
        /// Get a <see cref="bool"/> array of framed tiles (sprites) for saving to world header.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Throws when configuration data is not found.</exception>
        public bool[] GetTileFramesForVersion(int version)
        {
            if (SaveVersions.TryGetValue(version, out var data))
            {
                return data.GetFrames();
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(version), version, $"Error saving world version {version}: save configuration not found.");
            }
        }
    }

    public class SaveVersionData
    {
        public int SaveVersion { get; set; }
        public string GameVersion { get; set; }
        public int MaxTileId { get; set; }
        public int MaxWallId { get; set; }
        public int MaxItemId { get; set; }
        public int MaxNpcId { get; set; }
        public int MaxMoonId { get; set; }
        public HashSet<int> FramedTileIds { get; set; }

        public bool[] GetFrames()
        {
            var frames = FramedTileIds;
            var tileCount = MaxTileId;
            bool[] result = new bool[tileCount];

            for (int i = 0; i < tileCount; i++)
            {
                if (frames.Contains(i)) { result[i] = true; }
            }

            return result;
        }
    }
}
