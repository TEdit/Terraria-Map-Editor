// Find Life Crystals - Locates all Life Crystal sprites and shows them in the Find sidebar
// Life Crystal tile type is 12 (2x2 sprite), anchorOnly=true returns one result per crystal

finder.clear();
var results = batch.findTilesByType(12, true);

log.print("Found " + results.length + " Life Crystals");
for (var i = 0; i < results.length; i++) {
    finder.addResult("Life Crystal", results[i].x, results[i].y, "Script");
}

if (results.length > 0) {
    finder.navigateFirst();
}
