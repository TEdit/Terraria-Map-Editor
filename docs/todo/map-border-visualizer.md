# Map Border Visualizer

Related issues: #1691

## Overview

A toggleable render layer that draws boundary lines showing where the world becomes inaccessible in-game. Simple pixel lines, not filled regions.

## Border Offsets (unsafe tile count from edge)

| Edge | Unsafe Tiles |
|------|-------------|
| West (left) | 41 |
| East (right) | 42 |
| Top | 41 |
| Bottom | 42 |

The line is drawn **between** the last unsafe tile and the first safe tile.

## Implementation

### Line Drawing — Not Fill

Draw 4 red pixel-width lines forming a rectangle at the safe/unsafe boundary. Do NOT fill the unsafe area — just the border lines.

### Only Draw What's On Screen

Calculate which of the 4 border lines (left, right, top, bottom) intersect the current viewport. Draw 0–4 lines depending on visibility. No need to draw offscreen portions.

For each visible line, clip to the viewport and to the safe rectangle corners:
- **Left line**: vertical at x=41, from y=41 to y=(worldHeight - 42)
- **Right line**: vertical at x=(worldWidth - 42), from y=41 to y=(worldHeight - 42)
- **Top line**: horizontal at y=41, from x=41 to x=(worldWidth - 42)
- **Bottom line**: horizontal at y=(worldHeight - 42), from x=41 to x=(worldWidth - 42)

The lines connect at corners (41,41), (worldWidth-42, 41), (41, worldHeight-42), (worldWidth-42, worldHeight-42) — forming a complete rectangle when fully visible.

### Display Modes

Toggled via **Layers settings** (same as other layer visibility toggles). Two modes:

| Mode | Description | Color |
|------|-------------|-------|
| **Line** | 1px border lines at safe/unsafe boundary | `#A33D3DFF` |
| **Overlay** | Filled rectangles covering the unsafe area | `#A33D3D90` (semi-transparent) |

### Line Mode

- 4 pixel-width lines as described above
- 1px at screen resolution (stays 1px regardless of zoom)

### Overlay Mode

Fill the unsafe areas with 4 non-overlapping rectangles to avoid alpha stacking artifacts:

```
+--------+------------------+--------+
|  Top   |   Top (center)   |  Top   |  <- full width, 41 tiles tall
|  Left  |                  | Right  |
+--------+------------------+--------+
|        |                  |        |
| Left   |   (safe area)   | Right  |  <- left: 41 wide, right: 42 wide
|        |                  |        |
+--------+------------------+--------+
| Bottom |  Bottom (center) | Bottom |  <- full width, 42 tiles tall
|  Left  |                  | Right  |
+--------+------------------+--------+
```

The 4 rectangles with no overlap:
1. **Top**: x=0, y=0, width=worldWidth, height=41
2. **Bottom**: x=0, y=(worldHeight-42), width=worldWidth, height=42
3. **Left**: x=0, y=41, width=41, height=(worldHeight-42-41)
4. **Right**: x=(worldWidth-42), y=41, width=42, height=(worldHeight-42-41)

Left and right rectangles span only the vertical gap between top and bottom to avoid overlap.

### Rendering

- Rendered on top of tiles but below selection/cursor overlays
- Only draw rectangles/lines that intersect the current viewport
