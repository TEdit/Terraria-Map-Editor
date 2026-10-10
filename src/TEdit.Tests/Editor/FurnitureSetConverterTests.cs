using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TEdit.Editor;
using TEdit.Editor.Undo;
using TEdit.Geometry;
using TEdit.Terraria;
using TEdit.Terraria.Objects;
using TEdit.Tests.Scripting;
using Xunit;

namespace TEdit.Tests.Editor;

public sealed class FurnitureSetConverterTests
{
    private static readonly FurnitureSetConverter Converter = FurnitureSetConverter.Default;
    private static readonly RectangleInt32 Area = new(0, 0, 100, 100);

    private static TileProperty Tile(string name) => WorldConfiguration.TileProperties.First(t => t.Name == name);

    // Looks in "Tables" and "Tables (Group 2)" alike: newer sets keep their furniture in the group 2 tiles.
    private static (TileProperty Prop, FrameProperty Frame) Locate(string family, string frameName, string variety = null, int index = 0) =>
        WorldConfiguration.TileProperties
            .Where(t => t.Frames != null && (t.Name == family || t.Name.StartsWith(family + " (Group")))
            .SelectMany(t => t.Frames!.Select(f => (Prop: t, Frame: f)))
            .Where(p => p.Frame.Name.Trim() == frameName && (variety == null || p.Frame.Variety == variety))
            .ElementAt(index);

    private static Vector2Short SizeOf(TileProperty prop, FrameProperty frame) =>
        frame.Size.X > 0 && frame.Size.Y > 0 ? frame.Size : prop.GetFrameSize(frame.UV.Y);

    private static Vector2Short Place(World world, TileProperty prop, FrameProperty frame, int x, int y)
    {
        var size = SizeOf(prop, frame);
        var interval = SpritePlacer.GetTileStep(prop);
        for (int dx = 0; dx < size.X; dx++)
            for (int dy = 0; dy < size.Y; dy++)
                world.Tiles[x + dx, y + dy] = new Tile
                {
                    IsActive = true,
                    Type = (ushort)prop.Id,
                    U = (short)(frame.UV.X + dx * interval.X),
                    V = (short)(frame.UV.Y + dy * interval.Y),
                };
        return size;
    }

    private static Vector2Short Place(World world, string family, string frameName, int x, int y, string variety = null, int index = 0)
    {
        var (prop, frame) = Locate(family, frameName, variety, index);
        return Place(world, prop, frame, x, y);
    }

    private static void AssertSprite(World world, string family, string frameName, int x, int y, string variety = null, int index = 0)
    {
        var (prop, frame) = Locate(family, frameName, variety, index);
        var size = SizeOf(prop, frame);
        var interval = SpritePlacer.GetTileStep(prop);
        for (int dx = 0; dx < size.X; dx++)
            for (int dy = 0; dy < size.Y; dy++)
            {
                var tile = world.Tiles[x + dx, y + dy];
                Assert.True(tile.IsActive);
                Assert.Equal((ushort)prop.Id, tile.Type);
                Assert.Equal(frame.UV.X + dx * interval.X, tile.U);
                Assert.Equal(frame.UV.Y + dy * interval.Y, tile.V);
            }
    }

    private static FurnitureSetConversion Convert(World world, string from, string to, System.Func<int, int, bool> include = null, IUndoManager undo = null) =>
        Converter.Convert(world, Area, include, from, to, undo);

    private static ushort BlockId(string name) => (ushort)WorldConfiguration.TileProperties.First(t => t.Name == name && !t.IsFramed).Id;

    private static ushort WallId(string name) => (ushort)WorldConfiguration.WallProperties.First(w => w.Name == name).Id;

    // ── Set discovery ───────────────────────────────────────────────

    [Fact]
    public void Sets_AreDiscoveredFromFrameNames()
    {
        Assert.Contains("Sandstone", Converter.Sets);
        Assert.Contains("Skyware", Converter.Sets);
        Assert.Contains("Wooden", Converter.Sets);
        Assert.Contains("Martian", Converter.Sets);
        Assert.Contains("Martian Hover", Converter.Sets);
        Assert.True(Converter.Sets.Count > 40);
    }

