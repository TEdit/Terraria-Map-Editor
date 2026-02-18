# Bug Investigation: Track/Platform Tool Frame Data (#2068)

**Status:** Investigation complete, fix deferred
**Issues:** Platform and minecart track tiles have incorrect frame (U/V) values after placement in TEdit

## Platform Framing (Tile ID 19)

### The Problem

Platforms have `isSolid: true` AND `isPlatform: true` AND `isFramed: true` in tiles.json.

When placed via the tile paint tool, `SetPixelAutomatic()` runs the IsSolid branch:

```csharp
// WorldEditor.cs lines 565-569
if (WorldConfiguration.TileProperties[curTile.Type].IsSolid)
{
    curTile.U = -1;  // "needs framing" sentinel
    curTile.V = -1;
}
```

This sets U/V to -1, the Terraria sentinel for "needs auto-framing by the game engine". But TEdit never runs auto-framing — there is no equivalent of Terraria's `WorldGen.TileFrame()` for platforms.

### Why It Appears to Work

The renderer at `WorldRenderXna.xaml.cs` lines 4764-4824 completely ignores the tile's actual U/V values for platforms. Instead it computes display UV from `uvTileCache` (neighbor-based state computed per frame). So platforms **look correct** in TEdit despite having corrupted U/V in the tile data.

### The Real Impact

When TEdit saves the world with U/V = -1 for platforms:
- Terraria's engine re-frames them on load (usually fixing them)
- But in some edge cases, the re-framing may produce different results than intended
- Platform style (V value encodes the style variant row) is lost if V = -1

### How Terraria Actually Frames Platforms

In Terraria's `WorldGen.TileFrame()`, platforms get their U value based on neighbor connectivity:
- U encodes the connection pattern (flat, endcap-left, endcap-right, stair-up-left, stair-up-right, etc.)
- V encodes the platform style (V = style * 18)
- Each U/V combination maps to a specific frame in the sprite sheet

**Key:** The V value (style) should be preserved from the platform's original tile type variant, not reset to -1.

## Minecart Track Framing (Tile ID 314)

### How Tracks Work

Tracks use the dedicated `PaintMode.Track` path via `SetTrack()` at WorldEditor.cs lines 235-520. This is separate from the normal tile paint path.

The tile's U field stores the "front" track frame index (0-35, or 36-39 for boosters/sensors). The V field stores the "back" track frame for switches (or -1 if no switch).

### Track Frame Data

**File:** `src/TEdit.Terraria/Minecart.cs`

- `LeftSideConnection[36]` / `RightSideConnection[36]` - map frame index to directional connections
- `TrackType[36]` - track type (0=normal, 1=bumper, 2=pressure-variant)
- `TrackSwitchOptions[64]` - for each neighbor bitmask, valid frame indices

### Track Rendering

At WorldRenderXna.xaml.cs lines 4448-4503, `TrackUV()` maps integer frame indices (0-39) to texture coordinates.

### Track Issues Found

1. **Track placement generally works correctly** - `SetTrack()` properly computes U/V based on neighbor connectivity
2. **Pressure plate tracks** use V=21 sentinel which correctly renders the pressure plate overlay
3. **The `_checkTiles` generation gate** at line 76 could prevent re-entry, but `SetTrack()` with `check=true` calls itself recursively with `check=false` on neighbors, so recursion is properly bounded

### Potential Track Bug: Style Not Preserved on Edit

When a track tile at (x,y) already exists and a neighboring track is placed/removed:
- `SetTrack()` re-evaluates the tile's connections
- The `Front` value is recomputed from `TrackSwitchOptions`
- But if the tile was a special variant (booster at U=36-39), the variant may be lost

## Shared Root Cause: SetPixelAutomatic UV Reset

Both platform and boulder issues stem from the same code path:

```csharp
if (WorldConfiguration.TileProperties[curTile.Type].IsSolid)
{
    curTile.U = -1;
    curTile.V = -1;
}
```

This should NOT apply to tiles where `IsFramed` is true, since those tiles need their UV coordinates preserved.

## Key Code Locations

