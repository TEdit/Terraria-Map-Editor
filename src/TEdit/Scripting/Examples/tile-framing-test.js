// Tile Framing Test Grid
// Generates labeled blocks of tile types to visually verify framing fixes:
// - Phase 1: SelfFrame8Way gemspark blocks
// - Phase 2: Deterministic variation (no more flickering)
// - Phase 3: tileLargeFrames phlebas/lazure patterns
// Place selection where you want the grid, or it uses world spawn.

var cx, cy;
if (selection.isActive) {
    cx = selection.x + 2;
    cy = selection.y + 2;
} else {
    cx = world.spawnX;
    cy = world.spawnY - 20;
}

var BLOCK_SIZE = 12;   // each test block is 12x12 tiles
var GAP = 3;           // gap between blocks
var COL_STRIDE = BLOCK_SIZE + GAP;
var ROW_STRIDE = BLOCK_SIZE + GAP;

var placed = 0;

function place(x, y, tileType) {
    if (x >= 0 && x < world.width && y >= 0 && y < world.height) {
        tile.setType(x, y, tileType);
        placed++;
    }
}

function clearArea(x, y, w, h) {
    for (var dx = 0; dx < w; dx++) {
        for (var dy = 0; dy < h; dy++) {
            var tx = x + dx;
            var ty = y + dy;
            if (tx >= 0 && tx < world.width && ty >= 0 && ty < world.height) {
                tile.clear(tx, ty);
            }
        }
    }
}

// Fill a block-sized region with a tile type, with some internal patterns
function fillBlock(bx, by, tileType) {
    for (var dx = 0; dx < BLOCK_SIZE; dx++) {
        for (var dy = 0; dy < BLOCK_SIZE; dy++) {
            place(bx + dx, by + dy, tileType);
        }
    }
}

// Fill block with mixed pattern: solid center + isolated tiles + edges
function fillMixedBlock(bx, by, tileType) {
    // Solid 6x6 center
    for (var dx = 3; dx < 9; dx++) {
        for (var dy = 3; dy < 9; dy++) {
            place(bx + dx, by + dy, tileType);
        }
    }
    // Isolated single tiles in corners
    place(bx + 0, by + 0, tileType);
    place(bx + 11, by + 0, tileType);
    place(bx + 0, by + 11, tileType);
    place(bx + 11, by + 11, tileType);
    // Horizontal strip at top
    for (var dx = 2; dx < 10; dx++) {
        place(bx + dx, by + 1, tileType);
    }
    // Vertical strip at left
    for (var dy = 2; dy < 10; dy++) {
        place(bx + 1, by + dy, tileType);
    }
    // L-shape bottom-right
    place(bx + 10, by + 10, tileType);
    place(bx + 10, by + 11, tileType);
    place(bx + 11, by + 10, tileType);
}

// Fill block with slope test pattern: center solid + slopes around edges
function fillSlopeBlock(bx, by, tileType) {
    // Solid 8x8 center
    for (var dx = 2; dx < 10; dx++) {
        for (var dy = 2; dy < 10; dy++) {
            place(bx + dx, by + dy, tileType);
        }
    }
    // Top edge: SlopeTopRight (2) and SlopeTopLeft (3)
    for (var dx = 2; dx < 6; dx++) {
        place(bx + dx, by + 1, tileType);
        tile.setSlope(bx + dx, by + 1, "SlopeTopRight");
    }
    for (var dx = 6; dx < 10; dx++) {
        place(bx + dx, by + 1, tileType);
        tile.setSlope(bx + dx, by + 1, "SlopeTopLeft");
    }
    // Bottom edge: SlopeBottomRight (4) and SlopeBottomLeft (5)
    for (var dx = 2; dx < 6; dx++) {
        place(bx + dx, by + 10, tileType);
        tile.setSlope(bx + dx, by + 10, "SlopeBottomRight");
    }
    for (var dx = 6; dx < 10; dx++) {
        place(bx + dx, by + 10, tileType);
        tile.setSlope(bx + dx, by + 10, "SlopeBottomLeft");
    }
}

