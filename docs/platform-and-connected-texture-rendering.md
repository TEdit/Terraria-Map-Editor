# Platform and Connected Texture Rendering

Technical reference for how TEdit renders platforms, solid tiles, and connected textures (auto-tiling).

## Per-Tile Cache Fields

Every `Tile` instance has non-serialized cache fields (`src/TEdit.Terraria/Tile.cs`):

```csharp
public ushort uvTileCache = 0xFFFF;   // Cached UV column/row for tile texture
public ushort uvWallCache = 0xFFFF;   // Cached UV column/row for wall texture
public byte lazyMergeId = 0xFF;       // Which edges are "interior" vs "exposed"
public bool hasLazyChecked = false;   // Whether lazyMergeId has been computed
```

**Encoding:** `uvTileCache = (ushort)((row << 8) + column)`
- Low byte = column (X) in the texture grid
- High byte = row (Y) in the texture grid
- `0xFFFF` = sentinel meaning "needs recomputation"

These fields are never saved to disk. They're recomputed on demand during rendering.

## Rendering Dispatch

`DrawTileTextures()` in `WorldRenderXna.xaml.cs` iterates all visible tiles plus a 1-tile border. For each tile it:

1. Looks up `tileprop = WorldConfiguration.GetTileProperties(curtile.Type)`
2. Populates `neighborTile[8]` — East(0), North(1), West(2), South(3), NE(4), NW(5), SW(6), SE(7)
3. Dispatches to one of four rendering paths:

| Priority | Condition | Path |
|----------|-----------|------|
| 1 | `tileprop.IsFramed` | Sprite/multi-tile: uses `curtile.U/V` directly from world data |
| 2 | `tileprop.IsPlatform` | Platform: horizontal 2-neighbour logic |
| 3 | `tileprop.IsCactus` | Cactus: biome-variant via downward scan |
| 4 | `tileprop.CanBlend` or default | Connected texture: BlendRules engine |

## Platform Rendering

Platforms use two framing systems: **flat** (columns 0-7, computed from W/E neighbours) and **stair** (columns 8-26, stored in tile U value by the placement tool).

### Flat Platform Framing (Columns 0-7)

Flat platforms check only West and East neighbours. No vertical connections, no BlendRules involvement.

#### State Bit Construction

```
bit 0 (0x01): west neighbour is same tile type
bit 1 (0x02): west neighbour is different type but HasSlopes (solid/slope tile)
bit 2 (0x04): east neighbour is same tile type
bit 3 (0x08): east neighbour is different type but HasSlopes
```

`HasSlopes` = `IsSolid || SaveSlope` on the neighbour's tile property.

#### State-to-Column Mapping

| State | Column | Meaning |
|-------|--------|---------|
| 0x00 | 5 | Isolated (no neighbours) |
| 0x0A | 5 | Both sides are non-same slope tiles (also isolated look) |
| 0x01 | 1 | West same-type only |
| 0x02 | 6 | West is slope (not same type) |
| 0x04 | 2 | East same-type only |
| 0x05 | 0 | Both same-type (middle of run) |
| 0x06 | 3 | East same-type + west slope |
| 0x08 | 7 | East is slope (not same type) |
| 0x09 | 4 | East slope + west same-type |

### Stair Platform Framing (Columns 8-26)

Stair frames are computed by `WorldEditor.ComputePlatformFrameX()` during placement and stored in the tile's U value. The renderer reads U directly for stair columns (U/18 >= 8).

Stair detection checks all four diagonal neighbours for same-type platforms:
- **Up-right stair**: platform at (x-1, y+1) without W flat neighbour, or platform at (x+1, y-1) without E flat neighbour
- **Up-left stair**: platform at (x+1, y+1) without E flat neighbour, or platform at (x-1, y-1) without W flat neighbour

