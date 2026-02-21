using System.Drawing;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
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

    [Fact(Skip = "Run manually to verify sprite data")]
    public void SpriteData_FrameVerify()
    {
        var overlaps = new Dictionary<int, HashSet<string>>();
        var outOfBounds = new Dictionary<int, HashSet<string>>();
        var duplicates = new Dictionary<int, HashSet<string>>();
        var noNames = new Dictionary<int, HashSet<string>>();

        foreach (var tile in WorldConfiguration.TileProperties)
        {
            if (tile.Frames == null || tile.Frames.Count == 0) continue;

            var path = Path.Combine(Directory.GetCurrentDirectory(), "Images", $"Tiles_{tile.Id}.png");
            if (!File.Exists(path)) continue;
            using var img = Image.FromFile(path);
            using var bitmap = new Bitmap(img);
            var texSize = new Vector2Short((short)img.Width, (short)img.Height);

            var overlapsIssues = new HashSet<string>();
            var outOfBoundsIssues = new HashSet<string>();
            var duplicatesIssues = new HashSet<string>();
            var noNamesIssues = new HashSet<string>();

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

                    return new { Frame = frame, Rect = rect, RenderUV = renderUv };
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
                var renderUv = current.RenderUV;

                // out of bounds
                if (rect.Left < 0 || rect.Top < 0 ||
                    rect.Right > texSize.Width + tile.TextureGrid.X ||
                    rect.Bottom > texSize.Height + tile.TextureGrid.Y)
                {
                    if (tile.Id != 617)
                    {
                        outOfBoundsIssues.Add($"{frame.Name} @ ({frame.UV.X},{frame.UV.Y}) out of bounds ({renderUv.X},{renderUv.Y})");
                    }
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

            // no names
            var renderUVSet = new HashSet<Vector2Short>(rects.Select(r => r.RenderUV));
            if (!tile.IsAnimated && tile.FrameSize != null && tile.FrameSize.Length > 0)
            {
                var baseStep = tile.FrameSize[0] * (tile.TextureGrid + tile.FrameGap);

                if (baseStep.X > 0 && baseStep.Y > 0)
                {
                    for (int pY = 0; pY < texSize.Height;)
                    {
                        var rowSize = tile.GetFrameSize((short)pY);
                        var rowStep = rowSize * (tile.TextureGrid + tile.FrameGap);
                        var stepY = rowStep.Y > 0 ? rowStep.Y : baseStep.Y;
                        var stepX = rowStep.X > 0 ? rowStep.X : baseStep.X;

                        for (int pX = 0; pX < texSize.Width;)
                        {
                            var maxX = Math.Min(pX + stepX, texSize.Width);
                            var maxY = Math.Min(pY + stepY, texSize.Height);
                            var hasColor = false;

                            for (int x = pX; x < maxX && !hasColor; x++)
                            {
                                for (int y = pY; y < maxY; y++)
                                {
                                    if (bitmap.GetPixel(x, y).A > 0)
                                    {
                                        hasColor = true;
                                        break;
                                    }
                                }
                            }

                            if (hasColor)
                            {
                                var renderUV = new Vector2Short((short)pX, (short)pY);
                                if (!renderUVSet.Contains(renderUV))
                                {
                                    // noNamesIssues.Add($"{renderUV.X},{renderUV.Y}");
                                }
                            }

                            pX += stepX;
                        }
                        pY += stepY;
                    }
                }
            }

            if (overlapsIssues.Count > 0) overlaps[tile.Id] = overlapsIssues;
            if (outOfBoundsIssues.Count > 0) outOfBounds[tile.Id] = outOfBoundsIssues;
            if (duplicatesIssues.Count > 0) duplicates[tile.Id] = duplicatesIssues;
            if (noNamesIssues.Count > 0) noNames[tile.Id] = noNamesIssues;
        }

        if (overlaps.Count > 0 || outOfBounds.Count > 0 || duplicates.Count > 0 || noNames.Count > 0)
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
            if (noNames.Count > 0)
            {
                sections.Add("No names:");
                sections.Add(string.Join(
                    Environment.NewLine,
                    noNames.Select(kvp => $"Tile {kvp.Key}:\n{string.Join(", ", kvp.Value)}")));
            }

            throw new Exception(string.Join(Environment.NewLine, sections));
        }
    }

    private static string GetRepoRoot()
    {
        var dir = Directory.GetCurrentDirectory();
        while (dir != null && !File.Exists(Path.Combine(dir, "TEdit.slnx")) && !File.Exists(Path.Combine(dir, "src", "TEdit.slnx")))
        {
            dir = Directory.GetParent(dir)?.FullName;
        }
        if (dir != null && File.Exists(Path.Combine(dir, "src", "TEdit.slnx")))
            return dir;
        if (dir != null && File.Exists(Path.Combine(dir, "TEdit.slnx")))
            return Directory.GetParent(dir)?.FullName ?? dir;
        throw new InvalidOperationException("Could not find repository root");
    }

    private static string CompactNumericArrays(string json)
    {
        // Compact arrays containing only numbers (any length)
        json = Regex.Replace(json, @"\[\s*(-?\d+(?:\s*,\s*-?\d+)*)\s*\]",
            m => "[" + Regex.Replace(m.Groups[1].Value, @"\s+", " ").Trim() + "]");

        // Compact arrays of 2-element arrays: [[1, 2], [3, 4]]
        json = Regex.Replace(json, @"\[\s*(\[-?\d+, -?\d+\](?:\s*,\s*\[-?\d+, -?\d+\])*)\s*\]",
            m => "[" + Regex.Replace(m.Groups[1].Value, @"\s+", " ").Trim() + "]");

        // Compact small objects with 2-4 properties containing only simple values
        // Match: { "key": value, "key2": value2 } where values are strings, numbers, or compact arrays
        json = Regex.Replace(json,
            @"\{\s*\n\s*(""[^""]+"":\s*(?:""[^""]*""|-?\d+(?:\.\d+)?|\[[^\[\]]*\]|true|false|null))" +
            @"(?:,\s*\n\s*(""[^""]+"":\s*(?:""[^""]*""|-?\d+(?:\.\d+)?|\[[^\[\]]*\]|true|false|null))){1,3}" +
            @"\s*\n\s*\}",
            m =>
            {
                // Extract all the property matches and join them
                var content = m.Value;
                // Remove newlines and excess whitespace, keeping structure
                var compact = Regex.Replace(content, @"\s*\n\s*", " ");
                compact = Regex.Replace(compact, @"\{\s+", "{ ");
                compact = Regex.Replace(compact, @"\s+\}", " }");
                compact = Regex.Replace(compact, @",\s+", ", ");
                return compact;
            });

        return json;
    }

    [Theory]
    [InlineData(93, 0, 0, "Tiki Torch: On")]           // Non-wrapped lamp, first tile
    [InlineData(93, 0, 18, "Tiki Torch: On")]           // Non-wrapped lamp, second tile
    [InlineData(93, 0, 36, "Tiki Torch: On")]           // Non-wrapped lamp, third tile
    [InlineData(93, 0, 2052, "Sandstone Lamp: On")]     // Wrapped lamp (V > 1998), first tile
    [InlineData(93, 0, 2070, "Sandstone Lamp: On")]     // Wrapped lamp, second tile
    [InlineData(93, 0, 2088, "Sandstone Lamp: On")]     // Wrapped lamp, third tile
    [InlineData(93, 18, 2052, "Sandstone Lamp: Off")]   // Wrapped lamp off variant, first tile
    [InlineData(93, 18, 2070, "Sandstone Lamp: Off")]   // Wrapped lamp off variant, second tile
    [InlineData(93, 18, 2088, "Sandstone Lamp: Off")]   // Wrapped lamp off variant, third tile
    public void GetStyleFromUV_AllTilesInSprite_ReturnSameStyle(int tileId, short u, short v, string expectedName)
    {
        // Build sprite sheet from config (same as WorldViewModel.BuildSpritesFromConfig)
        var tile = WorldConfiguration.TileProperties[tileId];
        var sprite = new SpriteSheet
        {
            Tile = (ushort)tile.Id,
            Name = tile.Name,
            SizeTiles = tile.FrameSize,
            SizePixelsRender = tile.TextureGrid,
            SizePixelsInterval = tile.TextureGrid + tile.FrameGap,
            IsAnimated = tile.IsAnimated
        };

        int styleIndex = 0;
        foreach (var frame in tile.Frames)
        {
            sprite.Styles.Add(new SpriteItem
            {
                Tile = sprite.Tile,
                Style = styleIndex++,
                Name = frame.ToString(),
                UV = frame.UV,
                SizeTiles = frame.Size.X > 0 && frame.Size.Y > 0 ? frame.Size : tile.FrameSize[0],
                SizePixelsInterval = sprite.SizePixelsInterval,
                Anchor = frame.Anchor,
            });
        }

        var result = sprite.GetStyleFromUV(new Vector2Short(u, v));

        result.ShouldNotBeNull($"No style found for UV ({u}, {v})");
        result.Name.ShouldBe(expectedName);
    }

    [Fact(Skip = "Run manually to update textureWrap")]
    public void UpdateTextureWrap()
    {
        var repoRoot = GetRepoRoot();
        var tilesJsonPath = Path.Combine(repoRoot, "src", "TEdit.Terraria", "Data", "tiles.json");
        var tileProps = JsonNode.Parse(File.ReadAllText(tilesJsonPath))!.AsArray();
        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

        foreach (var tileNode in tileProps)
        {
            var obj = tileNode!.AsObject();
            var isFramed = obj["isFramed"]?.GetValue<bool>() ?? false;
            if (!isFramed) continue;

            var id = obj["id"]?.GetValue<int>() ?? 0;
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Images", $"Tiles_{id}.png");
            if (!File.Exists(path)) continue;
            using var img = Image.FromFile(path);
            var texture = new Vector2Short((short)img.Width, (short)img.Height);

            var frames = obj["frames"]?.AsArray();
            if (frames == null || frames.Count == 0) continue;

            int maxU = 0;
            int maxV = 0;
            foreach (var frameNode in frames)
            {
                var fobj = frameNode!.AsObject();
                var uv = fobj["uv"]?.AsArray();
                if (uv == null || uv.Count < 2) continue;
                var u = uv[0]!.GetValue<int>();
                var v = uv[1]!.GetValue<int>();
                if (u > maxU) maxU = u;
                if (v > maxV) maxV = v;
            }

            var maxUV = new Vector2Short((short)maxU, (short)maxV);
            if (maxUV.X < texture.Width && maxUV.Y < texture.Height)
            {
                obj.Remove("textureWrap");
                continue;
            }

            if (id == 617) continue;

            var textureGrid = obj["textureGrid"]?.AsArray();
            var frameGap = obj["frameGap"]?.AsArray();
            if (textureGrid == null || frameGap == null || textureGrid.Count < 2 || frameGap.Count < 2) continue;

            var interval = new Vector2Short(
                (short)(textureGrid[0]!.GetValue<int>() + frameGap[0]!.GetValue<int>()),
                (short)(textureGrid[1]!.GetValue<int>() + frameGap[1]!.GetValue<int>())
            );

            var frameSizeArr = obj["frameSize"]?.AsArray();
            if (frameSizeArr == null || frameSizeArr.Count == 0) continue;
            var frameSize0 = frameSizeArr[0]!.AsArray();
            if (frameSize0 == null || frameSize0.Count < 2) continue;

            var frameSize = new Vector2Short(
                (short)(frameSize0[0]!.GetValue<int>() * interval.X),
                (short)(frameSize0[1]!.GetValue<int>() * interval.Y)
            );

            if (maxUV.X >= texture.Width)
            {
                obj["textureWrap"] = new JsonObject
                {
                    ["axis"] = "U",
                    ["offsetIncrement"] = (short)(maxUV.Y + frameSize.Y),
                    ["wrapThreshold"] = ((texture.Width + (short)frameGap[0]!.GetValue<int>()) / frameSize.X) * frameSize.X
                };
            }
            else
            {
                obj["textureWrap"] = new JsonObject
                {
                    ["axis"] = "V",
                    ["offsetIncrement"] = (short)(maxUV.X + frameSize.X),
                    ["wrapThreshold"] = ((texture.Height + (short)frameGap[1]!.GetValue<int>()) / frameSize.Y) * frameSize.Y
                };
            }
        }

        File.WriteAllText(tilesJsonPath, CompactNumericArrays(tileProps.ToJsonString(jsonOptions)));
    }
}
