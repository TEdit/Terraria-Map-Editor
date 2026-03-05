// TEdit Scripting API - TypeScript Declarations
// Generated from ScriptApiMetadata and C# source classes.
// Place alongside your .js scripts for editor autocomplete.
//
// Global objects available in TEdit scripts:
//   tile, batch, geometry, selection, chests, signs, npcs,
//   world, metadata, log, tools, finder, sprites, draw,
//   tileEntities, generate, print

// ─── Common Shapes ──────────────────────────────────────────────────────────

interface Point {
    x: number;
    y: number;
}

interface ChestItem {
    slot: number;
    id: number;
    name: string;
    stack: number;
    prefix: number;
}

interface ChestInfo {
    x: number;
    y: number;
    name: string;
    items: ChestItem[];
}

interface SignInfo {
    x: number;
    y: number;
    text: string;
}

interface NpcInfo {
    name: string;
    displayName: string;
    x: number;
    y: number;
    homeX: number;
    homeY: number;
    isHomeless: boolean;
}

interface IdName {
    id: number;
    name: string;
}

/** Biome shape types for controlling biome region geometry. */
type BiomeShape = "rectangle" | "ellipse" | "diagonalLeft" | "diagonalRight" | "trapezoid" | "hemisphere";

interface TreeTypeInfo {
    name: string;
    tileId: number;
}

interface OreTypeInfo {
    name: string;
    tileId: number;
}

interface SpriteSheetInfo {
    tileId: number;
    name: string;
    styleCount: number;
}

interface SpriteStyleInfo {
    index: number;
    name: string;
    width: number;
    height: number;
}

interface TileEntityInfo {
    x: number;
    y: number;
    type: string;
    id: number;
    [key: string]: any;
}

// ─── tile ────────────────────────────────────────────────────────────────────

/** Low-level tile read/write operations. */
declare const tile: {
    /** Check if tile at (x, y) is active (has a block). */
    isActive(x: number, y: number): boolean;
    /** Get tile type ID at (x, y). */
    getTileType(x: number, y: number): number;
    /** Get wall type ID at (x, y). */
    getWall(x: number, y: number): number;
    /** Get tile paint color at (x, y). */
    getPaint(x: number, y: number): number;
    /** Get wall paint color at (x, y). */
    getWallPaint(x: number, y: number): number;
    /** Get liquid amount (0-255) at (x, y). */
    getLiquidAmount(x: number, y: number): number;
    /** Get liquid type (0=none, 1=water, 2=lava, 3=honey) at (x, y). */
    getLiquidType(x: number, y: number): number;
    /** Get sprite frame U coordinate at (x, y). */
    getFrameU(x: number, y: number): number;
    /** Get sprite frame V coordinate at (x, y). */
    getFrameV(x: number, y: number): number;
    /** Get slope type as string at (x, y). */
    getSlope(x: number, y: number): string;
    /** Check wire state. color: 1=red, 2=blue, 3=green, 4=yellow. */
    getWire(x: number, y: number, color: number): boolean;

    /** Set tile active state at (x, y). */
    setActive(x: number, y: number, active: boolean): void;
    /** Set tile type at (x, y). Also activates the tile. */
    setType(x: number, y: number, type: number): void;
    /** Set wall type at (x, y). */
    setWall(x: number, y: number, wallType: number): void;
    /** Set tile paint color at (x, y). */
    setPaint(x: number, y: number, color: number): void;
    /** Set wall paint color at (x, y). */
    setWallPaint(x: number, y: number, color: number): void;
    /** Set liquid amount and type at (x, y). */
    setLiquid(x: number, y: number, amount: number, type: number): void;
    /** Set wire state. color: 1=red, 2=blue, 3=green, 4=yellow. */
    setWire(x: number, y: number, color: number, enabled: boolean): void;
    /** Set slope type. Values: "None", "HalfBrick", "SlopeTopRight", "SlopeTopLeft", "SlopeBottomRight", "SlopeBottomLeft". */
    setSlope(x: number, y: number, slope: string): void;
    /** Set sprite frame coordinates at (x, y). */
    setFrameUV(x: number, y: number, u: number, v: number): void;
    /** Reset tile to default empty state. */
    clear(x: number, y: number): void;
    /** Copy all properties from one tile to another. */
    copy(fromX: number, fromY: number, toX: number, toY: number): void;
};

// ─── batch ───────────────────────────────────────────────────────────────────

