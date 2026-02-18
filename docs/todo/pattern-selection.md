# Pattern Fill & Tile Picker Requirements

Related issues: #826, #1274, #1041

## Unified Tile Picker Control

A reusable tile picker component with two display modes, used across the editor.

### Compact Mode (24x24 Preview)

- Single 24x24 pixel composite preview image
- Renders all active layers composited together: tile, wall, liquid, tile paint, wall paint, coating, actuator state
- Visual indicators for special states:
  - "Erase" mode: red X overlay or strikethrough
  - "Disabled/Empty": dimmed or dashed border
  - Actuator: small indicator icon in corner
- Click to expand into Full Mode
- Right-click context menu: Clear, Set to Erase, Copy, Paste
- Used in: pattern palette slots, mask picker, recently-used tiles, tile search results

### Full Mode (Expanded Picker)

- Dropdown/combobox for each property:
  - **Tile**: type selector with search/filter
  - **Tile Paint**: color dropdown
  - **Wall**: type selector with search/filter
  - **Wall Paint**: color dropdown
  - **Liquid**: None / Water / Lava / Honey / Shimmer
  - **Coating**: None / Echo / Illuminant
  - **Actuator**: On / Off / Not Set
- Each property has an independent enable/disable toggle (checkbox)
- Disabled properties are not written during tool operations
- "Erase" toggle per-property: when enabled, that layer is removed instead of placed
- Used in: top edit bar (primary tile picker), pattern slot expansion, mask picker expansion

### Shared UserControl Architecture

- Both modes are the same `TilePicker` UserControl with a `DisplayMode` property (Compact / Full)
- Compact mode binds to the same data as Full mode — expanding just changes the visual template
- Data model: `TilePickerSlot` with properties for each layer + enabled/erase flags

---

## Pattern Fill System

### Overview

A pattern system that works with **brush** and **bucket fill** tools, allowing users to paint repeating tile patterns instead of single tiles.

### Mode Toggle

- Toggle in the tool options bar: **Single Tile** | **Pattern**
- Single Tile mode: current behavior, one tile picker
- Pattern mode: shows the pattern editor with 2–16 palette slots
- Switching modes preserves each mode's state

### Pattern Definition

- A pattern is a 2D grid of **palette indices** (0–15), where each index maps to a tile picker slot
- Slot index 0 = "transparent / no change" (skip these cells during painting)
- Any slot can independently be set to **Disabled** (skip) or **Erase** (remove that layer)
- Patterns are rectangular, minimum 1x1, no enforced maximum

### Pattern Palette

- **2–16 tile picker slots**, each using the Compact Mode tile picker (24x24 preview)
- Displayed as a grid (e.g., 4x4 max), with a slider or +/- to adjust active slot count
- Click any slot to expand to Full Mode picker inline or in a popup
- Each slot has:
  - 24x24 composite preview
  - A palette index label (0–15)
  - Enable/Disable toggle
  - Erase toggle
- Slot 0 defaults to transparent; slots 1+ default to unassigned
- Unassigned slots in the pattern grid are treated as no-op

### Pattern Editor

| Control | Description |
|---------|-------------|
| **Grid canvas** | Click to paint palette indices into the pattern grid; resize handles to change dimensions |
| **Scale** | Integer multiplier (1x–8x) — each pattern cell covers NxN tiles |
| **Offset X/Y** | Shift the pattern origin in tile coordinates |
| **Rotation** | 0, 90, 180, 270 degrees (quarter turns for tile alignment) |
| **Mirror** | Horizontal and/or vertical flip toggles |

### Image Import

- **Load Image** button accepts PNG/BMP/JPG
- Image is quantized to the nearest 16 colors (median-cut or similar)
- Each quantized color auto-mapped to closest matching tile by tile color (using TEdit/minimap color data)
- User can manually reassign any color-to-tile mapping after import
- Pattern grid generated from quantized image at 1:1 (1 pixel = 1 pattern cell)

### Tool Integration

- When pattern mode is active, it **replaces single-tile behavior** of brush and bucket fill
- **Brush**: pattern is stamped aligned to world coordinates (respecting offset), tiled/repeated across the brush shape
- **Bucket fill**: pattern is tiled across the filled region, aligned to world coordinates (respecting offset)
- Existing **mask tool** and **selection area** still apply — masking filters which tiles get placed, selection bounds the operation
- Pattern respects all current tool settings (undo, mask mode, etc.)
- Each layer within a slot respects its own enabled/erase state independently

### Presets

- Built-in presets: checkerboard, horizontal stripes, vertical stripes, diagonal, brick
- Save/load custom patterns to JSON files
- Pattern file format:
  - Grid dimensions (width x height)
  - 2D array of palette indices
  - Array of palette slot definitions (tile type, paint, wall, etc. + enabled/erase flags)
  - Scale, offset, rotation, mirror settings
