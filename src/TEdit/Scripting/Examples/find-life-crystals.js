// Find Life Crystals - Locates all Life Crystal tiles in the world
// Life Crystal tile type is 12

var results = batch.findTilesByType(12);

log.print("Found " + results.length + " Life Crystal tiles:");
for (var i = 0; i < results.length; i++) {
    log.print("  (" + results[i].x + ", " + results[i].y + ")");
}
