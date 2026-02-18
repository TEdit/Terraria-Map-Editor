# Pixelmap Color Mode Toggle

Related issues: #2049

## Overview

The minimap color generator already exists in the codebase. Expose it as a rendering option so users can switch between color palettes for the zoomed-out pixelmap view.

## Requirements

### Color Modes

| Mode | Description |
|------|-------------|
| **Default TEdit Colors** | Current TEdit color palette (existing behavior) |
| **Realistic Colors** | Texture-sampled colors (existing option) |
| **Minimap Colors** | Uses the in-game minimap color algorithm to match Terraria's map view |

### Behavior
- Toggleable from the same UI location as the current "Realistic Colors" option
- 3-way toggle, radio buttons, or dropdown
- Applies to the zoomed-out pixelmap/minimap rendering
- Selection persists across sessions as a user preference (saved to appSettings.json)
