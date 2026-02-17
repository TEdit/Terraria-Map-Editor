// Replace Block - Replace one tile type with another in the current selection
// Edit the fromType and toType values below to customize

var fromType = 1;  // Stone
var toType = 56;   // Obsidian

if (!selection.isActive) {
    log.warn("No selection active - operating on entire world");
}

var count = 0;

if (selection.isActive) {
    count = batch.replaceTileInSelection(fromType, toType);
} else {
    count = batch.replaceTile(fromType, toType);
}

var fromName = metadata.tileName(fromType) || ("Tile #" + fromType);
var toName = metadata.tileName(toType) || ("Tile #" + toType);
log.print("Replaced " + count + " " + fromName + " tiles with " + toName);
