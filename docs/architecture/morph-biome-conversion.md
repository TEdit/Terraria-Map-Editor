# Morph / Biome Conversion — Technical Reference

Related issue: #533 | Remaining work: `docs/todo/morph-tool.md`

---

## 1. TEdit Morph Tool Architecture

### 1.1 Component Overview

| Component | Location | Purpose |
|---|---|---|
| `MorphBiomeData` | `TEdit.Terraria/DataModel/MorphBiomeData.cs` | Data model for a single biome's rules |
| `MorphConfiguration` | `TEdit.Terraria/DataModel/MorphConfiguration.cs` | Root config: biome dict + moss type registry |
| `MorphBiomeDataApplier` | `TEdit.Editor/MorphBiomeDataApplier.cs` | Applies morph rules to individual tiles |
| `MorphToolOptions` | `TEdit.Editor/MorphToolOptions.cs` | Reactive UI options bound to the tool panel |
| `MorphTool` | `src/TEdit/Editor/Tools/MorphTool.cs` | Interactive brush tool (WPF) |
| `CleanseWorldPlugin` | `src/TEdit/Editor/Plugins/CleanseWorldPlugin.cs` | Whole-world Purify batch operation |
| `morphBiomes.json` | `TEdit.Terraria/Data/morphBiomes.json` | Embedded data file — all biome rules |

`TEdit.Editor` is a shared library (`net10.0`) with no WPF dependency. `MorphBiomeDataApplier` accepts `World` as a parameter, making it usable from both the WPF app and Avalonia (TEdit5).

### 1.2 Data Loading

`MorphConfiguration` is loaded from `morphBiomes.json` (embedded resource) via `TEdit.Terraria.Loaders.JsonDataLoader` and deserialized with `TEditJsonSerializer.DefaultOptions`. The result is stored in `WorldConfiguration.MorphSettings` (static) and `WorldConfiguration.Biomes` (key list exposed to the biome combo box).

```csharp
public class MorphConfiguration
{
    public Dictionary<string, MorphBiomeData> Biomes { get; set; }
    public Dictionary<string, int> MossTypes { get; set; }     // name → tile ID
    public static HashSet<ushort> NotSolidTiles { get; }        // populated by WorldConfiguration
    public bool IsMoss(ushort type) { ... }                     // cached hash set lookup
}
```

### 1.3 MorphBiomeData and MorphId

Each biome entry contains two lists: one for tile rules and one for wall rules.

```csharp
public class MorphBiomeData
{
    public string Name { get; set; }
    public List<MorphId> MorphTiles { get; set; }
    public List<MorphId> MorphWalls { get; set; }
}

public class MorphId
{
    public string Name { get; set; }          // Descriptive label (not used in logic)
    public bool Delete { get; set; }          // Remove tile/wall instead of converting
    public bool UseMoss { get; set; }         // Enable moss substitution for stone
    public MorphIdLevels Default { get; set; }      // Target IDs by depth
    public MorphIdLevels? TouchingAir { get; set; } // Override when adjacent to air/non-solid
    public MorphIdLevels? Gravity { get; set; }     // Override when tile below is air (sand physics)
    public HashSet<ushort> SourceIds { get; set; }  // Tile/wall IDs that this rule matches
    public List<MorphSpriteUVOffset> SpriteOffsets { get; set; } // UV patch rules for framed tiles
}
```

**Lookup is by source ID, not by name.** On initialization, `MorphBiomeDataApplier.InitCache()` builds two flat dictionaries (`_tileCache`, `_wallCache`) mapping each source ID to its `MorphId`. Duplicate source IDs across entries within the same biome throw `IndexOutOfRangeException`.

### 1.4 MorphIdLevels — Depth-Dependent Target IDs

```csharp
public class MorphIdLevels
{
    public ushort? EvilId { get; set; }       // Used when EnableEvilTiles=true
    public ushort? SkyId { get; set; }
    public ushort? DirtId { get; set; }
    public ushort? RockId { get; set; }
    public ushort? DeepRockId { get; set; }
    public ushort? HellId { get; set; }

    public ushort? GetId(MorphLevel level, bool useEvil)
    {
        if (useEvil && EvilId != null) return EvilId;
        // Falls through levels: Hell → DeepRock → Rock → Dirt → Sky
        // Returns null if no applicable ID is set
    }
}
```

`EvilId` takes priority over depth when `EnableEvilTiles` is enabled. This is used by the Forest biome to preserve Corruption/Crimson/Hallow variants of walls and tiles rather than converting them all to the neutral form.

### 1.5 MorphLevel Enum and ComputeMorphLevel

