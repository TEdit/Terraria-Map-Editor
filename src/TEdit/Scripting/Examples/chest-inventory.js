// Chest Inventory - Reports all items across all chests with counts

var allChests = chests.getAll();
var itemCounts = {};

log.print("Scanning " + allChests.length + " chests...");

for (var i = 0; i < allChests.length; i++) {
    var chest = allChests[i];
    var items = chest.items;
    for (var j = 0; j < items.length; j++) {
        var item = items[j];
        var name = item.name || ("Item #" + item.id);
        if (!itemCounts[name]) {
            itemCounts[name] = 0;
        }
        itemCounts[name] += item.stack;
    }
}

// Sort and display
var names = Object.keys(itemCounts).sort();
log.print("=== Item Summary (" + names.length + " unique items) ===");
for (var k = 0; k < names.length; k++) {
    log.print("  " + names[k] + ": " + itemCounts[names[k]]);
}
