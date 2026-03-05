# World Generation Scripting API

TEdit's scripting engine exposes a `generate` object for procedural world creation. All methods are undo-tracked and can be called from JavaScript or Lua scripts.

## Feature Tracker

Comparison of TEdit worldgen features vs Terraria's worldgen:

| Feature | Terraria | TEdit Status | Notes |
|---------|----------|-------------|-------|
| Surface terrain profile | TerrainPass random walk | Done | JS-side in showcase |
| Dirt/stone fill | Column fill | Done | |
| Rock/dirt scatter | TileRunner blobs | Done | |
| Small holes | TileRunner clear | Done | |
| Dirt/rock layer caves | TileRunner clear | Done | |
| Surface caves | Vertical shafts | Done | |
| Grass spreading | Surface detection | Done | JS-side |
| Ore veins (Cu/Fe/Ag/Au) | TileRunner per zone | Done | |
| Clay/silt patches | TileRunner | Done | |
| Water lakes | Lakinater | Done | |
| Lava pools | Lakinater | Done | |
| Honey pockets | Lakinater | Done | |
| Oceans (exponential depth) | TuneOceanDepth | Done | |
| Ice biome (trapezoid shape) | Per-row random walk | Done | Shape param: trapezoid default |
| Jungle biome (rectangle+dither) | TileRunner blob, full vertical | Done | Shape: rectangle, dithered edges (15 tiles) |
| Desert biome (ellipse) | sqrt(1-t^4) dome + tall ellipse | Done | Shape param: ellipse default |
| Mushroom biome (ellipse) | ShroomPatch ellipse | Done | Shape param: ellipse default |
| Corruption (diagonal V) | ChasmRunner | Done | Shape param: diagonalLeft default |
| Crimson (diagonal V) | CrimStart + veins | Done | Shape param: diagonalRight default |
| Hallow (diagonal) | GERunner diagonal | Done | Shape param: diagonalLeft default |
| Underworld (hell) | Ash ceiling/lava floor | Done | Connects to stone layer at lavaLine |
| Dungeon (branching) | Room-corridor crawl | Done | BFS room layout with branching corridors |
| Jungle trees | Living mahogany | Done | "jungle" tree type (tile 596) |
| Mushroom trees | Giant shrooms | Done | "mushroom" tree type in biome |
| Marble caves | TileRunner marble | Done | |
| Granite caves | TileRunner granite | Done | |
| Spider caves | Cobweb + spider wall | Done | |
| Beehives | Elliptical hive shell | Done | |
| Pyramids | Sandstone brick | Done | |
| Living trees | Living wood trunk | Done | |
| Underground houses | Room with walls | Done | |
| Jungle temple | Lihzahrd brick grid | Done | |
| Vines (forest/jungle/hallow) | Hanging from grass | Done | |
| Plants on grass | Random frame variants | Done | |
| Clay pots | Random scatter | Done | |
| Stalactites/stalagmites | On stone/ice surfaces | Done | |
| Life crystals | 2x2 placement | Done | |
| Sunflowers | On grass surface | Done | |
| Terrain smoothing | Auto-slope edges | Done | |
| Thorns (corruption/crimson) | On evil grass | Done | |
| Dart traps | Wall-mounted | Done | |
| Floating islands | Sky islands | Phase 2 | |
| NPC houses | Surface village | Phase 2 | |
| Enchanted sword shrine | Buried shrine | Phase 2 | |
| Hell houses/fortresses | Ash brick buildings | Phase 2 | |
| Gem caves | Gemstone-lined | Phase 2 | |
| Shimmer lake | Underground shimmer | Phase 2 | |
| Hardmode ore altars | Ore redistribution | Phase 2 | |
| Fossil deposits | Desert fossils | Phase 2 | |
| Cobalt/mythril/adamantite veins | Hardmode ores | Phase 2 | |
| Chlorophyte veins | Jungle deep ore | Phase 2 | |
| Surface cabin/shelter | Wooden structure | Phase 2 | |
| Demon altars | Evil biome altar | Phase 2 | |
| Shadow orbs / Crimson hearts | Breakable orbs | Phase 2 | |
| Dungeon furniture | Bookshelves, spikes | Phase 2 | |
| Dungeon chests | Locked golden chests | Phase 2 | |
| Surface dunes | Wind-shaped sand hills | Phase 2 | |
| Jungle shrines | Underground shrine | Phase 2 | |
| Ice biome chests | Frozen chests | Phase 2 | |
| Glowing moss caves | Moss-covered caves | Phase 2 | |
| Bee larva in hives | Spawn point for Queen Bee | Phase 2 | |
| Ash trees | Underworld trees | Phase 2 | |
| Coral reef | Ocean floor coral clusters | Phase 2 | |

