using System.Collections.Generic;
using Shouldly;
using TEdit.Editor;
using TEdit.Editor.Undo;
using TEdit.Geometry;
using TEdit.Terraria;
using TEdit.Terraria.Objects;
using Xunit;

namespace TEdit.Tests.Editor;

/// <summary>
/// Tests that cannonball tiles (726) — framed + solid — can have slopes applied
/// via the editor tools and paint modes.
/// </summary>
public class CannonballSlopeTests
{
    private const ushort CannonballTileId = 726;

    /// <summary>
    /// Verify that tile 726 is configured as both framed and solid with slopes.
    /// </summary>
    [Fact]
    public void TileProperty_Cannonball_IsFramedSolidWithSlopes()
    {
        var tp = WorldConfiguration.TileProperties[CannonballTileId];
        tp.IsFramed.ShouldBeTrue("Cannonball should be framed");
        tp.IsSolid.ShouldBeTrue("Cannonball should be solid");
        tp.HasSlopes.ShouldBeTrue("Cannonball should support slopes");
    }

    /// <summary>
    /// TileAndWall paint mode with BrickStyleActive should set slope on cannonball.
    /// </summary>
    [Fact]
    public void SetPixel_TileAndWall_BrickStyle_SetsSlope()
    {
        var (editor, world) = CreateEditor();

        // Place a cannonball tile
        PlaceCannonball(world, 5, 5);
        PlaceSolidNeighbors(world, 5, 5);

        // Configure picker for brick style
        editor.TilePicker.PaintMode = PaintMode.TileAndWall;
        editor.TilePicker.TileStyleActive = false;
        editor.TilePicker.WallStyleActive = false;
        editor.TilePicker.BrickStyleActive = true;
        editor.TilePicker.ExtrasActive = true;
        editor.TilePicker.BrickStyle = BrickStyle.SlopeTopRight;

        // Act
        editor.SetPixel(5, 5);

        // Assert
        world.Tiles[5, 5].BrickStyle.ShouldBe(BrickStyle.SlopeTopRight,
            "TileAndWall mode should set slope on cannonball");
    }

    /// <summary>
    /// Sprite paint mode with BrickStyleActive should set slope on cannonball
    /// since it has HasSlopes=true.
    /// </summary>
    [Fact]
    public void SetPixel_Sprites_BrickStyle_SetsSlopeOnCannonball()
    {
        var (editor, world) = CreateEditor();
        PlaceCannonball(world, 5, 5);

        // Configure picker for sprite mode with brick style
        // PaintMode.Sprites auto-sets IsEraser=true; override for paint-only mode
        editor.TilePicker.PaintMode = PaintMode.Sprites;
        editor.TilePicker.IsEraser = false;
        editor.TilePicker.BrickStyleActive = true;
        editor.TilePicker.ExtrasActive = true;
        editor.TilePicker.BrickStyle = BrickStyle.SlopeTopLeft;

        // Act
        editor.SetPixel(5, 5, mode: PaintMode.Sprites);

        // Assert
        world.Tiles[5, 5].BrickStyle.ShouldBe(BrickStyle.SlopeTopLeft,
            "Sprite mode should set slope on cannonball (framed + HasSlopes)");
    }

    /// <summary>
    /// Sprite paint mode should NOT set slope on a normal framed tile without HasSlopes.
    /// </summary>
    [Fact]
    public void SetPixel_Sprites_BrickStyle_DoesNotSlopeNormalFramedTile()
    {
        var (editor, world) = CreateEditor();

        // Place a torch (framed, not solid, no HasSlopes)
        ushort torchId = 4;
        world.Tiles[5, 5].IsActive = true;
        world.Tiles[5, 5].Type = torchId;
        world.Tiles[5, 5].U = 0;
        world.Tiles[5, 5].V = 0;
        world.TileFrameImportant[torchId] = true;

        // Configure picker for sprite mode with brick style
        editor.TilePicker.PaintMode = PaintMode.Sprites;
        editor.TilePicker.BrickStyleActive = true;
        editor.TilePicker.ExtrasActive = true;
        editor.TilePicker.BrickStyle = BrickStyle.SlopeTopRight;

        // Act
        editor.SetPixel(5, 5);

        // Assert
        world.Tiles[5, 5].BrickStyle.ShouldBe(BrickStyle.Full,
            "Normal framed tile without HasSlopes should remain Full");
    }