```csharp
public enum MorphLevel { Sky, Dirt, Rock, DeepRock, Hell }

public static MorphLevel ComputeMorphLevel(World world, int y)
{
    int dirtLayer   = (int)world.GroundLevel;
    int rockLayer   = (int)world.RockLevel;
    int deepRock    = (int)(rockLayer + (world.TilesHigh - rockLayer) / 2);
    int hell        = world.TilesHigh - 200;

    if (y > hell)      return MorphLevel.Hell;
    if (y > deepRock)  return MorphLevel.DeepRock;
    if (y > rockLayer) return MorphLevel.Rock;
    if (y > dirtLayer) return MorphLevel.Dirt;
    return MorphLevel.Sky;
}
```

This is the single canonical depth-level calculation used by both `MorphTool` and `CleanseWorldPlugin`. Previously, each caller computed depth independently, leading to inconsistent results.

### 1.6 MorphBiomeDataApplier — Per-Tile Logic

`MorphBiomeDataApplier` is cached per `MorphBiomeData` instance via a static dictionary:

```csharp
static Dictionary<MorphBiomeData, MorphBiomeDataApplier> _morpherCache = new();
public static MorphBiomeDataApplier GetMorpher(MorphBiomeData biome) { ... }
```

The `ApplyMorph` method delegates to `ApplyTileMorph` then `ApplyWallMorph`:

**Tile conversion flow:**
1. Skip if `!source.IsActive` or source type not in `_tileCache`.
2. If `morphId.Delete` and `EnableBaseTiles`: clear tile, return.
3. If `EnableBaseTiles`: resolve target ID via gravity check → touching-air check → default. Apply moss substitution if `morphId.UseMoss && EnableMoss && (IsMoss(source.Type) || TouchingAir(...))`.
4. If `EnableSprites` and tile is framed (`TileProperties[id].IsFramed`) and rule has sprite offsets: find first matching `MorphSpriteUVOffset` and apply it.

**Wall conversion flow:**
1. Skip if `source.Wall == 0` or wall ID not in `_wallCache`.
2. If `morphId.Delete`: set wall to 0, return.
3. Resolve target via gravity check → touching-air check → default.

Note: `EnableBaseTiles` guards tile-type conversion but **not** wall conversion. Wall conversion always runs when the wall is in the cache.

### 1.7 Sprite UV Offset System

```csharp
public class MorphSpriteUVOffset
{
    public short MinU { get; set; }     // Filter: tile.U must be >= MinU
    public short MaxU { get; set; }     // Filter: tile.U must be <= MaxU
    public short OffsetU { get; set; }  // Adjustment to apply to tile.U
    public short MinV { get; set; }     // Filter: tile.V >= MinV (only when UseFilterV)
    public short MaxV { get; set; }     // Filter: tile.V <= MaxV (only when UseFilterV)
    public short OffsetV { get; set; }  // Adjustment to apply to tile.V (only when UseFilterV)
    public bool UseFilterV { get; set; }
    public bool Delete { get; set; }    // Delete tile instead of offsetting
}
```

The first matching offset in the list is applied (short-circuit). When `UseFilterV` is false only the U range is checked. `Delete = true` removes the tile rather than shifting coordinates.

**Torch conversion:** Tile 4 (Torch) uses this system. UV is `style * 22` on the U axis. Offset entries translate between torch styles (e.g., normal=0, corruption=396, crimson=418, hallow=440, glowing mushroom=484, ice=198, desert=352). Each biome's torch entry maps from all-style source IDs down to a single target style.

### 1.8 MorphToolOptions

```csharp
public partial class MorphToolOptions : ReactiveObject
{
    string TargetBiome = "Purify";  // Key into WorldConfiguration.MorphSettings.Biomes
    int    MossType    = 179;       // Tile ID for moss (default: Krypton Moss)
    bool   EnableBaseTiles = true;  // Guard on tile-type conversion
    bool   EnableEvilTiles = true;  // Prefer EvilId in MorphIdLevels.GetId()
    bool   EnableMoss      = true;  // Allow stone→moss substitution
    bool   EnableSprites   = true;  // Apply SpriteOffset patches to framed tiles
}
```

### 1.9 Air Adjacency Checks

Both `AirBelow` and `TouchingAir` check all 8 neighbors using a `[ThreadStatic]` tile buffer (safe for parallel use). The `MorphConfiguration.NotSolidTiles` hash set (populated from `WorldConfiguration.TileProperties`) defines which tile IDs count as passable (platforms, furniture, etc.).

- `AirBelow(world, x, y)` — checks tile at `(x, y+1)`. Used for sand/hardened-sand gravity: if the tile below is air, sand converts to hardened sand instead of normal sand.
- `TouchingAir(world, x, y)` — checks all 8 neighbors. Used for moss exposure and some wall/tile rules.

### 1.10 MorphTool (Interactive)

