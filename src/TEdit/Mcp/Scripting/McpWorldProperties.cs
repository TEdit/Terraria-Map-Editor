using System;
using System.Collections.Generic;
using System.Linq;

namespace TEdit.Mcp.Scripting;

/// <summary>
/// WorldInfoApi members that MCP scripts may read and write through world.get and world.set:
/// exactly the fields the World Properties sidebar lets a user edit. McpWorldPropertiesTests
/// fails when the sidebar gains or loses an editable field, so this list stays in step with the UI.
/// </summary>
internal static class McpWorldProperties
{
    internal static readonly string[] Members =
    [
        "AltarCount", "AnglerQuest", "Apocalypse", "BloodMoon", "BoughtBunny", "BoughtCat", "BoughtDog",
        "CaveBackStyle0", "CaveBackStyle1", "CaveBackStyle2", "CaveBackStyle3", "CaveBackX0", "CaveBackX1",
        "CaveBackX2", "CelestialNebulaActive", "CelestialSolarActive", "CelestialStardustActive",
        "CelestialVortexActive", "CloudBgActive", "CombatBookUsed", "CombatBookVolumeTwoWasUsed", "CultistDelay",
        "DontStarveWorld", "DownedCelestialNebula", "DownedCelestialSolar", "DownedCelestialStardust",
        "DownedCelestialVortex", "DownedChristmasQueen", "DownedChristmasTree", "DownedDD2InvasionT1",
        "DownedDD2InvasionT2", "DownedDD2InvasionT3", "DownedDeerclops", "DownedDestroyer", "DownedEaterOfWorlds",
        "DownedEmpressOfLight", "DownedEyeOfCthulhu", "DownedFishron", "DownedFrost", "DownedGoblins", "DownedGolem",
        "DownedHalloweenKing", "DownedHalloweenTree", "DownedLunaticCultist", "DownedMartians", "DownedMoonlord",
        "DownedPirates", "DownedPlantera", "DownedQueenBee", "DownedQueenSlime", "DownedSanta", "DownedSkeletron",
        "DownedSkeletronPrime", "DownedSlimeKing", "DownedTwins", "DrunkWorld", "DualDungeonsSeed", "FastForwardTime",
        "FastForwardTimeToDusk", "ForceHalloweenForToday", "ForceHalloweenForever", "ForceXMasForToday",
        "ForceXMasForever", "GameMode", "GoodWorld", "HardMode", "HellBackStyle", "IceBackStyle", "InfectedSeed",
        "InvasionSize", "InvasionType", "InvasionX", "IsChinese", "IsConsole", "IsCrimson", "IsEclipse", "IsRaining",
        "JungleBackStyle", "LanternNightCooldown", "LanternNightGenuine", "LanternNightManual",
        "LanternNightNextNightIsGenuine", "MoonPhase", "MoonType", "MoondialCooldown", "MoreLightningSeed",
        "NoLightningSeed", "NoSpidersSeed", "NoTrapsWorld", "NotTheBeesWorld", "NumClouds", "PartyOfDoom",
        "PeddlersSatchelWasUsed", "RemixWorld", "RockLevel", "SafeGroundLayers", "SandStormHappening",
        "SandStormIntendedSeverity", "SandStormSeverity", "SandStormTimeLeft", "SavedAngler", "SavedBartender",
        "SavedGoblin", "SavedGolfer", "SavedMech", "SavedOreTiersAdamantite", "SavedOreTiersCobalt",
        "SavedOreTiersCopper", "SavedOreTiersGold", "SavedOreTiersIron", "SavedOreTiersMythril",
        "SavedOreTiersSilver", "SavedStylist", "SavedTaxCollector", "SavedWizard", "Seed", "ShadowOrbCount",
        "SkyblockWorld", "SlimeRainTime", "SpawnMeteor", "SundialCooldown", "SurfaceLevel", "TeamBasedSpawnsSeed",
        "TempMaxRain", "TenthAnniversaryWorld", "Title", "TreeStyle0", "TreeStyle1", "TreeStyle2", "TreeStyle3",
        "TreeX0", "TreeX1", "TreeX2", "UnlockedArmsDealerSpawn", "UnlockedDemolitionistSpawn",
        "UnlockedDyeTraderSpawn", "UnlockedMerchantSpawn", "UnlockedNurseSpawn", "UnlockedPartyGirlSpawn",
        "UnlockedPrincessSpawn", "UnlockedSlimeBlueSpawn", "UnlockedSlimeCopperSpawn", "UnlockedSlimeGreenSpawn",
        "UnlockedSlimeOldSpawn", "UnlockedSlimePurpleSpawn", "UnlockedSlimeRainbowSpawn", "UnlockedSlimeRedSpawn",
        "UnlockedSlimeYellowSpawn", "UnlockedTruffleSpawn", "VampireSeed", "WindSpeedSet", "WorldGuid", "WorldId",
        "ZenithWorld"
    ];

    private static readonly HashSet<string> Exposed = new(Members, StringComparer.OrdinalIgnoreCase);

    internal static bool IsExposed(string name) => Exposed.Contains(name);

    /// <summary>Names as scripts write them, matching the local sidebar API (camelCase).</summary>
    internal static IEnumerable<string> ScriptNames => Members.Select(m => char.ToLowerInvariant(m[0]) + m[1..]);
}
