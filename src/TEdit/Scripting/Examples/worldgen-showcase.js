// WorldGen Showcase - Generates natural terrain at actual Terraria worldgen density
// Use on a new world created via File > New World (any size).
// Uses world.surfaceLevel and world.rockLevel set by the New World dialog.
// Terrain profile ported from Terraria's TerrainPass (feature-segment random walk).
//
// For a Small world (4200x1200):
//   ~15,000 cave calls, ~6,000 ore veins, lakes, lava pools, trees

var w = world.width;
var h = world.height;
var area = w * h;

// Layer boundaries from world properties (set by New World dialog)
var baseSurface = Math.floor(world.surfaceLevel);
var baseRock = Math.floor(world.rockLevel);
var lavaLine = Math.floor(h * 0.8);

// Derived zone boundaries (±margins for ore depth zones)
var surfaceLow = baseSurface - 20;
var surfaceHigh = baseSurface + 40;
var rockLayerLow = baseRock - 20;
var rockLayerHigh = baseRock + 20;

// Helper: random int in [min, max)
function rand(min, max) { return min + Math.floor(Math.random() * (max - min)); }

// Tile IDs
var DIRT = metadata.tileId("Dirt Block");
var STONE = metadata.tileId("Stone Block");
var GRASS = metadata.tileId("Grass Block");
var ASH = metadata.tileId("Ash Block");
var CLAY = metadata.tileId("Clay Block");
var SILT = metadata.tileId("Silt Block");
var MUD = metadata.tileId("Mud Block");
var SAND = metadata.tileId("Sand Block");

// ═══════════════════════════════════════════════════════════════════════
// Step 1: Generate surface height profile (TerrainPass port)
// ═══════════════════════════════════════════════════════════════════════
log.print("Step 1: Generating terrain profile...");

// Feature types: 0=Plateau, 1=Hill, 2=Dale, 3=Mountain, 4=Valley
// Each feature biases the per-column height offset using geometric random loops.
function surfaceOffset(featureType) {
    var off = 0;
    switch (featureType) {
        case 0: // Plateau — very gentle
            while (rand(0, 7) === 0) off += rand(-1, 2);
            break;
        case 1: // Hill — biased upward (lower Y = higher ground)
            while (rand(0, 4) === 0) off--;
            while (rand(0, 10) === 0) off++;
            break;
        case 2: // Dale — biased downward
            while (rand(0, 4) === 0) off++;
            while (rand(0, 10) === 0) off--;
            break;
        case 3: // Mountain — strongly upward
            while (rand(0, 2) === 0) off--;
            while (rand(0, 6) === 0) off++;
            break;
        case 4: // Valley — strongly downward
            while (rand(0, 2) === 0) off++;
            while (rand(0, 5) === 0) off--;
            break;
    }
    return off;
}

// Height constraints: surface can't go above 17% or below 26% of world height
var heightFloor = Math.floor(h * 0.17);
var heightCeil = Math.floor(h * 0.26);

// Beach zones: first/last ~8% of world are flat
var beachLeft = Math.floor(w * 0.08);
var beachRight = w - beachLeft;

// Center ~4% forced to plateau (spawn area)
var centerLeft = Math.floor(w * 0.48);
var centerRight = Math.floor(w * 0.52);

// Generate per-column surface and rock layer heights
var surfaceY = new Array(w);
var rockY = new Array(w);

var curSurface = baseSurface;
var curRock = baseRock;
var featureType = 0;
var featureCountdown = rand(10, 30);

