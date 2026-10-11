using System;
using System.IO;
using TEdit.Terraria.TModLoader;
using Shouldly;
using Xunit;

namespace TEdit.Terraria.Tests;

public class WorldSaveModItemsTests
{
    [Fact]
    public void Save_RebuildsModChestItemsFromWorld()
    {
        var world = new World(10, 10, "Test", 1) { Version = WorldConfiguration.CompatibleVersion, IsTModLoader = true };
        world.Tiles = new Tile[10, 10];
        for (int x = 0; x < 10; x++)
            for (int y = 0; y < 10; y++)
                world.Tiles[x, y] = new Tile();
        world.TwldData = new TwldData { TileWallDataParsed = true };

        var chest = new Chest(1, 1);
        chest.Items[0] = new Item(1, 10) { ModName = "TestMod", ModItemName = "TestItem" };
        world.Chests.Add(chest);

        string dir = Path.Combine(Path.GetTempPath(), $"tedit_sync_save_{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        try
        {
            World.Save(world, Path.Combine(dir, "Test.wld"));

            world.TwldData.ModChestItems.Count.ShouldBe(1,
                "the sync save must write mod chest items edited in the world, like SaveAsync");
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }
}