`MorphTool` is a standard brush tool using `BaseTool`'s `DrawLine`/`FillSolid` infrastructure. Key behaviors:
- On `MouseDown`: resolves `_targetBiome` and `_biomeMorpher` from `MorphToolOptions.TargetBiome`.
- Uses a `CheckTiles` generation counter to skip already-visited tiles within a single drag stroke.
- Respects `_wvm.Selection.IsValid(pixel)` — morph is confined to the active selection.
- Supports all brush shapes (Square, Round, Star, Triangle, Crescent, Donut) and transforms (rotation, flip).
- Calls `BlendRules.ResetUVCache` after each tile to invalidate the render cache.

### 1.11 CleanseWorldPlugin

Iterates the entire world from bottom to top, computes `MorphLevel` per row (not per tile), and applies the Purify biome morpher. No progress reporting or confirmation dialog exists yet.

---

## 2. Terraria WorldGen.Convert() Reference

### 2.1 BiomeConversionID

Terraria defines 22 conversion type IDs. Only 8 are player-accessible via Clentaminator solutions:

| ID | Name | Solution |
|---|---|---|
| 0 | Purification | Green |
| 1 | Corruption | Purple |
| 2 | Hallow | Blue |
| 3 | Mushroom | Dark Blue |
| 4 | Crimson | Red |
| 5 | Snow | Light Blue |
| 6 | Desert | Orange |
| 7 | Forest | Brown |

The remaining IDs (8–21) are used internally by worldgen for shimmer, graveyard, underworld, and other special zones.

### 2.2 What the Game Converts

`WorldGen.Convert()` operates on a circular radius around the sprayed tile. Per tile it converts:

**Tiles (foreground blocks):**
- Grass: 2 (Forest), 23 (Corruption), 199 (Crimson), 109 (Hallow)
- Stone: 1 (Stone), 25 (Ebonstone), 203 (Crimstone), 117 (Pearlstone)
- Ice: 161 (Ice), 163 (Corrupt Ice), 200 (Crimson Ice), 164 (Hallow Ice)
- Sand: 53, 112, 234, 116
- Hardened Sand: 397, 398, 399, 402
- Sandstone: 396, 400, 401, 403
- Moss: various IDs via `IsStone` + exposure check
- Moss Brick: 512–517, 535, 537, 540, 626, 628

**Framed sprites:**
- Cave spikes (tile 165): style shifted by U offset
- Vines (tile 52, 636, 205, 115): type-swapped
- Thorns (tile 32, 352): deleted or converted
- Torches (tile 4): `U = style * 22`, style determined by biome

**Walls (background):**
- Natural stone, grass, mud, sand, sandstone, rock walls all have biome variants

