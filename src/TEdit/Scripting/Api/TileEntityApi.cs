using System;
using System.Collections.Generic;
using System.Linq;
using TEdit.Terraria;

namespace TEdit.Scripting.Api;

public class TileEntityApi
{
    private readonly World _world;

    public TileEntityApi(World world)
    {
        _world = world;
    }

    // --- Counts ---

    public int Count => _world.TileEntities.Count;

    public int MannequinCount => _world.TileEntities.Count(e => e.EntityType == TileEntityType.DisplayDoll);
    public int WeaponRackCount => _world.TileEntities.Count(e => e.EntityType == TileEntityType.WeaponRack);
    public int HatRackCount => _world.TileEntities.Count(e => e.EntityType == TileEntityType.HatRack);
    public int ItemFrameCount => _world.TileEntities.Count(e => e.EntityType == TileEntityType.ItemFrame);
    public int FoodPlatterCount => _world.TileEntities.Count(e => e.EntityType == TileEntityType.FoodPlatter);
    public int LogicSensorCount => _world.TileEntities.Count(e => e.EntityType == TileEntityType.LogicSensor);
    public int TrainingDummyCount => _world.TileEntities.Count(e => e.EntityType == TileEntityType.TrainingDummy);
    public int PylonCount => _world.TileEntities.Count(e => e.EntityType == TileEntityType.TeleportationPylon);

    // --- Get All by Type ---

    public List<Dictionary<string, object>> GetAll()
    {
        return _world.TileEntities.Select(EntityToDict).ToList();
    }

    public List<Dictionary<string, object>> GetAllByType(string typeName)
    {
        var type = ParseEntityType(typeName);
        return _world.TileEntities
            .Where(e => e.EntityType == type)
            .Select(EntityToDict)
            .ToList();
    }

    public List<Dictionary<string, object>> GetAllMannequins() => GetAllByType("DisplayDoll");
    public List<Dictionary<string, object>> GetAllWeaponRacks() => GetAllByType("WeaponRack");
    public List<Dictionary<string, object>> GetAllHatRacks() => GetAllByType("HatRack");
    public List<Dictionary<string, object>> GetAllItemFrames() => GetAllByType("ItemFrame");
    public List<Dictionary<string, object>> GetAllFoodPlatters() => GetAllByType("FoodPlatter");

    // --- Get at Position ---

    public Dictionary<string, object>? GetAt(int x, int y)
    {
        var entity = _world.GetTileEntityAtTile(x, y);
        return entity != null ? EntityToDict(entity) : null;
    }

    // --- Find by Item ---

    public List<Dictionary<string, object>> FindByItem(int itemId)
    {
        return _world.TileEntities
            .Where(e => EntityContainsItem(e, itemId))
            .Select(EntityToDict)
            .ToList();
    }

    // --- Mannequin Equipment (DisplayDoll) ---

    public void SetEquipment(int x, int y, int slot, int itemId, int prefix = 0)
    {
        var entity = GetEntityOrThrow(x, y, TileEntityType.DisplayDoll);
        ValidateSlot(slot, entity.Items.Count, "equipment");
        SetTileEntityItem(entity.Items[slot], itemId, prefix);
    }

    public void ClearEquipment(int x, int y, int slot)
    {
        var entity = GetEntityOrThrow(x, y, TileEntityType.DisplayDoll);
        ValidateSlot(slot, entity.Items.Count, "equipment");
        ClearTileEntityItem(entity.Items[slot]);
    }

    public void SetDye(int x, int y, int slot, int dyeId, int prefix = 0)
    {
        var entity = GetEntityOrThrow(x, y, TileEntityType.DisplayDoll);
        ValidateSlot(slot, entity.Dyes.Count, "dye");
        SetTileEntityItem(entity.Dyes[slot], dyeId, prefix);
    }

    public void ClearDye(int x, int y, int slot)
    {
        var entity = GetEntityOrThrow(x, y, TileEntityType.DisplayDoll);
        ValidateSlot(slot, entity.Dyes.Count, "dye");
        ClearTileEntityItem(entity.Dyes[slot]);
    }