/** High-performance bulk operations across world or selection. */
declare const batch: {
    /** Iterate entire world, calls callback(x, y) for each tile. */
    forEachTile(callback: (x: number, y: number) => void): void;
    /** Iterate selection area, calls callback(x, y). */
    forEachInSelection(callback: (x: number, y: number) => void): void;
    /** Find tiles matching predicate (max 10,000 results). */
    findTiles(predicate: (x: number, y: number) => boolean): Point[];
    /** Find tiles of a specific type. anchorOnly=true returns only sprite origin tiles. */
    findTilesByType(tileType: number, anchorOnly?: boolean): Point[];
    /** Find all tiles with specific wall type (max 10,000). */
    findTilesByWall(wallType: number): Point[];
    /** Replace all tiles of one type with another. Returns count replaced. */
    replaceTile(fromType: number, toType: number): number;
    /** Replace tiles in selection only. Returns count replaced. */
    replaceTileInSelection(fromType: number, toType: number): number;
    /** Replace all walls of one type with another. Returns count replaced. */
    replaceWall(fromType: number, toType: number): number;
    /** Clear all tiles of specified type. Returns count cleared. */
    clearTilesByType(tileType: number): number;
};

// ─── geometry ────────────────────────────────────────────────────────────────

/** Shape generation and bulk fill operations. */
declare const geometry: {
    /** Generate line coordinates between two points. */
    line(x1: number, y1: number, x2: number, y2: number): Point[];
    /** Generate rectangle outline coordinates. */
    rect(x: number, y: number, w: number, h: number): Point[];
    /** Generate ellipse outline coordinates. */
    ellipse(cx: number, cy: number, rx: number, ry: number): Point[];
    /** Generate filled rectangle coordinates. */
    fillRect(x: number, y: number, w: number, h: number): Point[];
    /** Generate filled ellipse coordinates. */
    fillEllipse(cx: number, cy: number, rx: number, ry: number): Point[];
    /** Fill rectangle with tile type. */
    setTiles(tileType: number, x: number, y: number, w: number, h: number): void;
    /** Fill rectangle with wall type. */
    setWalls(wallType: number, x: number, y: number, w: number, h: number): void;
    /** Clear all tiles in rectangle. */
    clearTiles(x: number, y: number, w: number, h: number): void;
};

// ─── selection ───────────────────────────────────────────────────────────────

/** Query and manipulate the current selection rectangle. */
declare const selection: {
    /** Whether a selection is active. */
    readonly isActive: boolean;
    /** Selection X coordinate. */
    readonly x: number;
    /** Selection Y coordinate. */
    readonly y: number;
    /** Selection width. */
    readonly width: number;
    /** Selection height. */
    readonly height: number;
    /** Left edge. */
    readonly left: number;
    /** Top edge. */
    readonly top: number;
    /** Right edge (exclusive). */
    readonly right: number;
    /** Bottom edge (exclusive). */
    readonly bottom: number;
    /** Create and activate a selection. */
    set(x: number, y: number, width: number, height: number): void;
    /** Deactivate selection. */
    clear(): void;
    /** Check if coordinate is in selection. */
    contains(x: number, y: number): boolean;
};

// ─── chests ──────────────────────────────────────────────────────────────────

/** Query and modify chest inventories. */
declare const chests: {
    /** Total number of chests. */
    readonly count: number;
    /** Get all chests. */
    getAll(): ChestInfo[];
    /** Get chest at tile coordinates. */
    getAt(x: number, y: number): ChestInfo | null;
    /** Find chests containing item ID. */
    findByItem(itemId: number): ChestInfo[];
    /** Find chests containing item by name. */
    findByItemName(name: string): ChestInfo[];
    /** Find chests by name (partial match). */
    findByName(name: string): ChestInfo[];
    /** Set item in specific slot. */
    setItem(x: number, y: number, slot: number, itemId: number, stack: number, prefix: number): void;
    /** Clear item from slot. */
    clearItem(x: number, y: number, slot: number): void;
    /** Set chest name/label. */
    setName(x: number, y: number, name: string): void;
    /** Add item to first empty slot. Returns true if added. */
    addItem(x: number, y: number, itemId: number, stack: number, prefix: number): boolean;
};

// ─── signs ───────────────────────────────────────────────────────────────────

/** Read and modify sign text. */
declare const signs: {
    /** Total number of signs. */
    readonly count: number;
    /** Get all signs. */
    getAll(): SignInfo[];
    /** Get sign at tile coordinates. */
    getAt(x: number, y: number): SignInfo | null;
    /** Update sign text. */
    setText(x: number, y: number, text: string): void;
};

// ─── npcs ────────────────────────────────────────────────────────────────────

