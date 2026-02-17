using System;
using System.Collections.Generic;
using System.Linq;
using TEdit.Terraria;

namespace TEdit.Scripting.Api;

public class ChestApi
{
    private readonly World _world;

    public ChestApi(World world)
    {
        _world = world;
    }

    public int Count => _world.Chests.Count;

    public List<Dictionary<string, object>> GetAll()
    {
        return _world.Chests.Select(ChestToDict).ToList();
    }

    public Dictionary<string, object>? GetAt(int x, int y)
    {
        var chest = _world.GetChestAtTile(x, y);
        return chest != null ? ChestToDict(chest) : null;
    }

    public List<Dictionary<string, object>> FindByItem(int itemId)
    {
        return _world.Chests
            .Where(c => c.Items.Any(i => i.NetId == itemId && i.StackSize > 0))
            .Select(ChestToDict)
            .ToList();
    }

    public List<Dictionary<string, object>> FindByItemName(string name)
    {
        var lower = name.ToLowerInvariant();
        return _world.Chests
            .Where(c => c.Items.Any(i => i.StackSize > 0 &&
                (i.Name?.ToLowerInvariant().Contains(lower) ?? false)))
            .Select(ChestToDict)
            .ToList();
    }

    public void SetItem(int x, int y, int slot, int itemId, int stack, int prefix)
    {
        var chest = _world.GetChestAtTile(x, y);
        if (chest == null) throw new ArgumentException($"No chest at ({x}, {y})");
        if (slot < 0 || slot >= chest.Items.Count) throw new ArgumentException($"Invalid slot {slot}");

        chest.Items[slot].NetId = itemId;
        chest.Items[slot].StackSize = stack;
        chest.Items[slot].Prefix = (byte)prefix;
    }

    public void ClearItem(int x, int y, int slot)
    {
        var chest = _world.GetChestAtTile(x, y);
        if (chest == null) throw new ArgumentException($"No chest at ({x}, {y})");
        if (slot < 0 || slot >= chest.Items.Count) throw new ArgumentException($"Invalid slot {slot}");

        chest.Items[slot].NetId = 0;
        chest.Items[slot].StackSize = 0;
        chest.Items[slot].Prefix = 0;
    }

    public bool AddItem(int x, int y, int itemId, int stack, int prefix)
    {
        var chest = _world.GetChestAtTile(x, y);
        if (chest == null) throw new ArgumentException($"No chest at ({x}, {y})");

        for (int i = 0; i < chest.Items.Count; i++)
        {
            if (chest.Items[i].StackSize == 0 || chest.Items[i].NetId == 0)
            {
                chest.Items[i].NetId = itemId;
                chest.Items[i].StackSize = stack;
                chest.Items[i].Prefix = (byte)prefix;
                return true;
            }
        }
        return false; // chest is full
    }

    private static Dictionary<string, object> ChestToDict(Chest c)
    {
        var items = c.Items.Select((item, idx) => new Dictionary<string, object>
        {
            { "slot", idx },
            { "id", item.NetId },
            { "name", item.Name ?? "" },
            { "stack", item.StackSize },
            { "prefix", item.Prefix }
        }).Where(d => (int)d["stack"] > 0).ToList();

        return new Dictionary<string, object>
        {
            { "x", c.X },
            { "y", c.Y },
            { "name", c.Name ?? "" },
            { "items", items }
        };
    }
}