for (var x = 0; x < w; x++) {
    // Pick next feature when countdown expires
    if (featureCountdown <= 0) {
        // Force plateau in center spawn zone
        if (x >= centerLeft && x <= centerRight) {
            featureType = 0;
        } else {
            featureType = rand(0, 5);
        }
        featureCountdown = rand(5, 40);
        if (featureType === 0) {
            featureCountdown = Math.floor(featureCountdown * rand(5, 30) * 0.2);
        }
    }
    featureCountdown--;

    // Apply feature offset to surface
    curSurface += surfaceOffset(featureType);

    // Beach zones: clamp to flat
    if (x < beachLeft || x > beachRight) {
        curSurface = Math.max(curSurface, baseSurface - 5);
        curSurface = Math.min(curSurface, baseSurface + 5);
    }

    // Hard clamp to valid range
    if (curSurface < heightFloor) curSurface = heightFloor;
    if (curSurface > heightCeil) curSurface = heightCeil;

    surfaceY[x] = Math.floor(curSurface);

    // Rock layer random walk (slower, constrained 6-35% below surface)
    while (rand(0, 3) === 0) curRock += rand(-2, 3);
    var minRock = curSurface + h * 0.06;
    var maxRock = curSurface + h * 0.35;
    if (curRock < minRock) curRock++;
    if (curRock > maxRock) curRock--;

    rockY[x] = Math.floor(curRock);

    if (x % 500 === 0) log.progress(x / w * 0.02);
}
log.progress(0.02);
log.print("  Profile generated. Range: " +
    Math.min.apply(null, surfaceY) + " to " + Math.max.apply(null, surfaceY));

// ═══════════════════════════════════════════════════════════════════════
// Step 2: Fill terrain columns following the height profile
// ═══════════════════════════════════════════════════════════════════════
log.print("Step 2: Filling terrain layers...");

for (var x = 0; x < w; x++) {
    var sy = surfaceY[x];
    var ry = rockY[x];

    // Surface dirt layer
    for (var y = sy; y < ry; y++) {
        tile.setType(x, y, DIRT);
    }
    // Stone layer
    for (var y = ry; y < lavaLine; y++) {
        tile.setType(x, y, STONE);
    }
    // Ash/underworld layer
    for (var y = lavaLine; y < h - 6; y++) {
        tile.setType(x, y, ASH);
    }

    if (x % 500 === 0) log.progress(0.02 + (x / w) * 0.06);
}
log.progress(0.08);
log.print("  Terrain filled.");

// ═══════════════════════════════════════════════════════════════════════
// Step 3: Rocks-in-dirt and dirt-in-rocks scatter (breaks up pure layers)
// ═══════════════════════════════════════════════════════════════════════
log.print("Step 3: Scattering rock/dirt blobs...");

// Rocks in dirt: stone patches in the dirt layer
var rocksInDirt = Math.floor(area * 1.5e-4);
for (var i = 0; i < rocksInDirt; i++) {
    var px = rand(10, w - 10);
    var py = rand(surfaceLow, rockLayerHigh);
    generate.tileRunner(px, py, rand(4, 10), rand(5, 30), STONE);
}

// Dirt in rocks: dirt patches in the stone layer
var dirtInRocks = Math.floor(area * 1.5e-4);
for (var i = 0; i < dirtInRocks; i++) {
    var px = rand(10, w - 10);
    var py = rand(rockLayerLow, h - 40);
    generate.tileRunner(px, py, rand(4, 10), rand(5, 30), DIRT);
}

// Mud pockets in stone layer
var mudCount = Math.floor(area * 3e-5);
for (var i = 0; i < mudCount; i++) {
    var px = rand(10, w - 10);
    var py = rand(rockLayerLow, lavaLine);
    generate.tileRunner(px, py, rand(3, 8), rand(5, 25), MUD);
}
log.progress(0.13);
log.print("  Scattered " + rocksInDirt + " stone + " + dirtInRocks + " dirt + " + mudCount + " mud blobs.");

// ═══════════════════════════════════════════════════════════════════════
// Step 4: Small holes (WorldGen SmallHoles pass)
// Terraria: area * 0.0015 iterations, 2 TileRunner calls each
// ═══════════════════════════════════════════════════════════════════════
log.print("Step 4: Carving small holes...");
var smallHoleCount = Math.floor(area * 0.0015);
for (var i = 0; i < smallHoleCount; i++) {
    var tx = rand(10, w - 10);
    var ty = rand(surfaceHigh, h - 20);
    generate.tunnel(tx, ty, rand(2, 5), rand(2, 20));
    generate.tunnel(tx, ty, rand(8, 15), rand(7, 30));
    if (i % 2000 === 0) log.progress(0.13 + (i / smallHoleCount) * 0.20);
}
log.progress(0.33);
log.print("  Carved " + smallHoleCount + " small hole pairs (" + (smallHoleCount * 2) + " calls).");

