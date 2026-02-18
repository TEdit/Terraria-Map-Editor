# Bug Investigation: Boulder Trap Actuator Loss (#1830)

**Status:** Investigation complete, fix deferred
**Issue:** Boulder traps lose their actuator state after editing in TEdit
**Tile ID:** 138 (Boulder) - 2x2 framed sprite with `isSolid: true`

## Root Cause Chain

1. User loads a world with actuated boulder traps (placed by Terraria)
2. Actuator bits are correctly deserialized from the file (header3 bits 1-2)
3. User edits tiles near the boulder (paint, undo/redo, fill, etc.)
4. Some operation triggers `SetPixelAutomatic()` for the boulder tile
5. The `IsSolid` check forces `U = -1, V = -1` - corrupting the frame data
6. On save+reload, Terraria resets the corrupted boulder, wiping the actuator

## Bug #1: SetPixelAutomatic Clears U/V for Solid+Framed Tiles

**File:** `src/TEdit.Editor/WorldEditor.cs` lines 562-570

```csharp
curTile.Type = (ushort)tile;
curTile.IsActive = true;
if (WorldConfiguration.TileProperties[curTile.Type].IsSolid)
{
    curTile.U = -1;  // Corrupts framed tile UV!
    curTile.V = -1;
}
```

Boulder (138) has both `isSolid: true` AND `isFramed: true`. The `IsSolid` branch unconditionally resets UV without checking `IsFramed`. This destroys the 2x2 sprite frame coordinates.

**Fix:** Add `&& !IsFramed` guard to the IsSolid check, or better: never reset UV for tiles where `tileFrameImportant` is true.

## Bug #2: SpritePlacer Never Sets Actuators

**File:** `src/TEdit/Editor/SpritePlacer.cs` lines 38-57

When placing a boulder via the Sprite Tool, no actuator is applied to any of the 4 tiles. Users cannot create actuated boulder traps through normal sprite placement - they must manually paint actuators on each tile separately.

**Fix:** SpritePlacer should check `TilePicker.ExtrasActive` and apply `TilePicker.Actuator` / `TilePicker.ActuatorInActive` to each tile of the sprite.

## Bug #3: Erase Mode Forces Actuator to False

**File:** `src/TEdit.Editor/WorldEditor.cs` line 103

```csharp
if (TilePicker.ExtrasActive)
    SetPixelAutomatic(curTile, actuator: isErase ? false : TilePicker.Actuator, ...);
```

When using erase mode with Extras Active, actuators are always wiped even if the user only intended to erase the tile/wall, not the extras.

## Bug #4 (Potential): tileFrameImportant Mismatch

**File:** `src/TEdit.Terraria/World.FileV2.cs` line 1570

The file header contains a `tileFrameImportant` bitfield that may differ from TEdit's `WorldConfiguration`. If there's a mismatch for tile 138, UV values are set to 0 on load instead of being read from the file, corrupting the boulder.

## Affected Tiles

| Tile | ID | isSolid | isFramed | Affected by Bug #1 |
|------|-----|---------|----------|---------------------|
| Boulder | 138 | true | true | **YES** |
| Dart Trap | 137 | false | true | No |
| Other Traps | various | false | true | No |
| Platforms | 19 | true | true | **YES** (separate issue) |

## Key Code Locations

| File | What |
|------|------|
| `TEdit.Terraria/Tile.cs:26,90` | Actuator/InActive properties |
| `TEdit.Terraria/World.FileV2.cs:424-432` | Serialize actuator to file |
| `TEdit.Terraria/World.FileV2.cs:1654-1664` | Deserialize actuator from file |
| `TEdit.Editor/WorldEditor.cs:549-581` | SetPixelAutomatic - IsSolid UV reset + actuator apply |
| `TEdit/Editor/SpritePlacer.cs:38-57` | Sprite placement (no actuator handling) |
| `TEdit/View/WorldRenderXna.xaml.cs:4759` | Actuator overlay rendering |
| `TEdit.Terraria/Data/tiles.json:6894-6908` | Boulder tile definition |

## Recommended Fixes (Priority Order)

1. **Guard UV reset** - In `SetPixelAutomatic`, don't clear UV when `IsFramed` is true
2. **SpritePlacer actuator support** - Apply TilePicker extras to placed sprites
3. **Erase mode** - Don't force actuator=false unless explicitly erasing extras
4. **Validate tileFrameImportant** - Cross-check file header vs config on load

## Terraria Server Reference

Source: `D:\dev\ai\tedit\terraria-server-dasm\TerrariaServer\Terraria\`

### How Boulder Traps Are Placed by WorldGen

`WorldGen.placeTrap(x, y, type=-1)` case 1 places this layout:

```
y+1:     [Boulder (138)] ← 2x2, NO actuator, NO wire
y+2..y+4: [Stone (1)] × 2 columns × 3 rows ← wire=true, actuator=true
```

**The boulder tile itself (138) has NO actuator.** Only the stone support tiles beneath have actuators and wire.

### How Wiring Triggers the Trap

1. `HitWireSingle()` finds tiles with `actuator()` flag along the wire
2. Calls `ActuateForced()` which toggles `inActive`:
   - `DeActive()` sets `inActive=true` (tile becomes ghost/non-solid)
   - `ReActive()` sets `inActive=false` (tile becomes solid again)
3. `DeActive()` checks `tileSolid[type]` — only solid tiles can be deactivated
4. Support stone goes ghost → boulder loses support → falls by gravity

### Correct vs Incorrect Tile State

| Tile | Wire | Actuator | InActive | Notes |
|------|------|----------|----------|-------|
| Boulder (138) | No | **No** | No | Boulder itself is never actuated |
| Support Stone (1) below | Yes | **Yes** | No (at rest) | These are the actuated tiles |

**Common TEdit mistake:** User applies actuator to the boulder tile (138) instead of the support tiles. This does nothing in Terraria since boulders are NOT solid at runtime (`tileSolid[138] = false` after worldgen init).

### Cleanup

`ClearBrokenTraps()` removes actuator flags from orphaned wire networks with no trigger, converting dangling boulders back to stone (type 1).
