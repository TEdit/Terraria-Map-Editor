using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TEdit.Common.IO;

namespace TEdit.Terraria;

/// <summary>
/// Serializes/deserializes mod NBT data for undo buffers and clipboard files.
/// Appended after vanilla chest/sign/tile-entity data with a magic marker.
/// </summary>
public static class ModDataSerializer
{
    /// <summary>Magic marker "TNBT" identifying the start of a mod NBT payload.</summary>
    public static readonly byte[] Magic = { 0x54, 0x4E, 0x42, 0x54 }; // "TNBT"

    /// <summary>Current NBT payload format version.</summary>
    public const int CurrentVersion = 1;

    /// <summary>
    /// Writes mod NBT payload (chest items + tile entities) to the stream.
    /// Call after World.SaveTileEntities().
    /// </summary>
    public static void SaveModPayload(BinaryWriter bw, IList<Chest> chests, IList<TileEntity> entities)
    {
        var payload = BuildPayload(chests, entities);
        if (payload == null) return; // nothing to write

        bw.Write(Magic);
        bw.Write(CurrentVersion);
        TagIO.ToStream(payload, bw.BaseStream, compressed: false);
    }

    /// <summary>
    /// Reads mod NBT payload from the stream if present.
    /// Call after World.LoadTileEntityData(). Applies mod data back to the loaded chests/entities.
    /// Returns true if payload was found and applied.
    /// </summary>
    public static bool LoadModPayload(BinaryReader br, IList<Chest> chests, IList<TileEntity> entities)
    {
        if (!TryReadMagic(br)) return false;

        int version = br.ReadInt32();
        if (version < 1) return false;

        var payload = TagIO.FromStream(br.BaseStream, compressed: false);
        ApplyPayload(payload, chests, entities);
        return true;
    }

    private static bool TryReadMagic(BinaryReader br)
    {
        try
        {
            var stream = br.BaseStream;
            if (stream.Position + 4 > stream.Length) return false;

            byte[] marker = br.ReadBytes(4);
            if (marker.Length == 4 &&
                marker[0] == Magic[0] &&
                marker[1] == Magic[1] &&
                marker[2] == Magic[2] &&
                marker[3] == Magic[3])
            {
                return true;
            }

            // Not our marker — seek back
            stream.Position -= marker.Length;
            return false;
        }
        catch
        {
            return false;
        }
    }

    internal static TagCompound BuildPayload(IList<Chest> chests, IList<TileEntity> entities)
    {
        var chestMods = BuildChestMods(chests);
        var entityMods = BuildEntityMods(entities);

        if (chestMods.Count == 0 && entityMods.Count == 0) return null;

        var payload = new TagCompound();
        payload.Set("v", CurrentVersion);
        if (chestMods.Count > 0) payload.Set("chestMods", chestMods);
        if (entityMods.Count > 0) payload.Set("entityMods", entityMods);
        return payload;
    }

    private static List<TagCompound> BuildChestMods(IList<Chest> chests)
    {
        var result = new List<TagCompound>();
        for (int i = 0; i < chests.Count; i++)
        {
            var chest = chests[i];
            var modItems = BuildModItems(chest.Items);
            if (modItems.Count == 0) continue;

            var entry = new TagCompound();
            entry.Set("idx", i);
            entry.Set("items", modItems);
            result.Add(entry);
        }
        return result;
    }

    private static List<TagCompound> BuildEntityMods(IList<TileEntity> entities)
    {
        var result = new List<TagCompound>();
        for (int i = 0; i < entities.Count; i++)
        {
            var te = entities[i];
            bool hasEntityMod = te.ModItemData != null || te.ModGlobalData != null ||
                                !string.IsNullOrEmpty(te.ModName);
            var modItems = BuildModItems(te.Items);

            if (!hasEntityMod && modItems.Count == 0) continue;

            var entry = new TagCompound();
            entry.Set("idx", i);
            if (!string.IsNullOrEmpty(te.ModName)) entry.Set("mn", te.ModName);
            if (!string.IsNullOrEmpty(te.ModItemName)) entry.Set("mi", te.ModItemName);
            if (!string.IsNullOrEmpty(te.ModPrefixMod)) entry.Set("pm", te.ModPrefixMod);
            if (!string.IsNullOrEmpty(te.ModPrefixName)) entry.Set("pn", te.ModPrefixName);
            if (te.ModItemData != null) entry.Set("id", te.ModItemData.Clone());
            if (te.ModGlobalData != null)
                entry.Set("gd", te.ModGlobalData.Select(t => t?.Clone()).Where(t => t != null).ToList());
            if (modItems.Count > 0) entry.Set("items", modItems);
            result.Add(entry);
        }
        return result;
    }