/** Query and modify town NPC data. */
declare const npcs: {
    /** Total number of NPCs. */
    readonly count: number;
    /** Get all NPCs. */
    getAll(): NpcInfo[];
    /** Set NPC home location by name. */
    setHome(name: string, x: number, y: number): void;
};

// ─── world ───────────────────────────────────────────────────────────────────

/** Read and modify world properties. Properties marked readonly cannot be set. */
declare const world: {
    // World Size
    readonly width: number;
    readonly height: number;

    // Metadata
    title: string;
    worldId: number;
    seed: string;
    isFavorite: boolean;
    isChinese: boolean;
    isConsole: boolean;
    fileRevision: number;
    worldGenVersion: number;
    creationTime: number;
    lastPlayed: number;

    // Spawn & Dungeon
    spawnX: number;
    spawnY: number;
    dungeonX: number;
    dungeonY: number;

    // Levels
    surfaceLevel: number;
    rockLevel: number;
    safeGroundLayers: boolean;

    // Time
    time: number;
    dayTime: boolean;
    fastForwardTime: boolean;
    sundialCooldown: number;
    fastForwardTimeToDusk: boolean;
    moondialCooldown: number;

    // Moon
    moonPhase: number;
    bloodMoon: boolean;
    moonType: number;
    isEclipse: boolean;

    // Weather
    isRaining: boolean;
    tempRainTime: number;
    tempMaxRain: number;
    slimeRainTime: number;
    tempMeteorShowerCount: number;
    tempCoinRain: number;
    numClouds: number;
    windSpeedSet: number;
    cloudBgActive: number;

    // Sandstorm
    sandStormHappening: boolean;
    sandStormTimeLeft: number;
    sandStormSeverity: number;
    sandStormIntendedSeverity: number;

    // Holidays
    forceHalloweenForToday: boolean;
    forceXMasForToday: boolean;
    forceHalloweenForever: boolean;
    forceXMasForever: boolean;

    // Difficulty
    hardMode: boolean;
    gameMode: number;
    spawnMeteor: boolean;
    combatBookUsed: boolean;
    combatBookVolumeTwoWasUsed: boolean;
    peddlersSatchelWasUsed: boolean;
    partyOfDoom: boolean;

    // World Seeds
    drunkWorld: boolean;
    goodWorld: boolean;
    tenthAnniversaryWorld: boolean;
    dontStarveWorld: boolean;
    notTheBeesWorld: boolean;
    remixWorld: boolean;
    noTrapsWorld: boolean;
    zenithWorld: boolean;
    skyblockWorld: boolean;
    vampireSeed: boolean;
    infectedSeed: boolean;
    dualDungeonsSeed: boolean;

    // Ore Tiers
    isCrimson: boolean;
    altarCount: number;
    shadowOrbSmashed: boolean;
    shadowOrbCount: number;
    savedOreTiersCopper: number;
    savedOreTiersIron: number;
    savedOreTiersSilver: number;
    savedOreTiersGold: number;
    savedOreTiersCobalt: number;
    savedOreTiersMythril: number;
    savedOreTiersAdamantite: number;

    // Bosses: Pre-Hardmode
    downedSlimeKing: boolean;
    downedEyeOfCthulhu: boolean;
    downedEaterOfWorlds: boolean;
    downedQueenBee: boolean;
    downedSkeletron: boolean;

    // Bosses: Hardmode
    downedDestroyer: boolean;
    downedTwins: boolean;
    downedSkeletronPrime: boolean;
    readonly downedMechBossAny: boolean;
    downedPlantera: boolean;
    downedGolem: boolean;
    downedFishron: boolean;
    downedLunaticCultist: boolean;
    downedMoonlord: boolean;

    // Bosses: Journey's End
    downedEmpressOfLight: boolean;
    downedQueenSlime: boolean;
    downedDeerclops: boolean;

    // Boss Events
    downedHalloweenTree: boolean;
    downedHalloweenKing: boolean;
    downedChristmasTree: boolean;
    downedSanta: boolean;
    downedChristmasQueen: boolean;
    downedCelestialSolar: boolean;
    downedCelestialNebula: boolean;
    downedCelestialVortex: boolean;
    downedCelestialStardust: boolean;
    celestialSolarActive: boolean;
    celestialVortexActive: boolean;
    celestialNebulaActive: boolean;
    celestialStardustActive: boolean;

    // Old One's Army
    downedDD2InvasionT1: boolean;
    downedDD2InvasionT2: boolean;
    downedDD2InvasionT3: boolean;

    // Invasions
    downedGoblins: boolean;
    downedFrost: boolean;
    downedPirates: boolean;
    downedMartians: boolean;
    invasionType: number;
    invasionSize: number;
    invasionX: number;

    // NPCs Saved
    savedGoblin: boolean;
    savedMech: boolean;
    savedWizard: boolean;
    savedStylist: boolean;
    savedTaxCollector: boolean;
    savedBartender: boolean;
    savedGolfer: boolean;
    savedAngler: boolean;
    anglerQuest: number;

    // NPCs Bought
    boughtCat: boolean;
    boughtDog: boolean;
    boughtBunny: boolean;

    // NPCs Unlocked
    unlockedMerchantSpawn: boolean;
    unlockedDemolitionistSpawn: boolean;
    unlockedPartyGirlSpawn: boolean;
    unlockedDyeTraderSpawn: boolean;
    unlockedTruffleSpawn: boolean;
    unlockedArmsDealerSpawn: boolean;
    unlockedNurseSpawn: boolean;
    unlockedPrincessSpawn: boolean;

    // Town Slimes
    unlockedSlimeBlueSpawn: boolean;
    unlockedSlimeGreenSpawn: boolean;
    unlockedSlimeOldSpawn: boolean;
    unlockedSlimePurpleSpawn: boolean;
    unlockedSlimeRainbowSpawn: boolean;
    unlockedSlimeRedSpawn: boolean;
    unlockedSlimeYellowSpawn: boolean;
    unlockedSlimeCopperSpawn: boolean;

    // Lantern Night
    lanternNightCooldown: number;
    lanternNightManual: boolean;
    lanternNightGenuine: boolean;
    lanternNightNextNightIsGenuine: boolean;

    // Party
    partyManual: boolean;
    partyGenuine: boolean;
    partyCooldown: number;

    // Backgrounds
    bgOcean: number;
    bgDesert: number;
    bgCrimson: number;
    bgHallow: number;
    bgSnow: number;
    bgJungle: number;
    bgCorruption: number;
    bgTree: number;
    bgTree2: number;
    bgTree3: number;
    bgTree4: number;
    underworldBg: number;
    mushroomBg: number;
    iceBackStyle: number;
    jungleBackStyle: number;
    hellBackStyle: number;

    // Tree/Cave Positions & Styles
    treeX0: number;
    treeX1: number;
    treeX2: number;
    treeStyle0: number;
    treeStyle1: number;
    treeStyle2: number;
    treeStyle3: number;
    treeTop1: number;
    treeTop2: number;
    treeTop3: number;
    treeTop4: number;
    caveBackX0: number;
    caveBackX1: number;
    caveBackX2: number;
    caveBackStyle0: number;
    caveBackStyle1: number;
    caveBackStyle2: number;
    caveBackStyle3: number;

    // World Bounds
    bottomWorld: number;
    topWorld: number;
    rightWorld: number;
    leftWorld: number;

    // Other
    cultistDelay: number;
    apocalypse: boolean;
    downedClown: boolean;
};