    public void SetWeapon(int x, int y, int itemId, int prefix = 0)
    {
        var entity = GetEntityOrThrow(x, y, TileEntityType.DisplayDoll);
        if (entity.Misc.Count == 0) throw new InvalidOperationException("Mannequin has no weapon slot");
        SetTileEntityItem(entity.Misc[0], itemId, prefix);
    }

    public void ClearWeapon(int x, int y)
    {
        var entity = GetEntityOrThrow(x, y, TileEntityType.DisplayDoll);
        if (entity.Misc.Count == 0) throw new InvalidOperationException("Mannequin has no weapon slot");
        ClearTileEntityItem(entity.Misc[0]);
    }

    public void SetPose(int x, int y, int poseId)
    {
        var entity = GetEntityOrThrow(x, y, TileEntityType.DisplayDoll);
        if (poseId < 0 || poseId > 8) throw new ArgumentException($"Invalid pose {poseId}, must be 0-8");
        entity.Pose = (byte)poseId;
    }

    public int GetPose(int x, int y)
    {
        var entity = GetEntityOrThrow(x, y, TileEntityType.DisplayDoll);
        return entity.Pose;
    }

    // --- Hat Rack ---

    public void SetHatRackItem(int x, int y, int slot, int itemId, int prefix = 0)
    {
        var entity = GetEntityOrThrow(x, y, TileEntityType.HatRack);
        ValidateSlot(slot, entity.Items.Count, "hat rack item");
        SetTileEntityItem(entity.Items[slot], itemId, prefix);
    }

    public void ClearHatRackItem(int x, int y, int slot)
    {
        var entity = GetEntityOrThrow(x, y, TileEntityType.HatRack);
        ValidateSlot(slot, entity.Items.Count, "hat rack item");
        ClearTileEntityItem(entity.Items[slot]);
    }

    public void SetHatRackDye(int x, int y, int slot, int dyeId, int prefix = 0)
    {
        var entity = GetEntityOrThrow(x, y, TileEntityType.HatRack);
        ValidateSlot(slot, entity.Dyes.Count, "hat rack dye");
        SetTileEntityItem(entity.Dyes[slot], dyeId, prefix);
    }

    public void ClearHatRackDye(int x, int y, int slot)
    {
        var entity = GetEntityOrThrow(x, y, TileEntityType.HatRack);
        ValidateSlot(slot, entity.Dyes.Count, "hat rack dye");
        ClearTileEntityItem(entity.Dyes[slot]);
    }

    // --- Single-Item Entities (WeaponRack, ItemFrame, FoodPlatter, DeadCellsDisplayJar) ---

    public void SetItem(int x, int y, int itemId, int prefix = 0, int stack = 1)
    {
        var entity = GetEntityOrThrow(x, y);
        if (!IsSingleItemEntity(entity.EntityType))
            throw new InvalidOperationException($"Entity at ({x}, {y}) is {entity.EntityType}, not a single-item entity. Use SetEquipment/SetHatRackItem instead.");

        entity.NetId = itemId;
        entity.Prefix = (byte)prefix;
        entity.StackSize = (short)stack;
    }

    public void ClearItem(int x, int y)
    {
        var entity = GetEntityOrThrow(x, y);
        if (!IsSingleItemEntity(entity.EntityType))
            throw new InvalidOperationException($"Entity at ({x}, {y}) is {entity.EntityType}, not a single-item entity.");

        entity.NetId = 0;
        entity.Prefix = 0;
        entity.StackSize = 0;
    }

    // --- Logic Sensor ---

    public void SetLogicSensor(int x, int y, int logicCheck, bool on)
    {
        var entity = GetEntityOrThrow(x, y, TileEntityType.LogicSensor);
        entity.LogicCheck = (byte)logicCheck;
        entity.On = on;
    }

    // --- Training Dummy ---

    public void SetTrainingDummyNpc(int x, int y, int npcId)
    {
        var entity = GetEntityOrThrow(x, y, TileEntityType.TrainingDummy);
        entity.Npc = (short)npcId;
    }

    // --- Helpers ---

    private TileEntity GetEntityOrThrow(int x, int y, TileEntityType? expectedType = null)
    {
        var entity = _world.GetTileEntityAtTile(x, y);
        if (entity == null) throw new ArgumentException($"No tile entity at ({x}, {y})");
        if (expectedType.HasValue && entity.EntityType != expectedType.Value)
            throw new ArgumentException($"Entity at ({x}, {y}) is {entity.EntityType}, expected {expectedType.Value}");
        return entity;
    }

