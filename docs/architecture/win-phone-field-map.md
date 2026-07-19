# Windows Phone editor field map

This map records how the typed pre-tile primitive sequence is connected to
TEdit's `World` model. Primitive indices are logical entries, not byte offsets.
All unknown entries retain their original kind and value when saving.

Status values:

- **Editable**: loaded into an existing `World` property and written back from it.
- **Derived**: written from structural world data but not independently editable.
- **Opaque**: preserved exactly; no editor property is assigned yet.

## Container fields

| Field | v49 | v60 | Status |
|---|---|---|---|
| Version | `uint32` | `uint32` | Structural; changing version is rejected |
| CRC | zero in tutorial | reflected CRC-32 | Recalculated for nonzero-CRC containers |
| Title | UTF-8 | UTF-16LE | Editable through `World.Title` |
| Cloud flag/history | absent | native header values | Opaque |

## Common metadata primitives

| Index | Kind | Meaning | TEdit property | Status |
|---:|---|---|---|---|
| 0 | `int32` | World ID | `WorldId` | Editable |
| 1 | `int32` | Native revision/legacy value | — | Opaque |
| 2 | `int32` | Right pixel bound | `RightWorld` | Derived |
| 3 | `int16` | Bottom pixel bound | `BottomWorld` | Derived |
| 4 | `int16` | Tile height | `TilesHigh` | Derived; resizing not supported |
| 5 | `int16` | Tile width | `TilesWide` | Derived; resizing not supported |
| 6 | `int16` | Dungeon X | `DungeonX` | Editable model field |
| 7 | `int16` | Dungeon Y | `DungeonY` | Editable model field |
| 8 | `int16` | Surface level | `GroundLevel` | Editable |
| 9 | `int16` | Rock level | `RockLevel` | Editable |
| 10 | `int32` bits | Time as `float32` | `Time` | Editable |
| 11 | `byte` | Daytime flag | `DayTime` | Editable through time control |
| 12 | `byte` | Moon phase | `MoonPhase` | Editable |
| 13 | `byte` | Blood moon flag | `BloodMoon` | Editable |
| 14 | `int16` | Compact time/moon helper state | — | Opaque |

## v49 tail

| Indices | Meaning | Status |
|---|---|---|
| 15–16 | Spawn X/Y (`int16`) | Editable through spawn tool |
| 17–19 | Eye of Cthulhu, Eater of Worlds, Skeletron defeated | Editable |
| 20–22 | Goblin, Wizard, Mechanic rescued | Editable |
| 23–25 | Goblin Army, Clown, Frost Legion defeated | Editable |
| 26 | Shadow orb smashed | Editable |
| 27 | Meteor queued | Editable |
| 28 | Shadow orb count (`byte`) | Editable |
| 29 | Altar count (`int32`) | Editable |
| 30 | Hardmode | Editable |
| 31 | Invasion delay (`byte`) | Editable model field; not shown separately in the editor |
| 32 | Invasion size (`int16`) | Editable |
| 33 | Invasion type (`byte`) | Editable |
| 34 | Invasion X as `float32` bits | Editable |

## v60 tail

| Indices | Meaning | Status |
|---|---|---|
| 15–16 | Additional compact helper bytes | Opaque |
| 17–18 | Spawn X/Y (`int16`) | Editable through spawn tool |
| 19 | Crimson-world flag | Editable through `IsCrimson` |
| 20 | Raining flag | Editable through `IsRaining` |
| 21 | Rain time | Editable through `TempRainTime` |
| 22 | Maximum rain intensity as `float32` bits | Editable through `TempMaxRain` |
| 23 | Native weather scheduling value | Opaque |
| 24–32 | Same nine common progression flags as v49 indices 17–25 | Editable |
| 33–34 | Lepus and Turkor defeated | Opaque; mobile-only with no TEdit model field |
| 35 | Queen Bee defeated | Editable |
| 36–38 | Destroyer, Twins, Skeletron Prime defeated | Editable |
| 39 | Any mechanical boss defeated | Derived from the three mechanical boss fields |
| 40–42 | Plantera, Golem, Pirate Invasion defeated | Editable |
| 43–46 | Shadow orb smashed, meteor queued, orb count, altar count | Editable |
| 47–49 | Cobalt, Mythril, Adamantite ore tier IDs (`int16`) | Editable |
| 50 | Hardmode | Editable |
| 51 | Additional time-state snapshot value (`int16`) | Opaque |
| 52–55 | Invasion delay, size, type, and X (`byte`, `int16`, `byte`, `float32`) | Editable |
| 56–69 | Two compact biome-state blocks | Opaque |
| 70–78 | Packed styles and eight background bytes | Opaque |

## Editable sections

Tiles, chests, signs, NPCs, and character names use the existing editor models.
Chests and signs are reconciled into their fixed 1,000 native slots at save time.
For tiles, unchanged columns preserve native records; edited columns are written
as validated one-tile records to avoid stale RLE boundaries.

The corresponding raw Ghidra functions and direct-call indexes are stored under
`docs/reverse-engineering/winphone/ghidra/`.
