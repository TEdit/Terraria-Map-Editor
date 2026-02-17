// Geometric Mandala - Creates a symmetric geometric pattern using stone blocks
// Place your selection where you want the pattern centered, or it uses world spawn
// The pattern has 8-fold symmetry with concentric rings, rays, and diamond accents

var STONE = 1;
var GOLD = 167;    // Gold Brick
var DIAMOND = 117; // Amethyst (purple gem)
var OBSIDIAN = 56;

// Determine center point from selection or spawn
var cx, cy;
if (selection.isActive) {
    cx = Math.floor(selection.x + selection.width / 2);
    cy = Math.floor(selection.y + selection.height / 2);
} else {
    cx = world.spawnX;
    cy = world.spawnY;
}

var radius = 40;
var placed = 0;

// Helper: place a tile at (x,y) with bounds checking
function place(x, y, tileType) {
    if (x >= 0 && x < world.width && y >= 0 && y < world.height) {
        tile.setActive(x, y, true);
        tile.setType(x, y, tileType);
        placed++;
    }
}

// Helper: mirror a point with 8-fold symmetry around center
function mirror8(dx, dy, tileType) {
    place(cx + dx, cy + dy, tileType);
    place(cx - dx, cy + dy, tileType);
    place(cx + dx, cy - dy, tileType);
    place(cx - dx, cy - dy, tileType);
    place(cx + dy, cy + dx, tileType);
    place(cx - dy, cy + dx, tileType);
    place(cx + dy, cy - dx, tileType);
    place(cx - dy, cy - dx, tileType);
}

log.print("Drawing mandala centered at (" + cx + ", " + cy + ") with radius " + radius);

// --- Concentric rings ---
var ringRadii = [10, 20, 30, 40];
for (var r = 0; r < ringRadii.length; r++) {
    var rad = ringRadii[r];
    var ringType = (r % 2 === 0) ? STONE : GOLD;
    var points = geometry.ellipse(cx, cy, rad, rad);
    for (var i = 0; i < points.length; i++) {
        place(points[i].x, points[i].y, ringType);
    }
    log.progress((r + 1) / (ringRadii.length + 8));
}

// --- 8 radial rays from center ---
var angles = 8;
for (var a = 0; a < angles; a++) {
    var angle = (a * Math.PI * 2) / angles;
    var x2 = cx + Math.round(Math.cos(angle) * radius);
    var y2 = cy + Math.round(Math.sin(angle) * radius);
    var rayPoints = geometry.line(cx, cy, x2, y2);
    for (var i = 0; i < rayPoints.length; i++) {
        place(rayPoints[i].x, rayPoints[i].y, STONE);
    }
    log.progress((ringRadii.length + a + 1) / (ringRadii.length + angles));
}

// --- Diamond accents between rings ---
var diamondOffsets = [15, 25, 35];
for (var d = 0; d < diamondOffsets.length; d++) {
    var dist = diamondOffsets[d];
    var size = 3;
    // Place diamonds along 8 axes
    for (var a = 0; a < 8; a++) {
        var angle = (a * Math.PI * 2) / 8;
        var dcx = cx + Math.round(Math.cos(angle) * dist);
        var dcy = cy + Math.round(Math.sin(angle) * dist);
        // Draw small diamond shape
        for (var s = 0; s <= size; s++) {
            place(dcx + s, dcy, DIAMOND);
            place(dcx - s, dcy, DIAMOND);
            place(dcx, dcy + s, DIAMOND);
            place(dcx, dcy - s, DIAMOND);
        }
    }
}

// --- Inner starburst pattern ---
for (var a = 0; a < 16; a++) {
    var angle = (a * Math.PI * 2) / 16;
    var innerLen = (a % 2 === 0) ? 8 : 5;
    var x2 = cx + Math.round(Math.cos(angle) * innerLen);
    var y2 = cy + Math.round(Math.sin(angle) * innerLen);
    var starPoints = geometry.line(cx, cy, x2, y2);
    var starType = (a % 2 === 0) ? OBSIDIAN : GOLD;
    for (var i = 0; i < starPoints.length; i++) {
        place(starPoints[i].x, starPoints[i].y, starType);
    }
}

// --- Decorative arcs between the outer rays ---
for (var a = 0; a < 8; a++) {
    var angle1 = (a * Math.PI * 2) / 8;
    var angle2 = ((a + 1) * Math.PI * 2) / 8;
    var midAngle = (angle1 + angle2) / 2;
    // Scallop arc: 5 points along arc between rays at radius 38
    for (var t = 0; t <= 6; t++) {
        var frac = t / 6;
        var arcAngle = angle1 + frac * (angle2 - angle1);
        // Vary radius to create scalloped edge
        var arcR = 38 + Math.round(3 * Math.sin(frac * Math.PI));
        var ax = cx + Math.round(Math.cos(arcAngle) * arcR);
        var ay = cy + Math.round(Math.sin(arcAngle) * arcR);
        place(ax, ay, OBSIDIAN);
    }
}

log.progress(1.0);
log.print("Mandala complete! Placed " + placed + " tiles.");
log.print("Materials: Stone, Gold Brick, Amethyst, Obsidian");