    private static void ValidateSlot(int slot, int maxSlots, string name)
    {
        if (slot < 0 || slot >= maxSlots)
            throw new ArgumentException($"Invalid {name} slot {slot}, must be 0-{maxSlots - 1}");
    }

    private static void SetTileEntityItem(TileEntityItem item, int id, int prefix)
    {
        item.Id = (short)id;
        item.Prefix = (byte)prefix;
        item.StackSize = 1;
    }

    private static void ClearTileEntityItem(TileEntityItem item)
    {
        item.Id = 0;
        item.Prefix = 0;
        item.StackSize = 0;
    }

    private static bool IsSingleItemEntity(TileEntityType type) =>
        type is TileEntityType.WeaponRack
            or TileEntityType.ItemFrame
            or TileEntityType.FoodPlatter
            or TileEntityType.DeadCellsDisplayJar;

    private static bool EntityContainsItem(TileEntity e, int itemId)
    {
        // Single-item entities
        if (IsSingleItemEntity(e.EntityType))
            return e.NetId == itemId && e.StackSize > 0;

        // Multi-slot entities (DisplayDoll, HatRack)
        if (e.Items.Any(i => i.Id == itemId && i.IsValid)) return true;
        if (e.Misc.Any(i => i.Id == itemId && i.IsValid)) return true;

        return false;
    }

    private static TileEntityType ParseEntityType(string typeName)
    {
        if (Enum.TryParse<TileEntityType>(typeName, ignoreCase: true, out var result))
            return result;

        throw new ArgumentException($"Unknown tile entity type: {typeName}. Valid types: {string.Join(", ", Enum.GetNames(typeof(TileEntityType)))}");
    }

    private static Dictionary<string, object> EntityToDict(TileEntity e)
    {
        var dict = new Dictionary<string, object>
        {
            { "x", (int)e.PosX },
            { "y", (int)e.PosY },
            { "type", e.EntityType.ToString() },
            { "id", e.Id },
        };

        switch (e.EntityType)
        {
            case TileEntityType.DisplayDoll:
                dict["pose"] = (int)e.Pose;
                dict["poseName"] = ((DisplayDollPoseID)e.Pose).ToString();
                dict["equipment"] = ItemCollectionToList(e.Items);
                dict["dyes"] = ItemCollectionToList(e.Dyes);
                dict["weapon"] = e.Misc.Count > 0 && e.Misc[0].IsValid
                    ? TileEntityItemToDict(e.Misc[0], 0)
                    : null;
                break;

            case TileEntityType.HatRack:
                dict["items"] = ItemCollectionToList(e.Items);
                dict["dyes"] = ItemCollectionToList(e.Dyes);
                break;

            case TileEntityType.WeaponRack:
            case TileEntityType.ItemFrame:
            case TileEntityType.FoodPlatter:
            case TileEntityType.DeadCellsDisplayJar:
                dict["itemId"] = e.NetId;
                dict["prefix"] = (int)e.Prefix;
                dict["stack"] = (int)e.StackSize;
                break;

            case TileEntityType.TrainingDummy:
                dict["npc"] = (int)e.Npc;
                break;

            case TileEntityType.LogicSensor:
                dict["logicCheck"] = (int)e.LogicCheck;
                dict["on"] = e.On;
                break;

            case TileEntityType.TeleportationPylon:
                break;

            case TileEntityType.KiteAnchor:
            case TileEntityType.CritterAnchor:
                dict["itemId"] = e.NetId;
                break;
        }

        return dict;
    }

    private static List<Dictionary<string, object>> ItemCollectionToList(
        System.Collections.ObjectModel.ObservableCollection<TileEntityItem> items)
    {
        return items
            .Select((item, idx) => TileEntityItemToDict(item, idx))
            .Where(d => d != null)
            .ToList()!;
    }

    private static Dictionary<string, object>? TileEntityItemToDict(TileEntityItem item, int slot)
    {
        if (!item.IsValid) return null;

        return new Dictionary<string, object>
        {
            { "slot", slot },
            { "id", (int)item.Id },
            { "prefix", (int)item.Prefix },
            { "stack", (int)item.StackSize }
        };
    }
}
