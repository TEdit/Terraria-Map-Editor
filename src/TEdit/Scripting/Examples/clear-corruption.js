// Clear Corruption - Replace corruption tiles and walls with their pure equivalents
// Ebonstone (25) -> Stone (1), Corrupt Grass (23) -> Grass (2)
// Corrupt Thorns (32) -> removed, Purple Ice (163) -> Ice (161)

var replaced = 0;

replaced += batch.replaceTile(25, 1);    // Ebonstone -> Stone
replaced += batch.replaceTile(23, 2);    // Corrupt Grass -> Grass
replaced += batch.replaceTile(163, 161); // Purple Ice -> Ice
replaced += batch.replaceTile(112, 53);  // Ebonsand -> Sand

// Remove corrupt thorns (native method - no script callbacks)
var thornsCleared = batch.clearTilesByType(32);

log.print("Corruption cleansed!");
log.print("  Tiles replaced: " + replaced);
log.print("  Thorns removed: " + thornsCleared);
