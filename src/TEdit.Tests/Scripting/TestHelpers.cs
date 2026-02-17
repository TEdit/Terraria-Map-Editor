using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TEdit.Editor.Undo;
using TEdit.Geometry;
using TEdit.Terraria;

namespace TEdit.Tests.Scripting;

public class NoOpUndoManager : IUndoManager
{
    public List<(int X, int Y)> SavedTiles { get; } = new();

    public void SaveTile(World world, Vector2Int32 location, bool removeEntities = false) =>
        SavedTiles.Add((location.X, location.Y));

    public void SaveTile(World world, int x, int y, bool removeEntities = false) =>
        SavedTiles.Add((x, y));

    public Task SaveUndoAsync() => Task.CompletedTask;
    public Task StartUndoAsync() => Task.CompletedTask;
    public Task UndoAsync(World world) => Task.CompletedTask;
    public Task RedoAsync(World world) => Task.CompletedTask;
    public void Dispose() { }
}

public static class TestWorldFactory
{
    private static bool _initialized;

    private static void EnsureInitialized()
    {
        if (!_initialized)
        {
            WorldConfiguration.Initialize();
            _initialized = true;
        }
    }

    public static World CreateSmallWorld(int width = 100, int height = 100, string title = "Test World")
    {
        EnsureInitialized();

        var world = new World(height, width, title, seed: 42);
        world.Version = 279;
        world.GroundLevel = 35;
        world.RockLevel = 50;
        world.SpawnX = width / 2;
        world.SpawnY = 30;
        world.BottomWorld = height * 16;
        world.RightWorld = width * 16;

        // Initialize tiles
        world.Tiles = new Tile[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                world.Tiles[x, y] = new Tile();
            }
        }

        return world;
    }

    public static World CreateWorldWithTerrain(int width = 100, int height = 100)
    {
        var world = CreateSmallWorld(width, height);

        // Create simple terrain: air above ground level, dirt below
        int groundLevel = (int)world.GroundLevel;
        for (int x = 0; x < width; x++)
        {
            for (int y = groundLevel; y < height; y++)
            {
                world.Tiles[x, y].IsActive = true;
                world.Tiles[x, y].Type = y < groundLevel + 5 ? (ushort)0 : (ushort)1; // Dirt then Stone
                world.Tiles[x, y].Wall = y > groundLevel + 2 ? (ushort)2 : (ushort)0; // Dirt Wall underground
            }
        }

        return world;
    }

    public static World CreateWorldWithChestsAndSigns(int width = 100, int height = 100)
    {
        var world = CreateWorldWithTerrain(width, height);

        // Add a chest at (10, 40) with some items
        var chest1 = new Chest(10, 40, "Gold Chest");
        for (int i = 0; i < 40; i++) chest1.Items.Add(new Item());
        chest1.Items[0].NetId = 29; // Life Crystal
        chest1.Items[0].StackSize = 1;
        chest1.Items[1].NetId = 73; // Gold Coin
        chest1.Items[1].StackSize = 50;
        world.Chests.Add(chest1);

        // Add a second chest at (20, 40)
        var chest2 = new Chest(20, 40, "Wooden Chest");
        for (int i = 0; i < 40; i++) chest2.Items.Add(new Item());
        chest2.Items[0].NetId = 73; // Gold Coin
        chest2.Items[0].StackSize = 10;
        world.Chests.Add(chest2);

        // Place chest tiles
        world.Tiles[10, 40].IsActive = true;
        world.Tiles[10, 40].Type = 21; // Chest tile type
        world.Tiles[20, 40].IsActive = true;
        world.Tiles[20, 40].Type = 21;

        // Add a sign at (15, 40)
        world.Signs.Add(new Sign(15, 40, "Hello from TEdit!"));
        world.Tiles[15, 40].IsActive = true;
        world.Tiles[15, 40].Type = 55; // Sign tile type

        return world;
    }
}