// ─── metadata ────────────────────────────────────────────────────────────────

/** Lookup Terraria game data (tile/wall/item names and IDs). */
declare const metadata: {
    /** Get tile ID by name (0 if not found). */
    tileId(name: string): number;
    /** Get wall ID by name (0 if not found). */
    wallId(name: string): number;
    /** Get item ID by name (0 if not found). */
    itemId(name: string): number;
    /** Get tile name by ID. */
    tileName(id: number): string;
    /** Get wall name by ID. */
    wallName(id: number): string;
    /** Get item name by ID. */
    itemName(id: number): string;
    /** Get all tile definitions. */
    allTiles(): IdName[];
    /** Get all wall definitions. */
    allWalls(): IdName[];
    /** Get all item definitions. */
    allItems(): IdName[];
};

// ─── log ─────────────────────────────────────────────────────────────────────

/** Output messages and progress from scripts. */
declare const log: {
    /** Log info message. */
    print(message: string): void;
    /** Log warning message. */
    warn(message: string): void;
    /** Log error message. */
    error(message: string): void;
    /** Update progress bar (0.0 to 1.0). */
    progress(value: number): void;
};

// ─── tools ───────────────────────────────────────────────────────────────────

interface CloudWorldsFolder {
    userId: string;
    path: string;
}

