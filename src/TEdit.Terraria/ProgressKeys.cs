namespace TEdit.Terraria;

/// <summary>
/// Stable progress message identifiers reported by world file operations.
/// The TEdit.Terraria core library cannot reference the application's resource
/// strings, so the values stay English and double as the fallback display text;
/// the application layer translates them for the UI.
/// </summary>
public static class ProgressKeys
{
    // File headers
    public const string LoadingFileHeader = "Loading File Header...";
    public const string SaveHeaders = "Save headers...";

    // Tiles
    public const string LoadingTiles = "Loading Tiles...";
    public const string SaveTiles = "Save Tiles...";
    public const string SavingTiles = "Saving Tiles...";
    public const string RemovingFutureTiles = "Removing Future Tiles...";

    // Chests
    public const string LoadingChests = "Loading Chests...";
    public const string SaveChests = "Save Chests...";
    public const string SavingChests = "Saving Chests...";

    // Signs
    public const string LoadingSigns = "Loading Signs...";
    public const string SaveSigns = "Save Signs...";
    public const string SavingSigns = "Saving Signs...";

    // NPCs
    public const string LoadingNpcs = "Loading NPCs...";
    public const string LoadingNpcData = "Loading NPC Data...";
    public const string LoadingNpcNames = "Loading NPC Names...";
    public const string SaveNpcs = "Save NPCs...";
    public const string SavingNpcData = "Saving NPC Data...";
    public const string SavingNpcNames = "Saving NPC Names...";

    // Tile entities & sections
    public const string LoadingTileEntitiesSection = "Loading Tile Entities Section...";
    public const string SaveTileEntitiesSection = "Save Tile Entities Section...";
    public const string SaveWeightedPressurePlates = "Save Weighted Pressure Plates...";
    public const string SaveTownManager = "Save Town Manager...";
    public const string SaveBestiary = "Save Bestiary...";
    public const string SaveCreativePowers = "Save Creative Powers...";
    public const string SaveFooters = "Save Footers...";

    // Verification & physics
    public const string ValidatingFile = "Validating File...";
    public const string SavingValidationData = "Saving Validation Data...";
    public const string SettlingSand = "Settling Sand...";

    // Completion
    public const string WorldLoadComplete = "World Load Complete.";
    public const string WorldSaveComplete = "World Save Complete.";
    public const string SaveComplete = "Save Complete.";

    // Additional file operations
    public const string VerifyingFile = "Verifying File...";
    public const string LoadComplete = "Load Complete.";
    public const string ValidatingComplete = "Validating Complete...";
    public const string ResettingTime = "Resetting Time...";
    public const string PreparingTModLoaderData = "Preparing tModLoader data...";
    public const string SavingTModLoaderData = "Saving tModLoader data...";
    public const string LoadingTModLoaderData = "Loading tModLoader data...";
    public const string LoadedWindowsPhoneHeader = "Loaded Windows Phone world header.";
    public const string ConvertingToWindowsPhone = "Converting world to Windows Phone...";

}
