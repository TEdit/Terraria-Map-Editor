using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using TEdit.Common.Serialization;

namespace TEdit.Terraria.DataModel;

public class SaveVersionManager
{
    public Dictionary<string, uint> GameVersionToSaveVersion { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<SaveVersionData> SaveVersions { get; set; } = [];

    [JsonIgnore] private Dictionary<string, SaveVersionData>? _byGameVersion;
    [JsonIgnore] private Dictionary<int, SaveVersionData>? _latestBySaveVersion;
    [JsonIgnore] private Dictionary<int, List<SaveVersionData>>? _allBySaveVersion;

    public int GetMaxVersion()
    {
        EnsureIndexes();
        return _latestBySaveVersion!.Keys.Max();
    }

    public SaveVersionData GetData(uint version) => GetData((int)version);

    public SaveVersionData GetData(int version)
    {
        EnsureIndexes();

        int useVersion = (version > GetMaxVersion()) ? GetMaxVersion() : version;

        if (_latestBySaveVersion!.TryGetValue(useVersion, out var data))
            return data;

        throw new ArgumentOutOfRangeException(nameof(version), $"Missing settings for world file version: {version}");
    }

    public SaveVersionData GetDataForGameVersion(string gameVersion)
    {
        EnsureIndexes();

        string key = NormalizeGameVersion(gameVersion);

        if (_byGameVersion!.TryGetValue(key, out var exact))
            return exact;

        if (GameVersionToSaveVersion.TryGetValue(key, out uint sv))
        {
            int saveVersion = (int)sv;

            if (_allBySaveVersion!.TryGetValue(saveVersion, out var bucket) && bucket.Count > 0)
            {
                Version target = ParseGameVersion(key);

                var best = bucket
                    .OrderBy(d => ParseGameVersion(d.GameVersion))
                    .LastOrDefault(d => ParseGameVersion(d.GameVersion) <= target);

                return best ?? bucket.OrderBy(d => ParseGameVersion(d.GameVersion)).Last();
            }

            return GetData(saveVersion);
        }

        Version overallTarget = ParseGameVersion(key);

        var overallBest = SaveVersions
            .OrderBy(d => ParseGameVersion(d.GameVersion))
            .LastOrDefault(d => ParseGameVersion(d.GameVersion) <= overallTarget);

        if (overallBest != null)
            return overallBest;

        var newest = SaveVersions
            .OrderBy(d => ParseGameVersion(d.GameVersion))
            .LastOrDefault();

        if (newest != null)
            return newest;

        throw new InvalidOperationException("Save configuration contains no saveVersions entries.");
    }

    public bool[] GetTileFramesForVersion(int version)
    {
        EnsureIndexes();

        if (_latestBySaveVersion!.TryGetValue(version, out var data))
            return data.GetFrames();

        throw new ArgumentOutOfRangeException(nameof(version), version, $"Error saving world version {version}: save configuration not found.");
    }

    public bool[] GetTileFramesForGameVersion(string gameVersion)
    {
        var data = GetDataForGameVersion(gameVersion);
        return data.GetFrames();
    }

    public static SaveVersionManager Load(Stream stream)
    {
        var mgr = JsonSerializer.Deserialize<SaveVersionManager>(stream, TEditJsonSerializer.DefaultOptions)
            ?? throw new InvalidOperationException("Failed to deserialize save version configuration.");
        mgr.BuildIndexes();
        return mgr;
    }

    public static SaveVersionManager LoadFile(string fileName)
    {
        using var stream = File.OpenRead(fileName);
        return Load(stream);
    }

    public void BuildIndexes()
    {
        SaveVersions ??= [];

        _byGameVersion = new Dictionary<string, SaveVersionData>(StringComparer.OrdinalIgnoreCase);

        foreach (var d in SaveVersions)
        {
            if (d == null) continue;
            string gv = NormalizeGameVersion(d.GameVersion);
            _byGameVersion[gv] = d;
        }

        _allBySaveVersion = SaveVersions
            .Where(d => d != null)
            .GroupBy(d => d.SaveVersion)
            .ToDictionary(g => g.Key, g => g.ToList());

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

    private static string NormalizeGameVersion(string v)
    {
        if (string.IsNullOrWhiteSpace(v))
            return string.Empty;

        v = v.Trim();

        if (v.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            v = v.Substring(1);

        return v;
    }

    private static Version ParseGameVersion(string v)
    {
        v = NormalizeGameVersion(v);

        if (Version.TryParse(v, out var ver))
            return ver;

        return new Version(0, 0);
    }
}
