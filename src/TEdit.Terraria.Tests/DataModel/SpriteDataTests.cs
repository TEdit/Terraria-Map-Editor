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

    [Fact(Skip = "Run manually to find overlap frames")]
    public void SpriteData_FrameDoNotOverlap()
    {
        var tileOverlaps = new Dictionary<int, List<string>>();

        foreach (var tile in WorldConfiguration.TileProperties)
        {
            if (tile.Frames == null || tile.Frames.Count == 0) continue;

            var overlaps = new List<string>();
            var rects = tile.Frames
                .Select(frame =>
                {
                    var size = frame.Size.X > 0 && frame.Size.Y > 0
                        ? frame.Size
                        : tile.GetFrameSize(frame.UV.Y);

                    var rect = new Rectangle(
                        frame.UV.X,
                        frame.UV.Y,
                        size.X * (tile.TextureGrid.X + tile.FrameGap.X),
                        size.Y * (tile.TextureGrid.Y + tile.FrameGap.Y));

                    return new { Frame = frame, Rect = rect };
                })
                .ToList();

            for (int i = 0; i < rects.Count; i++)
            {
                for (int j = i + 1; j < rects.Count; j++)
                {
                    if (rects[i].Rect.IntersectsWith(rects[j].Rect))
                    {
                        overlaps.Add($"{rects[i].Frame.Name} @ ({rects[i].Frame.UV.X},{rects[i].Frame.UV.Y}) overlaps {rects[j].Frame.Name} @ ({rects[j].Frame.UV.X},{rects[j].Frame.UV.Y})");
                    }
                }
            }

            if (overlaps.Count > 0) tileOverlaps[tile.Id] = overlaps;
        }

        if (tileOverlaps.Count > 0)
        {
            var message = string.Join(
                Environment.NewLine,
                tileOverlaps.Select(kvp => $"Tile {kvp.Key}:\n{string.Join(", ", kvp.Value)}"));

            throw new Exception($"Sprite frame overlapping:{Environment.NewLine}{message}");
        }
    }
}
