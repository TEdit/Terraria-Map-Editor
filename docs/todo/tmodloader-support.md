# tModLoader Support - Remaining Work

Related: #1075

## Completed

- [x] .twld NBT parsing (GZip-compressed TagCompound)
- [x] Mod tile/wall overlay on world grid (virtual IDs beyond vanilla range)
- [x] Round-trip save: strip mod data before .wld save, reapply after
- [x] Both .twld formats: legacy interleaved and new dense tileData/wallData
- [x] ModScraper: extract tile/wall average colors from .tmod archives
- [x] modColors.json: pre-scraped color data for common mods
- [x] Mod tile/wall registration in WorldConfiguration (TileProperties, WallProperties)
- [x] Mod framed tile sprite placeholders in Sprites2
- [x] NBT Explorer panel for inspecting .twld data
- [x] World Explorer: Vanilla/tModLoader toggle button
- [x] Tile/wall texture extraction from .tmod archives at world-load time
- [x] Texture2D loading from PNG/rawimg bytes into XNA texture dictionary
- [x] Cascading .tmod file search (tModLoader Mods folder, Steam workshop)
- [x] Chest mod item editing: UI display, copy/paste, item textures
- [x] Chest mod items: preserve mod identity during load/save round-trip
- [x] Tile entity mod item editing: ItemFrame, WeaponRack, FoodPlatter, DeadCellsDisplayJar
- [x] Tile entity mod item editing: HatRack, DisplayDoll/Mannequin (multi-slot)
- [x] Tile entity copy/paste preserves mod item identity
- [x] Fix: sync save path missing RebuildModChestItems

## In Progress

### Texture Rendering Quality
- [ ] Verify UV-based rendering works for non-framed mod tiles (texture sheet sampling)
- [ ] Verify wall texture rendering with neighbor-based UV selection
- [ ] Test with Calamity, Thorium, and other large mods
- [ ] Texture cache on disk (`%APPDATA%/TEdit/ModTextures/{modName}_{version}/`) to avoid re-extracting every load
- [ ] User-configurable .tmod search path override in settings

## Todo

### Tile Placement
- [ ] Paint mode: place mod tiles from the tile picker dropdown
- [ ] Mod tile picker: show mod tiles grouped by mod in the brick selector
- [ ] Frame-correct placement for non-framed mod tiles (auto-framing with neighbors)
- [ ] Framed mod tile placement via sprite panel (1x1 and multi-tile)
- [ ] Mod tile property editing (color, actuator, etc.)

### Sprite Placement / Copy-Paste
- [ ] Copy/paste of mod tiles preserves virtual IDs correctly
- [ ] Copy/paste across worlds with different mod lists (remap virtual IDs)
- [ ] Mod tile entities: preserve TileEntity NBT data during undo/redo
- [ ] Multi-tile mod sprites: correct frame coordinate assignment on paste

### Mod NPC Support
- [ ] Render mod NPCs (extract NPC textures from .tmod)
- [ ] NPC editor: display/edit mod NPC positions and data
- [ ] Town NPC mod variants

### Mod World Flags / Global Data
- [ ] NBT Explorer: edit mod global data (modData section in .twld)
- [ ] World flags panel: expose mod-specific world state (boss kills, events, etc.)
- [ ] Preserve `alteredVanillaFields` section during save

### Advanced Rendering
- [ ] Glow mask overlays for mod tiles (extract `*_Glow.png` from .tmod)
- [ ] Animated mod tiles (frame cycling)
- [ ] Mod liquid rendering
- [ ] Mod tree/cactus rendering (custom tree tops, branches)

### Robustness
- [ ] Handle .tmod format changes across tModLoader versions
- [ ] Graceful handling of partially-loaded mods (some textures missing)
- [ ] Warning when world references mods not found on disk
- [ ] Validate .twld integrity before and after save
- [ ] Support worlds with 100+ mods (performance of virtual ID space)

## Architecture Notes

### Virtual ID System
Mod tiles/walls are assigned virtual IDs starting at `WorldConfiguration.TileCount` / `WorldConfiguration.WallCount`. These IDs exist only in TEdit's runtime and are not saved to .wld files. The .twld file uses its own saveType IDs mapped via tileMap/wallMap NBT entries.

