using TEdit.Terraria;

namespace TEdit.Scripting.Api;

public class WorldInfoApi
{
    private readonly World _world;

    public WorldInfoApi(World world)
    {
        _world = world;
    }

    public int Width => _world.TilesWide;
    public int Height => _world.TilesHigh;
    public string Title => _world.Title ?? "";
    public string Seed => _world.Seed ?? "";
    public int SpawnX => _world.SpawnX;
    public int SpawnY => _world.SpawnY;
    public double SurfaceLevel => _world.GroundLevel;
    public double RockLevel => _world.RockLevel;
}
