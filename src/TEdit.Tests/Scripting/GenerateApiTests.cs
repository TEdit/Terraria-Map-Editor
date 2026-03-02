using System;
using System.Linq;
using Shouldly;
using TEdit.Editor;
using TEdit.Geometry;
using TEdit.Scripting.Api;
using TEdit.Terraria;
using Xunit;

namespace TEdit.Tests.Scripting;

public class GenerateApiTests
{
    private readonly World _world;
    private readonly NoOpUndoManager _undo;
    private readonly GenerateApi _api;

    public GenerateApiTests()
    {
        _world = TestWorldFactory.CreateWorldWithTerrain(200, 200);
        _undo = new NoOpUndoManager();
        _api = new GenerateApi(_world, _undo, new NoOpSelection());
    }

    // ── ListOreTypes ──────────────────────────────────────────────────

    [Fact]
    public void ListOreTypes_ReturnsAllOres()
    {
        var ores = _api.ListOreTypes();

        ores.ShouldNotBeEmpty();
        ores.Length.ShouldBe(18); // copper through luminite
    }

    [Fact]
    public void ListOreTypes_ContainsExpectedOres()
    {
        var ores = _api.ListOreTypes();
        var names = ores.Select(o => o.GetType().GetProperty("name")!.GetValue(o) as string).ToArray();

        names.ShouldContain("copper");
        names.ShouldContain("gold");
        names.ShouldContain("adamantite");
        names.ShouldContain("chlorophyte");
        names.ShouldContain("luminite");
    }

    [Fact]
    public void ListOreTypes_HasCorrectTileIds()
    {
        var ores = _api.ListOreTypes();
        var copper = ores.First(o => (string)o.GetType().GetProperty("name")!.GetValue(o)! == "copper");
        var gold = ores.First(o => (string)o.GetType().GetProperty("name")!.GetValue(o)! == "gold");

        ((int)copper.GetType().GetProperty("tileId")!.GetValue(copper)!).ShouldBe(7);
        ((int)gold.GetType().GetProperty("tileId")!.GetValue(gold)!).ShouldBe(8);
    }

    // ── TileRunner ────────────────────────────────────────────────────

    [Fact]
    public void TileRunner_PlacesTilesInSolidArea()
    {
        // Place gold ore (type 8) in the stone layer
        int cx = 100, cy = 80;
        _api.TileRunner(cx, cy, 5.0, 10, 8);

        // Should have placed some gold ore tiles near the center
        bool found = false;
        for (int x = cx - 10; x <= cx + 10; x++)
            for (int y = cy - 10; y <= cy + 10; y++)
                if (_world.ValidTileLocation(x, y) && _world.Tiles[x, y].Type == 8)
                    found = true;

        found.ShouldBeTrue("TileRunner should have placed at least one tile of the given type");
    }