| Column | FrameX | Name | When Used |
|--------|--------|------|-----------|
| 8 | 144 | Stair Up-Right Riser | Middle/top of up-right stair |
| 9 | 162 | Stair Up-Right Stringer | Bottom of up-right stair |
| 10 | 180 | Stair Up-Left Riser | Middle/top of up-left stair |
| 11 | 198 | Stair Up-Left Stringer | Bottom of up-left stair |
| 12 | 216 | Stair Top Landing R | Flat left + stair below-right |
| 13 | 234 | Stair Top Landing L | Flat right + stair below-left |
| 14 | 252 | Stair Top Landing L-R | Both sides flat + stair below |
| 15 | 270 | Stair Landing R Endcap | Endcap variant |
| 16 | 288 | Stair Landing L Endcap | Endcap variant |
| 17 | 306 | Stair Bottom Landing R | Stair above-right + flat right |
| 18 | 324 | Stair Bottom Landing L | Stair above-left + flat left |
| 19-24 | 342-432 | Stair Inset variants | Inside corner connections |
| 25-26 | 450-468 | Stair Inverted | Inverted stair variants |

### Row Variant and Style

```csharp
int style = curtile.V >= 0 ? curtile.V / 18 : 0;   // platform material variant
int variation = ((x * 7) + (y * 11)) % 3;            // visual variation (0-2)
uv.Y = style * 3 + variation;                        // full row in spritesheet
```

### Texture Lookup

```csharp
var source = new Rectangle(
    column * (texsize.X + 2),   // column * 18px stride
    row    * (texsize.Y + 2),   // row * 18px stride
    texsize.X, texsize.Y);      // 16x16 tile
```

Platform textures have 27 columns and 3 rows per style. Each style occupies 3 consecutive rows in the spritesheet, selected by `frameY / 18`.

## Connected Texture Rendering (BlendRules)

All solid non-framed tiles (stone, dirt, grass, sand, etc.) and any tile with `CanBlend=true` use this path.

### Cache Check

```csharp
if (curtile.uvTileCache == 0xFFFF || curtile.hasLazyChecked == false)
```

Two triggers force recomputation: sentinel cache value, or lazy merge not yet validated.

### Special Case: GemSpark Tiles

Tile IDs 255-268, 385, 446-448 use `TileFraming.CalculateSelfFrame8Way()` — an 8-way bitmask (256 possible states) with per-face compatibility. Does NOT use BlendRules.

### General BlendRules Path

**Step 1: Build `sameStyle` neighbour mask**

A 32-bit mask encoding which of 8 neighbours are "compatible" with the current tile:

```
Bits 0x0001 / 0x0010 / 0x0100 / 0x1000         = E / N / W / S cardinals
Bits 0x00010000 / 0x00100000 / 0x01000000 / 0x10000000 = NE / NW / SW / SE diagonals
```

Compatibility rules depend on tile type:
- **Cobweb** (`MergeWith == -1`): any active tile counts as neighbour
- **Stone** (`IsStone == true`): neighbour must also be `IsStone`
- **Everything else**: neighbour matches if `tileprop.Merges(neighborProp)` OR exact same type

**Step 2: Lazy merge correction**

On second pass, neighbour `lazyMergeId` values can cancel cardinal bits from `sameStyle`. This prevents false "connected" appearances when a neighbour's facing edge is actually a transition edge.

**Step 3: Build `mergeMask`** (when `MergeWith > -1`)

A second 32-bit mask for neighbours matching `tileprop.MergeWith.Value` exactly. Enables cross-type transition frames (e.g., dirt blending into stone).

**Step 4: Determine strictness**

| Condition | Strictness | Rule set |
|-----------|-----------|----------|
| Default | 0 | `baseRules` |
| `MergeWith > -1` | 1 | `blendRules` (cross-type transitions) |
| `IsGrass == true` | 2 | `grassRules` (relaxed OR-logic for corners) |

**Step 5: Variant selection**

```csharp
int variant = TileFraming.DetermineFrameNumber(curtile.Type, x, y);
```

- `LargeFrameType == 0` (default): `((x*7) + (y*11)) % 3` → 0, 1, or 2
- `LargeFrameType == 1` (phlebas): `PhlebasLookup[y%4][x%3] - 1`
- `LargeFrameType == 2` (lazure): `LazureLookup[x%2][y%2] - 1`

**Step 6: GetUVForMasks**

```csharp
Vector2Int32 uv = blendRules.GetUVForMasks(sameStyle, mergeMask, strictness, variant);
curtile.uvTileCache = (ushort)((uv.Y << 8) + uv.X);
curtile.lazyMergeId = blendRules.lazyMergeValidation[uv.Y, uv.X];
```

## BlendRules Engine

`src/TEdit/Render/BlendRules.cs`

### Bucket System