// ═══════════════════════════════════════════════════════════════════════
// Step 5: Dirt layer caves
// Terraria: area * 3e-5, strength 5-15, steps 30-200
// ═══════════════════════════════════════════════════════════════════════
log.print("Step 5: Carving dirt layer caves...");
var dirtCaveCount = Math.floor(area * 3e-5);
for (var i = 0; i < dirtCaveCount; i++) {
    var tx = rand(10, w - 10);
    var ty = rand(surfaceLow, rockLayerHigh);
    generate.tunnel(tx, ty, rand(5, 15), rand(30, 200));
}
log.progress(0.37);
log.print("  Carved " + dirtCaveCount + " dirt layer caves.");

// ═══════════════════════════════════════════════════════════════════════
// Step 6: Rock layer caves
// Terraria: area * 0.00013, strength 6-20, steps 50-300
// ═══════════════════════════════════════════════════════════════════════
log.print("Step 6: Carving rock layer caves...");
var rockCaveCount = Math.floor(area * 0.00013);
for (var i = 0; i < rockCaveCount; i++) {
    var tx = rand(10, w - 10);
    var ty = rand(rockLayerHigh, h - 20);
    generate.tunnel(tx, ty, rand(6, 20), rand(50, 300));
    if (i % 200 === 0) log.progress(0.37 + (i / rockCaveCount) * 0.08);
}
log.progress(0.45);
log.print("  Carved " + rockCaveCount + " rock layer caves.");

// ═══════════════════════════════════════════════════════════════════════
// Step 7: Surface caves (vertical shafts + horizontal tunnels)
// ═══════════════════════════════════════════════════════════════════════
log.print("Step 7: Carving surface caves...");
var surfSmall = Math.floor(w * 0.002);     // ~8 small vertical
var surfMedium = Math.floor(w * 0.0007);   // ~2 medium vertical
var surfLarge = Math.floor(w * 0.0003);    // ~1 large branching
var surfHoriz = Math.floor(w * 0.0004);    // ~1 horizontal

for (var i = 0; i < surfSmall; i++) {
    var tx = rand(beachLeft, beachRight);
    var sy = surfaceY[tx] || baseSurface;
    generate.tunnel(tx, sy, rand(3, 6), rand(5, 50), Math.random() * 0.1, 1.0);
}
for (var i = 0; i < surfMedium; i++) {
    var tx = rand(beachLeft, beachRight);
    var sy = surfaceY[tx] || baseSurface;
    generate.tunnel(tx, sy, rand(10, 15), rand(50, 130), Math.random() * 0.1, 2.0);
}
for (var i = 0; i < surfLarge; i++) {
    var tx = rand(beachLeft, beachRight);
    var sy = surfaceY[tx] || baseSurface;
    generate.tunnel(tx, sy, rand(12, 25), rand(150, 500), 0, 4.0);
    generate.tunnel(tx, sy, rand(8, 17), rand(60, 200), 0, 2.0);
    generate.tunnel(tx, sy, rand(5, 13), rand(40, 170), 0, 2.0);
}
for (var i = 0; i < surfHoriz; i++) {
    var tx = rand(beachLeft, beachRight);
    var sy = surfaceY[tx] || baseSurface;
    generate.tunnel(tx, sy + rand(10, 40), rand(7, 12), rand(150, 250), 0, 1.0);
}
log.progress(0.47);
log.print("  Carved " + (surfSmall + surfMedium + surfLarge * 3 + surfHoriz) + " surface caves.");