---

## Biome Shapes

All biome methods accept an optional `shape` parameter controlling the region geometry. Available shapes:

| Shape | Description | Best For |
|-------|-------------|----------|
| `"rectangle"` | Axis-aligned box | Simple regions |
| `"ellipse"` | Elliptical distance check with edge jitter | Jungle, mushroom |
| `"trapezoid"` | Wider at bottom, narrower at top | Ice biome |
| `"diagonalLeft"` | Parallelogram slanting left | Corruption, hallow V-strip |
| `"diagonalRight"` | Parallelogram slanting right | Crimson V-strip |
| `"hemisphere"` | Dome/semicircle at top, rectangle below | Desert surface mound |

Each shape adds per-tile random jitter at the boundary for natural edges.

---

The API is implemented as a partial class split across five files in `src/TEdit/Scripting/Api/`:

| File | Category | Methods |
|------|----------|---------|
| `GenerateApi.cs` | Trees & core | `tree`, `forest`, `forestInSelection`, `listTreeTypes` |
| `GenerateApi.WorldGen.cs` | Terrain primitives | `tileRunner`, `tunnel`, `lake`, `oreVein`, `findSurface`, `listOreTypes` |
| `GenerateApi.Biomes.cs` | Biome regions | `iceBiome`, `corruption`, `crimson`, `hallow`, `mushroomBiome`, `marbleCave`, `graniteCave`, `spiderCave` |
| `GenerateApi.Structures.cs` | Structures | `ocean`, `desert`, `jungle`, `underworld`, `beehive`, `pyramid`, `livingTree`, `dungeon`, `jungleTemple`, `undergroundHouse` |
| `GenerateApi.Deco.cs` | Decoration | `placeVines`, `placePlants`, `placePots`, `placeStalactites`, `placeTraps`, `placeLifeCrystals`, `smoothWorld`, `placeSunflowers`, `placeThorns` |

---

## Trees

### `tree(type, x, y) → bool`

Place a single tree at base coordinate (x, y). The y coordinate is the ground level — the tree grows upward.

**Parameters:**
- `type` (string) — tree type name (case-insensitive)
- `x`, `y` (int) — base position (ground level)

**Returns:** `true` if placed successfully.

### `forest(types, x, y, w, h, density?) → int`

Place multiple trees within a rectangle. Picks randomly from the types array. Minimum 4-tile spacing between trunks.

**Parameters:**
- `types` (string[]) — array of tree type names
- `x`, `y`, `w`, `h` (int) — bounding rectangle
- `density` (double, default 0.15) — 0.0–1.0 coverage density

**Returns:** count of trees placed.

### `forestInSelection(types, density?) → int`

Same as `forest` but uses the current selection rectangle.

### `listTreeTypes() → object[]`

Returns all supported tree types with their tile IDs.

**Supported tree types:**

