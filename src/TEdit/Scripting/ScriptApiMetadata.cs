using System.Collections.Generic;

namespace TEdit.Scripting;

/// <summary>
/// Central metadata for all scripting API modules. Drives autocomplete, in-app reference, and doc generation.
/// </summary>
public static class ScriptApiMetadata
{
    public record ApiModule(string Name, string Description, IReadOnlyList<ApiMethod> Methods);
    public record ApiMethod(string Name, string Signature, string Description);

    public static IReadOnlyList<ApiModule> Modules { get; } = BuildModules();

    private static List<ApiModule> BuildModules() =>
    [
        new("tile", "Low-level tile read/write operations",
        [
            new("isActive",      "isActive(x, y) → bool",         "Check if tile is active"),
            new("getTileType",   "getTileType(x, y) → int",       "Get tile type ID"),
            new("getWall",       "getWall(x, y) → int",           "Get wall type ID"),
            new("getPaint",      "getPaint(x, y) → int",          "Get tile paint color"),
            new("getWallPaint",  "getWallPaint(x, y) → int",      "Get wall paint color"),
            new("getLiquidAmount","getLiquidAmount(x, y) → int",   "Get liquid amount (0-255)"),
            new("getLiquidType", "getLiquidType(x, y) → int",      "Get liquid type (0=none, 1=water, 2=lava, 3=honey)"),
            new("getFrameU",     "getFrameU(x, y) → int",         "Get sprite frame U coordinate"),
            new("getFrameV",     "getFrameV(x, y) → int",         "Get sprite frame V coordinate"),
            new("getSlope",      "getSlope(x, y) → string",       "Get slope type as string"),
            new("getWire",       "getWire(x, y, color) → bool",   "Check wire (color: 1=red, 2=blue, 3=green, 4=yellow)"),
            new("setActive",     "setActive(x, y, active)",        "Set tile active state"),
            new("setType",       "setType(x, y, type)",            "Set tile type (also activates tile)"),
            new("setWall",       "setWall(x, y, wallType)",        "Set wall type"),
            new("setPaint",      "setPaint(x, y, color)",          "Set tile paint color"),
            new("setWallPaint",  "setWallPaint(x, y, color)",      "Set wall paint color"),
            new("setLiquid",     "setLiquid(x, y, amount, type)",  "Set liquid amount and type"),
            new("setWire",       "setWire(x, y, color, enabled)",  "Set wire state"),
            new("setSlope",      "setSlope(x, y, slope)",          "Set slope (None, HalfBrick, SlopeTopRight, etc.)"),
            new("setFrameUV",    "setFrameUV(x, y, u, v)",        "Set sprite frame coordinates"),
            new("clear",         "clear(x, y)",                    "Reset tile to default empty state"),
            new("copy",          "copy(fromX, fromY, toX, toY)",   "Copy all properties from one tile to another"),
        ]),

        new("batch", "High-performance bulk operations across world or selection",
        [
            new("forEachTile",          "forEachTile(callback)",                  "Iterate entire world, calls callback(x, y) for each tile"),
            new("forEachInSelection",   "forEachInSelection(callback)",           "Iterate selection area, calls callback(x, y)"),
            new("findTiles",            "findTiles(predicate) → [{x, y}]",       "Find tiles matching predicate(x, y) → bool (max 10,000)"),
            new("findTilesByType",      "findTilesByType(tileType, anchorOnly?) → [{x, y}]",  "Find tiles of type; anchorOnly=true returns only sprite origin tiles"),
            new("findTilesByWall",      "findTilesByWall(wallType) → [{x, y}]",  "Find all tiles with specific wall (max 10,000)"),
            new("replaceTile",          "replaceTile(fromType, toType) → int",    "Replace all tiles of one type with another"),
            new("replaceTileInSelection","replaceTileInSelection(from, to) → int","Replace tiles in selection only"),
            new("replaceWall",          "replaceWall(fromType, toType) → int",    "Replace all walls of one type with another"),
            new("clearTilesByType",     "clearTilesByType(tileType) → int",       "Clear all tiles of specified type"),
        ]),

        new("geometry", "Shape generation and bulk fill operations",
        [
            new("line",        "line(x1, y1, x2, y2) → [{x, y}]",      "Generate line coordinates"),
            new("rect",        "rect(x, y, w, h) → [{x, y}]",          "Generate rectangle outline coordinates"),
            new("ellipse",     "ellipse(cx, cy, rx, ry) → [{x, y}]",   "Generate ellipse outline coordinates"),
            new("fillRect",    "fillRect(x, y, w, h) → [{x, y}]",      "Generate filled rectangle coordinates"),
            new("fillEllipse", "fillEllipse(cx, cy, rx, ry) → [{x, y}]","Generate filled ellipse coordinates"),
            new("setTiles",    "setTiles(tileType, x, y, w, h)",         "Fill rectangle with tile type"),
            new("setWalls",    "setWalls(wallType, x, y, w, h)",         "Fill rectangle with wall type"),
            new("clearTiles",  "clearTiles(x, y, w, h)",                 "Clear all tiles in rectangle"),
        ]),

        new("selection", "Query and manipulate the current selection rectangle",
        [
            new("isActive", "isActive → bool",    "Whether a selection is active"),
            new("x",        "x → int",            "Selection X coordinate"),
            new("y",        "y → int",            "Selection Y coordinate"),
            new("width",    "width → int",         "Selection width"),
            new("height",   "height → int",        "Selection height"),
            new("left",     "left → int",          "Left edge"),
            new("top",      "top → int",           "Top edge"),
            new("right",    "right → int",         "Right edge (exclusive)"),
            new("bottom",   "bottom → int",        "Bottom edge (exclusive)"),
            new("set",      "set(x, y, width, height)", "Create and activate a selection"),
            new("clear",    "clear()",              "Deactivate selection"),
            new("contains", "contains(x, y) → bool","Check if coordinate is in selection"),
        ]),

        new("chests", "Query and modify chest inventories",
        [
            new("count",        "count → int",                            "Total number of chests"),
            new("getAll",       "getAll() → [{x, y, name, items}]",     "Get all chests"),
            new("getAt",        "getAt(x, y) → {x, y, name, items}",   "Get chest at tile coordinates"),
            new("findByItem",   "findByItem(itemId) → [{...}]",         "Find chests containing item ID"),
            new("findByItemName","findByItemName(name) → [{...}]",      "Find chests containing item by name"),
            new("setItem",      "setItem(x, y, slot, itemId, stack, prefix)", "Set item in specific slot"),
            new("clearItem",    "clearItem(x, y, slot)",                 "Clear item from slot"),
            new("addItem",      "addItem(x, y, itemId, stack, prefix) → bool", "Add item to first empty slot"),
        ]),

        new("signs", "Read and modify sign text",
        [
            new("count",   "count → int",                     "Total number of signs"),
            new("getAll",  "getAll() → [{x, y, text}]",      "Get all signs"),
            new("getAt",   "getAt(x, y) → {x, y, text}",    "Get sign at tile coordinates"),
            new("setText",  "setText(x, y, text)",             "Update sign text"),
        ]),

        new("npcs", "Query and modify town NPC data",
        [
            new("count",   "count → int",                      "Total number of NPCs"),
            new("getAll",  "getAll() → [{name, displayName, x, y, homeX, homeY, isHomeless}]", "Get all NPCs"),
            new("setHome", "setHome(name, x, y)",              "Set NPC home location by name"),
        ]),

        new("world", "Read-only world metadata",
        [
            new("width",        "width → int",        "World width in tiles"),
            new("height",       "height → int",       "World height in tiles"),
            new("title",        "title → string",     "World name"),
            new("seed",         "seed → string",      "World seed"),
            new("spawnX",       "spawnX → int",       "Player spawn X coordinate"),
            new("spawnY",       "spawnY → int",       "Player spawn Y coordinate"),
            new("surfaceLevel", "surfaceLevel → double", "Surface level Y coordinate"),
            new("rockLevel",    "rockLevel → double",    "Underground level Y coordinate"),
        ]),

        new("metadata", "Lookup Terraria game data (tile/wall/item names and IDs)",
        [
            new("tileId",    "tileId(name) → int",        "Get tile ID by name (-1 if not found)"),
            new("wallId",    "wallId(name) → int",        "Get wall ID by name (-1 if not found)"),
            new("itemId",    "itemId(name) → int",        "Get item ID by name (-1 if not found)"),
            new("tileName",  "tileName(id) → string",     "Get tile name by ID"),
            new("wallName",  "wallName(id) → string",     "Get wall name by ID"),
            new("itemName",  "itemName(id) → string",     "Get item name by ID"),
            new("allTiles",  "allTiles() → [{id, name}]", "Get all tile definitions"),
            new("allWalls",  "allWalls() → [{id, name}]", "Get all wall definitions"),
            new("allItems",  "allItems() → [{id, name}]", "Get all item definitions"),
        ]),

        new("log", "Output messages and progress from scripts",
        [
            new("print",    "print(message)",           "Log info message"),
            new("warn",     "warn(message)",            "Log warning message"),
            new("error",    "error(message)",           "Log error message"),
            new("progress", "progress(value)",          "Update progress bar (0.0 to 1.0)"),
        ]),

        new("finder", "Populate Find sidebar with script search results",
        [
            new("clear",         "clear()",                                     "Clear all find results"),
            new("addResult",     "addResult(name, x, y, type, extraInfo?) → bool", "Add result to Find sidebar (max 1000)"),
            new("navigate",      "navigate(index)",                             "Navigate to result by index"),
            new("navigateFirst", "navigateFirst()",                             "Navigate to first result"),
        ]),

        new("tools", "Interact with TEdit's UI tools and clipboard",
        [
            new("listTools",         "listTools() → [string]",     "Get names of all available tools"),
            new("copySelection",     "copySelection()",            "Copy current selection to clipboard"),
            new("getTilePickerTile", "getTilePickerTile() → int",  "Get currently selected tile in picker"),
            new("getTilePickerWall", "getTilePickerWall() → int",  "Get currently selected wall in picker"),
            new("setTilePickerTile", "setTilePickerTile(tileType)","Set tile picker selection"),
            new("setTilePickerWall", "setTilePickerWall(wallType)","Set wall picker selection"),
        ]),
    ];
}