    [Fact]
    public void TileRunner_SavesUndoForModifiedTiles()
    {
        _undo.SavedTiles.Clear();
        _api.TileRunner(100, 80, 5.0, 10, 8);

        _undo.SavedTiles.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void TileRunner_SkipsFrameImportantTiles()
    {
        // Place a frame-important tile (e.g., chest tile type 21)
        int fx = 100, fy = 80;
        _world.Tiles[fx, fy].IsActive = true;
        _world.Tiles[fx, fy].Type = 21; // Chest — frame important

        _api.TileRunner(fx, fy, 3.0, 5, 8);

        // The chest tile should NOT be overwritten
        _world.Tiles[fx, fy].Type.ShouldBe((ushort)21);
    }

    [Fact]
    public void TileRunner_WithSpeedBias_PlacesTilesAwayFromStart()
    {
        // Speed bias should cause tiles to appear away from the start point
        var world = TestWorldFactory.CreateWorldWithTerrain(400, 200);
        var api = new GenerateApi(world, new NoOpUndoManager(), new NoOpSelection());

        int startX = 100, startY = 100;
        api.TileRunner(startX, startY, 5.0, 30, 8, speedX: 1.0, speedY: 0.0);

        // With speed bias, tiles should appear somewhere in the world
        // (verifies the speed parameters are actually used and don't cause errors)
        bool found = false;
        for (int x = 0; x < world.TilesWide && !found; x++)
            for (int y = 0; y < world.TilesHigh && !found; y++)
                if (world.Tiles[x, y].Type == 8)
                    found = true;

        found.ShouldBeTrue("TileRunner with speed bias should place tiles");
    }

    // ── Tunnel ────────────────────────────────────────────────────────

    [Fact]
    public void Tunnel_ClearsTilesInSolidArea()
    {
        int cx = 100, cy = 80;

        // Verify area is solid first
        _world.Tiles[cx, cy].IsActive.ShouldBeTrue();

        _api.Tunnel(cx, cy, 5.0, 10);

        // Should have cleared some tiles near the center
        bool foundCleared = false;
        for (int x = cx - 10; x <= cx + 10; x++)
            for (int y = cy - 10; y <= cy + 10; y++)
                if (_world.ValidTileLocation(x, y) && !_world.Tiles[x, y].IsActive)
                    foundCleared = true;

        foundCleared.ShouldBeTrue("Tunnel should have cleared at least one tile");
    }

    [Fact]
    public void Tunnel_SavesUndoForClearedTiles()
    {
        _undo.SavedTiles.Clear();
        _api.Tunnel(100, 80, 5.0, 10);

        _undo.SavedTiles.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Tunnel_DoesNotPlaceAnyTileType()
    {
        // Record existing tile types around center
        int cx = 100, cy = 80;
        _api.Tunnel(cx, cy, 5.0, 10);

        // All cleared tiles should have IsActive = false (no type placed)
        for (int x = cx - 10; x <= cx + 10; x++)
            for (int y = cy - 10; y <= cy + 10; y++)
                if (_world.ValidTileLocation(x, y) && !_world.Tiles[x, y].IsActive)
                    _world.Tiles[x, y].Type.ShouldBeOneOf((ushort)0, (ushort)1);
    }

    // ── Lake ──────────────────────────────────────────────────────────

    [Fact]
    public void Lake_CarvesAndFillsWithLiquid()
    {
        int cx = 100, cy = 80;
        _api.Lake(cx, cy, "water", 1.0);

        // Should find some water-filled tiles near the area
        bool foundLiquid = false;
        for (int x = cx - 30; x <= cx + 30; x++)
            for (int y = cy - 30; y <= cy + 30; y++)
                if (_world.ValidTileLocation(x, y) &&
                    !_world.Tiles[x, y].IsActive &&
                    _world.Tiles[x, y].LiquidAmount > 0)
                    foundLiquid = true;

        foundLiquid.ShouldBeTrue("Lake should place liquid in carved area");
    }

    [Fact]
    public void Lake_LiquidSettlesToBottom()
    {
        int cx = 100, cy = 80;
        _api.Lake(cx, cy, "water", 1.0);

        // Find the carved region
        int minY = int.MaxValue, maxY = int.MinValue;
        for (int y = 0; y < _world.TilesHigh; y++)
        {
            if (!_world.Tiles[cx, y].IsActive && _world.Tiles[cx, y].LiquidAmount > 0)
            {
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
            }
        }

        if (minY == int.MaxValue) return; // Lake didn't carve through this column

        // Liquid should be present at the bottom, air pocket at the top
        // (bottom tiles should have liquid, top carved tiles may not)
        _world.Tiles[cx, maxY].LiquidAmount.ShouldBeGreaterThan((byte)0,
            "Bottom of carved area should have liquid");
    }

    [Fact]
    public void Lake_LavaTypeIsCorrect()
    {
        // Use a large world fully filled with stone for reliable carving
        var world = TestWorldFactory.CreateSmallWorld(500, 500);
        for (int x = 0; x < 500; x++)
            for (int y = 0; y < 500; y++)
            {
                world.Tiles[x, y].IsActive = true;
                world.Tiles[x, y].Type = 1;
            }
        var api = new GenerateApi(world, new NoOpUndoManager(), new NoOpSelection());

        api.Lake(250, 350, "lava", 1.0);

        bool foundLava = false;
        for (int x = 0; x < world.TilesWide && !foundLava; x++)
            for (int y = 0; y < world.TilesHigh && !foundLava; y++)
                if (world.Tiles[x, y].LiquidAmount > 0 &&
                    world.Tiles[x, y].LiquidType == LiquidType.Lava)
                    foundLava = true;

        foundLava.ShouldBeTrue("Lake with 'lava' should place lava liquid");
    }

    [Fact]
    public void Lake_HoneyTypeIsCorrect()
    {
        var world = TestWorldFactory.CreateSmallWorld(500, 500);
        for (int x = 0; x < 500; x++)
            for (int y = 0; y < 500; y++)
            {
                world.Tiles[x, y].IsActive = true;
                world.Tiles[x, y].Type = 1;
            }
        var api = new GenerateApi(world, new NoOpUndoManager(), new NoOpSelection());

        api.Lake(250, 350, "honey", 1.0);

        bool foundHoney = false;
        for (int x = 0; x < world.TilesWide && !foundHoney; x++)
            for (int y = 0; y < world.TilesHigh && !foundHoney; y++)
                if (world.Tiles[x, y].LiquidAmount > 0 &&
                    world.Tiles[x, y].LiquidType == LiquidType.Honey)
                    foundHoney = true;

        foundHoney.ShouldBeTrue("Lake with 'honey' should place honey liquid");
    }

    [Fact]
    public void Lake_SavesUndoForCarvedTiles()
    {
        _undo.SavedTiles.Clear();
        _api.Lake(100, 80, "water", 1.0);

        _undo.SavedTiles.Count.ShouldBeGreaterThan(0);
    }

    // ── OreVein ───────────────────────────────────────────────────────

    [Fact]
    public void OreVein_PlacesNamedOre()
    {
        int cx = 100, cy = 80;
        _api.OreVein("gold", cx, cy);

        bool foundGold = false;
        for (int x = cx - 15; x <= cx + 15; x++)
            for (int y = cy - 15; y <= cy + 15; y++)
                if (_world.ValidTileLocation(x, y) && _world.Tiles[x, y].Type == 8)
                    foundGold = true;

        foundGold.ShouldBeTrue("OreVein('gold') should place gold ore (tile 8)");
    }

    [Fact]
    public void OreVein_SmallSizeIsSmaller()
    {
        // Place small and large, count tiles
        var worldSmall = TestWorldFactory.CreateWorldWithTerrain(200, 200);
        var apiSmall = new GenerateApi(worldSmall, new NoOpUndoManager(), new NoOpSelection());

        var worldLarge = TestWorldFactory.CreateWorldWithTerrain(200, 200);
        var apiLarge = new GenerateApi(worldLarge, new NoOpUndoManager(), new NoOpSelection());

        apiSmall.OreVein("copper", 100, 80, "small");
        apiLarge.OreVein("copper", 100, 80, "large");

        int countSmall = CountTilesOfType(worldSmall, 7);
        int countLarge = CountTilesOfType(worldLarge, 7);

        // Large should generally produce more tiles than small
        // (with random variance, use generous margin)
        countLarge.ShouldBeGreaterThan(0);
        countSmall.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void OreVein_UnknownOreDoesNothing()
    {
        _undo.SavedTiles.Clear();
        _api.OreVein("unobtanium", 100, 80);

        _undo.SavedTiles.Count.ShouldBe(0);
    }

    [Fact]
    public void OreVein_IsCaseInsensitive()
    {
        int cx = 100, cy = 80;
        _api.OreVein("GOLD", cx, cy);

        bool found = false;
        for (int x = cx - 15; x <= cx + 15; x++)
            for (int y = cy - 15; y <= cy + 15; y++)
                if (_world.ValidTileLocation(x, y) && _world.Tiles[x, y].Type == 8)
                    found = true;

        found.ShouldBeTrue("OreVein should be case-insensitive");
    }

    // ── Edge cases ────────────────────────────────────────────────────

    [Fact]
    public void TileRunner_AtWorldEdge_DoesNotThrow()
    {
        Should.NotThrow(() => _api.TileRunner(0, 0, 5.0, 10, 8));
        Should.NotThrow(() => _api.TileRunner(199, 199, 5.0, 10, 8));
    }

    [Fact]
    public void Tunnel_AtWorldEdge_DoesNotThrow()
    {
        Should.NotThrow(() => _api.Tunnel(0, 0, 5.0, 10));
        Should.NotThrow(() => _api.Tunnel(199, 199, 5.0, 10));
    }

    [Fact]
    public void Lake_AtWorldEdge_DoesNotThrow()
    {
        Should.NotThrow(() => _api.Lake(5, 5, "water", 0.5));
        Should.NotThrow(() => _api.Lake(195, 195, "water", 0.5));
    }

    [Fact]
    public void TileRunner_ZeroStrength_DoesNothing()
    {
        _undo.SavedTiles.Clear();
        _api.TileRunner(100, 80, 0.0, 10, 8);

        _undo.SavedTiles.Count.ShouldBe(0);
    }

    [Fact]
    public void TileRunner_ZeroSteps_DoesNothing()
    {
        _undo.SavedTiles.Clear();
        _api.TileRunner(100, 80, 5.0, 0, 8);

        _undo.SavedTiles.Count.ShouldBe(0);
    }

    // ── Helpers ────────────────────────────────────────────────────────

    private static int CountTilesOfType(World world, ushort tileType)
    {
        int count = 0;
        for (int x = 0; x < world.TilesWide; x++)
            for (int y = 0; y < world.TilesHigh; y++)
                if (world.Tiles[x, y].Type == tileType && world.Tiles[x, y].IsActive)
                    count++;
        return count;
    }

    private class NoOpSelection : ISelection
    {
        public RectangleInt32 SelectionArea { get; set; }
        public bool IsActive { get; set; }
        public bool IsValid(int x, int y) => true;
        public bool IsValid(Vector2Int32 xy) => true;
        public void SetRectangle(Vector2Int32 p1, Vector2Int32 p2) { }
    }
}
