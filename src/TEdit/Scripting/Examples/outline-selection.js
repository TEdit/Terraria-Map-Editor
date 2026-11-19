// Save a backup copy of the world before editing. Outline the active selection with stone.
if (selection.isActive) {
    const count = draw.rectangle(selection.left, selection.top, selection.right - 1,
        selection.bottom - 1, false, 1, "tile", 1, true, -1);
    log.print("Changed " + count + " tiles.");
}