/** Interact with TEdit's UI tools, clipboard, and file operations. */
declare const tools: {
    /** Get names of all available tools. */
    listTools(): string[];
    /** Copy current selection to clipboard. */
    copySelection(): void;
    /** Get currently selected tile in picker. */
    getTilePickerTile(): number;
    /** Get currently selected wall in picker. */
    getTilePickerWall(): number;
    /** Set tile picker selection. */
    setTilePickerTile(tileType: number): void;
    /** Set wall picker selection. */
    setTilePickerWall(wallType: number): void;

    /** Get the current world file path. */
    getFilePath(): string;
    /** Set the current world file path (does not save). */
    setFilePath(path: string): void;
    /** Get the default Terraria worlds folder path. */
    getWorldsFolder(): string;
    /** Get all Steam Cloud world folder paths. */
    getCloudWorldsFolders(): CloudWorldsFolder[];

    /** Save world to current file path (no UI dialog). */
    save(): boolean;
    /**
     * Save world to file (no UI dialog).
     * If filename has no path separators (e.g. "MyWorld" or "MyWorld.wld"),
     * saves to the default Terraria worlds folder. .wld extension added automatically.
     */
    saveAs(filename: string, version?: number): boolean;
    /**
     * Load a world file, replacing the current world. Blocks until complete.
     * If filename has no path separators, loads from the default Terraria worlds folder.
     * .wld extension added automatically.
     */
    load(filename: string): boolean;
};

// ─── finder ──────────────────────────────────────────────────────────────────

/** Populate Find sidebar with script search results. */
declare const finder: {
    /** Clear all find results. */
    clear(): void;
    /** Add result to Find sidebar (max 1000). Returns false when cap is hit. */
    addResult(name: string, x: number, y: number, resultType: string, extraInfo?: string): boolean;
    /** Navigate to result by index. */
    navigate(index: number): void;
    /** Navigate to first result. */
    navigateFirst(): void;
};

// ─── sprites ─────────────────────────────────────────────────────────────────

/** Place multi-tile sprites (furniture, torches, etc.). */
declare const sprites: {
    /** List all sprite sheets. */
    listSprites(): SpriteSheetInfo[];
    /** Get styles for a tile type. */
    getStyles(tileId: number): SpriteStyleInfo[];
    /** Place sprite by tile ID and style index. Returns false on failure. */
    place(tileId: number, styleIndex: number, x: number, y: number): boolean;
    /** Place first style matching name. Returns false if not found. */
    placeByName(name: string, x: number, y: number): boolean;
};

// ─── draw ────────────────────────────────────────────────────────────────────

