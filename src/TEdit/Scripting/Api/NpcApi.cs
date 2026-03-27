using System;
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
            { "spriteId", n.SpriteId },
            { "x", (int)n.Position.X },
            { "y", (int)n.Position.Y },
            { "homeX", n.Home.X },
            { "homeY", n.Home.Y },
            { "isHomeless", n.IsHomeless }
        }).ToList();
    }

    /// <summary>
    /// Sets the home for the first NPC matching the given name.
    /// Matches against both Name and DisplayName (case-insensitive).
    /// </summary>
    public void SetHome(string name, int x, int y)
    {
        var npc = _world.NPCs.FirstOrDefault(n =>
            string.Equals(n.Name, name, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(n.DisplayName, name, StringComparison.OrdinalIgnoreCase));

        if (npc != null)
        {
            npc.Home = new Vector2Int32Observable(x, y);
            npc.IsHomeless = false;
        }
    }

    /// <summary>
    /// Sets the home for the Nth NPC matching the given name (0-based index).
    /// Matches against both Name and DisplayName (case-insensitive).
    /// </summary>
    public void SetHome(string name, int x, int y, int index)
    {
        var matches = _world.NPCs.Where(n =>
            string.Equals(n.Name, name, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(n.DisplayName, name, StringComparison.OrdinalIgnoreCase)).ToList();

        if (index >= 0 && index < matches.Count)
        {
            matches[index].Home = new Vector2Int32Observable(x, y);
            matches[index].IsHomeless = false;
        }
    }

    /// <summary>
    /// Sets the home for all NPCs matching the given name.
    /// </summary>
    public int SetHomeAll(string name, int x, int y)
    {
        var matches = _world.NPCs.Where(n =>
            string.Equals(n.Name, name, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(n.DisplayName, name, StringComparison.OrdinalIgnoreCase)).ToList();

        foreach (var npc in matches)
        {
            npc.Home = new Vector2Int32Observable(x, y);
            npc.IsHomeless = false;
        }

        return matches.Count;
    }

    /// <summary>
    /// Creates a new NPC and adds it to the world.
    /// </summary>
    /// <param name="name">Internal NPC name (e.g., "Merchant") or numeric sprite ID.</param>
    /// <param name="displayName">Custom display name. If empty, uses the default name.</param>
    /// <param name="x">Home X tile coordinate.</param>
    /// <param name="y">Home Y tile coordinate.</param>
    public void Create(string name, string displayName, int x, int y)
    {
        int spriteId;
        string internalName;

        if (int.TryParse(name, out int id) && WorldConfiguration.NpcNames.ContainsKey(id))
        {
            spriteId = id;
            internalName = WorldConfiguration.NpcNames[id];
        }
        else if (WorldConfiguration.NpcIds.TryGetValue(name, out int foundId))
        {
            spriteId = foundId;
            internalName = name;
        }
        else
        {
            // Try case-insensitive match
            var match = WorldConfiguration.NpcIds
                .FirstOrDefault(kv => string.Equals(kv.Key, name, StringComparison.OrdinalIgnoreCase));

            if (match.Key == null)
                return;

            spriteId = match.Value;
            internalName = match.Key;
        }

        var npc = new NPC
        {
            SpriteId = spriteId,
            Name = internalName,
            DisplayName = string.IsNullOrEmpty(displayName) ? internalName : displayName,
            Position = new Vector2FloatObservable(x * 16f, y * 16f),
            Home = new Vector2Int32Observable(x, y),
            IsHomeless = false,
        };

        _world.NPCs.Add(npc);
    }

    /// <summary>
    /// Removes the first NPC matching the given name from the world.
    /// </summary>
    public bool Remove(string name)
    {
        var npc = _world.NPCs.FirstOrDefault(n =>
            string.Equals(n.Name, name, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(n.DisplayName, name, StringComparison.OrdinalIgnoreCase));

        if (npc != null)
        {
            _world.NPCs.Remove(npc);
            return true;
        }

        return false;
    }
}
