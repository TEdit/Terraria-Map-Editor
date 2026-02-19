# UI Improvements

Related issues: #2166

## 1. Sprite Selection Style Fix

The lower `SpriteView2` selection indicator doesn't match the upper grid selection style. This is a control template issue — update the lower control template to use the same selection highlight/border style as the upper grid.

## 2. Remember Camera Position Per World

- Store metadata per world file: last zoom level and pixel position (camera center)
- Metadata updated on a periodic timer (e.g., every 30 seconds or on significant movement), NOT on every frame — avoid file-write spam
- Storage: keyed by world file path in a user-local JSON file (e.g., `worldViewState.json` in app data)
- **Newly opened worlds** (no saved state): center on spawn point horizontally, center vertically on the map (not spawn Y)
- **Reopened worlds** (saved state exists): restore last zoom and camera position

## 3. Chest Clear Buttons

If not already added with recent item editor updates:

- **Per-row clear button**: each chest item row gets a clear/X button that empties that slot
- **"Edit All" clear button**: clears all items in the chest at once
- **"Edit Selected" clear button**: clears only the currently selected items
- Clear means setting the slot to empty (item ID 0, stack 0, prefix 0)

## 4. Inline Paste Per Row

- When an inline paste button on a chest row is clicked, it should **only** affect that specific row
- Should not paste into other rows or trigger any bulk operation
- Paste applies the clipboard item to the single target slot

## 5. Chest Editor Layout Reorganization

Current layout is confusing about which controls affect all items vs selected items.

New layout (top to bottom):
1. **"Edit All"** section — bulk operations (prefix, clear all, paste all) clearly labeled
2. **Save button** — moved above the list
3. **Item list** — scrollable chest contents with per-row controls
4. **"Edit Selected"** section — operations that apply to selected items only (prefix, clear selected, paste)

The distinction between "Edit All" and "Edit Selected" must be visually clear (labels, grouping, possibly different background).

## 6. Paste Sprite Should Not Switch Tabs

When pasting a sprite/schematic, the editor should remain on the clipboard/paste interface. It should NOT auto-navigate to the tile entity tab. The user needs to position the paste before confirming.
