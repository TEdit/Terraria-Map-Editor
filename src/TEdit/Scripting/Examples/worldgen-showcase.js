/// <reference path="tedit-api.d.ts" />
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
    // Stone layer (down to lava line only — underworld handles below)
    for (var y = ry; y < lavaLine; y++) {
        tile.setType(x, y, STONE);
    }

    if (x % 500 === 0) log.progress(0.02 + (x / w) * 0.06);
}
log.progress(0.08);
log.print("  Terrain filled (dirt + stone layers).");

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
    log.progress(0.49 + (z / oreZones.length) * 0.25);
}
log.progress(0.74);
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
log.progress(0.78);
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
log.progress(0.81);
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
log.progress(0.83);
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
log.progress(0.85);
log.print("  Created " + honeyCount + " honey pockets.");

// ═══════════════════════════════════════════════════════════════════════
// Step 14: Underworld (hell layer with natural ceiling/lava floor)
// ═══════════════════════════════════════════════════════════════════════
log.print("Step 14: Generating underworld...");
var underworldTiles = generate.underworld();
log.print("  Underworld: " + underworldTiles + " ash tiles + hellstone + lava");
log.progress(0.87);

// ═══════════════════════════════════════════════════════════════════════
// Step 15: Oceans at world edges
// ═══════════════════════════════════════════════════════════════════════
log.print("Step 15: Generating oceans...");
var oceanDepth = Math.floor(h * 0.07);  // ~7% of world height
var oceanWidth = Math.floor(w * 0.12);  // ~12% of world width each side

var leftOceanTiles = generate.ocean(-1, oceanWidth, oceanDepth);
log.print("  Left ocean: " + leftOceanTiles + " tiles");

var rightOceanTiles = generate.ocean(1, oceanWidth, oceanDepth);
log.print("  Right ocean: " + rightOceanTiles + " tiles");
log.progress(0.89);

// ═══════════════════════════════════════════════════════════════════════
// Step 16: Biome generation
// ═══════════════════════════════════════════════════════════════════════
log.print("Step 16: Generating biomes...");

// Ice biome — right third of the world, surface to rock layer (trapezoid: wider underground)
var iceX = Math.floor(w * 0.65);
var iceW = Math.floor(w * 0.25);
var iceTiles = generate.iceBiome(iceX, baseSurface - 10, iceW, baseRock - baseSurface + 30, "trapezoid");
log.print("  Ice biome: " + iceTiles + " tiles converted");

// Jungle — left quarter of the world, fills entire vertical (surface to near lavaLine)
var jungleX = Math.floor(w * 0.12);  // after ocean
var jungleW = Math.floor(w * 0.20);
var jungleH = lavaLine - baseSurface + 5;  // full vertical extent like Terraria
var jungleTiles = generate.jungle(jungleX, baseSurface - 5, jungleW, jungleH, "rectangle");
log.print("  Jungle: " + jungleTiles + " tiles converted");

// Desert — center-right area, tall ellipse spanning surface to lavaLine
var desertX = Math.floor(w * 0.42);
var desertW = Math.floor(w * 0.10);
var desertH = lavaLine - baseSurface + 5;  // full vertical extent like Terraria
var desertTiles = generate.desert(desertX, baseSurface - 5, desertW, desertH, "ellipse");
log.print("  Desert: " + desertTiles + " tiles converted");

// Corruption strip — diagonal V strip
var corruptX = Math.floor(w * 0.30);
var corruptDepth = baseRock - baseSurface + 40;
var corruptTiles = generate.corruption(corruptX, baseSurface - 3, 60, corruptDepth, "diagonalLeft");
log.print("  Corruption: " + corruptTiles + " tiles + chasms");

// Crimson strip — opposite diagonal
var crimsonX = Math.floor(w * 0.37);
var crimsonTiles = generate.crimson(crimsonX, baseSurface - 3, 60, corruptDepth, "diagonalRight");
log.print("  Crimson: " + crimsonTiles + " tiles + chasms");

// Hallow strip — diagonal like hardmode V
var hallowX = Math.floor(w * 0.53);
var hallowH = baseRock - baseSurface + 30;
var hallowTiles = generate.hallow(hallowX, baseSurface - 3, 50, hallowH, "diagonalLeft");
log.print("  Hallow: " + hallowTiles + " tiles converted");

// Mushroom biome — small patch in deep cavern (ellipse: oblate)
var mushroomX = Math.floor(w * 0.55);
var mushroomTiles = generate.mushroomBiome(mushroomX, baseRock + 10, 60, 40, "ellipse");
log.print("  Mushroom biome: " + mushroomTiles + " tiles converted");

// Marble and granite caves — scattered in rock layer
var marbleCount = Math.max(2, Math.floor(w * 0.001));
for (var i = 0; i < marbleCount; i++) {
    generate.marbleCave(rand(50, w - 50), rand(baseRock, lavaLine));
}
var graniteCount = Math.max(2, Math.floor(w * 0.001));
for (var i = 0; i < graniteCount; i++) {
    generate.graniteCave(rand(50, w - 50), rand(baseRock, lavaLine));
}
log.print("  Marble caves: " + marbleCount + ", Granite caves: " + graniteCount);

