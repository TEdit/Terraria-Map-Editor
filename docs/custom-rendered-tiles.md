# Analysis of Custom Rendered Tiles

This document describes the different rendering styles used for tiles in Terraria (version 1.4.5.4). Tiles are grouped by their **rendering style** rather than individual tile types, as many tiles share the same rendering logic.

## Rendering Type Legend

| Tag | Description |
|-----|-------------|
| `BASE TILE` | Standard tile rendering with neighbor detection and UV frame selection |
| `ANIMATION` | Tiles with animated frames, movement, or time-based changes |
| `SPECIAL EFFECT` | Enhanced rendering like glow masks, particles, multi-layer, or biome variants |

## Table of Contents

| # | Section | Type |
|---|---------|------|
| 1 | [Introduction](#1-introduction) | |
| 2 | [Basic/Blendable Tiles](#2-basicblendable-tiles) | `BASE TILE` |
| 3 | [Tree Rendering](#3-tree-rendering) | `BASE TILE` `ANIMATION` `SPECIAL EFFECT` |
| 4 | [Palm Tree Rendering](#4-palm-tree-rendering) | `BASE TILE` `SPECIAL EFFECT` |
| 5 | [Grass & Plant Rendering](#5-grass--plant-rendering) | `BASE TILE` `ANIMATION` |
| 6 | [Vine Rendering](#6-vine-rendering) | `BASE TILE` `ANIMATION` |
| 7 | [Cactus Rendering](#7-cactus-rendering) | `BASE TILE` `SPECIAL EFFECT` |
| 8 | [Platform Rendering](#8-platform-rendering) | `BASE TILE` |
| 9 | [Torch & Flame Rendering](#9-torch--flame-rendering) | `BASE TILE` `ANIMATION` `SPECIAL EFFECT` |
| 10 | [Christmas Tree Rendering](#10-christmas-tree-rendering) | `BASE TILE` `SPECIAL EFFECT` |
| 11 | [Minecart Track Rendering](#11-minecart-track-rendering) | `BASE TILE` `SPECIAL EFFECT` |
| 12 | [Animal Cage Rendering](#12-animal-cage-rendering) | `BASE TILE` `ANIMATION` |
| 13 | [Display Tile Rendering](#13-display-tile-rendering) | `BASE TILE` `SPECIAL EFFECT` |
| 14 | [Glow Mask Tiles](#14-glow-mask-tiles) | `SPECIAL EFFECT` |
| 15 | [Slope & Half-Brick Rendering](#15-slope--half-brick-rendering) | `BASE TILE` |
| 16 | [Special Cases](#16-special-cases) | `SPECIAL EFFECT` |
| 17 | [Wall Rendering](#17-wall-rendering) | `BASE TILE` |
| 18 | [Background & Horizon Rendering](#18-background--horizon-rendering) | `SPECIAL EFFECT` |
| 19 | [Nature Rendering (Wind & Foliage)](#19-nature-rendering-wind--foliage) | `ANIMATION` `SPECIAL EFFECT` |
| 20 | [Reference Tables](#20-reference-tables) | |

---

## 1. Introduction

Terraria's tile rendering system is uses several key concepts:

### Rendering Layers

Tiles are rendered in specific depth layers to ensure proper visual ordering:

| Layer | Constant | Purpose |
|-------|----------|---------|
| 0 | `Layer_LiquidBehindTiles` | Liquid rendering behind solid tiles |
| 1 | `Layer_BehindTiles` | Decorations behind main tiles |
| 2 | `Layer_Tiles` | Main tile layer |
| 3 | `Layer_OverTiles` | Decorations over main tiles |

### Key Concepts

- **Texture UV Coordinates**: Frame X (U) and Frame Y (V) determine which portion of a tile's texture sheet to render
- **Frame Calculations**: Dynamic offsets (`addFrX`, `addFrY`) adjust UV coordinates based on tile state, animation, or biome
- **Neighbor Detection**: Many tiles examine adjacent tiles to determine their appearance
- **Biome Selection**: Some tiles render differently based on the biome they're placed in
- **TileCounterType**: Special tiles are categorized for batch processing (trees, vines, grass, etc.)

### Standard Tile Dimensions

- Default tile size: 16x16 pixels
- Default frame gap: 2 pixels between frames in texture sheets
- Animation frame offset: `Main.tileFrame[type] * frameHeight`

---

## 2. Basic/Blendable Tiles

> `BASE TILE`

**Style**: Neighbor-based UV selection with lazy merge validation

Most solid blocks in Terraria use a blending system that examines neighboring tiles to select the appropriate texture frame, creating seamless transitions between tile types.

### Technical Details

- **CanBlend Property**: Enables 16-neighbor rule engine
- **MergeWith Property**: Allows cross-type blending (e.g., stone merges with dirt)
- **Neighbor Mask**: 4 cardinal neighbors create 16 possible configurations (2^4)
  - Bit 0: East neighbor
  - Bit 1: North neighbor
  - Bit 2: West neighbor
  - Bit 3: South neighbor
- **UV Variations**: 3 UV positions per rule for visual variety

### Blending Algorithm

```
1. Build sameStyle bitmask from 8 neighbors (cardinal + diagonal)
2. Build mergeMask if MergeWith is specified
3. Apply strictness rules:
   - Level 0: Regular blending
   - Level 1: Specific MergeWith required
   - Level 2: Grass-type strict blending
4. Call BlendRules.GetUVForMasks() to compute final UV
5. Cache result in uvTileCache with lazyMergeId
```

### Tiles Using This Style

| ID | Name | Notes |
|----|------|-------|
| 0 | Dirt Block | Base blending tile |
| 1 | Stone Block | `IsStone=true`, merges with dirt |
| 2 | Grass Block | `IsGrass=true` |
| 6 | Iron Ore | |
| 7 | Copper Ore | |
| 8 | Gold Ore | |
| 9 | Silver Ore | |
| 22 | Demonite Ore | |
| 23 | Corrupt Grass | |
| 25 | Ebonstone Block | |
| 37 | Meteorite | |
| 38 | Gray Brick | |
| 39 | Red Brick | |
| 40 | Clay Block | |
| 41 | Blue Dungeon Brick | |
| 43 | Green Dungeon Brick | |
| 44 | Pink Dungeon Brick | |
| 45 | Gold Brick | |
| 46 | Silver Brick | |
| 47 | Copper Brick | |
| 53 | Sand Block | |
| 54 | Glass Block | |
| 56 | Obsidian | |
| 57 | Ash Block | |
| 58 | Hellstone | |
| 59 | Mud Block | |
| 60 | Jungle Grass | |
| 63-68 | Gem blocks | Sapphire, Ruby, Emerald, Topaz, Amethyst, Diamond |
| 70 | Mushroom Grass | |
| 75 | Obsidian Brick | |
| 76 | Hellstone Brick | |
| 107 | Cobalt Ore | |
| 108 | Mythril Ore | |
| 109 | Hallowed Grass | |
| 111 | Adamantite Ore | |
| 112 | Ebonsand Block | |
| 116 | Pearlsand Block | |
| 117 | Pearlstone Block | |
| 118 | Pearlstone Brick | |
| 119-121 | Colored bricks | Iridescent, Mudstone, Cobalt |
| 147-148 | Ice blocks | Ice, Breakable Ice |
| 150 | Thin Ice | |
| 151 | Holiday Light Red | |
| 152 | Holiday Light Green | |
| 161 | Snow Block | |
| 162 | Adamantite Beam | |
| 163-164 | Sandstone blocks | |
| 166 | Tin Ore | |
| 167 | Lead Ore | |
| 168 | Tungsten Ore | |
| 169 | Platinum Ore | |
| 175 | Slime Block | |
| 176 | Flesh Block | |
| 177 | Rain Cloud | |
| 188 | Cactus Block | |
| 189 | Cloud Block | |
| 190 | Glowing Mushroom Block | |
| 191 | Living Wood | |
| 192 | Leaf Block | |
| 193 | Slime Brick | |
| 194 | Flesh Brick | |
| 195 | Mushroom Brick | |
| 196 | Rainbow Brick | |
| 197 | Ice Brick | |
| 198-200 | Thingy variants | |
| 202 | Asphalt Block | |
| 203 | Crimson Grass | |
| 204 | Crimsand Block (red ice) | |
| 206 | Sunplate Block | |
| 208 | Crimstone Block | |
| 211 | Chlorophyte Ore | |
| 221-223 | Palladium, Orichalcum, Titanium Ore | |
| 224-226 | Slush, Hive, Lihzahrd Brick | |
| 229-230 | Wooden Spike, Bone Block | |
| 232 | Honey Block | |
| 234 | Crimsand Block | |
| 248-261 | Various blocks | Palladium through Dynasty Wood |

*... and many more solid blocks follow the same pattern*

---

## 3. Tree Rendering

> `BASE TILE` `ANIMATION` `SPECIAL EFFECT`

**Style**: Multi-stage rendering with biome-aware variants

Trees are complex multi-tile structures rendered in separate passes for trunk, branches, and foliage (tops).

### Technical Details

#### Biome Detection (`GetTreeBiome`)

The tree biome is determined by scanning downward from the tree position to find the controlling grass type:

```csharp
static int GetTreeBiome(int tileX, int tileY, int tileFrameX, int tileFrameY)
{
    // Scan downward to find grass beneath tree
    // Returns biome style 0-6+ based on grass type found
}
```

#### Frame X Offset Calculation

```
tileFrameX += (short)(176 * (treeBiome + 1))
```

This offsets into different tree texture variants based on biome.

#### Tree Components

| Component | Frame Detection | Rendering Method |
|-----------|-----------------|------------------|
| Trunk | `frameY < 198` or `frameX < 22` | `DrawBasicTile` |
| Tree Top | `frameX == 22` and `frameY >= 198` | `DrawTrees` with `GetTreeTopTexture` |
| Left Branch | `frameX == 44` and `frameY >= 198` | `DrawTrees` with `GetTreeBranchTexture` |
| Right Branch | `frameX == 66` and `frameY >= 198` | `DrawTrees` with `GetTreeBranchTexture` |

#### Foliage Data Methods

| Tree Type | Method | Tile Types |
|-----------|--------|------------|
| Normal Trees | `GetCommonTreeFoliageData` | 5 |
| Gem Trees | `GetGemTreeFoliageData` | 583-589 |
| Vanity Trees | `GetVanityTreeFoliageData` | 596, 616 |
| Ash Trees | `GetAshTreeFoliageData` | 634 |

#### Tree Dimensions

- Trunk tile: `tileWidth = 20`, `tileHeight = 20`
- Tree top source: `80x80` pixels (configurable per style)
- Branch source: `40x40` pixels

#### Biome Variants

| Biome | Style Index | Grass Type Below |
|-------|-------------|------------------|
| Forest | 0-6 (world settings) | Normal Grass (2) |
| Corruption | Varies | Corrupt Grass (23) |
| Crimson | Varies | Crimson Grass (203) |
| Hallow | Varies | Hallowed Grass (109) |
| Jungle | Varies | Jungle Grass (60) |
| Snow | Varies | Snow Block (161) |
| Mushroom | Varies | Mushroom Grass (70) |

#### Wind Sway Effect

Trees sway in the wind using position-based calculations:

```csharp
float windCycle = GetWindCycle(x, y, _treeWindCounter);
position.X += windCycle * 2f;
position.Y += Math.Abs(windCycle) * 2f;
```

Rotation is also applied: `windCycle * 0.08f` for tops, `windCycle * 0.06f` for branches.

### Tiles Using This Style

| ID | Name | Notes |
|----|------|-------|
| 5 | Trees | Normal trees with 7+ biome variants |
| 583 | Topaz Gem Tree | |
| 584 | Amethyst Gem Tree | |
| 585 | Sapphire Gem Tree | |
| 586 | Emerald Gem Tree | |
| 587 | Ruby Gem Tree | |
| 588 | Diamond Gem Tree | |
| 589 | Amber Gem Tree | |
| 596 | Sakura Tree | Vanity tree |
| 616 | Willow Tree | Vanity tree |
| 634 | Ash Tree | Has glow mask overlay |

---

## 4. Palm Tree Rendering

> `BASE TILE` `SPECIAL EFFECT`

**Style**: Biome-conditional variant selection with special crown rendering

Palm trees have unique rendering for their fronds based on the sand type they're planted in.

### Technical Details

#### Biome Detection (`GetPalmTreeBiome`)

```csharp
int GetPalmTreeBiome(int tileX, int tileY)
{
    // Scan down to find sand type
    // Returns variant 0-7 based on location and sand type
}
```

#### Crown Detection

Crown tiles are identified when: `frameX >= 88 && frameX <= 132`

- `frameX == 88`: Crown variant 0
- `frameX == 110`: Crown variant 1
- `frameX == 132`: Crown variant 2

#### Texture Selection

| Location | Biome Index | Texture Index | Crown Size | Biome Rows |
|----------|-------------|---------------|------------|------------|
| Beach (within beachDistance) | 0-3 | Tree_Tops_15 | 80x80 | 4 (82px Y spacing) |
| Oasis (inland) | 4-7 | Tree_Tops_21 | 114x98 | 4 (98px Y spacing) |

#### Trunk Sand Type Variants (Tiles_323)

| Sand Type | ID | Biome Offset | Trunk Texture Row |
| --------- | --- | ------------ | ----------------- |
| Regular Sand | 53 | 0 (beach), 4 (oasis) | 0 |
| Crimsand | 234 | 1 (beach), 5 (oasis) | 1 |
| Pearlsand | 116 | 2 (beach), 6 (oasis) | 2 |
| Ebonsand | 112 | 3 (beach), 7 (oasis) | 3 |

**Note**: The trunk texture row order is Normal, Crimson, Hallowed, Corrupt (not alphabetical).

#### Beach Crown Texture (Tree_Tops_15)

Beach palm crowns have 4 biome variant rows (82px Y spacing each):

| Row | Y Offset | Biome    |
|-----|----------|----------|
| 0   | 0        | Normal   |
| 1   | 82       | Crimson  |
| 2   | 164      | Hallowed |
| 3   | 246      | Corrupt  |

#### Oasis Crown Texture (Tree_Tops_21)

Oasis palm crowns have 4 biome variant rows (98px Y spacing, no gap):

| Row | Y Offset | Biome    |
|-----|----------|----------|
| 0   | 0        | Normal   |
| 1   | 98       | Crimson  |
| 2   | 196      | Hallowed |
| 3   | 294      | Corrupt  |

#### Frame Y Calculation

```csharp
tileFrameY = (short)(22 * palmTreeBiome);
```

### Tiles Using This Style

| ID | Name | Notes |
|----|------|-------|
| 323 | Palm Tree | 8 total variants (4 beach + 4 oasis) |

---

## 5. Grass & Plant Rendering

> `BASE TILE` `ANIMATION`

**Style**: Animated with wind sway simulation

Short plants, flowers, and tall grass have special rendering with height adjustments and wind animation.

### Technical Details

#### Height Overrides

| Tile Types | tileHeight | tileTop | Notes |
|------------|------------|---------|-------|
| 3, 24, 61, 71, 110, 201, 637, 703 | 20 | 0 | Standard tall plants |
| 73, 74, 113 | 32 | -12 | Large grass/cattails |
| 82, 83, 84 | 20 | -2 | Herbs |

#### Sprite Flip Effect

For natural variation, plants are horizontally flipped on alternating columns:

```csharp
if (x % 2 == 0)
    tileSpriteEffect = SpriteEffects.FlipHorizontally;
```

Affected types: 3, 20, 24, 52, 61, 62, 71, 73, 74, 81, 82, 83, 84, 110, 113, 115, 127, 201, 205, 227, 270, 271, 382, 528, 572, 581, 590, 595, 637, 638, 703

#### TileCounterType Categories

Special grass tiles are categorized for batch wind processing:

| Category | Description | Tile Types |
|----------|-------------|------------|
| `WindyGrass` | Single-tile grass affected by wind | 530 |
| `MultiTileGrass` | Large grass patches | 27, 233, 236, 238, 485, 489, 490, 493, 519, 521-527, 530, 651, 652, 702 |
| `AnyDirectionalGrass` | Directional grass | 184 |

#### Wind Animation

The `DrawGrass()` method handles wind-affected vegetation:

```csharp
float windCycle = GetWindCycle(x, y, _grassWindCounter);
// Applied as rotation and position offset
```

### Tiles Using This Style

| ID | Name | Notes |
|----|------|-------|
| 3 | Forest Short Plants | 40+ varieties (flowers, mushrooms) |
| 24 | Corruption Thorns | |
| 27 | Sunflower | MultiTileGrass |
| 61 | Jungle Plants | |
| 71 | Mushroom Plants | |
| 73 | Tall Grass | height=32 |
| 74 | Tall Jungle Grass | height=32 |
| 82 | Herbs (immature) | |
| 83 | Herbs (mature) | |
| 84 | Herbs (blooming) | |
| 110 | Hallowed Plants | |
| 113 | Hallowed Tall Grass | height=32 |
| 184 | Moss | AnyDirectionalGrass |
| 201 | Crimson Plants | |
| 227 | Dye Plants | |
| 233 | Jungle Plants 2 | MultiTileGrass |
| 236-238 | Various plants | |
| 485, 489, 490, 493 | Various decorations | |
| 519, 521-527 | Oasis plants | |
| 530 | Sea Oats | WindyGrass |
| 637 | Ash Plants | |
| 651, 652 | Echo plants | |
| 702 | Cinder plants | |
| 703 | Ash Short Plants | |

---

## 6. Vine Rendering

> `BASE TILE` `ANIMATION`

**Style**: Directional with length-based height variation

Vines hang from ceilings or grow upward (reverse vines) with animated swaying.

### Technical Details

#### Vertical Offset

Vines render slightly above their tile position:

```csharp
// For types 52, 62, 115, 205, 382, 528, 636, 638
tileTop = -2;
```

#### Root Detection

Vines are processed from their root point:

- **Hanging Vines**: `CrawlToTopOfVineAndAddSpecialPoint` - finds topmost vine tile
- **Reverse Vines**: `CrawlToBottomOfReverseVineAndAddSpecialPoint` - finds bottom tile

#### Animation Frames

Some vines use animated frame offsets based on position:

```csharp
// Types 270, 271, 581
addFrX = x % 10 * 18; // Creates variation based on world position
```

#### TileCounterType Categories

| Category | Description | Tile Types |
|----------|-------------|------------|
| `Vine` | Standard hanging vines | 52, 62, 115, 205, 382, 528, 636, 638 |
| `ReverseVine` | Upward-growing vines | 549 |
| `MultiTileVine` | Multi-tile vine structures | 34, 42, 91, 95, 126, 270, 271, 444, 454, 465, 487, 528, 572, 581, 591, 592, 636, 638, 660, 698, 709 |

### Tiles Using This Style

| ID | Name | Category |
|----|------|----------|
| 52 | Jungle Vine | Vine |
| 62 | Hallowed Vine | Vine |
| 115 | Crimson Vine | Vine |
| 205 | Vine Rope | Vine |
| 382 | Vine | Vine |
| 528 | Silk Rope Coil | Vine |
| 549 | Reverse Vine | ReverseVine |
| 636 | Ash Vine | Vine |
| 638 | Corrupt Vine | Vine |
| 34 | Chandelier Chain | MultiTileVine |
| 42 | Chain Lantern | MultiTileVine |
| 91 | Banner | MultiTileVine |
| 95 | Chain | MultiTileVine |
| 126 | Disco Ball | MultiTileVine |
| 270 | Firefly in a Bottle | MultiTileVine |
| 271 | Lightning Bug in a Bottle | MultiTileVine |
| 444 | Lantern | MultiTileVine |
| 454 | Hanging Lantern | MultiTileVine |
| 465 | Hanging Cage | MultiTileVine |
| 487 | Flower Pot | MultiTileVine |
| 572 | Soul in a Bottle | MultiTileVine |
| 581 | Fireplace Smoke | MultiTileVine |
| 591 | Hanging Pot | MultiTileVine |
| 592 | Hanging Brazier | MultiTileVine |
| 660 | Wind Chimes | MultiTileVine |
| 698 | Dead Cells Display Jar | MultiTileVine |
| 709 | Hanging Potion | MultiTileVine |

---

## 7. Cactus Rendering

> `BASE TILE` `SPECIAL EFFECT`

**Style**: Directional with biome-specific textures

Cacti render differently based on the sand type they grow on and their orientation.

### Technical Details

#### Biome Detection (`GetCactusType`)

```csharp
WorldGen.GetCactusType(tileX, tileY, frameX, frameY, out bool evil, out bool good, out bool crimson)
```

Checks the sand below the cactus to determine biome variant.

#### Y Offset by Biome

| Sand Type | Y Offset | Result |
|-----------|----------|--------|
| Normal Sand | +0 | Standard cactus |
| Ebonsand (112) | +54 | Corruption cactus |
| Pearlsand (116) | +108 | Hallowed cactus |
| Crimsand (234) | +162 | Crimson cactus |

#### Flowering Cactus (Type 227)

Flowering cacti have larger dimensions and biome-specific X offsets:

```csharp
tileHeight = 38;
tileTop = (tileFrameX == 238) ? -6 : -20;

// X offset by biome:
if (good) tileFrameX += 238;    // Hallowed
if (evil) tileFrameX += 204;     // Corruption
if (crimson) tileFrameX += 272;  // Crimson
```

#### Sprite Flip

Cacti can be flipped for variety:

```csharp
if (x % 2 == 0)
    tileSpriteEffect = SpriteEffects.FlipHorizontally;
```

### Tiles Using This Style

| ID | Name | Notes |
|----|------|-------|
| 80 | Cactus | 4 biome variants |
| 227 | Flowering Cactus | Larger size, biome X offsets |

---

## 8. Platform Rendering

> `BASE TILE`

**Style**: Smart edge-aware connections

Platforms detect adjacent platforms to render appropriate edge tiles.

### Technical Details

#### Edge Detection Algorithm

Platforms examine their East and West neighbors to determine which frame to use:

```
State Bitmask (4 bits):
- 0x01: East neighbor is same platform type
- 0x02: West neighbor has slopes
- 0x04: West neighbor is same platform type
- 0x08: East neighbor has slopes
```

#### UV State Mapping

| State | UV.X | Description |
|-------|------|-------------|
| 0x00, 0x0A | 5 | Isolated |
| 0x01 | 1 | West end only |
| 0x02 | 6 | West has slopes |
| 0x04 | 2 | East end only |
| 0x05 | 0 | Connected both sides |
| 0x06 | 3 | West connected + slopes |
| 0x08 | 7 | East has slopes |
| 0x09 | 4 | East connected + slopes |

#### Y Variation

Random Y variation (0-2) provides 3 visual variants per state.

### Tiles Using This Style

| ID | Name |
|----|------|
| 19 | Wood Platform |
| 427-443 | Various platform types |
| Plus all other platform variants |

*Platforms are identified by the `IsPlatform` property in tile configuration.*

---

## 9. Torch & Flame Rendering

> `BASE TILE` `ANIMATION` `SPECIAL EFFECT`

**Style**: Anchored placement with particle effects

Torches and flame-emitting tiles have special rendering for positioning and animated flame particles.

### Technical Details

#### Tile Dimensions

```csharp
// Type 4 (Torches)
tileWidth = 20;
tileHeight = 20;
```

#### Wall Mount Detection

When a torch is placed below a solid tile:

```csharp
if (WorldGen.SolidTile(x, y - 1))
    tileTop = 4; // Lower the torch
```

#### Flame Particle System (`DrawSingleTile_Flames`)

The flame system uses `TileFlameData`:

```csharp
struct TileFlameData {
    Texture2D flameTexture;
    ulong flameSeed;
    int flameCount;           // Number of particles (typically 7)
    Color flameColor;         // Particle color
    int flameRangeXMin/Max;   // Horizontal spread (-10 to 11)
    int flameRangeYMin/Max;   // Vertical spread (-10 to 1)
    float flameRangeMultX/Y;  // Spread multipliers
}
```

#### Flame Configuration Examples

| Tile Type | Flame Count | Notes |
|-----------|-------------|-------|
| Standard Torch | 7 | Gray color, range +-10 |
| Large Furnaces | 8 | 0.1 multiplier |
| Small Decorative | 1 | 0.0 multiplier |
| Colored Furniture | Varies | Special tint colors |

### Tiles Using This Style

| ID | Name | Notes |
|----|------|-------|
| 4 | Torches | 20+ color variants |
| 33 | Candles | tileHeight=20, tileTop=-4 |
| 49 | Water Candle | tileHeight=20, tileTop=-4 |
| 174 | Platinum Candle | tileHeight=20, tileTop=-4 |
| 372 | Peace Candle | tileHeight=20, tileTop=-4 |
| 646 | Shadow Candle | tileHeight=20, tileTop=-4 |
| Plus campfires, lanterns, chandeliers, etc. |

---

## 10. Christmas Tree Rendering

> `BASE TILE` `SPECIAL EFFECT`

**Style**: Multi-layer decoration with bit-encoded variants

Christmas trees use a complex multi-layer system with decorations encoded in the frame values.

### Technical Details

#### Method: `DrawXmasTree`

The tree is rendered in multiple layers, each with selectable variants.

#### Bit Encoding in FrameY

```csharp
int star = frameY & 0x7;           // Bits 0-2: Star (8 options)
int garland = (frameY >> 3) & 0x7;  // Bits 3-5: Garland (8 options)
int bulb = (frameY >> 6) & 0xF;     // Bits 6-9: Bulbs (16 options)
int light = (frameY >> 10) & 0xF;   // Bits 10-13: Lights (16 options)
```

#### Texture Assets

| Index | Content |
|-------|---------|
| `XmasTree[0]` | Base tree |
| `XmasTree[1]` | Star variations |
| `XmasTree[2]` | Garland variations |
| `XmasTree[3]` | Bulb variations |
| `XmasTree[4]` | Light variations |

#### Rendering Process

1. Draw base tree texture
2. Overlay star layer (if star > 0)
3. Overlay garland layer (if garland > 0)
4. Overlay bulb layer (if bulb > 0)
5. Overlay light layer (if light > 0)

### Tiles Using This Style

| ID | Name | Notes |
|----|------|-------|
| 171 | Christmas Tree | Multi-layer holiday decoration |

---

## 11. Minecart Track Rendering

> `BASE TILE` `SPECIAL EFFECT`

**Style**: Directional segments with connection logic

Minecart tracks render multiple components including the track itself, decorations, and connection pieces.

### Technical Details

#### Method: `DrawTile_MinecartTrack`

#### Back Layer

Tracks first render a back layer via `DrawTile_BackRope`:

```csharp
DrawTile_BackRope(screenPosition, screenOffset, tileX, tileY, drawData);
```

#### Track Components

| Component | Condition | Notes |
|-----------|-----------|-------|
| Background track | Always | Uses `Minecart.TrackColors` |
| Foreground track | Always | |
| Left decoration | Based on pressure plate | |
| Right decoration | Based on pressure plate | |
| Bumper (normal) | Specific frame ranges | U=24-29 |
| Bumper (bouncy) | Specific frame ranges | |

#### Angled Track Rendering

Angled/curved sections use sliced rendering:

```csharp
// 6 slices of 2px width each
for (int slice = 0; slice < 6; slice++) {
    // Render segment with adjusted height
}
```

### Tiles Using This Style

| ID | Name | Notes |
|----|------|-------|
| 314 | Minecart Track | Directional with bumpers |

---

## 12. Animal Cage Rendering

> `BASE TILE` `ANIMATION`

**Style**: Dynamic animation with species variants

Animal cages contain animated creatures with frame cycling.

### Technical Details

#### Frame Determination

Two methods determine cage animation state:

- `BigAnimalCageFrame(x, y, frameX, frameY)` - Large cages
- `GetSmallAnimalCageFrame(x, y, frameX, frameY)` - Small cages

#### Frame Arrays

Each creature type has its own animation array:

```csharp
Main.birdCageFrame[]
Main.bunnyCageFrame[]
Main.squirrelCageFrame[]
Main.mallardCageFrame[]
Main.duckCageFrame[]
Main.penguinCageFrame[]
Main.scorpionCageFrame[]
Main.owlCageFrame[]
Main.turtleCageFrame[]
Main.grebeCageFrame[]
Main.seagullCageFrame[]
Main.seahorseCageFrame[]
Main.macawCageFrame[]
Main.jellyfishCageFrame[]
// ... and more
```

#### Frame Y Calculation

```csharp
addFrY = Main.xxxCageFrame[cageIndex, cageFrame] * 36; // or 54 for larger cages
```

### Tiles Using This Style

| ID Range | Name | Frame Height |
|----------|------|--------------|
| 275-282 | Bird Cages | 54 |
| 283-287 | Bunny/Squirrel Cages | 54 |
| 288-295 | Duck/Mallard Cages | 36 |
| 296-308 | Various Cages | Varies |
| 316-318 | Jellyfish Cages | 36 |
| 359-364 | Various Cages | 54 |
| 582 | Owl Cage | 54 |
| 619 | Seahorse Cage | 36 |
| 620 | Macaw Cage | 36 |

---

## 13. Display Tile Rendering

> `BASE TILE` `SPECIAL EFFECT`

Display tiles show items, armor, or other collectibles within the tile.

### 13.1 Mannequin / Display Doll

**Style**: Multi-part armor display

#### Technical Details

Armor ID is encoded in the U coordinate:

```csharp
int armorId = frameX / 100;
int position = frameX % 100;
bool facingRight = position >= 36;
```

#### Three-Part Rendering

| Part | Source Size | Position |
|------|-------------|----------|
| Head | 36x36 | V/18 == 0 |
| Body | 36x54 | V/18 == 1 |
| Legs | 36x12 | V/18 == 2, Y+42 |

#### Tiles

| ID | Name |
|----|------|
| 128 | Mannequin (Legacy) |
| 269 | Womannequin (Legacy) |
| 470 | Display Doll |

### 13.2 Weapon Rack

**Style**: Single-item display with flip orientation

#### Technical Details

Item ID is encoded when U >= 5000:

```csharp
int weaponId = (frameX % 5000) - 100;
bool flipped = frameX / 5000 == 1;
```

#### Auto-scaling

Items larger than 40px are scaled down:

```csharp
if (width > 40 || height > 40)
    scale = 40f / Math.Max(width, height);
```

#### Tiles

| ID | Name |
|----|------|
| 334 | Weapon Rack (Legacy) |
| 471 | Weapon Rack |

### 13.3 Item Frame

**Style**: Small item display

#### Detection

```csharp
if (frameY == 0 && frameX % 36 == 0)
    // This is an item frame origin
```

#### Rendering

Item is loaded from TileEntity data and scaled to fit 20px.

#### Tiles

| ID | Name |
|----|------|
| 395 | Item Frame |

### 13.4 Food Platter

**Style**: Food item display

Uses food item texture frame (0, 3) for display.

#### Tiles

| ID | Name |
|----|------|
| 520 | Food Platter |

### 13.5 Hat Rack

Similar to Display Doll but for hats only.

#### Tiles

| ID | Name |
|----|------|
| 475 | Hat Rack |

---

## 14. Glow Mask Tiles

> `SPECIAL EFFECT`

**Style**: Overlay luminescence with special lighting

Some tiles have a separate glow texture rendered on top for luminous effects.

### Technical Details

#### Glow Color Definitions

```csharp
_meteorGlow = new Color(100, 100, 100, 0);
_lavaMossGlow = new Color(150, 100, 50, 0);
_kryptonMossGlow = new Color(0, 200, 0, 0);
_xenonMossGlow = new Color(0, 180, 250, 0);
_argonMossGlow = new Color(225, 0, 125, 0);
_violetMossGlow = new Color(150, 0, 250, 0);
```

#### Glow Application

Glow textures are drawn with additive blending after the base tile.

### Tiles Using This Style

| ID | Name | Glow Color |
|----|------|------------|
| 129 | Crystal Ball | Animated rainbow |
| 209 | Portal | Portal-colored |
| 350 | Crystal | Pulsing (cosine-based) |
| 370, 390 | Meteor blocks | Gray glow |
| 381, 517, 687 | Lava Moss | Orange-yellow |
| 391 | Various | Bright white |
| 429, 445 | Various | Dynamic frame offset |
| 534, 535, 689 | Krypton Moss | Neon green |
| 536, 537, 690 | Xenon Moss | Cyan |
| 539, 540, 688 | Argon Moss | Purple-magenta |
| 625, 626, 691 | Violet Moss | Violet |
| 627, 628, 692 | Disco Moss | Disco color cycle |
| 633 | Various | Blended white |
| 634 | Ash Tree | White glow mask |
| 659, 667, 708 | Shimmer blocks | Shimmer glitter |
| 699 | Various | Pure white |
| 717 | Various | Lava-intensity based |
| 725 | Noir Tile | Filtered noir with opacity |

---

## 15. Slope & Half-Brick Rendering

> `BASE TILE`

**Style**: Sliced block rendering for diagonal geometry

Tiles with slopes are rendered in slices to create diagonal edges.

### Technical Details

#### BrickStyle Enum

```csharp
enum BrickStyle : byte {
    Full = 0,           // Standard full tile
    HalfBrick = 1,      // Bottom half only
    SlopeTopRight = 2,  // Diagonal ↗
    SlopeTopLeft = 3,   // Diagonal ↖
    SlopeBottomRight = 4, // Diagonal ↘
    SlopeBottomLeft = 5   // Diagonal ↙
}
```

#### Half-Brick Rendering

```csharp
// Source: full width, 50% height
// Destination: Y offset +8px, half zoom height
destRect.Y += 8;
destRect.Height = zoom / 2;
```

#### Slope Rendering (`DrawSingleTile_SlicedBlock`)

Slopes are rendered in 8 vertical slices of 2px each:

```csharp
for (int slice = 0; slice < 8; slice++) {
    int sliceHeight = CalculateSliceHeight(slope, slice); // 0-16px
    // Render slice with varying height
}
```

#### Slope Height Patterns

| Slope Type | Slice 0 Height | Slice 7 Height |
|------------|----------------|----------------|
| SlopeTopRight | 2 | 16 |
| SlopeTopLeft | 16 | 2 |
| SlopeBottomRight | 14 | 0 |
| SlopeBottomLeft | 0 | 14 |

### Applicable Tiles

Any tile with `SaveSlope = true` in its configuration can have slopes applied. This includes most solid blocks, platforms, and many decorative tiles.

---

## 16. Special Cases

> `SPECIAL EFFECT`

Some tiles have unique rendering logic that doesn't fit other categories.

### 16.1 Teleportation Pylon (597)

**TileCounterType**: `TeleportationPylon`

Rendered with special positioning and glow effects at frame origin (`frameX % 54 == 0` and `frameY == 0`).

### 16.2 Master Trophy (617)

**TileCounterType**: `MasterTrophy`

Large 3x4 tile trophy with special rendering at frame origin (`frameX % 54 == 0` and `frameY % 72 == 0`).

### 16.3 Void Lens (491)

**TileCounterType**: `VoidLens`

Item display with special detection at `frameX == 18` and `frameY == 18`.

### 16.4 Dead Cells Display Jar (698)

Multi-layer rendering with item display:

1. Main jar texture
2. Background glow layer (dimmed with rarity color)
3. Item rendering (centered, sized by variant)
4. Foreground border (rarity color with alpha)

3 variants based on U value (0, 18, 36).

### 16.5 Chimney (406) and Aether Monolith (658)

Special state-based offset logic for different activation states.

### 16.6 Training Dummy (378)

Dynamic frame based on associated `TETrainingDummy` entity and NPC animation state.

---

## 17. Wall Rendering

> `BASE TILE`

Wall rendering is handled by `WallDrawing.cs`, which inherits from `TileDrawingBase` and uses similar batch rendering infrastructure.

### Technical Details

#### Wall Rendering vs Tile Rendering

- Walls render **behind** tiles (lower Z-layer)
- Walls are hidden when completely occluded by full solid tiles
- Wall frame calculated from `tile.wallFrameX()` and `tile.wallFrameY()`
- Animation offset: `Main.wallFrame[wall] * 180`

#### Frame Calculation

```
rectangle.X = tile.wallFrameX()
rectangle.Y = tile.wallFrameY() + Main.wallFrame[wall] * 180
```

#### Wall Visibility Conditions

Walls render when:
1. `wall > 0` (wall exists)
2. Not occluded by a full tile (`FullTile()` check)
3. Not invisible wall type (318) unless debug mode
4. Has lighting (R, G, or B > 0) OR below underworld layer (y > 1000)

#### Neighbor Detection (Wall Outlines)

Walls use `wallBlend[]` array to determine outline rendering:
- Outlines drawn when adjacent walls have different blend properties
- 4-directional check: left, right, top, bottom
- Uses `TextureAssets.WallOutline` texture (2px strips)

### Special Wall Effects

| Wall Type | Effect |
|-----------|--------|
| 44, 346 | **Disco Walls** - Dynamic RGB color from `Main.DiscoR/G/B` |
| 318 | **Invisible Wall** - Only visible in debug mode |
| 341-345 | **Glowing Walls** - 50% lerp toward white |
| 347 | **Shimmer Wall** - Base texture + glitter overlay using `GlowMask[361]` |
| 242-243 | **Animated Walls** - Position-based staggered animation |

#### Shimmer Wall Rendering (Type 347)

```
1. Draw base wall texture
2. Calculate shimmer vertex colors (70% intensity)
3. Draw glitter overlay texture (GlowMask[361])
4. Apply shimmer color per-corner when bright lighting
```

#### Wall Outline Rendering

Outlines drawn when lighting brightness exceeds thresholds:
- R channel: `brightness * 0.40`
- G channel: `brightness * 0.35`
- B channel: `brightness * 0.30`

Outline positions:
- Left edge: Rectangle(0, 0, 2, 16)
- Right edge: Rectangle(14, 0, 2, 16)
- Top edge: Rectangle(0, 0, 16, 2)
- Bottom edge: Rectangle(0, 14, 16, 2)

---

## 18. Background & Horizon Rendering

> `SPECIAL EFFECT`

Background rendering involves multiple systems: horizon/sky, biome backgrounds, and tree style selection.

### Horizon Rendering System

The horizon system is implemented via `IHorizonRenderer` interface with `NextHorizonRenderer` as the primary implementation.

#### Sunrise/Sunset Textures

| Type | Variants | Texture Names |
|------|----------|---------------|
| Sunrise | 4 | Blue, Violet, Yellow, Aluminum |
| Sunset | 4 | Blue, Dark, Pink, Red |

Selection based on `Main.HorizonPhase` (0-3).

#### Visibility Calculation

```
Sunrise: time 0-2700 game ticks
Sunset: time 43200-54000 game ticks
```

Affected by: atmosphere, cloud alpha, mushroom light, eclipse flag.

#### Lens Flare System

- Only on surface with foreground sunlight effects
- 8 lens flare elements with different textures
- Sunrise colors: Blue/cyan (RGB: 0, 32, 43)
- Sunset colors: Orange/red (RGB: 43, 32, 0)
- Intensity affected by sun scorch counter

### World Tree Style System

Tree styles are determined by world position and biome properties. The system controls both the **tree trunk/branch textures** and the **tree top (foliage) textures**.

#### TreeX0, TreeX1, TreeX2 - Forest Region Division

The world is horizontally divided into **4 forest regions** using three X-coordinate boundaries:

```
Region 0         Region 1         Region 2         Region 3
|<-- TreeX0 -->|<-- TreeX1 -->|<-- TreeX2 -->|<-- World Edge
```

| Region | X Range | Style Property | Description |
|--------|---------|----------------|-------------|
| 0 | `x <= TreeX0` | TreeStyle0 | Left-most forest region |
| 1 | `TreeX0 < x <= TreeX1` | TreeStyle1 | Left-center forest region |
| 2 | `TreeX1 < x <= TreeX2` | TreeStyle2 | Right-center forest region |
| 3 | `x > TreeX2` | TreeStyle3 | Right-most forest region |

**Constraint**: `TreeX0 <= TreeX1 <= TreeX2` (enforced by property setters)

These properties are stored as integers and can range from 0 to `TilesWide`.

#### TreeStyle0, TreeStyle1, TreeStyle2, TreeStyle3 - Normal Forest Variants

Each `TreeStyleN` property controls which tree variant appears in that region for **normal grass trees only** (Tile 2).

**Valid Values**: 0-5 (0 = default)

**Index Mapping** (raw style → texture index):
```csharp
if (style == 0) return 0;
return style == 5 ? 10 : 5 + style;
```

| Raw Style | Texture Index | Visual Description |
|-----------|---------------|-------------------|
| 0 | 0 | Default forest tree |
| 1 | 6 | Forest variant 1 (leafy) |
| 2 | 7 | Forest variant 2 (broad) |
| 3 | 8 | Forest variant 3 (tall) |
| 4 | 9 | Forest variant 4 (pine-like) |
| 5 | 10 | Forest variant 5 (willow-like) |

#### Tree_Tops Texture Index Reference

The `Tree_Tops` textures control the foliage appearance:

| Index | Biome/Type | Notes |
|-------|------------|-------|
| 0 | Default Forest | Standard green foliage |
| 1 | Corruption | Purple/dark foliage |
| 2 | Jungle | Large tropical leaves |
| 3 | Hallow | Pink/pastel foliage |
| 4 | Snow (default) | Snow-covered foliage |
| 5 | Crimson | Red/fleshy foliage |
| 6-10 | Forest Variants | Match TreeStyle 1-5 |
| 11 | Jungle Alt | Used when `BgJungle == 1` |
| 12 | Snow Alt | Ice-crystal foliage |
| 13 | Underground Jungle | Cave jungle tree tops |
| 14 | Mushroom | Glowing mushroom caps |
| 15 | Palm (Beach) | Beach palm fronds |
| 16 | Snow Left | Asymmetric snow variant |
| 17 | Snow Right | Asymmetric snow variant |
| 18 | Snow Rare | Rare crystalline (x%10==0) |
| 21 | Palm (Oasis) | Oasis palm fronds |

#### Tree Style by Biome (Grass Type)

The grass type beneath a tree determines which biome tree style to use:

| Grass Type | Tile ID | Tree Type | Style Index | Notes |
|------------|---------|-----------|-------------|-------|
| Normal Grass | 2 | -1 (Normal) | Region-based | Uses TreeStyle0-3 |
| Corruption Grass | 23 | 0 | 1 | Always corruption tree |
| Jungle Grass | 60 | 1 | 2 or 11 | 11 if `BgJungle == 1` |
| Mushroom Grass | 70 | 6 | 14 | Glowing mushroom |
| Hallowed Grass | 109 | 2 | 3 | Always hallow tree |
| Snow Block | 147 | 3 | Complex | See snow variants below |
| Crimson Grass | 199 | 4 | 5 | Always crimson tree |

#### Snow Tree Style Variants (BgSnow)

**Important**: `BgSnow` is a **biome property**, not a seasonal trigger. It is set at world generation time and stored permanently in the world file. It does not change with in-game seasons or events.

Snow trees have complex variant selection based on `BgSnow` and world position:

| BgSnow Value | Left Half (x < width/2) | Right Half (x >= width/2) | Visual Effect |
|--------------|-------------------------|---------------------------|---------------|
| 0 | 18 (if x%10==0) else 12 | Same as left | Rare crystalline (10%) or ice-crystal |
| 1 | 4 | 4 | Default snow-covered |
| 2, 4 (even) | 16 | 17 | Asymmetric left/right variants |
| 3, 32, 42 (odd) | 17 | 16 | Asymmetric right/left variants |
| Other values | 4 | 4 | Default snow-covered |

**Why the unusual value range (0-42)?**

Re-Logic uses specific "magic numbers" (32, 42) for world generation variation rather than a simple sequential range. This creates more visual diversity across different worlds while keeping the file format compact.

```csharp
private int GetSnowTreeStyle(int x)
{
    if (BgSnow == 0) return x % 10 == 0 ? 18 : 12;
    if (BgSnow is 2 or 3 or 32 or 4 or 42)
    {
        bool isLeft = x < TilesWide / 2;
        return BgSnow % 2 == 0 ? (isLeft ? 16 : 17) : (isLeft ? 17 : 16);
    }
    return 4;
}
```

---

### Biome Background Properties (BgTree, BgOcean, etc.)

These byte properties control **surface biome background textures**. Each value selects a visual variant for that biome.

#### Surface Biome Backgrounds

| Property | Type | Valid Values | Description |
|----------|------|--------------|-------------|
| `BgTree` | byte | 0-13, 31, 51, 71-73 | Primary forest background style |
| `BgTree2` | byte | 0-13, 31, 51, 71-73 | Forest region 2 background (v195+) |
| `BgTree3` | byte | 0-13, 31, 51, 71-73 | Forest region 3 background (v195+) |
| `BgTree4` | byte | 0-13, 31, 51, 71-73 | Forest region 4 background (v195+) |
| `BgCorruption` | byte | 0-4, 51-52 | Corruption biome background |
| `BgJungle` | byte | 0-6 | Jungle biome background (also affects tree style) |
| `BgSnow` | byte | 0-8, 21-22, 31-32, 41-42 | Snow biome background (also affects tree style) |
| `BgHallow` | byte | 0-5 | Hallow biome background |
| `BgCrimson` | byte | 0-6 | Crimson biome background |
| `BgDesert` | byte | 0-4, 51-53 | Desert biome background |
| `BgOcean` | byte | 0-7 | Ocean biome background |
| `MushroomBg` | byte | 0-4 | Mushroom biome background (v195+) |
| `UnderworldBg` | byte | 0-2 | Underworld/Hell background (v215+) |

**Note**: `Bg8` is an alias for `BgTree` (legacy compatibility).

#### Surface Background Texture Mappings

Mappings from `WorldGen.setBG()` function (Terraria 1.4.5.4):

**Forest Backgrounds (BgTree, bg=0)**
| Value | Mountain Textures | Tree Textures | Name |
|-------|-------------------|---------------|------|
| 0 | 7, 8 | 9, 10, 11 | Default |
| 1 | 7, 8 | 50, 51, 52 | Variant 1 |
| 2 | 7, 8 | 53, 54, 55 | Variant 2 |
| 3 | 7, 90 | 91, -1, 92 | Variant 3 |
| 4 | 93, 94 | -1, -1, -1 | Mountains Only |
| 5 | 93, 94 | -1, -1, 55 | Mountains + Tree |
| 6 | 171, 172 | 173, -1, -1 | Variant 6 |
| 7 | 176, 177 | 178, -1, -1 | Variant 7 |
| 8 | 179, 180 | 184, -1, -1 | Variant 8 |
| 9 | 277, 278 | 279, -1, -1 | Variant 9 |
| 10 | 280, 281 | 282, -1, -1 | Variant 10 |
| 11 | 7, 331 | 330, 329, 328 | Variant 11 |
| 12 | 7, 336 | 335, 334, 333 | Variant 12 |
| 13 | 7, -1 | 343, 342, 341 | Variant 13 |
| 31 | 7, 90 | 91, -1, 11 | Variant 3 Alt |
| 51 | 93, 94 | -1, -1, 11 | Mountains Alt |
| 71-73 | 176, 177 | 178, -1, (11/52/55) | Variant 7 Alts |

**Corruption Background (bg=1)**
| Value | Textures | Name |
|-------|----------|------|
| 0 | 12, 13, 14 | Default |
| 1 | 56, 57, 58 | Variant 1 |
| 2 | 211, 212, 213 | Variant 2 |
| 3 | 225, 226, 227 | Variant 3 |
| 4 | 240, 241, 242 | Variant 4 |
| 51 | 324, 323, 322 | Remix 1 |
| 52 | 324, 226, 322 | Remix 2 |

**Jungle Background (bg=2)**
| Value | Textures | Name |
|-------|----------|------|
| 0 | 15, 16, 17 | Default |
| 1 | 59, 60, 61 | Variant 1 |
| 2 | 222, 223, 224 | Variant 2 |
| 3 | 237, 238, 239 | Variant 3 |
| 4 | 284, 285, 286 | Variant 4 |
| 5 | 271, 272, 273 | Variant 5 |
| 6 | 302, 301, 300 | Variant 6 |

**Snow Background (bg=3)**
| Value | Snow Textures | Mountain Textures | Name |
|-------|---------------|-------------------|------|
| 0 | 37, 38, 39 | 35, 36 | Default |
| 1 | 97, 96, 95 | 35, 36 | Variant 1 |
| 2 | -1, -1, -1 | 98, 99 | Mountains Only A |
| 3 | -1, -1, -1 | 98, 100 | Mountains Only B |
| 4 | -1, -1, -1 | 98, 101 | Mountains Only C |
| 5 | 258, 259, 260 | -1, -1 | Trees Only A |
| 6 | 263, 264, 265 | -1, -1 | Trees Only B |
| 7 | 267, 266, 268 | 269, 270 | Variant 7 |
| 8 | 299, 298, -1 | 35, 36 | Variant 8 |
| 21 | 95, 96, 97 | 98, 99 | Mnt 2 + Trees |
| 22 | 37, 38, 39 | 98, 99 | Mnt 2 + Default |
| 31 | 95, 96, 97 | 98, 100 | Mnt 3 + Trees |
| 32 | 37, 38, 39 | 98, 100 | Mnt 3 + Default |
| 41 | 95, 96, 97 | 98, 101 | Mnt 4 + Trees |
| 42 | 37, 38, 39 | 98, 101 | Mnt 4 + Default |

**Hallow Background (bg=4)**
| Value | Textures | Name |
|-------|----------|------|
| 0 | 29, 30, 31 | Default |
| 1 | 102, 103, 104 | Variant 1 |
| 2 | 219, 220, 221 | Variant 2 |
| 3 | 243, 244, 245 | Variant 3 |
| 4 | -1, 261, 262 | Variant 4 |
| 5 | 327, 326, 325 | Variant 5 |

**Crimson Background (bg=5)**
| Value | Textures | Name |
|-------|----------|------|
| 0 | 43, 44, 45 | Default |
| 1 | 105, 106, 107 | Variant 1 |
| 2 | 174, -1, 175 | Variant 2 |
| 3 | 214, 215, 216 | Variant 3 |
| 4 | -1, 229, 230 | Variant 4 |
| 5 | 255, 256, 257 | Variant 5 |
| 6 | 339, 338, 337 | Variant 6 |

**Desert Background (bg=6)**
| Value | Pure Textures | Notes |
|-------|---------------|-------|
| 0 | 21, 20, -1 | Default |
| 1 | 108, 109, -1 | Variant 1 |
| 2 | 207, 208, -1 | Variant 2 |
| 3 | 217, 218, -1 | Variant 3 |
| 4 | 248, 249, 250 | Variant 4 |
| 51-53 | 306, 303-305 | Multi-biome variants |

**Ocean Background (bg=7)**
| Value | Texture | Name |
|-------|---------|------|
| 0 | 28 | Default |
| 1 | 110 | Variant 1 |
| 2 | 111 | Variant 2 |
| 3 | 209 | Variant 3 |
| 4 | 210 | Variant 4 |
| 5 | 283 | Variant 5 |
| 6 | 332 | Variant 6 |
| 7 | 340 | Variant 7 |

**Mushroom Background (bg=8)**
| Value | Textures | Name |
|-------|----------|------|
| 0 | 46, 47, 48 | Default |
| 1 | 231, 232, 233 | Variant 1 |
| 2 | 234, 235, 236 | Variant 2 |
| 3 | 287, 288, 289 | Variant 3 |
| 4 | 321, 320, 319 | Variant 4 |

**Underworld Background (bg=9)**
| Value | Underworld Textures | Name |
|-------|---------------------|------|
| 0 | 0, 1, 2, 3, 4 | Default |
| 1 | 5, 6, 7, 8, 9 | Variant 1 |
| 2 | 10, 11, 12, 13, 9 | Variant 2 |

#### How BgTree/BgTree2/BgTree3/BgTree4 Work

These four properties work in conjunction with `TreeX0`/`TreeX1`/`TreeX2` to provide different forest backgrounds in each world region:

| X Position | Background Property Used |
|------------|--------------------------|
| `x <= TreeX0` | BgTree (or Bg8) |
| `TreeX0 < x <= TreeX1` | BgTree2 |
| `TreeX1 < x <= TreeX2` | BgTree3 |
| `x > TreeX2` | BgTree4 |

#### BgJungle Special Behavior

When `BgJungle == 1`, jungle trees use style index 11 instead of 2, giving an alternate tropical appearance.

#### BgSnow Special Behavior

`BgSnow` has extended range (0-42) because it controls both background AND snow tree asymmetry:
- Values 2, 3, 4, 32, 42 create left/right asymmetric snow trees
- Value 0 creates rare crystalline trees (1 in 10 chance)

---

### Cave Background System

Cave backgrounds use a similar regional division system to trees.

#### CaveBackX0, CaveBackX1, CaveBackX2 - Cave Region Division

The underground is divided into **4 horizontal regions** using three X-coordinate boundaries:

| Region | X Range | Style Property |
|--------|---------|----------------|
| 0 | `x <= CaveBackX0` | CaveBackStyle0 |
| 1 | `CaveBackX0 < x <= CaveBackX1` | CaveBackStyle1 |
| 2 | `CaveBackX1 < x <= CaveBackX2` | CaveBackStyle2 |
| 3 | `x > CaveBackX2` | CaveBackStyle3 |

**Constraint**: `CaveBackX0 <= CaveBackX1 <= CaveBackX2`

#### CaveBackStyle0-3 - Cave Background Variants

Each `CaveBackStyleN` property (0-8) selects a cave rock texture set for that region.

**Cave Backstyle Texture Array** (maps style → texture IDs per depth layer):

```csharp
static int[,] backstyle = new int[9,7]
{
//   Ground  Under   Rock    Cavern  Hell1   Hell2   Hell3
    {66,     67,     68,     69,     128,    125,    185},  // Style 0
    {70,     71,     68,     72,     128,    125,    185},  // Style 1
    {73,     74,     75,     76,     134,    125,    185},  // Style 2
    {77,     78,     79,     82,     134,    125,    185},  // Style 3
    {83,     84,     85,     86,     137,    125,    185},  // Style 4
    {83,     87,     88,     89,     137,    125,    185},  // Style 5
    {121,    122,    123,    124,    140,    125,    185},  // Style 6
    {153,    147,    148,    149,    150,    125,    185},  // Style 7
    {146,    154,    155,    156,    157,    125,    185}   // Style 8
};
```

**Depth Layers** (columns):
- **Column 0**: Ground level background
- **Column 1**: Underground layer (between surface and rock)
- **Column 2**: Rock layer background
- **Column 3**: Deep cavern background
- **Column 4**: Hell entrance (y > TilesHigh - 327)
- **Column 5**: Hell middle layer
- **Column 6**: Hell floor layer

#### Special Cave Background Styles

| Property | Valid Range | UI Label | Description |
|----------|-------------|----------|-------------|
| `IceBackStyle` | 0-3 | "Deep Ice BG" | Ice cave rock texture variant |
| `JungleBackStyle` | 0-5 | "Deep Jungle BG" | Jungle underground texture variant |
| `HellBackStyle` | 0-4 | "Hell BG" | Underworld lava rock variant |

**HellBackStyle** is applied as an **offset** to the hell layer textures:
```csharp
backTex = _textureDictionary.GetBackground(backstyle[backX, 4] + hellback);
```

#### Version History

| Version | Properties Added |
|---------|------------------|
| 60 | `CaveBackX[4]`, `CaveBackStyle[4]`, `IceBackStyle` |
| 61 | `JungleBackStyle`, `HellBackStyle` |
| 195 | `BgTree2`, `BgTree3`, `BgTree4`, `MushroomBg` |
| 215 | `UnderworldBg` |

---

### Background Gradient System

`BackgroundGradientDrawer` handles smooth transitions between sky colors and background layers.

#### Delegate Pattern
- `GetBackgroundDrawWeightMethod()` - Returns opacity (0.0-1.0)
- `BackgroundArrayGetterMethod()` - Returns texture indices for current position
- Blends with `Main.ColorOfSurfaceBackgroundsBase`

#### Depth Transitions

Background selection based on Y position:
```csharp
if (y <= GroundLevel)        // Surface backgrounds (BgTree, BgOcean, etc.)
else if (y <= RockLevel)     // Underground (backstyle column 1)
else if (y <= UnderworldTop) // Cavern (backstyle columns 2-3)
else                         // Hell (backstyle columns 4-6 + HellBackStyle)
```

---

## 19. Nature Rendering (Wind & Foliage)

> `ANIMATION` `SPECIAL EFFECT`

The nature rendering system handles environmental foliage with real-time wind physics.

### Architecture

Two renderer implementations:
- **OriginalNatureRenderer** - Simple immediate-mode drawing
- **NextNatureRenderer** - Deferred batch rendering with shader support

### Wind Animation System

#### Wind Counters

| Counter | Controls |
|---------|----------|
| `_treeWindCounter` | Tree foliage sway |
| `_grassWindCounter` | Grass sway |
| `_sunflowerWindCounter` | Multi-tile plant sway |
| `_vineWindCounter` | Vine sway |

#### Wind Cycle Calculation

```csharp
float positionFactor = (x * 0.5) + (y / 100 * 0.5);
float baseCycle = Math.Cos(windCounter * 2π + positionFactor) * 0.5f;
float resultWind = baseCycle + Main.WindForVisuals;
float lerpValue = Remap(|WindForVisuals|, 0.08f, 0.18f, 0.0f, 1.0f);
return resultWind * lerpValue;
```

### Rendering Components

#### Tree Foliage

| Component | Wind Rotation | Position Offset |
|-----------|---------------|-----------------|
| Tree Tops | `windCycle * 0.08f` | `windCycle * 2.0f` horizontal |
| Branches | `windCycle * 0.06f` | `windCycle * 2.0f` horizontal |

#### Grass

| Type | Wind Rotation | Position Offset |
|------|---------------|-----------------|
| Single Grass | `windCycle * 0.1f` | `windCycle * 1.0f` horizontal |
| Multi-tile | Stack-attenuated | Bottom tiles minimal sway |

#### Vines

Gravity curve applied:
```
num4 += 0.0075f per 5 tiles
num4 += 1/400f per 2 tiles
```

Wind response: `Lerp(0.2f, 1.0f, |WindForVisuals| / 1.2f)`

### Lit Nature Shader

NextNatureRenderer applies "LitNature" shader when:
- Daytime is active
- Sun position visible (based on time)
- Surface background should be drawn
- Foreground sunlight effects enabled

Color grading converts sun/moon colors to HSL with 8x saturation boost.

### Leaf Particle System

`EmitTreeLeaves()` spawns Gore particles:
- Frequency: `_leafFrequency` (10-2000 per 7 frames)
- Inversely proportional to wind speed
- Position based on tree frame configuration

---

## 20. Reference Tables

### TileCounterType Enum

```csharp
enum TileCounterType {
    Tree,              // 0 - Trees with foliage
    WindyGrass,        // 1 - Wind-affected grass
    MultiTileGrass,    // 2 - Large grass patches
    MultiTileVine,     // 3 - Multi-tile vines
    Vine,              // 4 - Standard vines
    BiomeGrass,        // 5 - (Unused in draw phase)
    VoidLens,          // 6 - Void Lens display
    ReverseVine,       // 7 - Upward-growing vines
    TeleportationPylon,// 8 - Pylons
    MasterTrophy,      // 9 - Master mode trophies
    AnyDirectionalGrass,// 10 - Directional grass
    Count              // 11
}
```

### Common Tile Height/Width Overrides

| tileWidth | tileHeight | tileTop | Tile Types |
|-----------|------------|---------|------------|
| 20 | 20 | 0 | 4 (Torch), 5 (Tree), 323 (Palm), 324 |
| 16 | 20 | 0 | 3, 24, 61, 71, 110, 201, 637, 703 |
| 16 | 20 | -4 | 33, 49, 174, 372, 646 |
| 16 | 20 | -2 | 82, 83, 84 |
| 16 | 32 | -12 | 73, 74, 113 |
| 24 | 26 | -8 | 81 |
| 16 | 18 | 0 | Many furniture tiles |
| 16 | 16 | 2 | Many placed objects |

### Tiles with Animated Frame Offsets

| Type | Animation Formula |
|------|-------------------|
| 12, 31, 96, 639, 665, 696 | `Main.tileFrame[type] * 36` |
| 106, 228, 231, 243, 247 | `Main.tileFrame[type] * 54` |
| 217, 218, 564 | `Main.tileFrame[type] * 36` |
| 219, 220, 642 | `Main.tileFrame[type] * 54` |
| 235 | `Main.tileFrame[type] * 18` |

### UV Wrapping Tiles

Some tiles have UV coordinates that exceed their texture dimensions and require wrapping:

| Type | Wrap Axis | Threshold | Offset |
|------|-----------|-----------|--------|
| 18 | U | 2016 | addFrY += 20 per wrap |
| 79 | V | 2016 | addFrX += 144 per wrap |
| 90 | V | 2016 | addFrX += 144 per wrap |
| 100 | V | 2016 | addFrX += 72 per wrap |
| 139 | V | 2016 | addFrX += 72 per wrap |

---

## Source References

### Tile & Wall Rendering

- `ref/Drawing/TileDrawing.cs` - Primary tile rendering
- `ref/Drawing/TileDrawingBase.cs` - Base class for batch rendering
- `ref/Drawing/WallDrawing.cs` - Wall rendering system
- `src/TEdit/View/WorldRenderXna.xaml.cs` - TEdit implementation
- `src/TEdit/Render/BlendRules.cs` - Neighbor blending rules

### Background & Nature Rendering

- `ref/Drawing/BackgroundGradientDrawer.cs` - Background gradient system
- `ref/Drawing/NextHorizonRenderer.cs` - Horizon/sky rendering
- `ref/Drawing/HorizonHelper.cs` - Sun/moon celestial rendering
- `ref/Drawing/IHorizonRenderer.cs` - Horizon interface
- `ref/Drawing/INatureRenderer.cs` - Nature rendering interface
- `ref/Drawing/NextNatureRenderer.cs` - Modern nature renderer with shaders
- `ref/Drawing/OriginalNatureRenderer.cs` - Legacy nature renderer

### Data Configuration

- `src/TEdit.Terraria/Data/tiles.json` - Tile properties and metadata
- `src/TEdit.Terraria/World.cs` - World properties including tree styles
- `src/TEdit.Terraria/World.Properties.cs` - Biome background properties

### Debug Texture Export

In DEBUG builds, `WorldRenderXna.xaml.cs` exports textures to the `textures/` folder for analysis:

| Export | Range | Filename Pattern |
|--------|-------|------------------|
| Tree Tops | 0-30 | `Tree_Tops_{i}.png` |
| Tree Branches | 0-30 | `Tree_Branches_{i}.png` |
| Backgrounds | 0-200 | `Background_{i}.png` |
| Underworld | 0-10 | `Underworld_{i}.png` |
| Tiles | All | `Tile_{id}.png` |
| Walls | All | `Wall_{id}.png` |
| NPCs | All | `NPC_{id}.png` |