// ═══════════════════════════════════════════════════════════════════════
// Step 8: Spread grass on exposed dirt
// ═══════════════════════════════════════════════════════════════════════
log.print("Step 8: Spreading grass...");
for (var x = 0; x < w; x++) {
    // Walk down from sky to find first active tile
    var startY = Math.max(0, surfaceY[x] - 5);
    for (var y = startY; y < surfaceY[x] + 30; y++) {
        if (y >= h) break;
        if (tile.isActive(x, y) && tile.getTileType(x, y) === DIRT) {
            // Check if exposed (air above)
            if (y === 0 || !tile.isActive(x, y - 1)) {
                tile.setType(x, y, GRASS);
            }
            break;
        }
    }
    if (x % 500 === 0) log.progress(0.47 + (x / w) * 0.02);
}
log.progress(0.49);
log.print("  Grass spread complete.");

// ═══════════════════════════════════════════════════════════════════════
// Step 9: Ore veins (actual Terraria counts, per depth zone)
// ═══════════════════════════════════════════════════════════════════════
log.print("Step 9: Placing ore veins...");

var oreZones = [
    // Copper — 3 depth zones
    { tile: metadata.tileId("Copper Ore"), mult: 6e-5,  y0: surfaceLow,  y1: surfaceHigh,  sMin: 3, sMax: 6, stMin: 2, stMax: 6 },
    { tile: metadata.tileId("Copper Ore"), mult: 8e-5,  y0: surfaceHigh, y1: rockLayerHigh, sMin: 3, sMax: 7, stMin: 3, stMax: 7 },
    { tile: metadata.tileId("Copper Ore"), mult: 2e-4,  y0: rockLayerLow, y1: h - 20,       sMin: 4, sMax: 9, stMin: 4, stMax: 8 },
    // Iron — 3 depth zones
    { tile: metadata.tileId("Iron Ore"),   mult: 3e-5,  y0: surfaceLow,  y1: surfaceHigh,  sMin: 3, sMax: 7, stMin: 2, stMax: 5 },
    { tile: metadata.tileId("Iron Ore"),   mult: 8e-5,  y0: surfaceHigh, y1: rockLayerHigh, sMin: 3, sMax: 6, stMin: 3, stMax: 6 },
    { tile: metadata.tileId("Iron Ore"),   mult: 2e-4,  y0: rockLayerLow, y1: h - 20,       sMin: 4, sMax: 9, stMin: 4, stMax: 8 },
    // Silver — 2 depth zones
    { tile: metadata.tileId("Silver Ore"), mult: 2.6e-5, y0: surfaceHigh, y1: rockLayerHigh, sMin: 3, sMax: 6, stMin: 3, stMax: 6 },
    { tile: metadata.tileId("Silver Ore"), mult: 1.5e-4, y0: rockLayerLow, y1: h - 20,       sMin: 4, sMax: 9, stMin: 4, stMax: 8 },
    // Gold — cavern only
    { tile: metadata.tileId("Gold Ore"),   mult: 1.2e-4, y0: rockLayerLow, y1: h - 20,       sMin: 4, sMax: 8, stMin: 4, stMax: 8 },
];

var totalOres = 0;
for (var z = 0; z < oreZones.length; z++) {
    var oz = oreZones[z];
    var count = Math.floor(area * oz.mult);
    for (var i = 0; i < count; i++) {
        var ox = rand(10, w - 10);
        var oy = rand(oz.y0, oz.y1);
        generate.tileRunner(ox, oy, rand(oz.sMin, oz.sMax), rand(oz.stMin, oz.stMax), oz.tile);
        totalOres++;
    }
    log.progress(0.49 + (z / oreZones.length) * 0.30);
}
log.progress(0.79);
log.print("  Placed " + totalOres + " ore veins.");

// ═══════════════════════════════════════════════════════════════════════
// Step 10: Clay and silt patches
// ═══════════════════════════════════════════════════════════════════════
log.print("Step 10: Placing clay and silt patches...");
var clayCount = Math.floor(area * 5e-5);
var siltCount = Math.floor(area * 3e-5);