/** Drawing tools: brush, pencil, fill, hammer with configurable tile picker. */
declare const draw: {
    // Picker configuration
    /** Set tile type to paint. */
    setTile(tileType: number): void;
    /** Set wall type to paint. */
    setWall(wallType: number): void;
    /** Enable/disable eraser mode. */
    setErase(erase: boolean): void;
    /** Set paint mode: "tile", "tileandwall", "wire", "liquid". */
    setPaintMode(mode: string): void;
    /** Set tile paint color (0 to disable). */
    setTileColor(color: number): void;
    /** Set wall paint color (0 to disable). */
    setWallColor(color: number): void;
    /** Set brick style: "full", "half", "topright", "topleft", "bottomright", "bottomleft". */
    setBrickStyle(style: string): void;
    /** Set actuator state. */
    setActuator(enabled: boolean, inactive?: boolean): void;
    /** Set tile coating (echo/illuminant). */
    setTileCoating(echo?: boolean, illuminant?: boolean): void;
    /** Set wall coating (echo/illuminant). */
    setWallCoating(echo?: boolean, illuminant?: boolean): void;
    /** Set liquid. type: "water"|"lava"|"honey"|"shimmer"|"none". amount: "full"|"half"|"quarter". */
    setLiquid(type: string, amount?: string): void;
    /** Set wire paint flags. */
    setWire(red?: boolean, blue?: boolean, green?: boolean, yellow?: boolean): void;

    // Mask configuration
    /** Set tile mask. mode: "off"|"match"|"empty"|"notmatching". */
    setTileMask(mode: string, tileType?: number): void;
    /** Set wall mask mode. */
    setWallMask(mode: string, wallType?: number): void;
    /** Set brick style mask. */
    setBrickStyleMask(mode: string, style?: string): void;
    /** Set actuator mask. */
    setActuatorMask(mode: string): void;
    /** Set tile paint mask. */
    setTilePaintMask(mode: string, color?: number): void;
    /** Set wall paint mask. */
    setWallPaintMask(mode: string, color?: number): void;
    /** Set tile coating mask. */
    setTileCoatingMask(echoMode?: string, illuminantMode?: string): void;
    /** Set wall coating mask. */
    setWallCoatingMask(echoMode?: string, illuminantMode?: string): void;
    /** Set wire mask. mode values: "off"|"match"|"empty"|"notmatching". */
    setWireMask(redMode?: string, blueMode?: string, greenMode?: string, yellowMode?: string): void;
    /** Set liquid type mask. */
    setLiquidTypeMask(mode: string, type?: string): void;
    /** Set liquid level mask. mode: "ignore"|"greaterthan"|"lessthan"|"equal". */
    setLiquidLevelMask(mode: string, level?: number): void;
    /** Set mask preset: "off"|"exact"|"matchall"|"custom". */
    setMaskPreset(preset: string): void;
    /** Clear all masks. */
    clearMasks(): void;
    /** Set exact mask from tile at (x, y). */
    setExactMask(x: number, y: number): void;

    // Brush configuration
    /** Set brush size and shape. shape: "square"|"round"|"right"|"left"|"star"|"triangle"|"crescent"|"donut"|"cross"|"x". */
    setBrush(width: number, height: number, shape?: string): void;
    /** Set brush rotation in degrees. */
    setRotation(degrees: number): void;
    /** Set brush flip. */
    setFlip(horizontal?: boolean, vertical?: boolean): void;
    /** Enable/disable spray mode with density and tick interval. */
    setSpray(enabled: boolean, density?: number, tickMs?: number): void;
    /** Set brush outline width and enable/disable. */
    setBrushOutline(outline: number, enabled: boolean): void;
    /** Reset tile picker and brush to defaults. */
    reset(): void;

    // Drawing operations
    /** Draw 1px line between two points. */
    pencil(x1: number, y1: number, x2: number, y2: number): void;
    /** Draw brush-width line between two points. */
    brush(x1: number, y1: number, x2: number, y2: number): void;
    /** Flood fill from point. */
    fill(x: number, y: number): void;
    /** Auto-slope tiles along brush-width line. */
    hammer(x1: number, y1: number, x2: number, y2: number): void;

    // Wire routing
    /** Route wire between two points. Returns tiles placed. mode: "90"|"45"|"miter". direction: "auto"|"h"|"v". */
    routeWire(x1: number, y1: number, x2: number, y2: number, mode?: string, direction?: string): number;
    /** Route wire bus (multiple parallel wires). Returns tiles placed. */
    routeBus(wireCount: number, x1: number, y1: number, x2: number, y2: number, mode?: string, direction?: string): number;
    /** Get wire route path as coordinates. */
    routeWirePath(x1: number, y1: number, x2: number, y2: number, mode?: string, direction?: string): Point[];
    /** Get bus route path as coordinates. */
    routeBusPath(wireCount: number, x1: number, y1: number, x2: number, y2: number, mode?: string, direction?: string): Point[];
};

// ─── tileEntities ────────────────────────────────────────────────────────────

/** Query and modify tile entities (mannequins, weapon racks, item frames, etc.). */
declare const tileEntities: {
    // Counts
    readonly count: number;
    readonly mannequinCount: number;
    readonly weaponRackCount: number;
    readonly hatRackCount: number;
    readonly itemFrameCount: number;
    readonly foodPlatterCount: number;
    readonly logicSensorCount: number;
    readonly trainingDummyCount: number;
    readonly pylonCount: number;

    // Query
    /** Get all tile entities. */
    getAll(): TileEntityInfo[];
    /** Get entities by type name (e.g. "DisplayDoll", "WeaponRack"). */
    getAllByType(typeName: string): TileEntityInfo[];
    /** Get all mannequins/display dolls. */
    getAllMannequins(): TileEntityInfo[];
    /** Get all weapon racks. */
    getAllWeaponRacks(): TileEntityInfo[];
    /** Get all hat racks. */
    getAllHatRacks(): TileEntityInfo[];
    /** Get all item frames. */
    getAllItemFrames(): TileEntityInfo[];
    /** Get all food platters. */
    getAllFoodPlatters(): TileEntityInfo[];
    /** Get entity at tile coordinates. */
    getAt(x: number, y: number): TileEntityInfo | null;
    /** Find entities containing item ID. */
    findByItem(itemId: number): TileEntityInfo[];

    // Mannequin (DisplayDoll)
    /** Set mannequin equipment slot. */
    setEquipment(x: number, y: number, slot: number, itemId: number, prefix?: number): void;
    /** Clear mannequin equipment slot. */
    clearEquipment(x: number, y: number, slot: number): void;
    /** Set mannequin dye slot. */
    setDye(x: number, y: number, slot: number, dyeId: number, prefix?: number): void;
    /** Clear mannequin dye slot. */
    clearDye(x: number, y: number, slot: number): void;
    /** Set mannequin held weapon. */
    setWeapon(x: number, y: number, itemId: number, prefix?: number): void;
    /** Clear mannequin weapon. */
    clearWeapon(x: number, y: number): void;
    /** Set mannequin pose (0-8). */
    setPose(x: number, y: number, poseId: number): void;
    /** Get mannequin pose. */
    getPose(x: number, y: number): number;

    // Hat Rack
    /** Set hat rack item slot. */
    setHatRackItem(x: number, y: number, slot: number, itemId: number, prefix?: number): void;
    /** Clear hat rack item slot. */
    clearHatRackItem(x: number, y: number, slot: number): void;
    /** Set hat rack dye slot. */
    setHatRackDye(x: number, y: number, slot: number, dyeId: number, prefix?: number): void;
    /** Clear hat rack dye slot. */
    clearHatRackDye(x: number, y: number, slot: number): void;

    // Single-Item Entities (WeaponRack, ItemFrame, FoodPlatter)
    /** Set item on single-item entity (prefix default 0, stack default 1). */
    setItem(x: number, y: number, itemId: number, prefix?: number, stack?: number): void;
    /** Remove item from single-item entity. */
    clearItem(x: number, y: number): void;

    // Logic Sensor / Training Dummy
    /** Set logic sensor type and state. */
    setLogicSensor(x: number, y: number, logicCheck: number, on: boolean): void;
    /** Set training dummy NPC. */
    setTrainingDummyNpc(x: number, y: number, npcId: number): void;
};