    /// <summary>
    /// All BrickStyle values should be applicable to cannonball.
    /// </summary>
    [Theory]
    [InlineData(BrickStyle.HalfBrick)]
    [InlineData(BrickStyle.SlopeTopRight)]
    [InlineData(BrickStyle.SlopeTopLeft)]
    [InlineData(BrickStyle.SlopeBottomRight)]
    [InlineData(BrickStyle.SlopeBottomLeft)]
    [InlineData(BrickStyle.Full)]
    public void SetPixel_Cannonball_AllBrickStyles(BrickStyle style)
    {
        var (editor, world) = CreateEditor();
        PlaceCannonball(world, 5, 5);

        editor.TilePicker.PaintMode = PaintMode.TileAndWall;
        editor.TilePicker.BrickStyleActive = true;
        editor.TilePicker.ExtrasActive = true;
        editor.TilePicker.BrickStyle = style;

        editor.SetPixel(5, 5);

        world.Tiles[5, 5].BrickStyle.ShouldBe(style);
    }

    #region Helpers

    private static (WorldEditor editor, World world) CreateEditor()
    {
        var world = new World(100, 100, "Test World");
        world.Tiles = new Tile[100, 100];
        var picker = new TilePicker();
        var mask = new TileMaskSettings();
        var selection = new NoOpSelection();
        var undo = new NoOpUndoManager();

        var editor = new WorldEditor(picker, mask, world, selection, undo, null);
        return (editor, world);
    }

    private static void PlaceCannonball(World world, int x, int y)
    {
        ref Tile t = ref world.Tiles[x, y];
        t.IsActive = true;
        t.Type = CannonballTileId;
        t.U = 0;
        t.V = 0;
        t.BrickStyle = BrickStyle.Full;
    }

    /// <summary>
    /// Place solid neighbors around (x,y) so slope checks pass.
    /// </summary>
    private static void PlaceSolidNeighbors(World world, int x, int y)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                int nx = x + dx, ny = y + dy;
                if (nx >= 0 && nx < world.TilesWide && ny >= 0 && ny < world.TilesHigh)
                {
                    world.Tiles[nx, ny].IsActive = true;
                    world.Tiles[nx, ny].Type = 1; // Stone — solid non-framed
                }
            }
        }
    }

    private class NoOpSelection : ISelection
    {
        public RectangleInt32 SelectionArea { get; set; }
        public bool IsActive { get; set; }
        public bool IsValid(int x, int y) => true;
        public bool IsValid(Vector2Int32 xy) => true;
        public void SetRectangle(Vector2Int32 p1, Vector2Int32 p2) { }
    }

    private class NoOpUndoManager : IUndoManager
    {
        private static readonly IReadOnlyList<Vector2Int32> Empty = Array.Empty<Vector2Int32>();
        public Task StartUndoAsync() => Task.CompletedTask;
        public void SaveTile(World world, Vector2Int32 location, bool removeEntities = false) { }
        public void SaveTile(World world, int x, int y, bool removeEntities = false) { }
        public Task SaveUndoAsync() => Task.CompletedTask;
        public Task<IReadOnlyList<Vector2Int32>> UndoAsync(World world) => Task.FromResult(Empty);
        public Task<IReadOnlyList<Vector2Int32>> RedoAsync(World world) => Task.FromResult(Empty);
        public void Dispose() { }
    }

    #endregion
}