for (var i = 0; i < clayCount; i++) {
    var px = rand(10, w - 10);
    var py = rand(baseSurface, baseRock);
    generate.tileRunner(px, py, rand(4, 14), rand(5, 40), CLAY);
}
for (var i = 0; i < siltCount; i++) {
    var px = rand(10, w - 10);
    var py = rand(baseRock, h - 40);
    generate.tileRunner(px, py, rand(3, 10), rand(5, 30), SILT);
}
log.progress(0.84);
log.print("  Placed " + clayCount + " clay + " + siltCount + " silt patches.");

// ═══════════════════════════════════════════════════════════════════════
// Step 11: Underground water lakes
// ═══════════════════════════════════════════════════════════════════════
log.print("Step 11: Creating underground lakes...");
var lakeCount = Math.floor(w * 0.01);
for (var i = 0; i < lakeCount; i++) {
    var lx = rand(50, w - 50);
    var ly = rand(baseRock, lavaLine);
    generate.lake(lx, ly, "water", 0.8 + Math.random() * 0.8);
}
log.progress(0.88);
log.print("  Created " + lakeCount + " water lakes.");

// ═══════════════════════════════════════════════════════════════════════
// Step 12: Lava pools in deep caverns
// ═══════════════════════════════════════════════════════════════════════
log.print("Step 12: Creating lava pools...");
var lavaCount = Math.floor(w * 0.005);
for (var i = 0; i < lavaCount; i++) {
    var lx = rand(50, w - 50);
    var ly = rand(lavaLine, h - 30);
    generate.lake(lx, ly, "lava", 0.6 + Math.random() * 0.6);
}
log.progress(0.91);
log.print("  Created " + lavaCount + " lava pools.");

// ═══════════════════════════════════════════════════════════════════════
// Step 13: Honey pockets
// ═══════════════════════════════════════════════════════════════════════
log.print("Step 13: Creating honey pockets...");
var honeyCount = Math.max(3, Math.floor(w * 0.002));
for (var i = 0; i < honeyCount; i++) {
    var lx = rand(50, w - 50);
    var ly = rand(baseRock, lavaLine);
    generate.lake(lx, ly, "honey", 0.4 + Math.random() * 0.4);
}
log.progress(0.93);
log.print("  Created " + honeyCount + " honey pockets.");

// ═══════════════════════════════════════════════════════════════════════
// Step 14: Plant surface forest
// ═══════════════════════════════════════════════════════════════════════
log.print("Step 14: Growing surface forest...");
var treesPlaced = generate.forest(
    ["oak", "oak", "oak", "sakura", "willow"],
    beachLeft, Math.min.apply(null, surfaceY) - 30,
    beachRight - beachLeft, Math.max.apply(null, surfaceY) - Math.min.apply(null, surfaceY) + 60,
    0.15
);
log.progress(1.0);
log.print("  Grew " + treesPlaced + " trees.");

// ═══════════════════════════════════════════════════════════════════════
// Summary
// ═══════════════════════════════════════════════════════════════════════
var totalCaves = (smallHoleCount * 2) + dirtCaveCount + rockCaveCount +
    surfSmall + surfMedium + (surfLarge * 3) + surfHoriz;
log.print("");
log.print("=== WorldGen Showcase Complete ===");
log.print("  World: " + w + "x" + h + " (" + area + " tiles)");
log.print("  Surface: " + baseSurface + " (range " +
    Math.min.apply(null, surfaceY) + "-" + Math.max.apply(null, surfaceY) + ")");
log.print("  Rock layer: " + baseRock + ", Lava: " + lavaLine);
log.print("  Scatter blobs: " + rocksInDirt + " stone-in-dirt, " +
    dirtInRocks + " dirt-in-rocks, " + mudCount + " mud");
log.print("  Cave calls: " + totalCaves);
log.print("  Ore veins: " + totalOres);
log.print("  Lakes: " + lakeCount + " water, " + lavaCount + " lava, " + honeyCount + " honey");
log.print("  Trees: " + treesPlaced);
