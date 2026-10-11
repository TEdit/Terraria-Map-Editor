using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TEdit.Editor;
using TEdit.Geometry;
using TEdit.Terraria;
using Xunit;

namespace TEdit.Tests.Editor;

/// <summary>
/// Converts every furniture piece of a real world to every other set and reports what did not convert.
/// Opt-in: set TEDIT_AUDIT_WORLD to a .wld path (optionally TEDIT_AUDIT_REPORT for the report file).
/// Fails on partial or wrong conversions; lists "no equivalent" cases where the target set does have the item.
/// </summary>
public sealed class FurnitureSetWorldAudit
{
    [Fact]
    public void EveryFurniturePiece_ConvertsToEverySet()
    {
        string path = Environment.GetEnvironmentVariable("TEDIT_AUDIT_WORLD");
        if (string.IsNullOrEmpty(path))
            return; // shortcut: opt-in, needs a local world file; skipped in CI.

        var (world, error) = World.LoadWorld(path);
        Assert.Null(error);
        var converter = FurnitureSetConverter.Default;

        // One entry per sprite: its area, what it is and its tiles to restore after each conversion.
        var sprites = new List<(RectangleInt32 Area, (string Set, string Family, string Item) Item, Tile[] Tiles)>();
        var seen = new HashSet<RectangleInt32>();
        for (int x = 0; x < world.TilesWide; x++)
            for (int y = 0; y < world.TilesHigh; y++)
            {
                if (converter.GetFurniture(world.Tiles[x, y]) is not { } item)
                    continue;
                var area = SpritePlacer.GetSpriteBounds(world, x, y) ?? new RectangleInt32(x, y, 1, 1);
                if (seen.Add(area))
                    sprites.Add((area, item, Snapshot(world, area)));
            }
        Assert.NotEmpty(sprites);

        var failures = new SortedDictionary<string, int>();
        var missing = new SortedDictionary<string, int>();
        void Count(SortedDictionary<string, int> into, string key) => into[key] = into.GetValueOrDefault(key) + 1;

        foreach (var target in converter.Sets)
        {
            foreach (var (area, item, tiles) in sprites.Where(s => s.Item.Set != target))
            {
                var result = converter.Convert(world, area, null, null, target, null);
                string what = $"{item.Set} {item.Family}/{item.Item} -> {target}";

                if (result.Partial > 0)
                    Count(failures, "partial: " + what);
                else if (result.Unmatched > 0 && converter.HasItem(target, item.Family, item.Item))
                    Count(missing, what);
                else if (result.Sprites > 0 && !TilesOf(area).All(p => converter.GetFurniture(world.Tiles[p.X, p.Y])?.Set == target))
                    Count(failures, "wrong set: " + what);

                Restore(world, area, tiles);
            }
        }

        var report = new StringBuilder()
            .AppendLine($"{path}: {sprites.Count} furniture pieces, {converter.Sets.Count} sets")
            .AppendLine($"pieces by set: {string.Join(", ", sprites.GroupBy(s => s.Item.Set).Select(g => $"{g.Key} {g.Count()}"))}")
            .AppendLine().AppendLine($"FAILURES ({failures.Count})");
        foreach (var (key, n) in failures) report.AppendLine($"  {key} x{n}");
        report.AppendLine().AppendLine($"NO EQUIVALENT ALTHOUGH THE TARGET HAS THE ITEM ({missing.Count}, usually a size difference)");
        foreach (var (key, n) in missing) report.AppendLine($"  {key} x{n}");

        string reportPath = Environment.GetEnvironmentVariable("TEDIT_AUDIT_REPORT") ?? Path.Combine(Path.GetTempPath(), "furniture-set-audit.txt");
        File.WriteAllText(reportPath, report.ToString());
        Assert.True(failures.Count == 0, $"{failures.Count} failures, see {reportPath}");
    }

    private static IEnumerable<Vector2Int32> TilesOf(RectangleInt32 area)
    {
        for (int x = area.Left; x < area.Right; x++)
            for (int y = area.Top; y < area.Bottom; y++)
                yield return new Vector2Int32(x, y);
    }

    private static Tile[] Snapshot(World world, RectangleInt32 area) =>
        TilesOf(area).Select(p => world.Tiles[p.X, p.Y]).ToArray();

    private static void Restore(World world, RectangleInt32 area, Tile[] tiles)
    {
        int i = 0;
        foreach (var p in TilesOf(area))
            world.Tiles[p.X, p.Y] = tiles[i++];
    }
}
