# Mannequin Script API

Related issues: #2167

## Overview

Add scripting API for mannequins (display dolls), matching the existing pattern for chests and signs. The data model and editor UI already exist — this just needs an API class and registration.

## Existing Patterns

| Entity | API Class | Storage |
|--------|-----------|---------|
| Chests | `ChestApi.cs` | `World.Chests` |
| Signs | `SignApi.cs` | `World.Signs` |
| **Mannequins** | **`MannequinApi.cs` (new)** | `World.TileEntities` where `Type == DisplayDoll` |

## Mannequin Data Model (already exists)

- `TileEntity.cs` with `TileEntityType.DisplayDoll` (type 3)
- 9 equipment slots (`Items[0-8]`): head, body, legs, 5 accessories, mount
- 9 dye slots (`Dyes[0-8]`): matching dyes + weapon dye
- 1 misc slot (`Misc[0]`): weapon display
- Pose via `DisplayDollPoseID` enum (Standing, Sitting, Jumping, Walking, Use1-5)
- Tile types: `MannequinLegacy` (128), `WomannequinLegacy` (269), `DisplayDoll` (470)

## API Methods

### Query
- `Count` — number of display dolls in world
- `GetAll()` — list all mannequins with position, equipment, dyes, pose
- `GetAt(x, y)` — get mannequin at tile position
- `FindByEquipment(itemId)` — find mannequins containing a specific equipment item
- `FindByDye(dyeId)` — find mannequins with a specific dye
- `FindByWeapon(itemId)` — find mannequins displaying a specific weapon

### Equipment (slots 0-7)
- `SetEquipment(x, y, slot, itemId, prefix)` — set an equipment slot
- `ClearEquipment(x, y, slot)` — clear an equipment slot

### Dyes (slots 0-8, slot 8 = weapon dye)
- `SetDye(x, y, slot, dyeId, prefix)` — set a dye slot
- `ClearDye(x, y, slot)` — clear a dye slot

### Weapon (Misc[0])
- `SetWeapon(x, y, itemId, prefix)` — set displayed weapon
- `ClearWeapon(x, y)` — clear weapon

### Mount (Item8)
- `SetMount(x, y, itemId)` — set mount
- `ClearMount(x, y)` — clear mount

### Pose
- `SetPose(x, y, poseId)` — set pose (0-8)
- `GetPose(x, y)` — get current pose

## Return Data Format

```javascript
{
  "x": 100, "y": 50, "pose": 0,
  "equipment": [
    { "slot": 0, "id": 123, "name": "Copper Helmet", "prefix": 0 },
    { "slot": 1, "id": 124, "name": "Copper Breastplate", "prefix": 5 }
  ],
  "dyes": [
    { "slot": 0, "id": 264, "name": "Red Dye", "prefix": 0 }
  ],
  "weapon": { "id": 456, "name": "Iron Sword", "prefix": 2 },
  "mount": { "id": 789, "name": "Antlion Chariot", "prefix": 0 }
}
```

## Files

| Action | File |
|--------|------|
| Create | `src/TEdit/Scripting/Api/MannequinApi.cs` |
| Modify | `src/TEdit/Scripting/Api/ScriptApi.cs` (register) |
| Create | `src/TEdit.Tests/Scripting/MannequinApiTests.cs` |
| Create | `src/TEdit/Scripting/Examples/mannequin-setup.js` (example script) |

## Reference

- `src/TEdit/Scripting/Api/ChestApi.cs` — follow this pattern
- `src/TEdit.Tests/Scripting/ChestApiTests.cs` — follow test pattern
- `src/TEdit.Terraria/TileEntity.cs` — data model