// Spider caves — scattered in cavern
var spiderCount = Math.max(2, Math.floor(w * 0.001));
for (var i = 0; i < spiderCount; i++) {
    generate.spiderCave(rand(50, w - 50), rand(baseRock, lavaLine));
}
log.print("  Spider caves: " + spiderCount);

log.progress(0.91);

// ═══════════════════════════════════════════════════════════════════════
// Step 17: Structures
// ═══════════════════════════════════════════════════════════════════════
log.print("Step 17: Placing structures...");

// Beehives in jungle underground
var hiveCount = Math.max(1, Math.floor(jungleW * 0.02));
for (var i = 0; i < hiveCount; i++) {
    generate.beehive(
        rand(jungleX + 10, jungleX + jungleW - 10),
        rand(baseRock + 10, lavaLine - 30)
    );
}
log.print("  Beehives: " + hiveCount);

// Underground houses — scattered in dirt/rock
var houseCount = Math.max(5, Math.floor(w * 0.005));
for (var i = 0; i < houseCount; i++) {
    generate.undergroundHouse(rand(50, w - 50), rand(baseSurface + 20, lavaLine - 20));
}
log.print("  Underground houses: " + houseCount);

log.progress(0.93);

// ═══════════════════════════════════════════════════════════════════════
// Step 18: Plant surface forest
// ═══════════════════════════════════════════════════════════════════════
log.print("Step 18: Growing forests...");
var treesPlaced = generate.forest(
    ["oak", "oak", "oak", "sakura", "willow"],
    beachLeft, Math.min.apply(null, surfaceY) - 30,
    beachRight - beachLeft, Math.max.apply(null, surfaceY) - Math.min.apply(null, surfaceY) + 60,
    0.15
);
log.print("  Surface trees: " + treesPlaced);

// Jungle trees (living mahogany)
var jungleTrees = generate.forest(["jungle"], jungleX, baseSurface - 10, jungleW, 30, 0.12);
log.print("  Jungle trees: " + jungleTrees);

// Mushroom trees in mushroom biome
var mushroomTrees = generate.forest(["mushroom"], mushroomX, baseRock + 10, 60, 40, 0.08);
log.print("  Mushroom trees: " + mushroomTrees);

// ═══════════════════════════════════════════════════════════════════════
// Step 19: Decoration pass
// ═══════════════════════════════════════════════════════════════════════
log.print("Step 19: Decorating world...");

// Vines in forest
var forestVines = generate.placeVines(beachLeft, baseSurface - 10, beachRight - beachLeft, baseRock - baseSurface + 30, "forest");
log.print("  Forest vines: " + forestVines);

// Vines in jungle
var jungleVines = generate.placeVines(jungleX, baseSurface - 5, jungleW, baseRock - baseSurface + 60, "jungle");
log.print("  Jungle vines: " + jungleVines);

// Plants
var forestPlants = generate.placePlants(beachLeft, baseSurface - 10, beachRight - beachLeft, 30, "forest");
var junglePlants = generate.placePlants(jungleX, baseSurface - 5, jungleW, 30, "jungle");
log.print("  Plants: " + forestPlants + " forest, " + junglePlants + " jungle");

// Pots underground
var pots = generate.placePots(0, baseSurface + 20, w, lavaLine - baseSurface - 20);
log.print("  Pots: " + pots);

// Stalactites in caves
var stalactites = generate.placeStalactites(0, baseRock, w, lavaLine - baseRock);
log.print("  Stalactites: " + stalactites);

// Life crystals underground
var crystals = generate.placeLifeCrystals(0, baseRock, w, lavaLine - baseRock);
log.print("  Life crystals: " + crystals);

// Sunflowers on grass
var sunflowers = generate.placeSunflowers(beachLeft, baseSurface - 10, beachRight - beachLeft, 30);
log.print("  Sunflowers: " + sunflowers);

// Thorns in corruption/crimson
var corruptThorns = generate.placeThorns(corruptX, baseSurface - 3, 60, corruptDepth, "corruption");
var crimsonThorns = generate.placeThorns(crimsonX, baseSurface - 3, 60, corruptDepth, "crimson");
log.print("  Thorns: " + corruptThorns + " corruption, " + crimsonThorns + " crimson");

// Traps underground
var traps = generate.placeTraps(0, baseRock, w, lavaLine - baseRock);
log.print("  Traps: " + traps);

// Crimson vines
var crimsonVines = generate.placeVines(crimsonX, baseSurface - 3, 60, 30, "crimson");
log.print("  Crimson vines: " + crimsonVines);

// Smooth world edges
var smoothed = generate.smoothWorld(0, baseSurface - 20, w, 40);
log.print("  Smoothed edges: " + smoothed);

log.progress(1.0);

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
log.print("  Underworld: " + underworldTiles + " ash tiles");
log.print("  Oceans: left " + leftOceanTiles + ", right " + rightOceanTiles + " tiles");
log.print("  Biomes: ice, jungle, desert, corruption, crimson, hallow, mushroom");
log.print("  Structures: " + hiveCount + " hives, " + houseCount + " houses");
log.print("  Decoration: " + forestVines + " vines, " + forestPlants + " plants, " + pots + " pots, " + traps + " traps");
log.print("  Trees: " + treesPlaced + " surface, " + jungleTrees + " jungle, " + mushroomTrees + " mushroom");
