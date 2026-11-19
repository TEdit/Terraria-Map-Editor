using TEdit.Terraria;

namespace TEdit.Tests.Scripting;

/// <summary>Synthetic worlds: no game installation or downloaded .wld files required.</summary>
internal static class ExecutionWorldFixture
{
    public static World Create(int width = 100, int height = 100)
    {
        if (width < 100) throw new ArgumentOutOfRangeException(nameof(width));
        if (height < 100) throw new ArgumentOutOfRangeException(nameof(height));
        var world = TestWorldFactory.CreateWorldWithTerrain(width, height);
        world.Title = "MCP execution fixture";
        world.WorldGUID = Guid.Parse("402afef0-de7c-454b-87d9-1b8a7d5ea42e");
        world.CreationTime = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToBinary();
        world.LastPlayed = world.CreationTime;
        world.Seed = "42";
        // Known valid ordinary tiles at opposite corners for boundary tests.
        world.Tiles[0, 0].IsActive = true;
        world.Tiles[0, 0].Type = 1;
        world.Tiles[width - 1, height - 1].IsActive = true;
        world.Tiles[width - 1, height - 1].Type = 1;
        return world;
    }
}
