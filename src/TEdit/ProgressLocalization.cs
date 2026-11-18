using TEdit.Properties;
using TEdit.Terraria;

namespace TEdit;

/// <summary>
/// Translates progress messages reported by the core library (which only emits
/// English <see cref="ProgressKeys"/> values) into localized UI text at the
/// view-model boundary. Unknown messages pass through unchanged.
/// </summary>
public static class ProgressLocalization
{
    public static object? Translate(object? state)
    {
        if (state is not string text)
            return state;

        return text switch
        {
            ProgressKeys.LoadingFileHeader => Language.progress_loading_file_header,
            ProgressKeys.SaveHeaders => Language.progress_save_headers,

            ProgressKeys.LoadingTiles => Language.progress_loading_tiles,
            ProgressKeys.SaveTiles => Language.progress_save_tiles,
            ProgressKeys.SavingTiles => Language.progress_saving_tiles,
            ProgressKeys.RemovingFutureTiles => Language.progress_removing_future_tiles,

            ProgressKeys.LoadingChests => Language.progress_loading_chests,
            ProgressKeys.SaveChests => Language.progress_save_chests,
            ProgressKeys.SavingChests => Language.progress_saving_chests,

            ProgressKeys.LoadingSigns => Language.progress_loading_signs,
            ProgressKeys.SaveSigns => Language.progress_save_signs,
            ProgressKeys.SavingSigns => Language.progress_saving_signs,

            ProgressKeys.LoadingNpcs => Language.progress_loading_npcs,
            ProgressKeys.LoadingNpcData => Language.progress_loading_npc_data,
            ProgressKeys.LoadingNpcNames => Language.progress_loading_npc_names,
            ProgressKeys.SaveNpcs => Language.progress_save_npcs,
            ProgressKeys.SavingNpcData => Language.progress_saving_npc_data,
            ProgressKeys.SavingNpcNames => Language.progress_saving_npc_names,

            ProgressKeys.LoadingTileEntitiesSection => Language.progress_loading_tile_entities_section,
            ProgressKeys.SaveTileEntitiesSection => Language.progress_save_tile_entities_section,
            ProgressKeys.SaveWeightedPressurePlates => Language.progress_save_weighted_pressure_plates,
            ProgressKeys.SaveTownManager => Language.progress_save_town_manager,
            ProgressKeys.SaveBestiary => Language.progress_save_bestiary,
            ProgressKeys.SaveCreativePowers => Language.progress_save_creative_powers,
            ProgressKeys.SaveFooters => Language.progress_save_footers,

            ProgressKeys.ValidatingFile => Language.progress_validating_file,
            ProgressKeys.SavingValidationData => Language.progress_saving_validation_data,
            ProgressKeys.SettlingSand => Language.progress_settling_sand,

            ProgressKeys.WorldLoadComplete => Language.progress_world_load_complete,
            ProgressKeys.WorldSaveComplete => Language.progress_world_save_complete,
            ProgressKeys.SaveComplete => Language.progress_save_complete,

            ProgressKeys.VerifyingFile => Language.progress_verifying_file,
            ProgressKeys.LoadComplete => Language.progress_load_complete,
            ProgressKeys.ValidatingComplete => Language.progress_validating_complete,
            ProgressKeys.ResettingTime => Language.progress_resetting_time,
            ProgressKeys.PreparingTModLoaderData => Language.progress_preparing_tmodloader_data,
            ProgressKeys.SavingTModLoaderData => Language.progress_saving_tmodloader_data,
            ProgressKeys.LoadingTModLoaderData => Language.progress_loading_tmodloader_data,
            ProgressKeys.LoadedWindowsPhoneHeader => Language.progress_loaded_windows_phone_header,
            ProgressKeys.ConvertingToWindowsPhone => Language.progress_converting_to_windows_phone,

            _ => text,
        };
    }
}
