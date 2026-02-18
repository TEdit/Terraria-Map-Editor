# Selection Tool Hotkey Rework

Related issues: #2165

## Overview

Update selection tool keyboard and mouse interactions for precise control over selection position and size.

## Requirements

### Arrow Key Movement (Selection tool active, selection exists)

| Modifier | Action | Step Size |
|----------|--------|-----------|
| _(none)_ | Move entire selection | 1 tile |
| **Shift** | Move entire selection | 5 tiles |
| **Ctrl** | Resize selection (expand/contract from bottom-right) | 1 tile |
| **Ctrl+Shift** | Resize selection (expand/contract from bottom-right) | 5 tiles |

### Mouse Interaction with Modifiers

| Modifier | Action |
|----------|--------|
| **Shift** + click/drag | Move the **end point** (bottom-right corner) to mouse position |
| **Ctrl** + click/drag | Move the **start point** (top-left corner) to mouse position |

- Click sets the position once
- Hold and drag continuously updates the corner to follow the mouse
- Without modifiers, default selection behavior (draw new selection) is unchanged