### Mod Item Data Flow (Chests & Tile Entities)
The `.twld` file stores mod item identity in two NBT sections:
- `"chests"` — mod items in chests, keyed by chest (x, y) position
- `"tileEntities"` — mod items in tile entities (item frames, weapon racks, mannequins, etc.)

**Load path:** After vanilla `.wld` load, `ApplyModChestItems` and `ApplyModTileEntityItems` overlay mod data from NBT onto in-memory objects.

**Save path:** Before `.twld` save, `RebuildModChestItems` and `RebuildModTileEntityItems` serialize in-memory mod data back to NBT tags. Orphaned entries (mod-only tile entities with no vanilla counterpart) are preserved.

**Single-item entities** (ItemFrame, WeaponRack, FoodPlatter): mod fields live on `TileEntity` directly (`ModName`, `ModItemName`, etc.). Use `TileEntity.ToItem()` / `FromItem()` for copy/paste.

**Multi-slot entities** (HatRack, DisplayDoll): mod fields live on individual `TileEntityItem` instances in `Items`/`Dyes` collections.

### .twld `"tileEntities"` NBT Format
Each entry: `{ "mod": string, "name": string, "X": short, "Y": short, "data"?: TagCompound }`

| Entity | name | data format |
|--------|------|-------------|
| ItemFrame | `TEItemFrame` | `{ "item": ItemIO tag }` |
| WeaponRack | `TEWeaponsRack` | `{ "item": ItemIO tag }` |
| FoodPlatter | `TEFoodPlatter` | `{ "item": ItemIO tag }` |
| HatRack | `TEHatRack` | `{ "items": List<slotted ItemIO>, "dyes": List<slotted ItemIO> }` |
| DisplayDoll | `TEDisplayDoll` | `{ "items": List<slotted ItemIO>, "dyes": List<slotted ItemIO> }` |

**ItemIO tag:** `{ "mod": string, "name"?: string, "id"?: int, "stack": int, "prefix": byte, "modPrefixMod"?: string, "modPrefixName"?: string, "data"?: TagCompound, "globalData"?: List<TagCompound> }`

**Slotted variant:** adds `"slot": short` to identify collection index.

### Key Files
| File | Purpose |
|------|---------|
| `TEdit.Terraria/TModLoader/TwldFile.cs` | .twld load/save, virtual ID assignment, mod property registration, chest/TE item apply/rebuild |
| `TEdit.Terraria/TModLoader/TwldData.cs` | In-memory model for all .twld data |
| `TEdit.Terraria/TModLoader/TmodTextureExtractor.cs` | .tmod archive reader + texture extraction |
| `TEdit.Terraria/TModLoader/ModTileEntry.cs` | Tile map entry (mod name, tile name, saveType) |
| `TEdit.Terraria/TileEntity.cs` | Tile entity model with mod fields for single-item entities |
| `TEdit.Terraria/TileEntityItem.cs` | Tile entity item model with mod fields for multi-slot entities |
| `TEdit.Terraria/Item.cs` | Item model with mod fields, ToTileEntityItem conversion |
| `TEdit/ViewModel/WorldViewModel.cs` | Load pipeline, `LoadModTextures()`, `RegisterModSprites()` |
| `TEdit/ViewModel/WorldViewModel.Commands.cs` | Copy/paste for chest items and tile entity items |
| `TEdit/Render/Textures.cs` | Texture dictionary, `LoadTextureFromPngBytes()` |
| `TEdit/View/WorldRenderXna.xaml.cs` | XNA rendering, deferred texture loading |

### tModLoader Source Reference
- `tModLoader/patches/tModLoader/Terraria/ModLoader/IO/TileIO_Basic.cs` — dense tile format
- `tModLoader/patches/tModLoader/Terraria/ModLoader/IO/WorldIO.cs` — .twld save/load entry point
- `tModLoader/patches/tModLoader/Terraria/ModLoader/IO/TagIO.cs` — NBT format
- `tModLoader/patches/tModLoader/Terraria/ModLoader/IO/ItemIO.cs` — item serialization format