Three arrays of `LinkedList<MatchRule>`, each 16 entries:
- `baseRules[16]` — standard same-type framing
- `blendRules[16]` — includes cross-type transitions (superset of baseRules)
- `grassRules[16]` — grass-specific with relaxed corner matching

The 16 buckets correspond to the 4-bit cardinal mask:

| Bucket | Cardinals connected |
|--------|---------------------|
| 0 | None (isolated) |
| 1 | E |
| 2 | N |
| 3 | N+E |
| 4 | W |
| 5 | W+E (horizontal tube) |
| 6 | W+N |
| 7 | W+N+E |
| 8 | S |
| 9 | S+E |
| 10 | S+N (vertical tube) |
| 11 | S+N+E |
| 12 | S+W |
| 13 | S+W+E |
| 14 | S+W+N |
| 15 | S+W+N+E (all four, center tile) |

Only cardinal bits select the bucket. Diagonal bits affect rule matching within the bucket.

### Rule Matching

Each `MatchRule` has inclusion/exclusion masks for both corners (diagonals) and blend (cross-type) edges. A rule matches when:
- Required diagonal same-type bits are present
- Excluded diagonal same-type bits are absent
- Required blend bits match exactly (cardinals) or are present (diagonals)
- Excluded blend bits are absent

Each rule stores 3 UV variants (for texture variation). The `variant` parameter selects which one.

### Grass Rules (Relaxed Matching)

For `strictness == 2`, corner checks use OR logic: a corner is satisfied if EITHER the same-type diagonal bit OR the blend diagonal bit is set. This produces smoother grass-to-dirt transitions.

When no grass rule matches, masks are merged (`sameStyle |= mergeMask`) and `baseRules` are used as fallback.

### lazyMergeValidation Table

A `byte[22, 16]` lookup table. After a UV is chosen, `lazyMergeValidation[row, col]` returns a byte encoding which faces of that UV cell are "interior" (fully connected). These propagate to neighbours during the lazy merge correction pass, preventing false connectivity across tile edges that are actually transitions.

## Cache Invalidation

`BlendRules.ResetUVCache()` is called whenever a tile is edited. It resets a region **one tile larger** than the changed area on all sides:

```csharp
curtile.uvTileCache = 0xFFFF;
curtile.lazyMergeId = 0xFF;
curtile.hasLazyChecked = false;
```

This ensures all neighbours of the edited tile also recompute their connected texture state.

## Wall Rendering

Walls use the same cache pattern but with different parameters:

- Cache: `uvWallCache` (same encoding as `uvTileCache`)
- Algorithm: `WallFraming.CalculateWallFrame()` — Terraria-accurate port
- 4-bit cardinal mask (N=1, W=2, E=4, S=8)
- Neighbour counts if `tile.Wall > 0` OR tile type is in `TruncatesWallsTileIds` (54, 328, 459, 748)
- Texture stride: **+4 pixels** (36px per cell) vs tiles' +2 (18px per cell)
- 4 frame variants (vs tiles' 3)

## Layer Order

```
LayerTileBackgroundTextures = 1 - 0.01   sky/underground backgrounds
LayerTileWallTextures       = 1 - 0.02   walls
LayerTileTrackBack          = 1 - 0.03   track switch backing
LayerTileSlopeLiquid        = 1 - 0.035  liquid behind slopes
LayerTileTextures           = 1 - 0.04   all tiles (solid, platform, blend)
LayerTileTrack              = 1 - 0.05   minecart track overlay
LayerTileActuator           = 1 - 0.06   actuator overlay
```

Platforms and solid tiles both render at `LayerTileTextures`.

## Platforms vs Solid Tiles: Key Differences

| Aspect | Platform | Solid Tile (BlendRules) |
|--------|----------|------------------------|
| Neighbours checked | W and E only | All 8 (4 cardinal + 4 diagonal) |
| Vertical connections | None | Yes (N and S are full peers) |
| Cross-type blending | Via `HasSlopes` flag | Via `MergeWith`, `IsStone`, `Merges()` |
| Lazy merge | Not used (stays 0xFF) | Used for edge propagation |
| Rule engine | Direct switch statement | BlendRules bucket + MatchRule system |
| Row variant formula | `((x*7)+(y*11)) % 3` | `TileFraming.DetermineFrameNumber()` |
| Max columns | 8 (0-7) | ~24 (varies by rule set) |
| Texture gap | +2px (18px stride) | +2px (18px stride) |