| Name | Tile ID | Height Range |
|------|---------|-------------|
| oak | 5 | 5–16 |
| jungle | 596 | 5–16 |
| palm | 323 | 10–20 |
| mushroom | 72 | 5–16 |
| topaz | 583 | 7–12 |
| amethyst | 584 | 7–12 |
| sapphire | 585 | 7–12 |
| emerald | 586 | 7–12 |
| ruby | 587 | 7–12 |
| diamond | 588 | 7–12 |
| amber | 589 | 7–12 |
| sakura | 596 | 7–12 |
| willow | 616 | 7–12 |
| ash | 634 | 7–12 |

---

## Terrain Primitives

### `tileRunner(x, y, strength, steps, tileType, speedX?, speedY?)`

Port of Terraria's `WorldGen.TileRunner`. A wandering painter that fills diamond-shaped blobs. Starts at (x, y), takes `steps` iterations. Each step fills a radius that tapers linearly from `strength` to 0. Skips frame-important tiles (furniture, plants).

**Parameters:**
- `x`, `y` (int) — starting position
- `strength` (double) — initial blob radius
- `steps` (int) — number of iterations
- `tileType` (int) — tile ID to place
- `speedX`, `speedY` (double, default 0.0) — initial velocity bias

### `tunnel(x, y, strength, steps, speedX?, speedY?)`

Same algorithm as `tileRunner` but clears tiles instead of placing them, creating natural-looking cave tunnels.

### `lake(x, y, liquidType?, strength?)`

Port of Terraria's `WorldGen.Lakinater`. Creates an irregular liquid pool in two passes: carves an irregular cavity, then fills liquid from the bottom up (TEdit has no liquid physics).

**Parameters:**
- `x`, `y` (int) — center position
- `liquidType` (string, default "water") — "water", "lava", "honey", or "shimmer"
- `strength` (double, default 1.0) — size multiplier (20% chance of 1.5x bonus)

### `oreVein(oreName, x, y, size?)`

Convenience wrapper around `tileRunner` with named ore types and preset strength/steps.

**Parameters:**
- `oreName` (string) — ore name (case-insensitive)
- `x`, `y` (int) — center position
- `size` (string, default "medium") — "small" (0.5x), "medium" (1.0x), or "large" (2.0x)

**Supported ore types:**

| Name | Tile ID | Strength | Steps |
|------|---------|----------|-------|
| copper | 7 | 4.0 | 10 |
| tin | 166 | 4.0 | 10 |
| iron | 6 | 4.0 | 12 |
| lead | 167 | 4.0 | 12 |
| silver | 9 | 4.5 | 12 |
| tungsten | 168 | 4.5 | 12 |
| gold | 8 | 5.0 | 14 |
| platinum | 169 | 5.0 | 14 |
| meteorite | 37 | 6.0 | 15 |
| hellstone | 58 | 6.0 | 15 |
| cobalt | 107 | 5.0 | 14 |
| palladium | 221 | 5.0 | 14 |
| mythril | 108 | 6.0 | 16 |
| orichalcum | 222 | 6.0 | 16 |
| adamantite | 111 | 7.0 | 18 |
| titanium | 223 | 7.0 | 18 |
| chlorophyte | 211 | 6.0 | 16 |
| luminite | 408 | 8.0 | 20 |

### `findSurface(x, yStart, yEnd) → int`

Scans downward from `yStart` to `yEnd` at column `x`, returns the y-coordinate of the first active solid tile. Returns -1 if no surface found.

### `listOreTypes() → object[]`

Returns all supported ore types with their tile IDs.

---

## Biomes

