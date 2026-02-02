using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.IO;
using System;

namespace TEdit.Configuration
{
    /// <summary>
    /// Robust version selection that supports:
    /// - Multiple Terraria game versions mapping to the SAME world SaveVersion (ex: 1.4.5.0/1.4.5.1/1.4.5.2 -> 315).
    /// - A distinct "Save As" target chosen by gameVersion, even when the saveVersion is shared.
    /// 
    /// Indexes:
    /// - _byGameVersion:       Exact lookup for Save-As target data.
    /// - _latestBySaveVersion: Fallback for opening worlds by saveVersion (use newest gameVersion for that saveVersion).
    /// - _allBySaveVersion:    All variants grouped by saveVersion.
    /// </summary>
    public class SaveVersionManager
    {
        #region Raw JSON Fields

        /// <summary>
        /// Maps Terraria gameVersion (ex: "1.4.5.2") -> world saveVersion (ex: 315).
        /// This is used as a fallback, not the primary selection mechanism for Save-As.
        /// </summary>
        [JsonProperty("gameVersionToSaveVersion")]
        public Dictionary<string, uint> GameVersionToSaveVersion { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Option B: Store save-version records as a LIST (array in JSON) so duplicates are allowed.
        /// Each entry includes saveVersion + gameVersion and any constraints/framed tiles.
        /// </summary>
        [JsonProperty("saveVersions")]
        public List<SaveVersionData> SaveVersions { get; set; } = [];

        #endregion

        #region Built Indexes (Not Serialized)

        [JsonIgnore] private Dictionary<string, SaveVersionData> _byGameVersion;
        [JsonIgnore] private Dictionary<int, SaveVersionData> _latestBySaveVersion;
        [JsonIgnore] private Dictionary<int, List<SaveVersionData>> _allBySaveVersion;

        #endregion

        #region Public API

        /// <summary>
        /// Returns the max saveVersion number that exists (based on built index).
        /// </summary>
        public int GetMaxVersion()
        {
            EnsureIndexes();
            return _latestBySaveVersion.Keys.Max();
        }

        /// <summary>
        /// Get a record by world saveVersion (used when opening/reading a world header).
        /// If there are multiple entries with the same saveVersion, we return the newest gameVersion for that saveVersion.
        /// </summary>
        public SaveVersionData GetData(uint version) => GetData((int)version);

        /// <summary>
        /// Get a record by world saveVersion (used when opening/reading a world header).
        /// If version is > max known, clamp to max.
        /// </summary>
        public SaveVersionData GetData(int version)
        {
            EnsureIndexes();

            int useVersion = (version > GetMaxVersion()) ? GetMaxVersion() : version;

            if (_latestBySaveVersion.TryGetValue(useVersion, out var data))
                return data;

            throw new ArgumentOutOfRangeException(nameof(version), $"Missing settings for world file version: {version}");
        }

        /// <summary>
        /// Get a record for a specific Terraria gameVersion (used for Save-As selection).
        ///
        /// Behavior:
        /// - Exact match on gameVersion if present.
        /// - Else, if gameVersion maps to a saveVersion, choose the closest <= within that saveVersion group.
        /// - Else, choose the closest <= overall, or newest overall if nothing matches.
        /// </summary>
        public SaveVersionData GetDataForGameVersion(string gameVersion)
        {
            EnsureIndexes();

            string key = NormalizeGameVersion(gameVersion);

            // 1) Exact match (best / intended for Save-As)
            if (_byGameVersion.TryGetValue(key, out var exact))
                return exact;

            // 2) If we know what saveVersion this game version uses, try to pick a sensible variant from that bucket.
            if (GameVersionToSaveVersion != null &&
                GameVersionToSaveVersion.TryGetValue(key, out uint sv))
            {
                int saveVersion = (int)sv;

                if (_allBySaveVersion.TryGetValue(saveVersion, out var bucket) && bucket.Count > 0)
                {
                    Version target = ParseGameVersion(key);

                    // Choose the closest gameVersion <= target within the same saveVersion bucket.
                    // If none are <=, fall back to newest in the bucket.
                    var best = bucket
                        .OrderBy(d => ParseGameVersion(d.GameVersion))
                        .LastOrDefault(d => ParseGameVersion(d.GameVersion) <= target);

                    return best ?? bucket.OrderBy(d => ParseGameVersion(d.GameVersion)).Last();
                }

                // If bucket missing for some reason, fallback to saveVersion lookup.
                return GetData(saveVersion);
            }

            // 3) Final fallback: find closest <= overall
            Version overallTarget = ParseGameVersion(key);

            var overallBest = SaveVersions
                .OrderBy(d => ParseGameVersion(d.GameVersion))
                .LastOrDefault(d => ParseGameVersion(d.GameVersion) <= overallTarget);

            if (overallBest != null)
                return overallBest;

            // 4) If parse failed or everything is newer, return newest overall.
            var newest = SaveVersions
                .OrderBy(d => ParseGameVersion(d.GameVersion))
                .LastOrDefault();

            if (newest != null)
                return newest;

            throw new InvalidOperationException("Save configuration contains no saveVersions entries.");
        }

        /// <summary>
        /// Get framed tile flags for a world saveVersion (saving to world header).
        /// </summary>
        public bool[] GetTileFramesForVersion(int version)
        {
            EnsureIndexes();

            if (_latestBySaveVersion.TryGetValue(version, out var data))
                return data.GetFrames();

            throw new ArgumentOutOfRangeException(nameof(version), version, $"Error saving world version {version}: save configuration not found.");
        }

        /// <summary>
        /// Optionally: Get framed tile flags for a specific Terraria gameVersion (Save-As rules).
        /// </summary>
        public bool[] GetTileFramesForGameVersion(string gameVersion)
        {
            var data = GetDataForGameVersion(gameVersion);
            return data.GetFrames();
        }
        #endregion

        #region Loading

        public static SaveVersionManager LoadJson(string fileName)
        {
            using StreamReader file = File.OpenText(fileName);
            using JsonTextReader reader = new(file);
            JsonSerializer serializer = new();
            var mgr = serializer.Deserialize<SaveVersionManager>(reader) ?? throw new InvalidOperationException($"Failed to deserialize save version configuration: {fileName}.");
            mgr.BuildIndexes();
            return mgr;
        }
        #endregion

        #region Index Building / Helpers

        /// <summary>
        /// Builds internal lookup tables.
        /// Call after load; safe to call multiple times.
        /// </summary>
        public void BuildIndexes()
        {
            SaveVersions ??= [];

            // Normalize and index by gameVersion
            _byGameVersion = new Dictionary<string, SaveVersionData>(StringComparer.OrdinalIgnoreCase);

            foreach (var d in SaveVersions)
            {
                if (d == null) continue;

                // Ensure we can parse / compare versions consistently.
                string gv = NormalizeGameVersion(d.GameVersion);

                // If there are duplicates by gameVersion, last wins intentionally (shouldn't happen).
                _byGameVersion[gv] = d;
            }

            // Group all variants by saveVersion
            _allBySaveVersion = SaveVersions
                .Where(d => d != null)
                .GroupBy(d => d.SaveVersion)
                .ToDictionary(g => g.Key, g => g.ToList());

            // For each saveVersion bucket, pick the newest gameVersion as the default for opening worlds.
            _latestBySaveVersion = [];

            foreach (var kv in _allBySaveVersion)
            {
                int sv = kv.Key;
                var bucket = kv.Value;

                if (bucket == null || bucket.Count == 0)
                    continue;

                var newest = bucket
                    .OrderBy(d => ParseGameVersion(d.GameVersion))
                    .Last();

                _latestBySaveVersion[sv] = newest;
            }
        }

        private void EnsureIndexes()
        {
            if (_latestBySaveVersion == null || _byGameVersion == null || _allBySaveVersion == null)
                BuildIndexes();
        }

        /// <summary>
        /// Normalizes a gameVersion string:
        /// - trims whitespace
        /// - removes leading 'v'
        /// Result examples: "1.4.5.2"
        /// </summary>
        private static string NormalizeGameVersion(string v)
        {
            if (string.IsNullOrWhiteSpace(v))
                return string.Empty;

            v = v.Trim();

            if (v.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                v = v.Substring(1);

            return v;
        }

        /// <summary>
        /// Parses normalized gameVersion into System.Version for ordering.
        /// Unknown/invalid versions become 0.0.
        /// </summary>
        private static Version ParseGameVersion(string v)
        {
            v = NormalizeGameVersion(v);

            if (Version.TryParse(v, out var ver))
                return ver;

            return new Version(0, 0);
        }
        #endregion
    }
}
