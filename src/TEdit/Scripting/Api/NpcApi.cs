using System.Collections.Generic;
using System.Linq;
using TEdit.Terraria;

namespace TEdit.Scripting.Api;

public class NpcApi
{
    private readonly World _world;

    public NpcApi(World world)
    {
        _world = world;
    }

    public int Count => _world.NPCs.Count;

    public List<Dictionary<string, object>> GetAll()
    {
        return _world.NPCs.Select(n => new Dictionary<string, object>
        {
            { "name", n.Name ?? "" },
            { "displayName", n.DisplayName ?? "" },
            { "x", (int)n.Position.X },
            { "y", (int)n.Position.Y },
            { "homeX", n.Home.X },
            { "homeY", n.Home.Y },
            { "isHomeless", n.IsHomeless }
        }).ToList();
    }

    public void SetHome(string name, int x, int y)
    {
        var npc = _world.NPCs.FirstOrDefault(n =>
            string.Equals(n.Name, name, System.StringComparison.OrdinalIgnoreCase) ||
            string.Equals(n.DisplayName, name, System.StringComparison.OrdinalIgnoreCase));

        if (npc != null)
        {
            npc.Home = new Vector2Int32Observable(x, y);
            npc.IsHomeless = false;
        }
    }
}
