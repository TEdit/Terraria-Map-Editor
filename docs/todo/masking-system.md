# Masking System Overhaul Requirements

Related issues: #1274, #900

## Overview

Decouple masking from the currently active operation so masks can apply across tool modes. Refactor the mask picker UI to use the unified tile picker control.

## Liquid Masking (#900)

- Add liquid type to the mask options: Water, Lava, Honey, Shimmer
- Mask modes for liquid: Match (only affect tiles with this liquid), Ignore (skip tiles with this liquid)
- Works with all tools: brush, pencil, bucket fill, eraser

## Decoupled Masking

Current behavior: tile mask only works when editing tiles, wall mask only when editing walls, etc.

New behavior:
- Masks apply **regardless** of which layers are being edited
- Example: set a tile mask to "Stone" → wall edits only affect tiles that are Stone
- Example: set a wall mask to "Dirt Wall" → actuator placement only happens on Dirt Wall tiles
- Mask is a filter on **which tiles are affected**, not what operation runs

### Mask Properties

The mask uses the same data model as the unified tile picker, with each property acting as a filter:

| Property | Match Mode | Description |
|----------|-----------|-------------|
| Tile type | Exact / Any / Empty | Filter by tile ID |
| Tile paint | Exact / Any / None | Filter by tile paint color |
| Wall type | Exact / Any / Empty | Filter by wall ID |
| Wall paint | Exact / Any / None | Filter by wall paint color |
| Liquid | Exact / Any / None | Filter by liquid type |
| Actuator | On / Off / Any | Filter by actuator state |
| Coating | Exact / Any / None | Filter by coating type |

- **Exact**: only affect tiles matching this value
- **Any**: don't filter on this property
- **Empty/None**: only affect tiles where this property is unset

## Mask Picker UI

- Uses the unified tile picker UserControl (see pattern-selection.md) in Compact Mode
- Expands to Full Mode on click, with an additional **match mode** dropdown per property (Exact / Any / Empty)
- Additional toggle: **Invert Mask** — affect everything EXCEPT matches

## Wire Mode Extension

- Extend decoupled masking to wire editing mode
- Example: place actuators only on tiles matching a specific tile ID
- Example: place wire only on tiles with a specific wall type
- All mask properties available in wire mode, same as other tool modes
