using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Shouldly;
using TEdit.Geometry;
using TEdit.Terraria.Objects;

namespace TEdit.Terraria.Tests.DataModel;

/// <summary>
/// Tests to verify sprite/frame data is being loaded correctly from tiles.json.
/// </summary>
[Collection("SharedState")]
public class SpriteDataTests : IDisposable
{
    public SpriteDataTests()
    {
        WorldConfiguration.Reset();
        TerrariaDataStore.Reset();
        WorldConfiguration.Initialize();
    }

    public void Dispose()
    {
        WorldConfiguration.Reset();
        TerrariaDataStore.Reset();
    }

    [Fact]
    public void TileProperties_LoadedFromJson()
    {
        WorldConfiguration.TileProperties.Count.ShouldBeGreaterThan(0);
    }

    [Theory]
    [InlineData(3, "Forest Short Plants", true)]  // Short plants - framed
    [InlineData(4, "Torches", true)]              // Torches - framed
    [InlineData(10, "Doors (Closed)", true)]      // Closed doors - framed
    [InlineData(11, "Doors (Open)", true)]        // Open doors - framed
    [InlineData(87, "Pianos", true)]              // Pianos - framed with texture wrap
    [InlineData(93, "Lamps", true)]               // Lamps - framed with texture wrap (V-axis)
    public void TileProperty_HasCorrectIsFramed(int tileId, string expectedName, bool expectedIsFramed)
    {
        var tile = WorldConfiguration.TileProperties[tileId];

        tile.Name.ShouldBe(expectedName);
        tile.IsFramed.ShouldBe(expectedIsFramed);
    }

    [Theory]
    [InlineData(3)]   // Grass
    [InlineData(4)]   // Torches
    [InlineData(11)]  // Doors
    [InlineData(87)]  // Pianos
    [InlineData(93)]  // Lamps
    [InlineData(101)] // Bookcases
    public void FramedTile_HasFrames(int tileId)
    {
        var tile = WorldConfiguration.TileProperties[tileId];

        tile.IsFramed.ShouldBeTrue($"Tile {tileId} should be framed");
        tile.Frames.ShouldNotBeNull($"Tile {tileId} Frames should not be null");
        tile.Frames.Count.ShouldBeGreaterThan(0, $"Tile {tileId} should have at least one frame");
    }

