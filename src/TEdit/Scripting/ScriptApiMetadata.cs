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
            new("setName",      "setName(x, y, name)",                         "Set chest name/label"),
            new("findByName",   "findByName(name) → [{...}]",                 "Find chests by name (partial match)"),
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

        new("world", "Read and modify world properties",
        [
            // World Size (read-only)
            new("width",        "width → int",        "World width in tiles (read-only)"),
            new("height",       "height → int",       "World height in tiles (read-only)"),

            // World Metadata
            new("title",              "title ↔ string",     "World name"),
            new("worldId",            "worldId ↔ int",      "World ID"),
            new("seed",               "seed ↔ string",      "World seed"),
            new("isFavorite",         "isFavorite ↔ bool",  "Favorite flag"),
            new("isChinese",          "isChinese ↔ bool",   "Chinese edition flag"),
            new("isConsole",          "isConsole ↔ bool",   "Console edition flag"),
            new("fileRevision",       "fileRevision ↔ uint","File revision number"),
            new("worldGenVersion",    "worldGenVersion ↔ ulong", "World generation version"),
            new("creationTime",       "creationTime ↔ long","World creation timestamp"),
            new("lastPlayed",         "lastPlayed ↔ long",  "Last played timestamp"),

            // Spawn & Dungeon
            new("spawnX",       "spawnX ↔ int",       "Player spawn X coordinate"),
            new("spawnY",       "spawnY ↔ int",       "Player spawn Y coordinate"),
            new("dungeonX",     "dungeonX ↔ int",     "Dungeon X coordinate"),
            new("dungeonY",     "dungeonY ↔ int",     "Dungeon Y coordinate"),

            // Levels
            new("surfaceLevel",    "surfaceLevel ↔ double", "Surface level Y coordinate"),
            new("rockLevel",       "rockLevel ↔ double",    "Underground level Y coordinate"),
            new("safeGroundLayers","safeGroundLayers ↔ bool","Enforce safe ground layer gap"),

            // Time
            new("time",                  "time ↔ double",  "Current time value"),
            new("dayTime",               "dayTime ↔ bool", "Is daytime"),
            new("fastForwardTime",       "fastForwardTime ↔ bool", "Sundial active"),
            new("sundialCooldown",       "sundialCooldown ↔ byte", "Sundial cooldown"),
            new("fastForwardTimeToDusk", "fastForwardTimeToDusk ↔ bool", "Moondial active"),
            new("moondialCooldown",      "moondialCooldown ↔ byte", "Moondial cooldown"),

            // Moon
            new("moonPhase",  "moonPhase ↔ int",   "Moon phase (0-7)"),
            new("bloodMoon",  "bloodMoon ↔ bool",   "Blood moon active"),
            new("moonType",   "moonType ↔ byte",    "Moon type"),
            new("isEclipse",  "isEclipse ↔ bool",   "Eclipse active"),

            // Weather
            new("isRaining",            "isRaining ↔ bool",   "Is raining"),
            new("tempRainTime",         "tempRainTime ↔ int",  "Rain time remaining"),
            new("tempMaxRain",          "tempMaxRain ↔ float", "Max rain intensity"),
            new("slimeRainTime",        "slimeRainTime ↔ double", "Slime rain time"),
            new("tempMeteorShowerCount","tempMeteorShowerCount ↔ int", "Meteor shower count"),
            new("tempCoinRain",         "tempCoinRain ↔ int",  "Coin rain count"),
            new("numClouds",            "numClouds ↔ short",   "Number of clouds"),
            new("windSpeedSet",         "windSpeedSet ↔ float","Wind speed"),
            new("cloudBgActive",        "cloudBgActive ↔ float","Cloud background opacity"),

            // Sandstorm
            new("sandStormHappening",          "sandStormHappening ↔ bool",  "Sandstorm active"),
            new("sandStormTimeLeft",           "sandStormTimeLeft ↔ int",    "Sandstorm time left"),
            new("sandStormSeverity",           "sandStormSeverity ↔ float",  "Sandstorm severity"),
            new("sandStormIntendedSeverity",   "sandStormIntendedSeverity ↔ float", "Sandstorm intended severity"),

            // Holidays
            new("forceHalloweenForToday",  "forceHalloweenForToday ↔ bool",  "Force Halloween today"),
            new("forceXMasForToday",       "forceXMasForToday ↔ bool",       "Force Christmas today"),
            new("forceHalloweenForever",   "forceHalloweenForever ↔ bool",   "Force Halloween permanently"),
            new("forceXMasForever",        "forceXMasForever ↔ bool",        "Force Christmas permanently"),

            // Difficulty
            new("hardMode",                       "hardMode ↔ bool",  "Hardmode enabled"),
            new("gameMode",                        "gameMode ↔ int",   "Game mode (0=Classic, 1=Expert, 2=Master, 3=Journey)"),
            new("spawnMeteor",                     "spawnMeteor ↔ bool","Meteor spawn pending"),
            new("combatBookUsed",                  "combatBookUsed ↔ bool", "Combat book used"),
            new("combatBookVolumeTwoWasUsed",      "combatBookVolumeTwoWasUsed ↔ bool", "Combat book vol 2 used"),
            new("peddlersSatchelWasUsed",           "peddlersSatchelWasUsed ↔ bool", "Peddler's satchel used"),
            new("partyOfDoom",                     "partyOfDoom ↔ bool", "Party of doom active"),

            // World Seeds
            new("drunkWorld",              "drunkWorld ↔ bool",              "Drunk world seed"),
            new("goodWorld",               "goodWorld ↔ bool",               "For the Worthy seed"),
            new("tenthAnniversaryWorld",   "tenthAnniversaryWorld ↔ bool",   "10th Anniversary seed"),
            new("dontStarveWorld",         "dontStarveWorld ↔ bool",         "Don't Starve seed"),
            new("notTheBeesWorld",         "notTheBeesWorld ↔ bool",         "Not the Bees seed"),
            new("remixWorld",              "remixWorld ↔ bool",              "Remix seed"),
            new("noTrapsWorld",            "noTrapsWorld ↔ bool",            "No Traps seed"),
            new("zenithWorld",             "zenithWorld ↔ bool",             "Zenith seed"),
            new("skyblockWorld",           "skyblockWorld ↔ bool",           "Skyblock seed"),
            new("vampireSeed",             "vampireSeed ↔ bool",             "Vampire seed"),
            new("infectedSeed",            "infectedSeed ↔ bool",            "Infected seed"),
            new("dualDungeonsSeed",        "dualDungeonsSeed ↔ bool",        "Dual Dungeons seed"),

            // Ore Tiers
            new("isCrimson",               "isCrimson ↔ bool",   "World is Crimson (false = Corruption)"),
            new("altarCount",              "altarCount ↔ int",   "Altars smashed count"),
            new("shadowOrbSmashed",        "shadowOrbSmashed ↔ bool", "Shadow orb/crimson heart smashed"),
            new("shadowOrbCount",          "shadowOrbCount ↔ int",    "Shadow orbs smashed count"),
            new("savedOreTiersCopper",     "savedOreTiersCopper ↔ int",     "Copper ore tier tile ID"),
            new("savedOreTiersIron",       "savedOreTiersIron ↔ int",       "Iron ore tier tile ID"),
            new("savedOreTiersSilver",     "savedOreTiersSilver ↔ int",     "Silver ore tier tile ID"),
            new("savedOreTiersGold",       "savedOreTiersGold ↔ int",       "Gold ore tier tile ID"),
            new("savedOreTiersCobalt",     "savedOreTiersCobalt ↔ int",     "Cobalt ore tier tile ID"),
            new("savedOreTiersMythril",    "savedOreTiersMythril ↔ int",    "Mythril ore tier tile ID"),
            new("savedOreTiersAdamantite", "savedOreTiersAdamantite ↔ int", "Adamantite ore tier tile ID"),

            // Bosses: Pre-Hardmode
            new("downedSlimeKing",     "downedSlimeKing ↔ bool",     "King Slime defeated"),
            new("downedEyeOfCthulhu",  "downedEyeOfCthulhu ↔ bool",  "Eye of Cthulhu defeated"),
            new("downedEaterOfWorlds", "downedEaterOfWorlds ↔ bool", "Eater of Worlds/Brain of Cthulhu defeated"),
            new("downedQueenBee",      "downedQueenBee ↔ bool",      "Queen Bee defeated"),
            new("downedSkeletron",     "downedSkeletron ↔ bool",     "Skeletron defeated"),

            // Bosses: Hardmode
            new("downedDestroyer",      "downedDestroyer ↔ bool",      "Destroyer defeated"),
            new("downedTwins",          "downedTwins ↔ bool",          "Twins defeated"),
            new("downedSkeletronPrime", "downedSkeletronPrime ↔ bool", "Skeletron Prime defeated"),
            new("downedMechBossAny",    "downedMechBossAny → bool",    "Any mech boss defeated (read-only, computed)"),
            new("downedPlantera",       "downedPlantera ↔ bool",       "Plantera defeated"),
            new("downedGolem",          "downedGolem ↔ bool",          "Golem defeated"),
            new("downedFishron",        "downedFishron ↔ bool",        "Duke Fishron defeated"),
            new("downedLunaticCultist", "downedLunaticCultist ↔ bool", "Lunatic Cultist defeated"),
            new("downedMoonlord",       "downedMoonlord ↔ bool",       "Moon Lord defeated"),

            // Bosses: Journey's End
            new("downedEmpressOfLight", "downedEmpressOfLight ↔ bool", "Empress of Light defeated"),
            new("downedQueenSlime",     "downedQueenSlime ↔ bool",     "Queen Slime defeated"),
            new("downedDeerclops",      "downedDeerclops ↔ bool",      "Deerclops defeated"),

            // Boss Events
            new("downedHalloweenTree",    "downedHalloweenTree ↔ bool",    "Mourning Wood defeated"),
            new("downedHalloweenKing",    "downedHalloweenKing ↔ bool",    "Pumpking defeated"),
            new("downedChristmasTree",    "downedChristmasTree ↔ bool",    "Everscream defeated"),
            new("downedSanta",            "downedSanta ↔ bool",            "Santa-NK1 defeated"),
            new("downedChristmasQueen",   "downedChristmasQueen ↔ bool",   "Ice Queen defeated"),
            new("downedCelestialSolar",   "downedCelestialSolar ↔ bool",   "Solar pillar defeated"),
            new("downedCelestialNebula",  "downedCelestialNebula ↔ bool",  "Nebula pillar defeated"),
            new("downedCelestialVortex",  "downedCelestialVortex ↔ bool",  "Vortex pillar defeated"),
            new("downedCelestialStardust","downedCelestialStardust ↔ bool","Stardust pillar defeated"),
            new("celestialSolarActive",   "celestialSolarActive ↔ bool",   "Solar pillar active"),
            new("celestialVortexActive",  "celestialVortexActive ↔ bool",  "Vortex pillar active"),
            new("celestialNebulaActive",  "celestialNebulaActive ↔ bool",  "Nebula pillar active"),
            new("celestialStardustActive","celestialStardustActive ↔ bool","Stardust pillar active"),

            // Old One's Army
            new("downedDD2InvasionT1", "downedDD2InvasionT1 ↔ bool", "Old One's Army tier 1 defeated"),
            new("downedDD2InvasionT2", "downedDD2InvasionT2 ↔ bool", "Old One's Army tier 2 defeated"),
            new("downedDD2InvasionT3", "downedDD2InvasionT3 ↔ bool", "Old One's Army tier 3 defeated"),

            // Invasions
            new("downedGoblins",  "downedGoblins ↔ bool",   "Goblin army defeated"),
            new("downedFrost",    "downedFrost ↔ bool",     "Frost Legion defeated"),
            new("downedPirates",  "downedPirates ↔ bool",   "Pirate invasion defeated"),
            new("downedMartians", "downedMartians ↔ bool",  "Martian Madness defeated"),
            new("invasionType",   "invasionType ↔ int",     "Current invasion type"),
            new("invasionSize",   "invasionSize ↔ int",     "Current invasion size"),
            new("invasionX",      "invasionX ↔ double",     "Invasion X position"),

            // NPCs Saved
            new("savedGoblin",       "savedGoblin ↔ bool",       "Goblin Tinkerer rescued"),
            new("savedMech",         "savedMech ↔ bool",         "Mechanic rescued"),
            new("savedWizard",       "savedWizard ↔ bool",       "Wizard rescued"),
            new("savedStylist",      "savedStylist ↔ bool",      "Stylist rescued"),
            new("savedTaxCollector", "savedTaxCollector ↔ bool", "Tax Collector rescued"),
            new("savedBartender",    "savedBartender ↔ bool",    "Tavernkeep rescued"),
            new("savedGolfer",       "savedGolfer ↔ bool",       "Golfer rescued"),
            new("savedAngler",       "savedAngler ↔ bool",       "Angler rescued"),
            new("anglerQuest",       "anglerQuest ↔ int",        "Current angler quest ID"),

            // NPCs Bought
            new("boughtCat",    "boughtCat ↔ bool",    "Cat pet purchased"),
            new("boughtDog",    "boughtDog ↔ bool",    "Dog pet purchased"),
            new("boughtBunny",  "boughtBunny ↔ bool",  "Bunny pet purchased"),

            // NPCs Unlocked
            new("unlockedMerchantSpawn",       "unlockedMerchantSpawn ↔ bool",       "Merchant spawn unlocked"),
            new("unlockedDemolitionistSpawn",   "unlockedDemolitionistSpawn ↔ bool",   "Demolitionist spawn unlocked"),
            new("unlockedPartyGirlSpawn",      "unlockedPartyGirlSpawn ↔ bool",      "Party Girl spawn unlocked"),
            new("unlockedDyeTraderSpawn",      "unlockedDyeTraderSpawn ↔ bool",      "Dye Trader spawn unlocked"),
            new("unlockedTruffleSpawn",        "unlockedTruffleSpawn ↔ bool",        "Truffle spawn unlocked"),
            new("unlockedArmsDealerSpawn",     "unlockedArmsDealerSpawn ↔ bool",     "Arms Dealer spawn unlocked"),
            new("unlockedNurseSpawn",          "unlockedNurseSpawn ↔ bool",          "Nurse spawn unlocked"),
            new("unlockedPrincessSpawn",       "unlockedPrincessSpawn ↔ bool",       "Princess spawn unlocked"),

            // Town Slimes Unlocked
            new("unlockedSlimeBlueSpawn",   "unlockedSlimeBlueSpawn ↔ bool",   "Blue town slime unlocked"),
            new("unlockedSlimeGreenSpawn",  "unlockedSlimeGreenSpawn ↔ bool",  "Green town slime unlocked"),
            new("unlockedSlimeOldSpawn",    "unlockedSlimeOldSpawn ↔ bool",    "Old town slime unlocked"),
            new("unlockedSlimePurpleSpawn", "unlockedSlimePurpleSpawn ↔ bool", "Purple town slime unlocked"),
            new("unlockedSlimeRainbowSpawn","unlockedSlimeRainbowSpawn ↔ bool","Rainbow town slime unlocked"),
            new("unlockedSlimeRedSpawn",    "unlockedSlimeRedSpawn ↔ bool",    "Red town slime unlocked"),
            new("unlockedSlimeYellowSpawn", "unlockedSlimeYellowSpawn ↔ bool", "Yellow town slime unlocked"),
            new("unlockedSlimeCopperSpawn", "unlockedSlimeCopperSpawn ↔ bool", "Copper town slime unlocked"),

            // Lantern Night
            new("lanternNightCooldown",            "lanternNightCooldown ↔ int",  "Lantern Night cooldown"),
            new("lanternNightManual",              "lanternNightManual ↔ bool",   "Lantern Night manual trigger"),
            new("lanternNightGenuine",             "lanternNightGenuine ↔ bool",  "Lantern Night genuine"),
            new("lanternNightNextNightIsGenuine",  "lanternNightNextNightIsGenuine ↔ bool", "Next night is genuine Lantern Night"),

            // Party
            new("partyManual",   "partyManual ↔ bool",  "Party manual trigger"),
            new("partyGenuine",  "partyGenuine ↔ bool", "Party genuine"),
            new("partyCooldown", "partyCooldown ↔ int",  "Party cooldown"),

            // Backgrounds
            new("bgOcean",         "bgOcean ↔ byte",         "Ocean background style"),
            new("bgDesert",        "bgDesert ↔ byte",        "Desert background style"),
            new("bgCrimson",       "bgCrimson ↔ byte",       "Crimson background style"),
            new("bgHallow",        "bgHallow ↔ byte",        "Hallow background style"),
            new("bgSnow",          "bgSnow ↔ byte",          "Snow background style"),
            new("bgJungle",        "bgJungle ↔ byte",        "Jungle background style"),
            new("bgCorruption",    "bgCorruption ↔ byte",    "Corruption background style"),
            new("bgTree",          "bgTree ↔ byte",          "Forest background style"),
            new("bgTree2",         "bgTree2 ↔ byte",         "Forest 2 background style"),
            new("bgTree3",         "bgTree3 ↔ byte",         "Forest 3 background style"),
            new("bgTree4",         "bgTree4 ↔ byte",         "Forest 4 background style"),
            new("underworldBg",    "underworldBg ↔ byte",    "Underworld background style"),
            new("mushroomBg",      "mushroomBg ↔ byte",      "Mushroom background style"),
            new("iceBackStyle",    "iceBackStyle ↔ int",     "Ice biome back style"),
            new("jungleBackStyle", "jungleBackStyle ↔ int",  "Jungle biome back style"),
            new("hellBackStyle",   "hellBackStyle ↔ int",    "Hell back style"),

            // Tree/Cave Positions & Styles
            new("treeX0",         "treeX0 ↔ int",         "Tree zone boundary 0"),
            new("treeX1",         "treeX1 ↔ int",         "Tree zone boundary 1"),
            new("treeX2",         "treeX2 ↔ int",         "Tree zone boundary 2"),
            new("treeStyle0",     "treeStyle0 ↔ int",     "Tree style zone 0"),
            new("treeStyle1",     "treeStyle1 ↔ int",     "Tree style zone 1"),
            new("treeStyle2",     "treeStyle2 ↔ int",     "Tree style zone 2"),
            new("treeStyle3",     "treeStyle3 ↔ int",     "Tree style zone 3"),
            new("treeTop1",       "treeTop1 ↔ int",       "Tree top variation 1"),
            new("treeTop2",       "treeTop2 ↔ int",       "Tree top variation 2"),
            new("treeTop3",       "treeTop3 ↔ int",       "Tree top variation 3"),
            new("treeTop4",       "treeTop4 ↔ int",       "Tree top variation 4"),
            new("caveBackX0",     "caveBackX0 ↔ int",     "Cave back boundary 0"),
            new("caveBackX1",     "caveBackX1 ↔ int",     "Cave back boundary 1"),
            new("caveBackX2",     "caveBackX2 ↔ int",     "Cave back boundary 2"),
            new("caveBackStyle0", "caveBackStyle0 ↔ int", "Cave back style 0"),
            new("caveBackStyle1", "caveBackStyle1 ↔ int", "Cave back style 1"),
            new("caveBackStyle2", "caveBackStyle2 ↔ int", "Cave back style 2"),
            new("caveBackStyle3", "caveBackStyle3 ↔ int", "Cave back style 3"),

            // World Bounds
            new("bottomWorld", "bottomWorld ↔ float", "World bottom bound"),
            new("topWorld",    "topWorld ↔ float",    "World top bound"),
            new("rightWorld",  "rightWorld ↔ float",  "World right bound"),
            new("leftWorld",   "leftWorld ↔ float",   "World left bound"),

            // Other
            new("cultistDelay", "cultistDelay ↔ int",   "Cultist spawn delay"),
            new("apocalypse",   "apocalypse ↔ bool",    "Apocalypse mode"),
            new("downedClown",  "downedClown ↔ bool",   "Clown defeated"),
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

        new("sprites", "Place multi-tile sprites (furniture, torches, etc.)",
        [
            new("listSprites",  "listSprites() → [{tileId, name, styleCount}]",          "List all sprite sheets"),
            new("getStyles",    "getStyles(tileId) → [{index, name, width, height}]",     "Get styles for a tile type"),
            new("place",        "place(tileId, styleIndex, x, y) → bool",                 "Place sprite by tile ID and style index"),
            new("placeByName",  "placeByName(name, x, y) → bool",                         "Place first style matching name"),
        ]),

        new("tileEntities", "Query and modify tile entities (mannequins, weapon racks, item frames, etc.)",
        [
            // Counts
            new("count",             "count → int",              "Total number of tile entities"),
            new("mannequinCount",    "mannequinCount → int",     "Number of mannequins/display dolls"),
            new("weaponRackCount",   "weaponRackCount → int",    "Number of weapon racks"),
            new("hatRackCount",      "hatRackCount → int",       "Number of hat racks"),
            new("itemFrameCount",    "itemFrameCount → int",     "Number of item frames"),
            new("foodPlatterCount",  "foodPlatterCount → int",   "Number of food platters"),
            new("logicSensorCount",  "logicSensorCount → int",   "Number of logic sensors"),
            new("trainingDummyCount","trainingDummyCount → int", "Number of training dummies"),
            new("pylonCount",        "pylonCount → int",         "Number of teleportation pylons"),

            // Query
            new("getAll",            "getAll() → [{x, y, type, id, ...}]",        "Get all tile entities"),
            new("getAllByType",      "getAllByType(typeName) → [{...}]",           "Get entities by type name"),
            new("getAllMannequins",  "getAllMannequins() → [{...}]",               "Get all mannequins/display dolls"),
            new("getAllWeaponRacks", "getAllWeaponRacks() → [{...}]",              "Get all weapon racks"),
            new("getAllHatRacks",    "getAllHatRacks() → [{...}]",                 "Get all hat racks"),
            new("getAllItemFrames",  "getAllItemFrames() → [{...}]",               "Get all item frames"),
            new("getAllFoodPlatters","getAllFoodPlatters() → [{...}]",             "Get all food platters"),
            new("getAt",            "getAt(x, y) → {x, y, type, ...}",            "Get entity at tile coordinates"),
            new("findByItem",       "findByItem(itemId) → [{...}]",               "Find entities containing item ID"),

            // Mannequin (DisplayDoll)
            new("setEquipment",  "setEquipment(x, y, slot, itemId, prefix?)",  "Set mannequin equipment slot (prefix default 0)"),
            new("clearEquipment","clearEquipment(x, y, slot)",                  "Clear mannequin equipment slot"),
            new("setDye",        "setDye(x, y, slot, dyeId, prefix?)",          "Set mannequin dye slot"),
            new("clearDye",      "clearDye(x, y, slot)",                        "Clear mannequin dye slot"),
            new("setWeapon",     "setWeapon(x, y, itemId, prefix?)",            "Set mannequin held weapon"),
            new("clearWeapon",   "clearWeapon(x, y)",                           "Clear mannequin weapon"),
            new("setPose",       "setPose(x, y, poseId)",                       "Set mannequin pose (0-8)"),
            new("getPose",       "getPose(x, y) → int",                         "Get mannequin pose"),

            // Hat Rack
            new("setHatRackItem",  "setHatRackItem(x, y, slot, itemId, prefix?)", "Set hat rack item slot"),
            new("clearHatRackItem","clearHatRackItem(x, y, slot)",                 "Clear hat rack item slot"),
            new("setHatRackDye",   "setHatRackDye(x, y, slot, dyeId, prefix?)",   "Set hat rack dye slot"),
            new("clearHatRackDye", "clearHatRackDye(x, y, slot)",                  "Clear hat rack dye slot"),

            // Single-Item Entities (WeaponRack, ItemFrame, FoodPlatter, DeadCellsDisplayJar)
            new("setItem",   "setItem(x, y, itemId, prefix?, stack?)", "Set item on single-item entity (prefix default 0, stack default 1)"),
            new("clearItem", "clearItem(x, y)",                         "Remove item from single-item entity"),

            // Logic Sensor / Training Dummy
            new("setLogicSensor",      "setLogicSensor(x, y, logicCheck, on)",  "Set logic sensor type and state"),
            new("setTrainingDummyNpc",  "setTrainingDummyNpc(x, y, npcId)",     "Set training dummy NPC"),
        ]),

        new("draw", "Drawing tools: brush, pencil, fill, hammer with configurable tile picker",
        [
            // Picker configuration
            new("setTile",         "setTile(tileType)",                         "Set tile type to paint"),
            new("setWall",         "setWall(wallType)",                         "Set wall type to paint"),
            new("setErase",        "setErase(bool)",                            "Enable/disable eraser mode"),
            new("setPaintMode",    "setPaintMode(mode)",                        "Set paint mode: 'tile', 'wire', 'liquid'"),
            new("setTileColor",    "setTileColor(color)",                       "Set tile paint color (0 to disable)"),
            new("setWallColor",    "setWallColor(color)",                       "Set wall paint color (0 to disable)"),
            new("setBrickStyle",   "setBrickStyle(style)",                      "Set brick style: 'full', 'half', 'topright', 'topleft', 'bottomright', 'bottomleft'"),
            new("setActuator",     "setActuator(enabled, inactive?)",           "Set actuator state"),
            new("setTileCoating",  "setTileCoating(echo?, illuminant?)",        "Set tile coating (echo/illuminant)"),
            new("setWallCoating",  "setWallCoating(echo?, illuminant?)",        "Set wall coating (echo/illuminant)"),
            new("setLiquid",       "setLiquid(type, amount?)",                  "Set liquid: type='water'|'lava'|'honey'|'shimmer', amount='full'|'half'|'quarter'"),
            new("setWire",         "setWire(red?, blue?, green?, yellow?)",     "Set wire paint flags"),
            new("setTileMask",     "setTileMask(mode, tileType?)",              "Set tile mask: 'off', 'match', 'empty', 'notMatching'"),
            new("setWallMask",     "setWallMask(mode, wallType?)",              "Set wall mask mode"),
            new("setBrush",        "setBrush(width, height, shape?)",           "Set brush size and shape: 'square', 'round', 'right', 'left'"),
            new("setBrushOutline", "setBrushOutline(outline, enabled)",         "Set brush outline width and enable/disable"),
            new("reset",           "reset()",                                   "Reset tile picker and brush to defaults"),

            // Drawing operations
            new("pencil",  "pencil(x1, y1, x2, y2)",   "Draw 1px line between two points"),
            new("brush",   "brush(x1, y1, x2, y2)",    "Draw brush-width line between two points"),
            new("fill",    "fill(x, y)",                "Flood fill from point"),
            new("hammer",  "hammer(x1, y1, x2, y2)",   "Auto-slope tiles along brush-width line"),
        ]),
    ];
}