// ─── generate ────────────────────────────────────────────────────────────────

/** Procedural generation: biomes, structures, terrain, trees, ores, caves, lakes, decoration. */
declare const generate: {
    // ── Trees & Forests ──

    /** List all supported tree type names and tile IDs. */
    listTreeTypes(): TreeTypeInfo[];
    /**
     * Place a single tree at (x, y) ground level.
     * Types: "oak", "palm", "mushroom", "jungle", "topaz", "amethyst", "sapphire",
     *        "emerald", "ruby", "diamond", "amber", "sakura", "willow", "ash".
     */
    tree(type: string, x: number, y: number): boolean;
    /** Place random trees in rectangle. density 0.0-1.0 (default 0.15). Returns count placed. */
    forest(types: string[], x: number, y: number, w: number, h: number, density?: number): number;
    /** Place random trees in current selection. Returns count placed. */
    forestInSelection(types: string[], density?: number): number;

    // ── Terrain Primitives ──

    /** Scan downward from yStart to yEnd at column x for first solid tile. Returns y or -1. */
    findSurface(x: number, yStart: number, yEnd: number): number;
    /**
     * Wandering painter: fills diamond-shaped blobs with tileType.
     * Port of Terraria's WorldGen.TileRunner. Skips frame-important tiles.
     */
    tileRunner(x: number, y: number, strength: number, steps: number, tileType: number, speedX?: number, speedY?: number): void;
    /** Carve natural cave tunnels (clears tiles along a wandering path). */
    tunnel(x: number, y: number, strength: number, steps: number, speedX?: number, speedY?: number): void;
    /**
     * Create irregular liquid pool. Two-pass: carve cavity, then fill.
     * liquidType: "water" (default), "lava", "honey", "shimmer".
     */
    lake(x: number, y: number, liquidType?: string, strength?: number): void;
    /**
     * Place named ore vein with preset parameters.
     * Ores: "copper", "tin", "iron", "lead", "silver", "tungsten", "gold", "platinum",
     *       "meteorite", "hellstone", "cobalt", "palladium", "mythril", "orichalcum",
     *       "adamantite", "titanium", "chlorophyte", "luminite".
     * size: "small" (0.5x), "medium" (default, 1.0x), "large" (2.0x).
     */
    oreVein(oreName: string, x: number, y: number, size?: string): void;
    /** List available ore names and tile IDs. */
    listOreTypes(): OreTypeInfo[];

    // ── Biome Regions ──

    /** Convert region to ice/snow biome. stone→ice, dirt→snow, mud→slush. shape default "trapezoid". Returns tiles converted. */
    iceBiome(x: number, y: number, w: number, h: number, shape?: BiomeShape): number;
    /** Convert region to glowing mushroom biome (mud + mushroom grass). shape default "ellipse". Returns tiles converted. */
    mushroomBiome(x: number, y: number, w: number, h: number, shape?: BiomeShape): number;
    /** Create marble cave with marble blocks and walls. strength default 40. */
    marbleCave(x: number, y: number, strength?: number): void;
    /** Create granite cave with granite blocks and walls. strength default 40. */
    graniteCave(x: number, y: number, strength?: number): void;
    /** Apply Corruption biome with chasms. shape default "diagonalLeft". Returns tiles converted. */
    corruption(x: number, y: number, w: number, depth: number, shape?: BiomeShape): number;
    /** Apply Crimson biome with organic chasms. shape default "diagonalRight". Returns tiles converted. */
    crimson(x: number, y: number, w: number, depth: number, shape?: BiomeShape): number;
    /** Apply Hallow biome. shape default "diagonalLeft". Returns tiles converted. */
    hallow(x: number, y: number, w: number, h: number, shape?: BiomeShape): number;
    /** Create spider cave filled with cobwebs and spider walls. strength default 10. */
    spiderCave(x: number, y: number, strength?: number): void;

    // ── Structures ──

    /**
     * Generate ocean at world edge with exponential depth slope.
     * direction: -1 (left/west) or 1 (right/east). Default 1.
     * oceanWidth: tiles wide (default ~15% of world). maxDepth: max depth in tiles (default 80).
     * Returns tiles placed.
     */
    ocean(direction?: number, oceanWidth?: number, maxDepth?: number): number;
    /** Generate desert: sand surface + sandstone underground + caves. shape default "ellipse". Returns tiles converted. */
    desert(x: number, y: number, w: number, h: number, shape?: BiomeShape): number;
    /** Generate jungle: mud/jungle grass + dithered edges + caves. shape default "rectangle", fills entire vertical. ditherWidth default 15. */
    jungle(x: number, y: number, w: number, h: number, shape?: BiomeShape, ditherWidth?: number): number;
    /**
     * Generate underworld (hell): random walk ceiling/lava floor, ash, hellstone, caves, chimneys.
     * yStart: optional top of underworld (default: auto ~200 tiles from bottom). Returns ash tiles placed.
     */
    underworld(yStart?: number): number;
    /** Create beehive: hive block shell filled with honey. size default 15-30 random. */
    beehive(x: number, y: number, size?: number): void;
    /** Create sandstone pyramid with internal rooms. height default 40-80 random. */
    pyramid(x: number, y: number, height?: number): void;
    /** Create hollow living tree with roots and leaf canopy. height default 40-80 random. */
    livingTree(x: number, y: number, height?: number): void;
    /** Generate dungeon rooms/corridors. direction: -1 or 1. style: 0=blue, 1=green, 2=pink. */
    dungeon(x: number, y: number, direction?: number, style?: number): void;
    /** Create lihzahrd brick temple with grid rooms. w default 80-150, h default 60-100. */
    jungleTemple(x: number, y: number, w?: number, h?: number): void;
    /** Place underground house. style: 0=wood, 1=stone, 2=dungeon. */
    undergroundHouse(x: number, y: number, style?: number): void;

    // ── Decoration ──

    /** Hang vines from grass. biome: "forest" (default), "jungle", "hallow", "crimson". Returns count placed. */
    placeVines(x: number, y: number, w: number, h: number, biome?: string): number;
    /** Place random plants on grass. biome: "forest", "jungle", "hallow", "corruption", "crimson", "mushroom". Returns count placed. */
    placePlants(x: number, y: number, w: number, h: number, biome?: string): number;
    /** Place clay pots on solid surfaces. count default area/2000. Returns count placed. */
    placePots(x: number, y: number, w: number, h: number, count?: number): number;
    /** Place stalactites/stalagmites in caves. count default area/1000. Returns count placed. */
    placeStalactites(x: number, y: number, w: number, h: number, count?: number): number;
    /** Place dart traps on walls facing open space. count default area/5000. Returns count placed. */
    placeTraps(x: number, y: number, w: number, h: number, count?: number): number;
    /** Place life crystals (2x2) on solid surfaces. count default area/10000. Returns count placed. */
    placeLifeCrystals(x: number, y: number, w: number, h: number, count?: number): number;
    /** Auto-slope exposed tile edges for natural terrain. Returns tiles modified. */
    smoothWorld(x: number, y: number, w: number, h: number): number;
    /** Place sunflowers on grass surfaces. count default w/30. Returns count placed. */
    placeSunflowers(x: number, y: number, w: number, h: number, count?: number): number;
    /** Place thorns on corruption/crimson grass. biome: "corruption" (default) or "crimson". Returns count placed. */
    placeThorns(x: number, y: number, w: number, h: number, biome?: string): number;
};

// ─── Global Functions ────────────────────────────────────────────────────────

/** Print a message to the script log (alias for log.print). */
declare function print(message: any): void;
