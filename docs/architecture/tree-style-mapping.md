# Tree Style Mapping Reference

How Terraria (and TEdit) selects which `Tree_Tops_X` and `Tree_Branches_X` texture to use for each tree, based on biome type, world properties, and position.

## Overview

When a tree tile (ID 5) has a top or branch frame (`frameY >= 198`), the renderer must determine which `Tree_Tops_X` texture to load. This depends on:

1. **Biome type** — determined by scanning downward to find the grass/block type beneath the tree
2. **World properties** — `TreeStyle0-3`, `BgJungle`, `BgSnow` stored in the world file
3. **X position** — for forest zone selection and snow left/right split

## Biome Detection (Tree Type)

The grass type below a tree determines its biome. The renderer scans downward up to 100 tiles from the tree position:

| Grass Type (Tile ID) | Tree Type | Biome |
|----------------------|-----------|-------|
| 2 (Normal Grass) | -1 | Forest |
| 23 (Corrupt Grass) | 0 | Corruption |
| 60 (Jungle Grass, surface) | 1 | Jungle |
| 60 (Jungle Grass, underground) | 5 | Underground Jungle |
| 70 (Mushroom Grass) | 6 | Mushroom |
| 109 (Hallowed Grass) | 2 | Hallow |
| 147 (Snow Block) | 3 | Snow |
| 199 (Crimson Grass) | 4 | Crimson |

The surface/underground split for Jungle uses the world's `GroundLevel` — tiles with the grass at `y > GroundLevel` are underground jungle.

## Tree Type → Tree_Tops Texture Index

### Fixed Biomes

These biomes always use the same texture regardless of world properties:

| Biome | Tree Type | Tree_Tops Index | Notes |
|-------|-----------|----------------|-------|
| Corruption | 0 | **1** | Always `Tree_Tops_1` |
| Hallow | 2 | **3** | Always `Tree_Tops_3` |
| Crimson | 4 | **5** | Always `Tree_Tops_5` |
| Underground Jungle | 5 | **13** | Always `Tree_Tops_13` |
| Mushroom | 6 | **14** | Always `Tree_Tops_14` |

### Jungle (depends on `BgJungle`)

| `BgJungle` Value | Tree_Tops Index | Notes |
|-------------------|----------------|-------|
| 0 | **2** | Default jungle canopy |
| 1 | **11** | Alternate jungle canopy |

### Snow (depends on `BgSnow` and X position)

Snow trees have the most complex selection logic:

| `BgSnow` Value | Tree_Tops Index | Notes |
|-----------------|----------------|-------|
| 0 | **12** (or **18** at `x%10==0`) | Frost variant, occasional alternate |
| 2, 3, 4, 32, 42 | **16** or **17** | Left/right split (see below) |
| 1, 5, 6, 7, 8, 21, 22, 31, 41 (all others) | **4** | Default boreal |

**Left/right split logic** for `BgSnow` values 2, 3, 4, 32, 42:
- If `BgSnow` is even (2, 4, 32, 42): left half of world → 16, right half → 17
- If `BgSnow` is odd (3): left half → 17, right half → 16

The split point is `TilesWide / 2`.

### Forest (depends on `TreeStyle0-3`, `TreeX0-2`, and X position)

Forest trees use a zone-based system. The world is divided into 4 horizontal zones by `TreeX0`, `TreeX1`, `TreeX2`:

| Zone | X Range | Style Property |
|------|---------|----------------|
| 0 | `x <= TreeX0` | `TreeStyle0` |
| 1 | `TreeX0 < x <= TreeX1` | `TreeStyle1` |
| 2 | `TreeX1 < x <= TreeX2` | `TreeStyle2` |
| 3 | `x > TreeX2` | `TreeStyle3` |

Each style property (0-5) maps to a Tree_Tops index:

| TreeStyle Value | Tree_Tops Index | Texture |
|-----------------|----------------|---------|
| 0 | **0** | `Tree_Tops_0` (default forest) |
| 1 | **6** | `Tree_Tops_6` |
| 2 | **7** | `Tree_Tops_7` |
| 3 | **8** | `Tree_Tops_8` |
| 4 | **9** | `Tree_Tops_9` |
| 5 | **10** | `Tree_Tops_10` |

Formula: `style == 0 ? 0 : (style == 5 ? 10 : 5 + style)`

## Tree_Tops Texture Dimensions

Tree top textures are sprite sheets with 3 frame variants side-by-side. Frame 0 is always the leftmost:

| Tree_Tops Index | Frame Width | Frame Height | Used By |
|-----------------|-------------|--------------|---------|
| 0, 4, 5, 6, 7, 8, 9, 10, 12, 14, 15, 16, 17, 18 | 80 | 80 | Most trees |
| 1 | 80 | 80 | Corruption |
| 2, 11, 13 | 114 | 96 | Jungle variants, UG Jungle |
| 3 | 80 | 140 | Hallow (tall) |
| 22-28, 31 | 116 | 96 | Gem trees, Ash tree |
| 29, 30 | 118 | 96 | Vanity trees (Sakura, Willow) |

## Special Tree Types

These tree tile IDs use their own fixed textures, not the biome mapping above:

| Tile ID | Tree Type | Tree_Tops Texture | Notes |
|---------|-----------|-------------------|-------|
| 583 | Topaz Gem | `Tree_Tops_22` | Fixed |
| 584 | Amethyst Gem | `Tree_Tops_23` | Fixed |
| 585 | Sapphire Gem | `Tree_Tops_24` | Fixed |
| 586 | Emerald Gem | `Tree_Tops_25` | Fixed |
| 587 | Ruby Gem | `Tree_Tops_26` | Fixed |
| 588 | Diamond Gem | `Tree_Tops_27` | Fixed |
| 589 | Amber Gem | `Tree_Tops_28` | Fixed |
| 596 | Sakura | `Tree_Tops_29` | Vanity |
| 616 | Willow | `Tree_Tops_30` | Vanity |
| 634 | Ash Tree | `Tree_Tops_31` | Has glow mask |

## Implementation in TEdit

### World.cs Methods

- `GetTreeTypeAtPosition(x, y)` — scans downward to determine biome (returns tree type -1 to 6)
- `GetTreeStyleAtPosition(x, y)` — combines biome detection with style mapping (returns Tree_Tops index)
- `GetNormalTreeStyle(x)` — zone-based forest style selection using `TreeX0-2` and `TreeStyle0-3`
- `GetSnowTreeStyle(x)` — snow variant selection using `BgSnow` and position

### Map Render (WorldRenderXna.xaml.cs)

The map render calls `World.GetTreeStyleAtPosition(x + baseX, y)` for tree top/branch tiles, where `baseX` is the trunk offset determined from the frame data.

### Tool Preview (WorldRenderXna.xaml.cs)

`GetTreeStyleForBiomeVariant(biomeVariantIndex)` maps the preview biome variant to a tree style, reading world properties (`TreeStyle0`, `BgJungle`, `BgSnow`) when a world is loaded.

### Combobox Previews (WorldPropertiesView)

Biome background comboboxes show the associated tree top beside the background preview:

| Biome | Background Property | Tree_Tops Mapping |
|-------|--------------------|--------------------|
| Corruption | `BgCorruption` | Always → 1 |
| Jungle | `BgJungle` | 0→2, 1→11 |
| Snow | `BgSnow` | Per-value (see table above) |
| Hallow | `BgHallow` | Always → 3 |
| Crimson | `BgCrimson` | Always → 5 |
| Mushroom | `MushroomBg` | Always → 14 |
| Desert | `BgDesert` | No tree |
| Ocean | `BgOcean` | No tree |
| Underworld | `UnderworldBg` | No tree |

Tree style comboboxes (TreeStyle0-3) show cropped frame 0 from each `Tree_Tops_X` texture.