    [Fact]
    public void Sets_MergeWoodSpellings()
    {
        // Frames use both "Boreal Chair" and "Boreal Wood ..." for the same set.
        Assert.Contains("Boreal Wood", Converter.Sets);
        Assert.DoesNotContain("Boreal", Converter.Sets);
    }

    [Fact]
    public void BuildingMaterials_AllResolveToGameData()
    {
        foreach (var (set, (block, wall)) in FurnitureSetConverter.BuildingMaterials)
        {
            Assert.Contains(set, Converter.Sets);
            if (block != null)
                Assert.True(Converter.GetBlock(set).HasValue, $"{set}: block '{block}' not found");
            if (wall != null)
                Assert.True(Converter.GetWall(set).HasValue, $"{set}: wall '{wall}' not found");
        }
    }

    [Fact]
    public void GetSet_RecognizesFurnitureBlocksAndWalls()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        Place(world, "Chairs", "Sandstone Chair", 10, 10);
        world.Tiles[20, 20] = new Tile { IsActive = true, Type = BlockId("Sunplate Block") };
        world.Tiles[21, 20] = new Tile { Wall = WallId("Smooth Sandstone Wall") };
        world.Tiles[22, 20] = new Tile { IsActive = true, Type = 1 };

        Assert.Equal("Sandstone", Converter.GetSet(world.Tiles[10, 11]));
        Assert.Equal("Skyware", Converter.GetSet(world.Tiles[20, 20]));
        Assert.Equal("Sandstone", Converter.GetSet(world.Tiles[21, 20]));
        Assert.Null(Converter.GetSet(world.Tiles[22, 20]));
    }

    // ── Furniture ───────────────────────────────────────────────────

    [Fact]
    public void Chair_BothFacings_ConvertToTargetSet()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        Place(world, "Chairs", "Sandstone Chair", 10, 10, index: 0);
        Place(world, "Chairs", "Sandstone Chair", 12, 10, index: 1);

        var result = Convert(world, "Sandstone", "Skyware");

        Assert.Equal(2, result.Sprites);
        AssertSprite(world, "Chairs", "Skyware Chair", 10, 10, index: 0);
        AssertSprite(world, "Chairs", "Skyware Chair", 12, 10, index: 1);
    }

    [Fact]
    public void MultiTileTable_ConvertsEveryTile()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        Place(world, "Tables", "Sandstone Table", 10, 10);

        var result = Convert(world, "Sandstone", "Skyware");

        Assert.Equal(1, result.Sprites);
        AssertSprite(world, "Tables", "Skyware Table", 10, 10);
    }

    [Fact]
    public void LampState_IsKept()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        Place(world, "Lamps", "Sandstone Lamp", 10, 10, variety: "On");
        Place(world, "Lamps", "Sandstone Lamp", 12, 10, variety: "Off");

        Convert(world, "Sandstone", "Skyware");

        AssertSprite(world, "Lamps", "Skyware Lamp", 10, 10, variety: "On");
        AssertSprite(world, "Lamps", "Skyware Lamp", 12, 10, variety: "Off");
    }

    [Fact]
    public void PlatformShapes_AreKept()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        Place(world, "Platforms", "Sandstone Platform", 10, 10, variety: "Endcap Left");
        Place(world, "Platforms", "Sandstone Platform", 11, 10, variety: "Flat");
        Place(world, "Platforms", "Sandstone Platform", 12, 10, variety: "Endcap Right");

        var result = Convert(world, "Sandstone", "Skyware");

        Assert.Equal(3, result.Sprites);
        AssertSprite(world, "Platforms", "Skyware Platform", 10, 10, variety: "Endcap Left");
        AssertSprite(world, "Platforms", "Skyware Platform", 11, 10, variety: "Flat");
        AssertSprite(world, "Platforms", "Skyware Platform", 12, 10, variety: "Endcap Right");
    }

    [Fact]
    public void WoodPlatform_BelongsToWoodenSet()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        Place(world, "Platforms", "Wood Platform", 10, 10, variety: "Flat");

        var result = Convert(world, "Wooden", "Skyware");

        Assert.Equal(1, result.Sprites);
        AssertSprite(world, "Platforms", "Skyware Platform", 10, 10, variety: "Flat");
    }

    [Fact]
    public void Chest_MovesBetweenChestTileGroups()
    {
        // Newer sets keep their chests in "Chests (Group 2)"; converting must switch the tile type.
        var group2 = Tile("Chests (Group 2)");
        var frame = group2.Frames!.First(f => Converter.Sets.Any(s => f.Name.Trim() == s + " Chest"));
        string set = Converter.Sets.First(s => frame.Name.Trim() == s + " Chest");
        var world = TestWorldFactory.CreateSmallWorld();
        Place(world, group2, frame, 10, 10);
        world.Chests.Add(new Chest(10, 10, "Loot"));

        var result = Convert(world, set, "Skyware");

        Assert.Equal(1, result.Sprites);
        AssertSprite(world, "Chests", "Skyware Chest", 10, 10);
        Assert.Equal("Loot", Assert.Single(world.Chests).Name);
    }

    [Fact]
    public void RenamedItem_MatchesWhenItIsTheOnlyOneInItsFamily()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        Place(world, "Bookcases", "Martian Holobookcase", 10, 10);

        var result = Convert(world, "Martian", "Skyware");

        Assert.Equal(1, result.Sprites);
        AssertSprite(world, "Bookcases", "Skyware Bookcase", 10, 10);
    }

    [Fact]
    public void ItemWithoutEquivalent_IsLeftAndCounted()
    {
        // Wooden has both a bench and a sofa; Skyware only a sofa, so the bench has no safe match.
        var world = TestWorldFactory.CreateSmallWorld();
        Place(world, "Benches", "Wooden Bench", 10, 10);

        var result = Convert(world, "Wooden", "Skyware");

        Assert.Equal(1, result.Unmatched);
        Assert.Equal(0, result.Sprites);
        AssertSprite(world, "Benches", "Wooden Bench", 10, 10);
    }

    [Fact]
    public void FromSet_OnlyConvertsThatSet()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        Place(world, "Chairs", "Sandstone Chair", 10, 10);
        Place(world, "Chairs", "Wooden Chair", 12, 10);

        var result = Convert(world, "Sandstone", "Skyware");

        Assert.Equal(1, result.Sprites);
        AssertSprite(world, "Chairs", "Skyware Chair", 10, 10);
        AssertSprite(world, "Chairs", "Wooden Chair", 12, 10);
    }

    [Fact]
    public void AnySet_ConvertsEverySetButLeavesTargetAlone()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        Place(world, "Chairs", "Sandstone Chair", 10, 10);
        Place(world, "Chairs", "Wooden Chair", 12, 10);
        Place(world, "Chairs", "Skyware Chair", 14, 10);

        var result = Convert(world, null, "Skyware");

        Assert.Equal(2, result.Sprites);
        AssertSprite(world, "Chairs", "Skyware Chair", 10, 10);
        AssertSprite(world, "Chairs", "Skyware Chair", 12, 10);
        AssertSprite(world, "Chairs", "Skyware Chair", 14, 10);
    }

    [Fact]
    public void SameOrUnknownTarget_IsNoOp()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        Place(world, "Chairs", "Sandstone Chair", 10, 10);

        Assert.Equal(default, Convert(world, "Sandstone", "Sandstone"));
        Assert.Equal(default, Convert(world, "Sandstone", "No Such Set"));
        Assert.Equal(default, Converter.Convert(null, Area, null, null, "Skyware", null));
        AssertSprite(world, "Chairs", "Sandstone Chair", 10, 10);
    }

    // ── Game-saved worlds ───────────────────────────────────────────

    // UVs below are copied from a real world. Tiles of one sprite are 18px apart even where FrameGap is larger.
    [Theory]
    [InlineData((ushort)15, 0, 960, 1, 2, 0, 1240)]     // Pumpkin Chair to Slime Chair, FrameGap (2, 4)
    [InlineData((ushort)497, 18, 760, 1, 2, 18, 960)]   // Pumpkin Toilet facing right, FrameGap (2, 4)
    [InlineData((ushort)172, 0, 722, 2, 2, 0, 988)]     // Pumpkin Sink, FrameGap (2, 3)
    [InlineData((ushort)93, 0, 1188, 1, 3, 0, 1134)]    // Pumpkin Lamp (on)
    public void GameSavedSprite_PumpkinToSlime(ushort type, int u, int v, int width, int height, int slimeU, int slimeV)
    {
        var world = TestWorldFactory.CreateSmallWorld();
        for (int dx = 0; dx < width; dx++)
            for (int dy = 0; dy < height; dy++)
                world.Tiles[10 + dx, 10 + dy] = new Tile { IsActive = true, Type = type, U = (short)(u + dx * 18), V = (short)(v + dy * 18) };

        var result = Convert(world, "Pumpkin", "Slime");

        Assert.Equal(1, result.Sprites);
        for (int dx = 0; dx < width; dx++)
            for (int dy = 0; dy < height; dy++)
            {
                Assert.Equal(type, world.Tiles[10 + dx, 10 + dy].Type);
                Assert.Equal(slimeU + dx * 18, world.Tiles[10 + dx, 10 + dy].U);
                Assert.Equal(slimeV + dy * 18, world.Tiles[10 + dx, 10 + dy].V);
            }
    }

    [Fact]
    public void ClosedDoor_WithDifferentVariantPerRow_KeepsEachRowsVariant()
    {
        // The game picks the A/B/C look per row of a closed door; real worlds mix them in one door.
        var world = TestWorldFactory.CreateSmallWorld();
        world.Tiles[10, 10] = new Tile { IsActive = true, Type = 10, U = 0, V = 1296 };
        world.Tiles[10, 11] = new Tile { IsActive = true, Type = 10, U = 36, V = 1314 };
        world.Tiles[10, 12] = new Tile { IsActive = true, Type = 10, U = 18, V = 1332 };

        var result = Convert(world, "Pumpkin", "Slime");

        Assert.Equal(1, result.Sprites);
        Assert.Equal((0, 1674), (world.Tiles[10, 10].U, world.Tiles[10, 10].V));
        Assert.Equal((36, 1692), (world.Tiles[10, 11].U, world.Tiles[10, 11].V));
        Assert.Equal((18, 1710), (world.Tiles[10, 12].U, world.Tiles[10, 12].V));
    }

    [Fact]
    public void SpriteWithTextureSpacing_StillConverts()
    {
        // TEdit's sprite tool spaces chair rows by TextureGrid + FrameGap (20px); keep whatever spacing is there.
        var world = TestWorldFactory.CreateSmallWorld();
        world.Tiles[10, 10] = new Tile { IsActive = true, Type = 15, U = 0, V = 960 };
        world.Tiles[10, 11] = new Tile { IsActive = true, Type = 15, U = 0, V = 980 };

        var result = Convert(world, "Pumpkin", "Slime");

        Assert.Equal(1, result.Sprites);
        Assert.Equal(1240, world.Tiles[10, 10].V);
        Assert.Equal(1260, world.Tiles[10, 11].V);
    }

    // ── Selection ───────────────────────────────────────────────────

    [Fact]
    public void PartlySelectedSprite_IsLeftWhole()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        Place(world, "Tables", "Sandstone Table", 10, 10);

        // Only the table's left column is selected.
        var result = Convert(world, "Sandstone", "Skyware", include: (x, y) => x == 10);

        Assert.Equal(1, result.Partial);
        Assert.Equal(0, result.Sprites);
        AssertSprite(world, "Tables", "Sandstone Table", 10, 10);
    }

    [Fact]
    public void SpriteCutByAreaEdge_IsLeftWhole()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        Place(world, "Tables", "Sandstone Table", 10, 10);

        var result = Converter.Convert(world, new RectangleInt32(11, 0, 50, 50), null, "Sandstone", "Skyware", null);

        Assert.Equal(1, result.Partial);
        AssertSprite(world, "Tables", "Sandstone Table", 10, 10);
    }

    [Fact]
    public void MaskedSelection_OnlyTouchesSelectedTiles()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        Place(world, "Chairs", "Sandstone Chair", 10, 10);
        Place(world, "Chairs", "Sandstone Chair", 20, 10);
        var selection = new Selection();
        selection.SetTiles(new[] { new Vector2Int32(10, 10), new Vector2Int32(10, 11) }, selected: true);
        selection.SetTiles(new[] { new Vector2Int32(25, 25) }, selected: true);

        var result = Converter.Convert(world, selection.SelectionArea, selection.IsValid, "Sandstone", "Skyware", null);

        Assert.Equal(1, result.Sprites);
        AssertSprite(world, "Chairs", "Skyware Chair", 10, 10);
        AssertSprite(world, "Chairs", "Sandstone Chair", 20, 10);
    }

    // ── Blocks and walls ────────────────────────────────────────────

    [Fact]
    public void BlocksAndWalls_ConvertAndKeepShapeAndPaint()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        world.Tiles[10, 10] = new Tile
        {
            IsActive = true,
            Type = BlockId("Smooth Sandstone Block"),
            BrickStyle = BrickStyle.SlopeTopRight,
            TileColor = 5,
            Wall = WallId("Smooth Sandstone Wall"),
            WallColor = 7,
        };
        world.Tiles[11, 10] = new Tile { IsActive = true, Type = 1, Wall = WallId("Smooth Sandstone Wall") };

        var result = Convert(world, "Sandstone", "Skyware");

        Assert.Equal(1, result.Blocks);
        Assert.Equal(2, result.Walls);
        var tile = world.Tiles[10, 10];
        Assert.Equal(BlockId("Sunplate Block"), tile.Type);
        Assert.Equal(BrickStyle.SlopeTopRight, tile.BrickStyle);
        Assert.Equal(5, tile.TileColor);
        Assert.Equal(WallId("Disc Wall"), tile.Wall);
        Assert.Equal(7, tile.WallColor);
        Assert.Equal(1, world.Tiles[11, 10].Type);
        Assert.Equal(WallId("Disc Wall"), world.Tiles[11, 10].Wall);
    }

    [Fact]
    public void UnrelatedBlocksAndWalls_AreUntouched()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        world.Tiles[10, 10] = new Tile { IsActive = true, Type = 1, Wall = 1 };

        var result = Convert(world, null, "Skyware");

        Assert.Equal(0, result.Changed);
        Assert.Equal(1, world.Tiles[10, 10].Type);
        Assert.Equal(1, world.Tiles[10, 10].Wall);
    }

    [Fact]
    public void BlockSharedByTwoSets_StaysWhenConvertingBetweenThem_AndConvertsElsewhere()
    {
        // Martian and Martian Hover furniture are both built from Martian Conduit Plating.
        var plating = BlockId("Martian Conduit Plating");
        var world = TestWorldFactory.CreateSmallWorld();
        world.Tiles[10, 10] = new Tile { IsActive = true, Type = plating };

        Assert.Equal(0, Convert(world, "Martian Hover", "Martian").Blocks);
        Assert.Equal(plating, world.Tiles[10, 10].Type);

        Assert.Equal(1, Convert(world, "Martian Hover", "Skyware").Blocks);
        Assert.Equal(BlockId("Sunplate Block"), world.Tiles[10, 10].Type);
    }

    // ── Undo ────────────────────────────────────────────────────────

    [Fact]
    public void Undo_SavesEachChangedTileOnceBeforeChangingIt()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        var size = Place(world, "Tables", "Sandstone Table", 10, 10);
        world.Tiles[10, 9] = new Tile { IsActive = true, Type = BlockId("Smooth Sandstone Block"), Wall = WallId("Smooth Sandstone Wall") };
        var undo = new RecordingUndo();

        Convert(world, "Sandstone", "Skyware", undo: undo);

        Assert.Equal(size.X * size.Y + 1, undo.Saved.Count);
        Assert.Equal(undo.Saved.Count, undo.Saved.Select(s => s.Location).Distinct().Count());
        Assert.All(undo.Saved.Where(s => s.Location.Y >= 10), s => Assert.Equal((ushort)Locate("Tables", "Sandstone Table").Prop.Id, s.Tile.Type));
        Assert.Equal(BlockId("Smooth Sandstone Block"), undo.Saved.Single(s => s.Location.Y == 9).Tile.Type);
    }

    // ── Whole sets ──────────────────────────────────────────────────

    [Theory]
    [InlineData("Sandstone", "Skyware")]
    [InlineData("Wooden", "Boreal Wood")]
    [InlineData("Granite", "Marble")]
    [InlineData("Martian", "Meteorite")]
    public void RoundTrip_EveryConvertibleItemComesBackUnchanged(string from, string to)
    {
        var world = TestWorldFactory.CreateSmallWorld();
        var placed = new List<(int X, int Y, Tile[] Tiles, Vector2Short Size)>();
        int x = 2, y = 2, rowHeight = 0;
        foreach (var prop in WorldConfiguration.TileProperties.Where(p => p.Frames != null))
        {
            foreach (var frame in prop.Frames!.Where(f => f.Name.StartsWith(from + " ")))
            {
                var size = SizeOf(prop, frame);
                if (x + size.X >= world.TilesWide - 2) { x = 2; y += rowHeight + 1; rowHeight = 0; }
                rowHeight = System.Math.Max(rowHeight, (int)size.Y);
                Place(world, prop, frame, x, y);
                placed.Add((x, y, Snapshot(world, x, y, size), size));
                x += size.X + 1;
            }
        }
        Assert.NotEmpty(placed);
        var area = new RectangleInt32(0, 0, world.TilesWide, world.TilesHigh);

        var there = Converter.Convert(world, area, null, from, to, null);
        var back = Converter.Convert(world, area, null, to, from, null);

        Assert.True(there.Sprites > 5, $"only {there.Sprites} sprites converted");
        Assert.Equal(there.Sprites, back.Sprites);
        foreach (var (px, py, tiles, size) in placed)
            Assert.Equal(tiles, Snapshot(world, px, py, size));
    }

    private static Tile[] Snapshot(World world, int x, int y, Vector2Short size)
    {
        var tiles = new List<Tile>();
        for (int dx = 0; dx < size.X; dx++)
            for (int dy = 0; dy < size.Y; dy++)
                tiles.Add(world.Tiles[x + dx, y + dy]);
        return tiles.ToArray();
    }

    private sealed class RecordingUndo : IUndoManager
    {
        public List<(Vector2Int32 Location, Tile Tile)> Saved { get; } = new();

        public void SaveTile(World world, Vector2Int32 location, bool removeEntities = false) =>
            Saved.Add((location, world.Tiles[location.X, location.Y]));

        public void SaveTile(World world, int x, int y, bool removeEntities = false) =>
            SaveTile(world, new Vector2Int32(x, y), removeEntities);

        public Task StartUndoAsync() => Task.CompletedTask;
        public Task SaveUndoAsync() => Task.CompletedTask;
        public Task<IReadOnlyList<Vector2Int32>> UndoAsync(World world) => Task.FromResult<IReadOnlyList<Vector2Int32>>(new List<Vector2Int32>());
        public Task<IReadOnlyList<Vector2Int32>> RedoAsync(World world) => Task.FromResult<IReadOnlyList<Vector2Int32>>(new List<Vector2Int32>());
        public void Dispose() { }
    }
}