**Special checks:**
- `TileIsExposedToAir` — controls moss placement (equivalent to TEdit's `TouchingAir`)
- `BlockBelowMakesSandConvertIntoHardenedSand` — equivalent to TEdit's `AirBelow`; if air is below, sand becomes hardened sand (gravity-affected form)

### 2.3 Torch Conversion Formula

In Terraria source: `tile.frameX = (short)(style * 22)` where `style` maps to:

| Style | U | Biome |
|---|---|---|
| 0 | 0 | Normal |
| 18 | 396 | Corruption |
| 19 | 418 | Crimson |
| 20 | 440 | Hallow |
| 22 | 484 | Glowing Mushroom |
| 9 | 198 | Ice |
| 16 | 352 | Desert |

TEdit's `spriteOffset` entries encode these same offsets as U-range filters with delta adjustments.

---

## 3. Current Biomes in TEdit

| Key | Notes |
|---|---|
| `Purify` | Converts evil/biome tiles to neutral. Deletes biome-specific sprites. |
| `Corruption` | Converts grass→ebongrass, stone→ebonstone, sand→corrupt sand, etc. |
| `Crimson` | Same structure as Corruption with Crimson IDs. |
| `Hallow` | Converts to hallowed variants. Does not delete thorns (converts them). |
| `GlowingMushroom` | Converts jungle grass→mushroom grass and mud walls→mushroom walls. Limited tile set. |
| `Forest` | Biome-aware: uses `EvilId` to preserve Corruption/Crimson/Hallow variants of walls. Converts sand to dirt at surface. |
| `Snow` | Converts most tiles to ice/snow variants. Deletes vines. |
| `Desert` | Uses `gravity` field for sand physics. Converts dirt to sand at surface. |

---

## 4. morphBiomes.json Data Format Reference

```jsonc
{
  "biomes": {
    "BiomeName": {
      "morphTiles": [
        {
          "name": "descriptive_label",      // not used in logic
          "delete": false,                  // if true, removes the tile (guarded by EnableBaseTiles)
          "useMoss": false,                 // if true, check EnableMoss + air exposure for moss substitution
          "default": {                      // target IDs by depth; all fields optional
            "evilId": 25,                   // returned when EnableEvilTiles=true
            "skyId": 1,                     // above GroundLevel
            "dirtId": 1,                    // GroundLevel → RockLevel
            "rockId": 1,                    // RockLevel → deepRockLayer
            "deepRockId": 1,                // deepRockLayer → hell
            "hellId": 1                     // bottom 200 rows
          },
          "touchingAir": { ... },           // same structure; overrides default when adjacent to air
          "gravity": { ... },               // same structure; overrides default when tile below is air
          "sourceIds": [1, 25, 203, 117],   // tile type IDs this rule matches
          "spriteOffsets": [                // applied to framed tiles when EnableSprites=true
            {
              "minU": 0,                    // U filter range (inclusive); default 0
              "maxU": 36,                   // U filter range (inclusive)
              "offsetU": -162,             // delta applied to tile.U
              "minV": 0,                    // V filter (only used when useFilterV=true)
              "maxV": 18,
              "offsetV": 0,
              "useFilterV": false,
              "delete": false              // delete tile instead of shifting UV
            }
          ]
        }
      ],
      "morphWalls": [
        // identical structure; delete/useMoss/touchingAir/gravity/spriteOffsets all apply
        // wall conversion is NOT guarded by EnableBaseTiles
      ]
    }
  },
  "mossTypes": {
    "Cave": 179,        // tile IDs considered "moss" for IsMoss() checks
    "Lush": 180,
    "Krypton": 381
    // ...
  }
}
```

**Fallback chain in `MorphIdLevels.GetId`:** `EvilId` (if enabled) → level-specific ID → next shallower level → ... → `SkyId` → null. A null return means "no change" for walls; for tiles it falls back to `source.Type` (no change) except in the Delete path.

**Level boundaries:**
- Sky: `y <= GroundLevel`
- Dirt: `GroundLevel < y <= RockLevel`
- Rock: `RockLevel < y <= deepRockLayer` where `deepRockLayer = rockLayer + (TilesHigh - rockLayer) / 2`
- DeepRock: `deepRockLayer < y <= TilesHigh - 200`
- Hell: `y > TilesHigh - 200`

---

## 5. Gap Analysis: TEdit vs. Terraria WorldGen.Convert()

### Recently Fixed / Added

- **Torch style conversions** — All 8 biomes now have `spriteTorch` entries using the UV offset system.
- **MossBrick tile conversions** — Tiles 512–517, 535, 537, 540, 626, 628 added to all biomes.
- **EnableBaseTiles enforcement** — Previously ignored; now correctly gates all tile-type changes.
- **CleanseWorldPlugin depth layers** — Fixed: all tiles were morphing at Sky level regardless of actual depth.
- **ComputeMorphLevel** — Extracted as a shared static helper; both MorphTool and CleanseWorldPlugin use it.
- **MorphBiomeDataApplier moved to TEdit.Editor** — No WPF dependency; usable from Avalonia/TEdit5.

### Missing vs. Game Behavior

**Jungle biome conversion** (not yet a target biome in TEdit):
- Jungle grass tile: 60 (Jungle Grass)
- Jungle-specific walls: 15 (Mud, natural), 64 (Mud, natural variant), 80 (Glowing Mushroom wall natural)
- Jungle torch style: 21 (U = 462)
- Jungle vines: tile 62 (Jungle Vines)
- Jungle thorns: tile 69 (Jungle Spikes) — would be converted, not deleted

**Dungeon biome conversion:**
- Dungeon brick tiles (41, 43, 44) and walls (7, 8, 9, 10, 94) have biome variants but no morph rules exist.

**Tree morphing:**
- Tree tile IDs vary by biome (regular trees tile 5, but type encoded in frame). Currently no tree entries in any biome.
- Correct behavior requires reading the tree's root location and applying biome-appropriate frame data.

**Masking system integration:**
- The morph tool does not consult the `TilePicker` mask settings (`TileMaskMode`/`WallMaskMode`).
- This means morphing a Jungle+Mushroom area with Purify converts both — there is no way to restrict to only Mushroom tiles.
- Issue: #1824

**Cleanse World UX:**
- No progress bar or callback during whole-world processing (issue: #913).
- No confirmation dialog showing estimated affected tile count.

**Desert morph decoration bug (issue: #1819):**
- Some underground desert rock decorations incorrectly convert to ice or mushroom variants when the target biome is Corruption or Crimson. Caused by overlapping source ID sets between Desert and Snow/Mushroom morph rules.

**Hive biome:**
- Hive blocks (tile 30) and honey blocks/walls not included in any biome conversion.

**Cave decoration handling (issue: #1053):**
- Biome-specific cave decorations (stalactites, gems embedded in stone) are not morphed with the rest of the biome.