    private static List<TagCompound> BuildModItems(IList<Item> items)
    {
        var result = new List<TagCompound>();
        if (items == null) return result;

        for (int s = 0; s < items.Count; s++)
        {
            var item = items[s];
            if (item == null) continue;
            bool hasMod = item.ModItemData != null || item.ModGlobalData != null ||
                          !string.IsNullOrEmpty(item.ModName);
            if (!hasMod) continue;

            var tag = new TagCompound();
            tag.Set("s", s);
            if (!string.IsNullOrEmpty(item.ModName)) tag.Set("mn", item.ModName);
            if (!string.IsNullOrEmpty(item.ModItemName)) tag.Set("mi", item.ModItemName);
            if (!string.IsNullOrEmpty(item.ModPrefixMod)) tag.Set("pm", item.ModPrefixMod);
            if (!string.IsNullOrEmpty(item.ModPrefixName)) tag.Set("pn", item.ModPrefixName);
            if (item.ModItemData != null) tag.Set("id", item.ModItemData.Clone());
            if (item.ModGlobalData != null)
                tag.Set("gd", item.ModGlobalData.Select(t => t?.Clone()).Where(t => t != null).ToList());
            result.Add(tag);
        }
        return result;
    }

    private static List<TagCompound> BuildModItems(IList<TileEntityItem> items)
    {
        var result = new List<TagCompound>();
        if (items == null) return result;

        for (int s = 0; s < items.Count; s++)
        {
            var item = items[s];
            if (item == null) continue;
            bool hasMod = item.ModItemData != null || item.ModGlobalData != null ||
                          !string.IsNullOrEmpty(item.ModName);
            if (!hasMod) continue;

            var tag = new TagCompound();
            tag.Set("s", s);
            if (!string.IsNullOrEmpty(item.ModName)) tag.Set("mn", item.ModName);
            if (!string.IsNullOrEmpty(item.ModItemName)) tag.Set("mi", item.ModItemName);
            if (!string.IsNullOrEmpty(item.ModPrefixMod)) tag.Set("pm", item.ModPrefixMod);
            if (!string.IsNullOrEmpty(item.ModPrefixName)) tag.Set("pn", item.ModPrefixName);
            if (item.ModItemData != null) tag.Set("id", item.ModItemData.Clone());
            if (item.ModGlobalData != null)
                tag.Set("gd", item.ModGlobalData.Select(t => t?.Clone()).Where(t => t != null).ToList());
            result.Add(tag);
        }
        return result;
    }

    private static void ApplyPayload(TagCompound payload, IList<Chest> chests, IList<TileEntity> entities)
    {
        var chestMods = payload.GetList<TagCompound>("chestMods");
        foreach (var cm in chestMods)
        {
            int idx = cm.GetInt("idx");
            if (idx < 0 || idx >= chests.Count) continue;
            ApplyModItems(cm.GetList<TagCompound>("items"), chests[idx].Items);
        }

        var entityMods = payload.GetList<TagCompound>("entityMods");
        foreach (var em in entityMods)
        {
            int idx = em.GetInt("idx");
            if (idx < 0 || idx >= entities.Count) continue;

            var te = entities[idx];
            if (em.ContainsKey("mn")) te.ModName = em.GetString("mn");
            if (em.ContainsKey("mi")) te.ModItemName = em.GetString("mi");
            if (em.ContainsKey("pm")) te.ModPrefixMod = em.GetString("pm");
            if (em.ContainsKey("pn")) te.ModPrefixName = em.GetString("pn");
            if (em.ContainsKey("id")) te.ModItemData = em.GetCompound("id");
            if (em.ContainsKey("gd")) te.ModGlobalData = em.GetList<TagCompound>("gd").ToList();
            ApplyModItems(em.GetList<TagCompound>("items"), te.Items);
        }
    }

    private static void ApplyModItems(List<TagCompound> modItems, IList<Item> items)
    {
        if (modItems == null || items == null) return;
        foreach (var tag in modItems)
        {
            int slot = tag.GetInt("s");
            if (slot < 0 || slot >= items.Count) continue;

            var item = items[slot];
            if (tag.ContainsKey("mn")) item.ModName = tag.GetString("mn");
            if (tag.ContainsKey("mi")) item.ModItemName = tag.GetString("mi");
            if (tag.ContainsKey("pm")) item.ModPrefixMod = tag.GetString("pm");
            if (tag.ContainsKey("pn")) item.ModPrefixName = tag.GetString("pn");
            if (tag.ContainsKey("id")) item.ModItemData = tag.GetCompound("id");
            if (tag.ContainsKey("gd")) item.ModGlobalData = tag.GetList<TagCompound>("gd").ToList();
        }
    }

    private static void ApplyModItems(List<TagCompound> modItems, IList<TileEntityItem> items)
    {
        if (modItems == null || items == null) return;
        foreach (var tag in modItems)
        {
            int slot = tag.GetInt("s");
            if (slot < 0 || slot >= items.Count) continue;

            var item = items[slot];
            if (tag.ContainsKey("mn")) item.ModName = tag.GetString("mn");
            if (tag.ContainsKey("mi")) item.ModItemName = tag.GetString("mi");
            if (tag.ContainsKey("pm")) item.ModPrefixMod = tag.GetString("pm");
            if (tag.ContainsKey("pn")) item.ModPrefixName = tag.GetString("pn");
            if (tag.ContainsKey("id")) item.ModItemData = tag.GetCompound("id");
            if (tag.ContainsKey("gd")) item.ModGlobalData = tag.GetList<TagCompound>("gd").ToList();
        }
    }
}