// Test tile definitions organized by category
var tests = [
    // Row 0: Gemspark blocks (Phase 1 - SelfFrame8Way)
    { name: "Amber Gemspark",    id: 255, row: 0, col: 0, mode: "mixed" },
    { name: "Amethyst Gemspark", id: 256, row: 0, col: 1, mode: "mixed" },
    { name: "Diamond Gemspark",  id: 263, row: 0, col: 2, mode: "mixed" },
    { name: "Ruby Gemspark",     id: 264, row: 0, col: 3, mode: "mixed" },
    { name: "Offline Gemspark",  id: 385, row: 0, col: 4, mode: "solid" },
    { name: "Gemspark slope",    id: 255, row: 0, col: 5, mode: "slope" },

    // Row 1: Common solid tiles (Phase 2 - deterministic variation)
    { name: "Dirt",              id: 0,   row: 1, col: 0, mode: "solid" },
    { name: "Stone",             id: 1,   row: 1, col: 1, mode: "solid" },
    { name: "Sand",              id: 53,  row: 1, col: 2, mode: "solid" },
    { name: "Mud",               id: 59,  row: 1, col: 3, mode: "solid" },
    { name: "Snow",              id: 147, row: 1, col: 4, mode: "solid" },
    { name: "Ice",               id: 161, row: 1, col: 5, mode: "solid" },

    // Row 2: Brick/merge tiles (Phase 2 - variation + merge)
    { name: "Gray Brick",       id: 38,  row: 2, col: 0, mode: "solid" },
    { name: "Red Brick",        id: 39,  row: 2, col: 1, mode: "solid" },
    { name: "Obsidian Brick",   id: 75,  row: 2, col: 2, mode: "solid" },
    { name: "Pearlstone",       id: 117, row: 2, col: 3, mode: "solid" },
    { name: "Ebonstone",        id: 25,  row: 2, col: 4, mode: "solid" },
    { name: "Crimstone",        id: 203, row: 2, col: 5, mode: "solid" },

    // Row 3: tileLargeFrames phlebas mode 1 (Phase 3)
    { name: "Tin Plating",      id: 273, row: 3, col: 0, mode: "solid" },
    { name: "Confetti (Blk)",   id: 274, row: 3, col: 1, mode: "solid" },
    { name: "Chlorophyte Brk",  id: 284, row: 3, col: 2, mode: "solid" },
    { name: "Martian Conduit",  id: 325, row: 3, col: 3, mode: "solid" },
    { name: "Smooth Marble",    id: 357, row: 3, col: 4, mode: "solid" },
    { name: "Sandstone Slab",   id: 618, row: 3, col: 5, mode: "solid" },

    // Row 4: tileLargeFrames lazure mode 2 (Phase 3)
    { name: "Argon Moss",       id: 669, row: 4, col: 0, mode: "solid" },
    { name: "Krypton Moss",     id: 670, row: 4, col: 1, mode: "solid" },
    { name: "Neon Moss",        id: 671, row: 4, col: 2, mode: "solid" },
    { name: "Xenon Moss",       id: 672, row: 4, col: 3, mode: "solid" },
    { name: "Echo Block",       id: 409, row: 4, col: 4, mode: "solid" },
    { name: "Shimmer Block",    id: 659, row: 4, col: 5, mode: "solid" },

    // Row 5: Slope tests (Phase 4 - slope-aware neighbor detection)
    { name: "Dirt slopes",      id: 0,   row: 5, col: 0, mode: "slope" },
    { name: "Stone slopes",     id: 1,   row: 5, col: 1, mode: "slope" },
    { name: "Tin Plating slp",  id: 273, row: 5, col: 2, mode: "slope" },
    { name: "Marble slopes",    id: 357, row: 5, col: 3, mode: "slope" },
    { name: "Gemspark Ameth",   id: 256, row: 5, col: 4, mode: "slope" },
    { name: "Snow slopes",      id: 147, row: 5, col: 5, mode: "slope" },
];

// Calculate total grid size for clear
var maxRows = 6;
var maxCols = 6;
var totalW = maxCols * COL_STRIDE + GAP;
var totalH = maxRows * ROW_STRIDE + GAP;

log.print("Tile Framing Test Grid at (" + cx + ", " + cy + ")");
log.print("Grid size: " + totalW + "x" + totalH + " tiles (" + tests.length + " test blocks)");

// Clear the entire grid area first
clearArea(cx - 1, cy - 1, totalW + 2, totalH + 2);
log.progress(0.1);

// Place each test block
for (var t = 0; t < tests.length; t++) {
    var test = tests[t];
    var bx = cx + test.col * COL_STRIDE;
    var by = cy + test.row * ROW_STRIDE;

    if (test.mode === "mixed") {
        fillMixedBlock(bx, by, test.id);
    } else if (test.mode === "slope") {
        fillSlopeBlock(bx, by, test.id);
    } else {
        fillBlock(bx, by, test.id);
    }

    log.progress(0.1 + 0.9 * (t + 1) / tests.length);
}

log.progress(1.0);
log.print("Done! Placed " + placed + " tiles in " + tests.length + " test blocks.");
log.print("");
log.print("Row 0: Gemspark blocks (SelfFrame8Way - should show 8-way framing)");
log.print("Row 1: Common solids (deterministic variation - no flickering)");
log.print("Row 2: Bricks/biome stones (merge + variation)");
log.print("Row 3: Phlebas tiles (4x3 repeating pattern)");
log.print("Row 4: Lazure tiles (2x2 repeating pattern)");
log.print("Row 5: Slope tests (slope-aware neighbor detection)");
