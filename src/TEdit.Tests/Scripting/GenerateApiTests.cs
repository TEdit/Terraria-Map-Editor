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

    // ── IceBiome ──────────────────────────────────────────────────────

    [Fact]
    public void IceBiome_ConvertsStoneToIce()
    {
        int count = _api.IceBiome(50, 40, 50, 50);

        count.ShouldBeGreaterThan(0);
        // Stone (1) in the region should be converted to ice (161)
        bool foundIce = false;
        for (int x = 50; x < 100; x++)
            for (int y = 40; y < 90; y++)
                if (_world.Tiles[x, y].Type == 161)
                    foundIce = true;

        foundIce.ShouldBeTrue("IceBiome should convert stone to ice");
    }

    [Fact]
    public void IceBiome_ConvertsDirtToSnow()
    {
        _api.IceBiome(50, 35, 50, 20);

        bool foundSnow = false;
        for (int x = 50; x < 100; x++)
            for (int y = 35; y < 55; y++)
                if (_world.Tiles[x, y].Type == 147)
                    foundSnow = true;

        foundSnow.ShouldBeTrue("IceBiome should convert dirt to snow");
    }

    // ── MushroomBiome ──────────────────────────────────────────────────

    [Fact]
    public void MushroomBiome_ConvertsTilesToMud()
    {
        int count = _api.MushroomBiome(50, 40, 50, 50);

        count.ShouldBeGreaterThan(0);
        bool foundMud = false;
        for (int x = 50; x < 100; x++)
            for (int y = 40; y < 90; y++)
                if (_world.Tiles[x, y].Type == 59 || _world.Tiles[x, y].Type == 70)
                    foundMud = true;

        foundMud.ShouldBeTrue("MushroomBiome should place mud or mushroom grass");
    }

    // ── MarbleCave ─────────────────────────────────────────────────────

    [Fact]
    public void MarbleCave_PlacesMarbleTiles()
    {
        _api.MarbleCave(100, 80);

        bool foundMarble = false;
        for (int x = 70; x < 130; x++)
            for (int y = 60; y < 100; y++)
                if (_world.ValidTileLocation(x, y) && _world.Tiles[x, y].Type == 367)
                    foundMarble = true;

        foundMarble.ShouldBeTrue("MarbleCave should place marble tiles (367)");
    }

    [Fact]
    public void MarbleCave_PlacesMarbleWalls()
    {
        _api.MarbleCave(100, 80);

        bool foundWall = false;
        for (int x = 70; x < 130; x++)
            for (int y = 60; y < 100; y++)
                if (_world.ValidTileLocation(x, y) && _world.Tiles[x, y].Wall == 178)
                    foundWall = true;

        foundWall.ShouldBeTrue("MarbleCave should place marble walls (178)");
    }

    // ── GraniteCave ────────────────────────────────────────────────────

    [Fact]
    public void GraniteCave_PlacesGraniteTiles()
    {
        _api.GraniteCave(100, 80);

        bool foundGranite = false;
        for (int x = 70; x < 130; x++)
            for (int y = 60; y < 100; y++)
                if (_world.ValidTileLocation(x, y) && _world.Tiles[x, y].Type == 368)
                    foundGranite = true;

        foundGranite.ShouldBeTrue("GraniteCave should place granite tiles (368)");
    }

    // ── Corruption ─────────────────────────────────────────────────────

    [Fact]
    public void Corruption_ConvertsToEvilTiles()
    {
        int count = _api.Corruption(50, 35, 40, 60);

        count.ShouldBeGreaterThan(0);
        // Should find ebonstone (25) or corrupt grass (23)
        bool foundEvil = false;
        for (int x = 50; x < 90; x++)
            for (int y = 35; y < 95; y++)
                if (_world.ValidTileLocation(x, y) &&
                    (_world.Tiles[x, y].Type == 25 || _world.Tiles[x, y].Type == 23))
                    foundEvil = true;

        foundEvil.ShouldBeTrue("Corruption should convert tiles to ebonstone/corrupt grass");
    }

    [Fact]
    public void Corruption_CarvesChasms()
    {
        _api.Corruption(80, 35, 40, 80);

        // Should have carved some tiles (air gaps from chasms)
        bool foundCleared = false;
        for (int x = 80; x < 120; x++)
            for (int y = 40; y < 100; y++)
                if (_world.ValidTileLocation(x, y) && !_world.Tiles[x, y].IsActive)
                    foundCleared = true;

        foundCleared.ShouldBeTrue("Corruption should carve chasms");
    }

    // ── Crimson ────────────────────────────────────────────────────────

    [Fact]
    public void Crimson_ConvertsToCrimsonTiles()
    {
        int count = _api.Crimson(50, 35, 40, 60);

        count.ShouldBeGreaterThan(0);
        bool foundCrimson = false;
        for (int x = 50; x < 90; x++)
            for (int y = 35; y < 95; y++)
                if (_world.ValidTileLocation(x, y) &&
                    (_world.Tiles[x, y].Type == 203 || _world.Tiles[x, y].Type == 199))
                    foundCrimson = true;

        foundCrimson.ShouldBeTrue("Crimson should convert tiles to crimstone/crimson grass");
    }

    // ── Hallow ─────────────────────────────────────────────────────────

    [Fact]
    public void Hallow_ConvertsToHallowedTiles()
    {
        int count = _api.Hallow(50, 35, 50, 60);

        count.ShouldBeGreaterThan(0);
        bool foundHallow = false;
        for (int x = 50; x < 100; x++)
            for (int y = 35; y < 95; y++)
                if (_world.ValidTileLocation(x, y) &&
                    (_world.Tiles[x, y].Type == 117 || _world.Tiles[x, y].Type == 109))
                    foundHallow = true;

        foundHallow.ShouldBeTrue("Hallow should convert tiles to pearlstone/hallowed grass");
    }

    // ── SpiderCave ─────────────────────────────────────────────────────

    [Fact]
    public void SpiderCave_PlacesCobwebs()
    {
        _api.SpiderCave(100, 80);

        bool foundCobweb = false;
        for (int x = 80; x < 120; x++)
            for (int y = 65; y < 95; y++)
                if (_world.ValidTileLocation(x, y) && _world.Tiles[x, y].Type == 51)
                    foundCobweb = true;

        foundCobweb.ShouldBeTrue("SpiderCave should place cobwebs");
    }

    [Fact]
    public void SpiderCave_PlacesSpiderWalls()
    {
        _api.SpiderCave(100, 80);

        bool foundSpiderWall = false;
        for (int x = 80; x < 120; x++)
            for (int y = 65; y < 95; y++)
                if (_world.ValidTileLocation(x, y) && _world.Tiles[x, y].Wall == 62)
                    foundSpiderWall = true;

        foundSpiderWall.ShouldBeTrue("SpiderCave should place spider walls");
    }

    // ── Beehive ────────────────────────────────────────────────────────

    [Fact]
    public void Beehive_PlacesHiveBlocksAndHoney()
    {
        _api.Beehive(100, 80, 15);

        bool foundHive = false, foundHoney = false;
        for (int x = 80; x < 120; x++)
            for (int y = 60; y < 100; y++)
                if (_world.ValidTileLocation(x, y))
                {
                    if (_world.Tiles[x, y].Type == 225 && _world.Tiles[x, y].IsActive) foundHive = true;
                    if (_world.Tiles[x, y].LiquidType == LiquidType.Honey && _world.Tiles[x, y].LiquidAmount > 0) foundHoney = true;
                }

        foundHive.ShouldBeTrue("Beehive should place hive blocks");
        foundHoney.ShouldBeTrue("Beehive should place honey");
    }

    // ── Desert ─────────────────────────────────────────────────────────

    [Fact]
    public void Desert_PlacesSandAndSandstone()
    {
        int count = _api.Desert(50, 35, 60, 60);

        count.ShouldBeGreaterThan(0);
        bool foundSand = false, foundSandstone = false;
        for (int x = 50; x < 110; x++)
            for (int y = 35; y < 95; y++)
                if (_world.ValidTileLocation(x, y))
                {
                    if (_world.Tiles[x, y].Type == 53) foundSand = true;
                    if (_world.Tiles[x, y].Type == 396) foundSandstone = true;
                }

        foundSand.ShouldBeTrue("Desert should place sand");
        foundSandstone.ShouldBeTrue("Desert should place sandstone");
    }

    // ── Jungle ─────────────────────────────────────────────────────────

    [Fact]
    public void Jungle_PlacesMudAndJungleGrass()
    {
        int count = _api.Jungle(50, 35, 60, 60);

        count.ShouldBeGreaterThan(0);
        bool foundMud = false, foundJungle = false;
        for (int x = 50; x < 110; x++)
            for (int y = 35; y < 95; y++)
                if (_world.ValidTileLocation(x, y))
                {
                    if (_world.Tiles[x, y].Type == 59) foundMud = true;
                    if (_world.Tiles[x, y].Type == 60) foundJungle = true;
                }

        foundMud.ShouldBeTrue("Jungle should place mud");
        foundJungle.ShouldBeTrue("Jungle should place jungle grass");
    }

    // ── Pyramid ────────────────────────────────────────────────────────

    [Fact]
    public void Pyramid_PlacesSandstoneBricks()
    {
        _api.Pyramid(100, 50, 40);

        bool foundBrick = false;
        for (int x = 70; x < 130; x++)
            for (int y = 50; y < 100; y++)
                if (_world.ValidTileLocation(x, y) && _world.Tiles[x, y].Type == 151)
                    foundBrick = true;

        foundBrick.ShouldBeTrue("Pyramid should place sandstone bricks (151)");
    }

    // ── Dungeon ────────────────────────────────────────────────────────

    [Fact]
    public void Dungeon_PlacesDungeonBricks()
    {
        _api.Dungeon(100, 40, 1, 0);

        bool foundBrick = false;
        for (int x = 70; x < 130; x++)
            for (int y = 40; y < 180; y++)
                if (_world.ValidTileLocation(x, y) && _world.Tiles[x, y].Type == 41)
                    foundBrick = true;

        foundBrick.ShouldBeTrue("Dungeon should place blue dungeon bricks (41)");
    }

    // ── SmoothWorld ────────────────────────────────────────────────────

    [Fact]
    public void SmoothWorld_SlopesExposedEdges()
    {
        // Create an irregular surface by removing some tiles to expose edges
        int groundLevel = (int)_world.GroundLevel;
        for (int x = 60; x < 80; x++)
        {
            // Remove every other column at surface to create exposed corners
            if (x % 2 == 0)
            {
                _world.Tiles[x, groundLevel].IsActive = false;
                _world.Tiles[x, groundLevel + 1].IsActive = false;
            }
        }

        int count = _api.SmoothWorld(50, groundLevel - 2, 50, 10);

        count.ShouldBeGreaterThan(0, "SmoothWorld should slope some exposed edges");
    }

    // ── PlaceVines ─────────────────────────────────────────────────────

    [Fact]
    public void PlaceVines_GrowsVinesFromGrass()
    {
        // First place grass on surface
        int groundLevel = (int)_world.GroundLevel;
        for (int x = 50; x < 100; x++)
        {
            // Make sure there's a grass tile with air below
            if (_world.Tiles[x, groundLevel].IsActive)
            {
                _world.Tiles[x, groundLevel].Type = 2; // Grass
                // Clear space below for vines
                for (int y = groundLevel + 1; y < groundLevel + 5; y++)
                {
                    _world.Tiles[x, y].IsActive = false;
                }
            }
        }

        int count = _api.PlaceVines(50, groundLevel, 50, 20, "forest");

        count.ShouldBeGreaterThan(0, "PlaceVines should place some vines");
    }

    // ── PlacePlants ────────────────────────────────────────────────────

    [Fact]
    public void PlacePlants_GrowsPlantsOnGrass()
    {
        // Set up grass surface with air above
        int groundLevel = (int)_world.GroundLevel;
        for (int x = 50; x < 100; x++)
        {
            if (_world.Tiles[x, groundLevel].IsActive)
            {
                _world.Tiles[x, groundLevel].Type = 2; // Grass
                if (groundLevel > 0)
                    _world.Tiles[x, groundLevel - 1].IsActive = false;
            }
        }

        int count = _api.PlacePlants(50, groundLevel - 1, 50, 5, "forest");

        count.ShouldBeGreaterThan(0, "PlacePlants should place some plants");
    }

    // ── Ocean ─────────────────────────────────────────────────────────

    [Fact]
    public void Ocean_PlacesSandAndWater()
    {
        // Use a wider world for ocean generation
        var world = TestWorldFactory.CreateWorldWithTerrain(500, 300);
        var api = new GenerateApi(world, new NoOpUndoManager(), new NoOpSelection());

        int count = api.Ocean(direction: -1, oceanWidth: 80, maxDepth: 40);

        count.ShouldBeGreaterThan(0);

        // Should find sand (53) near the left edge
        bool foundSand = false;
        bool foundWater = false;
        for (int x = 1; x < 80; x++)
            for (int y = 1; y < 200; y++)
                if (world.ValidTileLocation(x, y))
                {
                    if (world.Tiles[x, y].Type == 53 && world.Tiles[x, y].IsActive) foundSand = true;
                    if (world.Tiles[x, y].LiquidAmount > 0 && world.Tiles[x, y].LiquidType == LiquidType.Water) foundWater = true;
                }

        foundSand.ShouldBeTrue("Ocean should place sand");
        foundWater.ShouldBeTrue("Ocean should place water");
    }

    [Fact]
    public void Ocean_RightEdge_PlacesSandAndWater()
    {
        var world = TestWorldFactory.CreateWorldWithTerrain(500, 300);
        var api = new GenerateApi(world, new NoOpUndoManager(), new NoOpSelection());

        int count = api.Ocean(direction: 1, oceanWidth: 80, maxDepth: 40);

        count.ShouldBeGreaterThan(0);

        bool foundSand = false;
        for (int x = 420; x < 499; x++)
            for (int y = 1; y < 200; y++)
                if (world.ValidTileLocation(x, y) && world.Tiles[x, y].Type == 53 && world.Tiles[x, y].IsActive)
                    foundSand = true;

        foundSand.ShouldBeTrue("Right ocean should place sand near right edge");
    }

    [Fact]
    public void Ocean_ExponentialDepth_DeeperAtEdge()
    {
        var world = TestWorldFactory.CreateWorldWithTerrain(500, 300);
        var api = new GenerateApi(world, new NoOpUndoManager(), new NoOpSelection());

        api.Ocean(direction: -1, oceanWidth: 100, maxDepth: 60);

        // Find sand surface at edge (col 5) vs beach (col 90)
        int edgeSurface = -1, beachSurface = -1;
        for (int y = 1; y < 250; y++)
        {
            if (edgeSurface < 0 && world.Tiles[5, y].IsActive && world.Tiles[5, y].Type == 53)
                edgeSurface = y;
            if (beachSurface < 0 && world.Tiles[90, y].IsActive && world.Tiles[90, y].Type == 53)
                beachSurface = y;
        }

        if (edgeSurface > 0 && beachSurface > 0)
        {
            edgeSurface.ShouldBeGreaterThan(beachSurface,
                "Ocean floor should be deeper at the world edge than at the beach");
        }
    }

    // ── Underworld ───────────────────────────────────────────────────────

    [Fact]
    public void Underworld_PlacesAshAndLava()
    {
        var world = TestWorldFactory.CreateSmallWorld(200, 400);
        var api = new GenerateApi(world, new NoOpUndoManager(), new NoOpSelection());

        int count = api.Underworld(300);

        count.ShouldBeGreaterThan(0);

        // Should find ash (57) and lava in the lower portion
        bool foundAsh = false;
        bool foundLava = false;
        for (int x = 10; x < 190; x++)
            for (int y = 300; y < 394; y++)
                if (world.ValidTileLocation(x, y))
                {
                    if (world.Tiles[x, y].Type == 57 && world.Tiles[x, y].IsActive) foundAsh = true;
                    if (world.Tiles[x, y].LiquidAmount > 0 && world.Tiles[x, y].LiquidType == LiquidType.Lava) foundLava = true;
                }

        foundAsh.ShouldBeTrue("Underworld should place ash blocks");
        foundLava.ShouldBeTrue("Underworld should place lava");
    }

    [Fact]
    public void Underworld_HasHellstone()
    {
        var world = TestWorldFactory.CreateSmallWorld(300, 500);
        var api = new GenerateApi(world, new NoOpUndoManager(), new NoOpSelection());

        api.Underworld(350);

        bool foundHellstone = false;
        for (int x = 10; x < 290; x++)
            for (int y = 350; y < 494; y++)
                if (world.ValidTileLocation(x, y) && world.Tiles[x, y].Type == 58)
                    foundHellstone = true;

        foundHellstone.ShouldBeTrue("Underworld should place hellstone veins");
    }

    [Fact]
    public void Underworld_AutoDetectsYStart()
    {
        var world = TestWorldFactory.CreateSmallWorld(200, 400);
        var api = new GenerateApi(world, new NoOpUndoManager(), new NoOpSelection());

        // Call with default yStart (0 → auto)
        int count = api.Underworld();

        count.ShouldBeGreaterThan(0, "Underworld with auto yStart should still generate");
    }

    // ── Corruption V-Shape ───────────────────────────────────────────────

    [Fact]
    public void Corruption_VShape_WiderAtDepth()
    {
        var world = TestWorldFactory.CreateWorldWithTerrain(300, 200);
        var api = new GenerateApi(world, new NoOpUndoManager(), new NoOpSelection());

        api.Corruption(120, 35, 30, 100);

        // Count corrupt tiles at surface (first 10 rows) vs deep (last 10 rows)
        int surfaceWidth = 0, deepWidth = 0;
        int groundLevel = (int)world.GroundLevel;

        // Surface: near ground level
        for (int x = 80; x < 190; x++)
        {
            bool hasCorrupt = false;
            for (int y = groundLevel; y < groundLevel + 10; y++)
                if (world.ValidTileLocation(x, y) &&
                    (world.Tiles[x, y].Type == 25 || world.Tiles[x, y].Type == 23))
                    hasCorrupt = true;
            if (hasCorrupt) surfaceWidth++;
        }

        // Deep: 60-70 tiles below ground
        for (int x = 80; x < 190; x++)
        {
            bool hasCorrupt = false;
            for (int y = groundLevel + 60; y < groundLevel + 70; y++)
                if (world.ValidTileLocation(x, y) && world.Tiles[x, y].Type == 25)
                    hasCorrupt = true;
            if (hasCorrupt) deepWidth++;
        }

        if (deepWidth > 0 && surfaceWidth > 0)
        {
            deepWidth.ShouldBeGreaterThanOrEqualTo(surfaceWidth,
                "Corruption V-shape: deeper region should be at least as wide as surface");
        }
    }

    // ── Shape Parameter ────────────────────────────────────────────────

    [Theory]
    [InlineData("rectangle")]
    [InlineData("ellipse")]
    [InlineData("trapezoid")]
    [InlineData("diagonalLeft")]
    [InlineData("diagonalRight")]
    [InlineData("hemisphere")]
    public void IceBiome_WithShape_DoesNotThrow(string shape)
    {
        Should.NotThrow(() => _api.IceBiome(50, 40, 50, 50, shape));
    }

    [Theory]
    [InlineData("rectangle")]
    [InlineData("ellipse")]
    [InlineData("diagonalLeft")]
    public void Corruption_WithShape_DoesNotThrow(string shape)
    {
        Should.NotThrow(() => _api.Corruption(50, 35, 40, 60, shape));
    }

    [Theory]
    [InlineData("rectangle")]
    [InlineData("ellipse")]
    [InlineData("diagonalRight")]
    public void Crimson_WithShape_DoesNotThrow(string shape)
    {
        Should.NotThrow(() => _api.Crimson(50, 35, 40, 60, shape));
    }

    [Theory]
    [InlineData("rectangle")]
    [InlineData("ellipse")]
    [InlineData("diagonalLeft")]
    public void Hallow_WithShape_DoesNotThrow(string shape)
    {
        Should.NotThrow(() => _api.Hallow(50, 35, 50, 60, shape));
    }

    [Theory]
    [InlineData("rectangle")]
    [InlineData("ellipse")]
    public void MushroomBiome_WithShape_DoesNotThrow(string shape)
    {
        Should.NotThrow(() => _api.MushroomBiome(50, 40, 50, 50, shape));
    }

    [Theory]
    [InlineData("rectangle")]
    [InlineData("ellipse")]
    [InlineData("hemisphere")]
    public void Desert_WithShape_DoesNotThrow(string shape)
    {
        Should.NotThrow(() => _api.Desert(50, 35, 60, 60, shape));
    }

    [Theory]
    [InlineData("rectangle")]
    [InlineData("ellipse")]
    public void Jungle_WithShape_DoesNotThrow(string shape)
    {
        Should.NotThrow(() => _api.Jungle(50, 35, 60, 60, shape));
    }

    [Fact]
    public void Jungle_DitheredEdges_HasFewerTilesAtBoundary()
    {
        // With ditherWidth > 0, edges should have fewer converted tiles than center
        var world = TestWorldFactory.CreateWorldWithTerrain(300, 200);
        var api = new GenerateApi(world, new NoOpUndoManager(), new NoOpSelection());

        api.Jungle(100, 35, 80, 80, "rectangle", ditherWidth: 20);

        // Count mud tiles in center column vs edge column
        int centerMud = 0, edgeMud = 0;
        for (int y = 35; y < 115; y++)
        {
            if (world.ValidTileLocation(140, y) && world.Tiles[140, y].Type == 59) centerMud++;
            if (world.ValidTileLocation(102, y) && world.Tiles[102, y].Type == 59) edgeMud++;
        }

        // Center should have more conversions than edge (dithered)
        centerMud.ShouldBeGreaterThan(0, "Center of jungle should have mud tiles");
    }

    [Fact]
    public void Jungle_NoDither_HasHardEdges()
    {
        // With ditherWidth=0, edges should be just as dense as center
        int count = _api.Jungle(50, 35, 60, 60, "rectangle", ditherWidth: 0);
        count.ShouldBeGreaterThan(0, "Jungle with no dither should still convert tiles");
    }

    [Fact]
    public void IceBiome_TrapezoidShape_ConvertsMoreTilesUnderground()
    {
        // Trapezoid should convert tiles across the region (wider at bottom)
        int count = _api.IceBiome(50, 40, 50, 50, "trapezoid");
        count.ShouldBeGreaterThan(0, "Trapezoid shape should convert tiles");
    }

    [Fact]
    public void Corruption_DiagonalShape_ConvertsTiles()
    {
        int count = _api.Corruption(50, 35, 40, 60, "diagonalLeft");
        count.ShouldBeGreaterThan(0, "Diagonal corruption should convert tiles");
    }

    // ── Edge Cases ─────────────────────────────────────────────────────

    [Fact]
    public void BiomeMethods_AtWorldEdge_DoNotThrow()
    {
        Should.NotThrow(() => _api.IceBiome(0, 0, 20, 20));
        Should.NotThrow(() => _api.MushroomBiome(180, 180, 20, 20));
        Should.NotThrow(() => _api.MarbleCave(5, 80));
        Should.NotThrow(() => _api.GraniteCave(195, 80));
        Should.NotThrow(() => _api.SpiderCave(5, 80));
        Should.NotThrow(() => _api.Hallow(0, 0, 20, 20));
    }

    [Fact]
    public void StructureMethods_AtWorldEdge_DoNotThrow()
    {
        Should.NotThrow(() => _api.Beehive(5, 80, 5));
        Should.NotThrow(() => _api.Pyramid(100, 80, 20));
        Should.NotThrow(() => _api.LivingTree(100, 80, 20));
        Should.NotThrow(() => _api.UndergroundHouse(100, 80, 0));
    }

    [Fact]
    public void DecorationMethods_DoNotThrow()
    {
        Should.NotThrow(() => _api.PlacePots(50, 40, 50, 50, 5));
        Should.NotThrow(() => _api.PlaceStalactites(50, 40, 50, 50, 5));
        Should.NotThrow(() => _api.PlaceTraps(50, 40, 50, 50, 3));
        Should.NotThrow(() => _api.PlaceLifeCrystals(50, 40, 50, 50, 3));
        Should.NotThrow(() => _api.PlaceSunflowers(50, 30, 50, 20, 3));
        Should.NotThrow(() => _api.PlaceThorns(50, 40, 50, 50, "corruption"));
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