    [Theory]
    [InlineData(3)]   // Grass
    [InlineData(4)]   // Torches
    [InlineData(11)]  // Doors
    [InlineData(87)]  // Pianos
    [InlineData(93)]  // Lamps
    public void FramedTile_HasFrameSize(int tileId)
    {
        var tile = WorldConfiguration.TileProperties[tileId];

        tile.IsFramed.ShouldBeTrue();
        tile.FrameSize.ShouldNotBeNull();
        tile.FrameSize.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Piano_HasCorrectFrameData()
    {
        var tile = WorldConfiguration.TileProperties[87];

        tile.Name.ShouldBe("Pianos");
        tile.IsFramed.ShouldBeTrue();
        tile.FrameSize.ShouldNotBeNull();
        tile.FrameSize[0].X.ShouldBe((short)3); // 3 tiles wide
        tile.FrameSize[0].Y.ShouldBe((short)2); // 2 tiles tall

        // Verify frames are loaded
        tile.Frames.ShouldNotBeNull();
        tile.Frames.Count.ShouldBeGreaterThan(0);

        // First frame should be Wooden Piano at UV 0,0
        var firstFrame = tile.Frames[0];
        firstFrame.Name.ShouldBe("Wooden Piano");
        firstFrame.UV.X.ShouldBe((short)0);
        firstFrame.UV.Y.ShouldBe((short)0);
    }

    [Fact]
    public void Torch_HasCorrectFrameData()
    {
        var tile = WorldConfiguration.TileProperties[4];

        tile.Name.ShouldBe("Torches");
        tile.IsFramed.ShouldBeTrue();
        tile.Frames.ShouldNotBeNull();
        tile.Frames.Count.ShouldBeGreaterThan(0);

        // First frame should exist
        var firstFrame = tile.Frames[0];
        firstFrame.ShouldNotBeNull();
    }

    [Fact]
    public void Door_HasCorrectFrameSize()
    {
        var tile = WorldConfiguration.TileProperties[11];

        tile.Name.ShouldBe("Doors (Open)");
        tile.IsFramed.ShouldBeTrue();
        tile.FrameSize.ShouldNotBeNull();
        // Doors are 2 tiles wide, 3 tiles tall
        tile.FrameSize[0].X.ShouldBe((short)2);
        tile.FrameSize[0].Y.ShouldBe((short)3);
    }

    [Fact]
    public void AllFramedTiles_HaveNonNullFrames()
    {
        var framedTilesWithNullFrames = new List<int>();

        foreach (var tile in WorldConfiguration.TileProperties)
        {
            if (tile.IsFramed && tile.Frames == null)
            {
                framedTilesWithNullFrames.Add(tile.Id);
            }
        }

        framedTilesWithNullFrames.ShouldBeEmpty(
            $"The following framed tiles have null Frames: {string.Join(", ", framedTilesWithNullFrames)}");
    }

    [Fact]
    public void AllFramedTiles_HaveNonNullFrameSize()
    {
        var framedTilesWithNullFrameSize = new List<int>();

        foreach (var tile in WorldConfiguration.TileProperties)
        {
            if (tile.IsFramed && tile.FrameSize == null)
            {
                framedTilesWithNullFrameSize.Add(tile.Id);
            }
        }

        framedTilesWithNullFrameSize.ShouldBeEmpty(
            $"The following framed tiles have null FrameSize: {string.Join(", ", framedTilesWithNullFrameSize)}");
    }

    [Fact]
    public void TextureGrid_HasDefaultValues()
    {
        // Verify default texture grid values
        var tile = WorldConfiguration.TileProperties[87]; // Piano

        tile.TextureGrid.X.ShouldBe((short)16);
        tile.TextureGrid.Y.ShouldBe((short)16);
    }

    [Fact]
    public void FrameGap_HasDefaultValues()
    {
        // Verify default frame gap values
        var tile = WorldConfiguration.TileProperties[87]; // Piano

        tile.FrameGap.X.ShouldBe((short)2);
        tile.FrameGap.Y.ShouldBe((short)2);
    }

    [Fact]
    public void TerrariaDataStore_LoadsTiles()
    {
        var store = TerrariaDataStore.Instance;
        store.ShouldNotBeNull();
        store.Tiles.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void TerrariaDataStore_TilesMatchWorldConfiguration()
    {
        var store = TerrariaDataStore.Instance;

        // Both should have the same number of tiles
        store.Tiles.Count.ShouldBe(WorldConfiguration.TileProperties.Count);

        // Spot check a few tiles
        store.Tiles[0].Name.ShouldBe(WorldConfiguration.TileProperties[0].Name);
        store.Tiles[87].Name.ShouldBe(WorldConfiguration.TileProperties[87].Name);
    }

    [Fact]
    public void SpriteData_FrameVerify()
    {
        var overlaps = new Dictionary<int, HashSet<string>>();
        var outOfBounds = new Dictionary<int, HashSet<string>>();
        var duplicates = new Dictionary<int, HashSet<string>>();
        var noNames = new Dictionary<int, HashSet<string>>();

        foreach (var tile in WorldConfiguration.TileProperties)
        {
            if (tile.Frames == null || tile.Frames.Count == 0 || tile.Id == 617) continue;

            var path = Path.Combine(Directory.GetCurrentDirectory(), "Images", $"Tiles_{tile.Id}.png");
            if (!File.Exists(path)) continue;
            using var img = Image.FromFile(path);
            var texture = new Vector2Short((short)img.Width, (short)img.Height);

            var overlapsIssues = new HashSet<string>();
            var outOfBoundsIssues = new HashSet<string>();
            var duplicatesIssues = new HashSet<string>();

            var rects = tile.Frames
                .Select(frame =>
                {
                    var renderUv = TileProperty.GetRenderUV((ushort)tile.Id, frame.UV.X, frame.UV.Y);
                    var size = frame.Size.X > 0 && frame.Size.Y > 0
                        ? frame.Size
                        : tile.GetFrameSize(renderUv.Y);

                    var rect = new Rectangle(
                        renderUv.X,
                        renderUv.Y,
                        size.X * (tile.TextureGrid.X + tile.FrameGap.X),
                        size.Y * (tile.TextureGrid.Y + tile.FrameGap.Y));

                    return new { Frame = frame, Rect = rect, RenderUv = renderUv };
                })
                .ToList();

            // duplicates
            foreach (var group in tile.Frames.GroupBy(f => new { f.Name, f.Variety, f.Anchor }))
            {
                if (group.Count() > 1)
                {
                    duplicatesIssues.Add($"{group.Key.Name} @ {group.Key.Variety} [{group.Key.Anchor}]");
                }
            }

            for (int i = 0; i < rects.Count; i++)
            {
                var current = rects[i];
                var rect = current.Rect;
                var frame = current.Frame;
                var renderUv = current.RenderUv;

                // out of bounds
                if (rect.Left < 0 || rect.Top < 0 ||
                    rect.Right > texture.Width + tile.TextureGrid.X ||
                    rect.Bottom > texture.Height + tile.TextureGrid.Y)
                {
                    outOfBoundsIssues.Add($"{frame.Name} @ ({frame.UV.X},{frame.UV.Y}) out of bounds ({renderUv.X},{renderUv.Y})");
                }

                // overlaps
                for (int j = i + 1; j < rects.Count; j++)
                {
                    if (rect.IntersectsWith(rects[j].Rect))
                    {
                        overlapsIssues.Add($"{frame.Name} @ ({frame.UV.X},{frame.UV.Y}) overlaps {rects[j].Frame.Name} @ ({rects[j].Frame.UV.X},{rects[j].Frame.UV.Y})");
                    }
                }
            }

            if (overlapsIssues.Count > 0) overlaps[tile.Id] = overlapsIssues;
            if (outOfBoundsIssues.Count > 0) outOfBounds[tile.Id] = outOfBoundsIssues;
            if (duplicatesIssues.Count > 0) duplicates[tile.Id] = duplicatesIssues;
        }

        if (overlaps.Count > 0 || outOfBounds.Count > 0 || duplicates.Count > 0)
        {
            var sections = new List<string>();
            if (overlaps.Count > 0)
            {
                sections.Add("Overlaps:");
                sections.Add(string.Join(
                    Environment.NewLine,
                    overlaps.Select(kvp => $"Tile {kvp.Key}:\n{string.Join(", ", kvp.Value)}")));
            }
            if (outOfBounds.Count > 0)
            {
                sections.Add("Out of bounds:");
                sections.Add(string.Join(
                    Environment.NewLine,
                    outOfBounds.Select(kvp => $"Tile {kvp.Key}:\n{string.Join(", ", kvp.Value)}")));
            }
            if (duplicates.Count > 0)
            {
                sections.Add("Duplicates:");
                sections.Add(string.Join(
                    Environment.NewLine,
                    duplicates.Select(kvp => $"Tile {kvp.Key}:\n{string.Join(", ", kvp.Value)}")));
            }

            throw new Exception(string.Join(Environment.NewLine, sections));
        }
    }
}