All biome methods accept an optional `shape` parameter (see [Biome Shapes](#biome-shapes) above) controlling region geometry. Each biome has a sensible default shape.

### `iceBiome(x, y, w, h, shape?) → int`

Convert tiles in a region to ice/snow variants. Default shape: `"trapezoid"` (wider underground).

**Tile conversions:** stone → ice (161), dirt → snow (147), grass → snow, mud → slush (224), sand → hardened sand (397). Walls: dirt wall (2) → snow wall (40). Also places slush blobs and ice cave blobs for texture.

**Terraria reference:** Generated by 12+ passes in WorldGen.cs. Shape uses per-row random walk with ±4/±5 drift, smoothed against previous row. Spans from worldSurface (snowTop) to lavaLine-140 (snowBottom). Sub-structures include thin ice over water, exposed gems, icicle stalactites, ice small piles, and frozen chests.

### `corruption(x, y, w, depth, shape?) → int`

Apply Corruption biome with chasms. Default shape: `"diagonalLeft"`.

**Tile conversions:** stone → ebonstone (25), dirt → ebonstone, grass → corrupt grass (23), sand → ebonsand (112), ice → purple ice (163). Walls: dirt → ebonstone wall (3).

**Terraria reference:** Generated by `GenPassNameID.CorruptionAndCrimson`. Surface strip ~200–600 tiles wide. Vertical chasms via `ChasmRunner` (150+rng(150) steps), with side branches at worldSurface+20. Sub-structures: Shadow Orbs (tile 31, 2×2 at chasm bottom), Demon Altars (tile 26 style 0). Decorations: Deathweed plants (tile 24), corrupt vines (tile 636), vine thorns (tile 22) at chasm tips.

### `crimson(x, y, w, depth, shape?) → int`

Apply Crimson biome with organic chasms. Default shape: `"diagonalRight"`.

**Tile conversions:** stone → crimstone (203), dirt → crimstone, grass → crimson grass (199), sand → crimsand (234), ice → red ice (200). Walls: dirt → crimstone wall (83).

**Terraria reference:** Generated by `CrimStart` → `CrimVein` → `CrimPlaceHearts`. Surface strip same width as corruption. Underground: `CrimStart` creates a descent from surface (radius 15–25 tiles) to underground chamber (50 iterations of radius 40–55 blobs). 5–8 `CrimVein` calls radiate from chamber (radius 15–25, length 100–150 steps). Crimson Hearts (tile 31 frameX=36) placed at vein endpoints. Decorations: vicious mushroom plants (tile 201), crimson vines (tile 205).

### `hallow(x, y, w, h, shape?) → int`

Apply Hallow biome. Default shape: `"diagonalLeft"`.

**Tile conversions:** stone → pearlstone (117), grass → hallowed grass (109), sand → pearlsand (116), ice → pink ice (164), dirt → hallowed dirt (477). Walls: cave walls → hallow cave wall (28).

**Terraria reference:** NOT generated during initial worldgen — created by hardmode activation (`initializeHardMode`). `GERunner(i, 0, speedX, speedY, good=true)` creates a diagonal V-arm of radius 200–250 tiles sweeping through the world. Also converts corrupt/crimson tiles back to natural or hallowed variants. No unique sub-structures — crystal features grow organically in hardmode. Hallowed vines (tile 382) placed from dirt wall caves.

### `mushroomBiome(x, y, w, h, shape?) → int`

Create a glowing mushroom biome. Default shape: `"ellipse"`. Converts dirt/stone to mud, places mushroom grass on exposed surfaces, adds mushroom walls even in air tiles.

**Terraria reference:** Generated by `ShroomPatch` (called 6× per biome location). Uses elliptical loop with Y-distance multiplied by 2.3× (oblate ellipse — ~4–5× wider than tall). Horizontal radius 80–100, vertical radius 20–26, world-size scaled. Center drifts slightly upward during generation. Inner 20% carved to air + mushroom wall (80). 20%–40% filled with mud (59). Mud veins via `TileRunner` with downward bias. Giant mushroom trees (type 5) grown from mushroom grass (70). Placed at rockLayer+50 to maxTilesY-300. Up to 50 per world.

### `jungle(x, y, w, h, shape?, ditherWidth?) → int`

Generate a jungle biome. Default shape: `"rectangle"` — fills entire vertical space like Terraria's jungle. Edges use dithering (probability falloff) instead of hard boundaries.

**Parameters:**
- `ditherWidth` (int, default 15) — width of the dithered edge zone in tiles. Set to 0 for hard edges.

**Tile conversions:** stone/dirt/grass → mud (59), exposed mud → jungle grass (60). Walls: dirt wall → mud wall (15). Carves numerous jungle caves.

**Terraria reference:** ONE continuous biome from surface to near Underworld. Main `TileRunner` call: mud(59), strength=400–600×worldScale, steps=10000, speedY=-20, noYChange=true — shoots UPWARD from (maxTilesY+rockLayer)/2, creating massive organic blob edges. Sub-structures: Beehives (hive 225, hive wall 86), Jungle Shrines, Lihzahrd Temple (tile 226, wall 87), Living Mahogany Trees (tile 383/384). Decorations: Jungle Plants (tile 61), Jungle Vines (tile 62), 2×2 Jungle Plants (tile 233), surface water pools. Tile types: Mud (59), Jungle Grass (60), Living Mahogany (383/384), Hive (225), Lihzahrd (226).

### `desert(x, y, w, h, shape?) → int`

Generate a desert biome. Default shape: `"ellipse"` (tall ellipse spanning the map height). Sand on surface, hardened sand in middle, sandstone underground.

**Tile conversions:** surface → sand (53), middle → hardened sand (397), deep → sandstone (396). Walls: sandstone wall (187).

**Terraria reference:** Surface dome uses exact formula `sqrt(1 - t^4)` from SandMound.cs. For 4200-wide world: 320 tiles wide, 510–680 tiles tall (EXTREMELY deep). Three vertical zones: Surface SandMound dome → Desert transition → Underground DesertHive (Voronoi/metaball cluster algorithm). 4 entrance types: Chambers, Anthill, LarvaHole, Pit (33% chance each). Sub-structures: Pyramid, Oasis, Desert Houses (chest style 10), Fossil deposits (tile 404). Tile types: Sand (53), Sandstone (396), Hardened Sand (397), Fossil Ore (404). Wall types: Sandstone Wall (187), Hardened Sand Wall (216).

### `marbleCave(x, y, strength?)`

Create a marble cave using wandering blobs of marble tile (367) and marble walls (178) with carved interior caves.

- `strength` (double, default 40.0) — overall size

### `graniteCave(x, y, strength?)`

Create a granite cave with granite tiles (368), granite walls (180), and carved interior. More internal caves than marble.

- `strength` (double, default 40.0) — overall size

### `spiderCave(x, y, strength?)`

Create a spider cave: a small cave filled with cobwebs (51) and spider walls (62).

- `strength` (double, default 10.0) — overall size

---

## Structures

### `ocean(direction?, oceanWidth?, maxDepth?) → int`

Generate an ocean biome with exponential depth progression (port of Terraria's `TuneOceanDepth`). Sand floor slopes down exponentially from beach to deep water. Auto-detects world edges and baseline surface level.

**Parameters:**
- `direction` (int, default 1) — `-1` for left/west edge, `1` for right/east edge
- `oceanWidth` (int, default ~15% of world width) — how many tiles wide
- `maxDepth` (int, default 80) — maximum ocean depth in tiles

**Features:**
- Exponential depth curve: `depth = maxDepth × (1 - e^(-3.5 × distFromBeach))`
- Sand jitter (±4/column, clamped ±8) for natural floor
- Sand extends downward to seal any gaps/caves from pre-existing terrain
- Coral scattered on the ocean floor
- Water fills from sea level to sand floor

### `desert(x, y, w, h, shape?) → int`

Generate a desert biome: sand on surface, hardened sand in middle, sandstone underground. Default shape: `"ellipse"` (tall ellipse spanning map height). Adds sandstone walls and carves desert caves.

### `jungle(x, y, w, h, shape?, ditherWidth?) → int`

Generate a jungle biome. Default shape: `"rectangle"` — fills entire vertical space like Terraria's jungle. Edges use dithering (probability falloff over `ditherWidth` tiles, default 15) instead of hard boundaries. Converts existing terrain to mud/jungle grass, adds mud walls, and carves numerous jungle caves.

### `underworld(yStart?) → int`

Generate the underworld (hell) layer with natural features.

**Parameters:**
- `yStart` (int, default auto) — top of underworld (default: lavaLine at 80% of world height, connecting seamlessly to stone layer)

**Six-pass algorithm:**
1. **Ceiling/floor profiles** — Random walk ceiling (±3/column, clamped ±30 from center) and lava floor (±10/column)
2. **Ash fill** — Ash blocks between ceiling and world bottom
3. **Cave carving** — Large horizontal tunnels + smaller texture caves + chimney openings (1-in-50 column chance, 3–7 tiles wide, 30–80 tiles tall)
4. **Hellstone veins** — Scattered throughout ash via `tileRunner`
5. **Obsidian** — Placed near lava level via `tileRunner`
6. **Lava fill** — Fills all open spaces below the lava floor level

**Terraria reference:** Generated by 5-pass layer structure. Ash ceiling starts at maxTilesY-150..190 (varies by column via random walk). Lava floor at maxTilesY-40..70. Protrusions from ceiling/floor narrow the open space. Hellstone (tile 58) and obsidian (tile 56) veins scattered through ash (tile 57). Lava buffer fills remaining air below lava floor. HellFort structures: 5×10 room grid of obsidian brick buildings with platforms, doors, and furniture. Ash trees (hardmode) grow on outer 17% of world width. Tile types: Ash (57), Hellstone (58), Obsidian (56), Hellstone Brick (75), Obsidian Brick (76). Wall types: Obsidian Brick Wall (14), Lava Unsafe Wall (136).

### `beehive(x, y, size?)`

Create a beehive structure: elliptical hive block shell with hive walls, hollowed interior filled with honey (lower 60%).

- `size` (int, default 15–30 random) — radius

### `pyramid(x, y, height?)`

Create a sandstone pyramid with an internal diagonal corridor leading to a treasure room.

- `height` (int, default 40–80 random) — pyramid height from tip

### `livingTree(x, y, height?)`

Create a living tree: large hollow trunk with living wood walls, root system extending downward, and leaf block canopy.

- `height` (int, default 40–80 random) — trunk height

### `dungeon(x, y, direction?, style?)`

Generate a dungeon with branching room-and-corridor layout. Entrance shaft from surface, BFS room placement with 1-3 corridors per room. Corridors branch horizontally and vertically.

- `direction` (int, default 1) — `-1` for left, `1` for right
- `style` (int, default 0) — `0` = blue, `1` = green, `2` = pink

**Terraria reference:** Generated by `DungeonCrawler.MakeDungeon` using a sequential walk algorithm. Places 34–42 rooms via `DungeonRoom` with random direction changes (left/right/down). Room types: Regular (5×5–15×8), Wormlike (serpentine), LivingTree (organic hollows), BiomeSquare (large rectangular), L-shaped variants. Hall types: Normal (4–5 wide), Long, Tall, Intersection. Entrance always a vertical shaft from surface. Three dungeon colors (blue/green/pink) with distinct tile+wall pairs: Blue Dungeon Brick (41)/Blue Wall (7), Green Dungeon Brick (43)/Green Wall (8), Pink Dungeon Brick (44)/Pink Wall (9). 13+ global feature passes after room placement: spikes (tile 48), doors (tile 10), platforms (tile 19), bookshelves (tile 101), chairs/tables, dungeon chests (locked golden, tile 21 style 2), lanterns, banners, chandeliers, water fountains, cracked dungeon bricks, cobwebs (tile 51). Dungeon Guardian spawns if player enters before Skeletron defeat.

### `jungleTemple(x, y, w?, h?)`

Create a simplified jungle temple: lihzahrd brick maze with carved rooms and doorways.

- `w` (int, default 80–150 random) — width
- `h` (int, default 60–100 random) — height

### `undergroundHouse(x, y, style?)`

Place a simple underground house (room with walls, platforms, and interior).

- `style` (int, default 0) — `0` = wood, `1` = stone, `2` = dungeon

---

## Decoration

### `placeVines(x, y, w, h, biome?) → int`

Place vines hanging from grass surfaces. ~60% chance per eligible grass tile, 2–10 tiles long.

- `biome` (string, default "forest") — "forest", "jungle", "hallow", or "crimson"

### `placePlants(x, y, w, h, biome?) → int`

Place random small plants on grass surfaces. ~30% chance per eligible tile, random frame variants.

- `biome` (string, default "forest") — "forest", "jungle", "hallow", "corruption", "crimson", or "mushroom"

### `placePots(x, y, w, h, count?) → int`

Place clay pots on solid surfaces. Random placement with retry.

- `count` (int, default area/2000) — target count

### `placeStalactites(x, y, w, h, count?) → int`

Place stalactites (hanging from stone/ice ceilings) and stalagmites (growing from stone/ice floors).

- `count` (int, default area/1000) — target count

### `placeTraps(x, y, w, h, count?) → int`

Place dart traps on walls facing open space.

- `count` (int, default area/5000) — target count

### `placeLifeCrystals(x, y, w, h, count?) → int`

Place 2x2 life crystals on solid surfaces with 2x2 empty space above.

- `count` (int, default area/10000) — target count

### `smoothWorld(x, y, w, h) → int`

Auto-slope exposed tile edges for natural-looking terrain. Applies half-bricks and slopes based on neighboring tile layout. Skips frame-important tiles.

### `placeSunflowers(x, y, w, h, count?) → int`

Place 2-wide sunflowers on grass surfaces.

- `count` (int, default w/30) — target count

### `placeThorns(x, y, w, h, biome?) → int`

Place thorns growing upward from corruption/crimson grass. ~25% chance, 1–5 tiles tall.

- `biome` (string, default "corruption") — "corruption" or "crimson"

---

## Example: Worldgen Showcase

The included `worldgen-showcase.js` script demonstrates a complete 19-step world generation pipeline:

1. **Terrain profile** — TerrainPass port with feature-segment random walk (plateau, hill, dale, mountain, valley)
2. **Fill terrain** — Dirt and stone layers following the height profile
3. **Scatter blobs** — Stone-in-dirt, dirt-in-rocks, mud pockets
4. **Small holes** — WorldGen SmallHoles pass (~0.15% of area)
5. **Dirt layer caves** — Caves in the dirt layer
6. **Rock layer caves** — Caves in the stone layer
7. **Surface caves** — Vertical shafts and horizontal tunnels
8. **Grass spreading** — Grass on exposed dirt surfaces
9. **Ore veins** — Copper, iron, silver, gold across depth zones
10. **Clay and silt** — Surface clay and deep silt patches
11. **Water lakes** — Underground water bodies
12. **Lava pools** — Deep cavern lava lakes
13. **Honey pockets** — Small honey pools
14. **Underworld** — Hell layer with ceiling, lava floor, hellstone, chimneys
15. **Oceans** — Left and right world edges with exponential depth
16. **Biomes** — Ice (trapezoid), jungle (rectangle+dither), desert (ellipse), corruption (diagonalLeft), crimson (diagonalRight), hallow (diagonalLeft), mushroom (ellipse), marble, granite, spider
17. **Structures** — Beehives, underground houses
18. **Forest** — Surface trees (oak, sakura, willow), jungle trees, mushroom trees
19. **Decoration** — Vines, plants, pots, stalactites, life crystals, sunflowers, thorns, traps, terrain smoothing

Run it on a new world created via **File > New World** (any size). The script uses `world.surfaceLevel` and `world.rockLevel` set by the New World dialog.