| File | Lines | Purpose |
|------|-------|---------|
| `TEdit.Editor/WorldEditor.cs` | 235-520 | `SetTrack()` - track placement logic |
| `TEdit.Editor/WorldEditor.cs` | 562-570 | `SetPixelAutomatic()` - IsSolid UV reset bug |
| `TEdit.Terraria/Minecart.cs` | all | Track frame lookup tables |
| `TEdit/View/WorldRenderXna.xaml.cs` | 4448-4503 | Track rendering with `TrackUV()` |
| `TEdit/View/WorldRenderXna.xaml.cs` | 4764-4824 | Platform rendering (ignores actual UV) |
| `TEdit/View/WorldRenderXna.xaml.cs` | 5593-5760 | `TrackUV()` frame-to-texture mapping |
| `TEdit.Terraria/Data/tiles.json` | tile 19 | Platform: `isSolid:true, isPlatform:true, isFramed:true` |
| `TEdit.Terraria/Data/tiles.json` | tile 314 | Track: `isFramed:true` (no isSolid) |

## Recommended Fixes

### Platform Fix
1. In `SetPixelAutomatic()`, add `&& !IsFramed` guard to IsSolid UV reset
2. Implement platform auto-framing in TEdit (compute U from neighbor connectivity, V from style)
3. Reference: `TerrariaServer/Terraria/WorldGen.cs` `TileFrame()` for platform framing logic

### Track Fix
1. Track placement via `SetTrack()` generally works - no critical bugs found
2. Consider preserving booster/pressure plate variant when neighbors change
3. Reference: `TerrariaServer/Terraria/Minecart.cs` for authoritative frame tables

## Terraria Server Cross-Reference

Source: `D:\dev\ai\tedit\terraria-server-dasm\TerrariaServer\Terraria\`

### Platform Framing (WorldGen.cs TileFrame → TileFrameImportant)

Platform framing logic at WorldGen.cs lines 67010-67071:

1. Check left/right neighbors for connected platform tiles
2. Compute connection state: same halfBrick? slopes? ForbidsSloping tile above?
3. Assign `frameX` from ~23 possible values (0, 18, 36, 54, 72, 90, ...)

**Frame encoding:**
- `frameX` = connection pattern × 18px (dynamic, recomputed by TileFrame)
- `frameY` = style × 18px (persistent, set at placement)

**Placement** (WorldGen.cs line 46214):
```csharp
case 19:
    trackCache.frameY = (short)(18 * style);  // style is persistent
    trackCache.active(true);
    trackCache.type = (ushort)19;
    // frameX starts at 0, computed by TileFrame()
```

**Key insight:** `frameX` is always recomputed from neighbors — only `frameY` (style) matters for persistence. TEdit setting U=-1, V=-1 loses the style. Setting V=0 would preserve wood platform style, but all other styles would be wrong.

### Minecart Track Framing (Minecart.cs)

**Frame storage model is completely different from platforms:**
- `frameX` = front track frame ID (0-35, NOT pixel offset)
- `frameY` = back track frame ID (0-35 for switches, -1 for single-mode)

**Frame ID ranges:**
| Range | Type | Notes |
|-------|------|-------|
| 0-17 | Normal tracks | Various shapes/slopes |
| 18-23 | Pressure plate variants | `_trackType[id] == 1` |
| 24-35 | Booster variants | `_trackType[id] == 2` |
| 36-39 | Decoration frames | Bumpers, endcaps |

**UV is indirect:** `_texturePosition[frameX]` maps frame ID to pixel coordinates in spritesheet. TEdit's `TrackUV()` should match this table.

**Placement** (Minecart.PlaceTrack, line 1338):
```csharp
frameY = -1;  // no switch alternate
switch (style) {
    case 0: frameX = -1;   // let FrameTrack pick
    case 1: frameX = 20;   // first pressure plate frame
    case 2: frameX = ~24;  // first left booster
    case 3: frameX = ~28;  // first right booster
}
```

**Framing** (Minecart.FrameTrack):
1. Build 6-neighbor bitmask (upper-left, left, lower-left, upper-right, right, lower-right)
2. Lookup `_trackSwitchOptions[bitmask]` → valid frame IDs
3. Select best front track, set back track for switches

### Comparison: TEdit vs Terraria

| Aspect | TEdit | Terraria (Authoritative) |
|--------|-------|--------------------------|
| Platform frameX | Set to -1 (broken) | Computed from neighbors by TileFrame |
| Platform frameY | Set to -1 (broken) | `style * 18` (persistent) |
| Track frameX | Computed by SetTrack | Computed by FrameTrack |
| Track frameY | Computed by SetTrack | Computed by FrameTrack |
| Track UV lookup | `TrackUV()` | `_texturePosition[frameX]` |
