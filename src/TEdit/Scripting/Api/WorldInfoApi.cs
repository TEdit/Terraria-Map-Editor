using TEdit.Terraria;

namespace TEdit.Scripting.Api;

public class WorldInfoApi
{
    private readonly World _world;

    public WorldInfoApi(World world)
    {
        _world = world;
    }

    // ── World Size (read-only) ──────────────────────────────────────
    public int Width => _world.TilesWide;
    public int Height => _world.TilesHigh;

    // ── World Metadata ──────────────────────────────────────────────
    public string Title { get => _world.Title ?? ""; set => _world.Title = value; }
    public int WorldId { get => _world.WorldId; set => _world.WorldId = value; }
    public string Seed { get => _world.Seed ?? ""; set => _world.Seed = value; }
    public bool IsFavorite { get => _world.IsFavorite; set => _world.IsFavorite = value; }
    public bool IsChinese { get => _world.IsChinese; set => _world.IsChinese = value; }
    public bool IsConsole { get => _world.IsConsole; set => _world.IsConsole = value; }
    public uint FileRevision { get => _world.FileRevision; set => _world.FileRevision = value; }
    public ulong WorldGenVersion { get => _world.WorldGenVersion; set => _world.WorldGenVersion = value; }
    public long CreationTime { get => _world.CreationTime; set => _world.CreationTime = value; }
    public long LastPlayed { get => _world.LastPlayed; set => _world.LastPlayed = value; }

    // ── Spawn & Dungeon ─────────────────────────────────────────────
    public int SpawnX { get => _world.SpawnX; set => _world.SpawnX = value; }
    public int SpawnY { get => _world.SpawnY; set => _world.SpawnY = value; }
    public int DungeonX { get => _world.DungeonX; set => _world.DungeonX = value; }
    public int DungeonY { get => _world.DungeonY; set => _world.DungeonY = value; }

    // ── Levels ──────────────────────────────────────────────────────
    public double SurfaceLevel { get => _world.GroundLevel; set => _world.GroundLevel = value; }
    public double RockLevel { get => _world.RockLevel; set => _world.RockLevel = value; }
    public bool SafeGroundLayers { get => _world.SafeGroundLayers; set => _world.SafeGroundLayers = value; }

    // ── Time ────────────────────────────────────────────────────────
    public double Time { get => _world.Time; set => _world.Time = value; }
    public bool DayTime { get => _world.DayTime; set => _world.DayTime = value; }
    public bool FastForwardTime { get => _world.FastForwardTime; set => _world.FastForwardTime = value; }
    public byte SundialCooldown { get => _world.SundialCooldown; set => _world.SundialCooldown = value; }
    public bool FastForwardTimeToDusk { get => _world.FastForwardTimeToDusk; set => _world.FastForwardTimeToDusk = value; }
    public byte MoondialCooldown { get => _world.MoondialCooldown; set => _world.MoondialCooldown = value; }

    // ── Moon ────────────────────────────────────────────────────────
    public int MoonPhase { get => _world.MoonPhase; set => _world.MoonPhase = value; }
    public bool BloodMoon { get => _world.BloodMoon; set => _world.BloodMoon = value; }
    public byte MoonType { get => _world.MoonType; set => _world.MoonType = value; }
    public bool IsEclipse { get => _world.IsEclipse; set => _world.IsEclipse = value; }

    // ── Weather ─────────────────────────────────────────────────────
    public bool IsRaining { get => _world.IsRaining; set => _world.IsRaining = value; }
    public int TempRainTime { get => _world.TempRainTime; set => _world.TempRainTime = value; }
    public float TempMaxRain { get => _world.TempMaxRain; set => _world.TempMaxRain = value; }
    public double SlimeRainTime { get => _world.SlimeRainTime; set => _world.SlimeRainTime = value; }
    public int TempMeteorShowerCount { get => _world.TempMeteorShowerCount; set => _world.TempMeteorShowerCount = value; }
    public int TempCoinRain { get => _world.TempCoinRain; set => _world.TempCoinRain = value; }
    public short NumClouds { get => _world.NumClouds; set => _world.NumClouds = value; }
    public float WindSpeedSet { get => _world.WindSpeedSet; set => _world.WindSpeedSet = value; }
    public float CloudBgActive { get => _world.CloudBgActive; set => _world.CloudBgActive = value; }

    // ── Sandstorm ───────────────────────────────────────────────────
    public bool SandStormHappening { get => _world.SandStormHappening; set => _world.SandStormHappening = value; }
    public int SandStormTimeLeft { get => _world.SandStormTimeLeft; set => _world.SandStormTimeLeft = value; }
    public float SandStormSeverity { get => _world.SandStormSeverity; set => _world.SandStormSeverity = value; }
    public float SandStormIntendedSeverity { get => _world.SandStormIntendedSeverity; set => _world.SandStormIntendedSeverity = value; }

    // ── Holidays ────────────────────────────────────────────────────
    public bool ForceHalloweenForToday { get => _world.ForceHalloweenForToday; set => _world.ForceHalloweenForToday = value; }
    public bool ForceXMasForToday { get => _world.ForceXMasForToday; set => _world.ForceXMasForToday = value; }
    public bool ForceHalloweenForever { get => _world.ForceHalloweenForever; set => _world.ForceHalloweenForever = value; }
    public bool ForceXMasForever { get => _world.ForceXMasForever; set => _world.ForceXMasForever = value; }

    // ── Difficulty ──────────────────────────────────────────────────
    public bool HardMode { get => _world.HardMode; set => _world.HardMode = value; }
    public int GameMode { get => _world.GameMode; set => _world.GameMode = value; }
    public bool SpawnMeteor { get => _world.SpawnMeteor; set => _world.SpawnMeteor = value; }
    public bool CombatBookUsed { get => _world.CombatBookUsed; set => _world.CombatBookUsed = value; }
    public bool CombatBookVolumeTwoWasUsed { get => _world.CombatBookVolumeTwoWasUsed; set => _world.CombatBookVolumeTwoWasUsed = value; }
    public bool PeddlersSatchelWasUsed { get => _world.PeddlersSatchelWasUsed; set => _world.PeddlersSatchelWasUsed = value; }
    public bool PartyOfDoom { get => _world.PartyOfDoom; set => _world.PartyOfDoom = value; }

    // ── World Seeds ─────────────────────────────────────────────────
    public bool DrunkWorld { get => _world.DrunkWorld; set => _world.DrunkWorld = value; }
    public bool GoodWorld { get => _world.GoodWorld; set => _world.GoodWorld = value; }
    public bool TenthAnniversaryWorld { get => _world.TenthAnniversaryWorld; set => _world.TenthAnniversaryWorld = value; }
    public bool DontStarveWorld { get => _world.DontStarveWorld; set => _world.DontStarveWorld = value; }
    public bool NotTheBeesWorld { get => _world.NotTheBeesWorld; set => _world.NotTheBeesWorld = value; }
    public bool RemixWorld { get => _world.RemixWorld; set => _world.RemixWorld = value; }
    public bool NoTrapsWorld { get => _world.NoTrapsWorld; set => _world.NoTrapsWorld = value; }
    public bool ZenithWorld { get => _world.ZenithWorld; set => _world.ZenithWorld = value; }
    public bool SkyblockWorld { get => _world.SkyblockWorld; set => _world.SkyblockWorld = value; }
    public bool VampireSeed { get => _world.VampireSeed; set => _world.VampireSeed = value; }
    public bool InfectedSeed { get => _world.InfectedSeed; set => _world.InfectedSeed = value; }
    public bool DualDungeonsSeed { get => _world.DualDungeonsSeed; set => _world.DualDungeonsSeed = value; }

    // ── Ore Tiers ───────────────────────────────────────────────────
    public bool IsCrimson { get => _world.IsCrimson; set => _world.IsCrimson = value; }
    public int AltarCount { get => _world.AltarCount; set => _world.AltarCount = value; }
    public bool ShadowOrbSmashed { get => _world.ShadowOrbSmashed; set => _world.ShadowOrbSmashed = value; }
    public int ShadowOrbCount { get => _world.ShadowOrbCount; set => _world.ShadowOrbCount = value; }
    public int SavedOreTiersCopper { get => _world.SavedOreTiersCopper; set => _world.SavedOreTiersCopper = value; }
    public int SavedOreTiersIron { get => _world.SavedOreTiersIron; set => _world.SavedOreTiersIron = value; }
    public int SavedOreTiersSilver { get => _world.SavedOreTiersSilver; set => _world.SavedOreTiersSilver = value; }
    public int SavedOreTiersGold { get => _world.SavedOreTiersGold; set => _world.SavedOreTiersGold = value; }
    public int SavedOreTiersCobalt { get => _world.SavedOreTiersCobalt; set => _world.SavedOreTiersCobalt = value; }
    public int SavedOreTiersMythril { get => _world.SavedOreTiersMythril; set => _world.SavedOreTiersMythril = value; }
    public int SavedOreTiersAdamantite { get => _world.SavedOreTiersAdamantite; set => _world.SavedOreTiersAdamantite = value; }

    // ── Bosses: Pre-Hardmode ────────────────────────────────────────
    public bool DownedSlimeKing { get => _world.DownedSlimeKingBoss; set => _world.DownedSlimeKingBoss = value; }
    public bool DownedEyeOfCthulhu { get => _world.DownedBoss1EyeofCthulhu; set => _world.DownedBoss1EyeofCthulhu = value; }
    public bool DownedEaterOfWorlds { get => _world.DownedBoss2EaterofWorlds; set => _world.DownedBoss2EaterofWorlds = value; }
    public bool DownedQueenBee { get => _world.DownedQueenBee; set => _world.DownedQueenBee = value; }
    public bool DownedSkeletron { get => _world.DownedBoss3Skeletron; set => _world.DownedBoss3Skeletron = value; }

    // ── Bosses: Hardmode ────────────────────────────────────────────
    public bool DownedDestroyer { get => _world.DownedMechBoss1TheDestroyer; set => _world.DownedMechBoss1TheDestroyer = value; }
    public bool DownedTwins { get => _world.DownedMechBoss2TheTwins; set => _world.DownedMechBoss2TheTwins = value; }
    public bool DownedSkeletronPrime { get => _world.DownedMechBoss3SkeletronPrime; set => _world.DownedMechBoss3SkeletronPrime = value; }
    public bool DownedMechBossAny => _world.DownedMechBossAny; // read-only computed
    public bool DownedPlantera { get => _world.DownedPlantBoss; set => _world.DownedPlantBoss = value; }
    public bool DownedGolem { get => _world.DownedGolemBoss; set => _world.DownedGolemBoss = value; }
    public bool DownedFishron { get => _world.DownedFishron; set => _world.DownedFishron = value; }
    public bool DownedLunaticCultist { get => _world.DownedLunaticCultist; set => _world.DownedLunaticCultist = value; }
    public bool DownedMoonlord { get => _world.DownedMoonlord; set => _world.DownedMoonlord = value; }

    // ── Bosses: Journey's End ───────────────────────────────────────
    public bool DownedEmpressOfLight { get => _world.DownedEmpressOfLight; set => _world.DownedEmpressOfLight = value; }
    public bool DownedQueenSlime { get => _world.DownedQueenSlime; set => _world.DownedQueenSlime = value; }
    public bool DownedDeerclops { get => _world.DownedDeerclops; set => _world.DownedDeerclops = value; }

    // ── Boss Events ─────────────────────────────────────────────────
    public bool DownedHalloweenTree { get => _world.DownedHalloweenTree; set => _world.DownedHalloweenTree = value; }
    public bool DownedHalloweenKing { get => _world.DownedHalloweenKing; set => _world.DownedHalloweenKing = value; }
    public bool DownedChristmasTree { get => _world.DownedChristmasTree; set => _world.DownedChristmasTree = value; }
    public bool DownedSanta { get => _world.DownedSanta; set => _world.DownedSanta = value; }
    public bool DownedChristmasQueen { get => _world.DownedChristmasQueen; set => _world.DownedChristmasQueen = value; }
    public bool DownedCelestialSolar { get => _world.DownedCelestialSolar; set => _world.DownedCelestialSolar = value; }
    public bool DownedCelestialNebula { get => _world.DownedCelestialNebula; set => _world.DownedCelestialNebula = value; }
    public bool DownedCelestialVortex { get => _world.DownedCelestialVortex; set => _world.DownedCelestialVortex = value; }
    public bool DownedCelestialStardust { get => _world.DownedCelestialStardust; set => _world.DownedCelestialStardust = value; }
    public bool CelestialSolarActive { get => _world.CelestialSolarActive; set => _world.CelestialSolarActive = value; }
    public bool CelestialVortexActive { get => _world.CelestialVortexActive; set => _world.CelestialVortexActive = value; }
    public bool CelestialNebulaActive { get => _world.CelestialNebulaActive; set => _world.CelestialNebulaActive = value; }
    public bool CelestialStardustActive { get => _world.CelestialStardustActive; set => _world.CelestialStardustActive = value; }

    // ── Old One's Army ──────────────────────────────────────────────
    public bool DownedDD2InvasionT1 { get => _world.DownedDD2InvasionT1; set => _world.DownedDD2InvasionT1 = value; }
    public bool DownedDD2InvasionT2 { get => _world.DownedDD2InvasionT2; set => _world.DownedDD2InvasionT2 = value; }
    public bool DownedDD2InvasionT3 { get => _world.DownedDD2InvasionT3; set => _world.DownedDD2InvasionT3 = value; }

    // ── Invasions ───────────────────────────────────────────────────
    public bool DownedGoblins { get => _world.DownedGoblins; set => _world.DownedGoblins = value; }
    public bool DownedFrost { get => _world.DownedFrost; set => _world.DownedFrost = value; }
    public bool DownedPirates { get => _world.DownedPirates; set => _world.DownedPirates = value; }
    public bool DownedMartians { get => _world.DownedMartians; set => _world.DownedMartians = value; }
    public int InvasionType { get => _world.InvasionType; set => _world.InvasionType = value; }
    public int InvasionSize { get => _world.InvasionSize; set => _world.InvasionSize = value; }
    public double InvasionX { get => _world.InvasionX; set => _world.InvasionX = value; }

    // ── NPCs Saved ──────────────────────────────────────────────────
    public bool SavedGoblin { get => _world.SavedGoblin; set => _world.SavedGoblin = value; }
    public bool SavedMech { get => _world.SavedMech; set => _world.SavedMech = value; }
    public bool SavedWizard { get => _world.SavedWizard; set => _world.SavedWizard = value; }
    public bool SavedStylist { get => _world.SavedStylist; set => _world.SavedStylist = value; }
    public bool SavedTaxCollector { get => _world.SavedTaxCollector; set => _world.SavedTaxCollector = value; }
    public bool SavedBartender { get => _world.SavedBartender; set => _world.SavedBartender = value; }
    public bool SavedGolfer { get => _world.SavedGolfer; set => _world.SavedGolfer = value; }
    public bool SavedAngler { get => _world.SavedAngler; set => _world.SavedAngler = value; }
    public int AnglerQuest { get => _world.AnglerQuest; set => _world.AnglerQuest = value; }

    // ── NPCs Bought ─────────────────────────────────────────────────
    public bool BoughtCat { get => _world.BoughtCat; set => _world.BoughtCat = value; }
    public bool BoughtDog { get => _world.BoughtDog; set => _world.BoughtDog = value; }
    public bool BoughtBunny { get => _world.BoughtBunny; set => _world.BoughtBunny = value; }

    // ── NPCs Unlocked ───────────────────────────────────────────────
    public bool UnlockedMerchantSpawn { get => _world.UnlockedMerchantSpawn; set => _world.UnlockedMerchantSpawn = value; }
    public bool UnlockedDemolitionistSpawn { get => _world.UnlockedDemolitionistSpawn; set => _world.UnlockedDemolitionistSpawn = value; }
    public bool UnlockedPartyGirlSpawn { get => _world.UnlockedPartyGirlSpawn; set => _world.UnlockedPartyGirlSpawn = value; }
    public bool UnlockedDyeTraderSpawn { get => _world.UnlockedDyeTraderSpawn; set => _world.UnlockedDyeTraderSpawn = value; }
    public bool UnlockedTruffleSpawn { get => _world.UnlockedTruffleSpawn; set => _world.UnlockedTruffleSpawn = value; }
    public bool UnlockedArmsDealerSpawn { get => _world.UnlockedArmsDealerSpawn; set => _world.UnlockedArmsDealerSpawn = value; }
    public bool UnlockedNurseSpawn { get => _world.UnlockedNurseSpawn; set => _world.UnlockedNurseSpawn = value; }
    public bool UnlockedPrincessSpawn { get => _world.UnlockedPrincessSpawn; set => _world.UnlockedPrincessSpawn = value; }

    // ── Town Slimes Unlocked ────────────────────────────────────────
    public bool UnlockedSlimeBlueSpawn { get => _world.UnlockedSlimeBlueSpawn; set => _world.UnlockedSlimeBlueSpawn = value; }
    public bool UnlockedSlimeGreenSpawn { get => _world.UnlockedSlimeGreenSpawn; set => _world.UnlockedSlimeGreenSpawn = value; }
    public bool UnlockedSlimeOldSpawn { get => _world.UnlockedSlimeOldSpawn; set => _world.UnlockedSlimeOldSpawn = value; }
    public bool UnlockedSlimePurpleSpawn { get => _world.UnlockedSlimePurpleSpawn; set => _world.UnlockedSlimePurpleSpawn = value; }
    public bool UnlockedSlimeRainbowSpawn { get => _world.UnlockedSlimeRainbowSpawn; set => _world.UnlockedSlimeRainbowSpawn = value; }
    public bool UnlockedSlimeRedSpawn { get => _world.UnlockedSlimeRedSpawn; set => _world.UnlockedSlimeRedSpawn = value; }
    public bool UnlockedSlimeYellowSpawn { get => _world.UnlockedSlimeYellowSpawn; set => _world.UnlockedSlimeYellowSpawn = value; }
    public bool UnlockedSlimeCopperSpawn { get => _world.UnlockedSlimeCopperSpawn; set => _world.UnlockedSlimeCopperSpawn = value; }

    // ── Lantern Night ───────────────────────────────────────────────
    public int LanternNightCooldown { get => _world.LanternNightCooldown; set => _world.LanternNightCooldown = value; }
    public bool LanternNightManual { get => _world.LanternNightManual; set => _world.LanternNightManual = value; }
    public bool LanternNightGenuine { get => _world.LanternNightGenuine; set => _world.LanternNightGenuine = value; }
    public bool LanternNightNextNightIsGenuine { get => _world.LanternNightNextNightIsGenuine; set => _world.LanternNightNextNightIsGenuine = value; }

    // ── Party ───────────────────────────────────────────────────────
    public bool PartyManual { get => _world.PartyManual; set => _world.PartyManual = value; }
    public bool PartyGenuine { get => _world.PartyGenuine; set => _world.PartyGenuine = value; }
    public int PartyCooldown { get => _world.PartyCooldown; set => _world.PartyCooldown = value; }

    // ── Backgrounds ─────────────────────────────────────────────────
    public byte BgOcean { get => _world.BgOcean; set => _world.BgOcean = value; }
    public byte BgDesert { get => _world.BgDesert; set => _world.BgDesert = value; }
    public byte BgCrimson { get => _world.BgCrimson; set => _world.BgCrimson = value; }
    public byte BgHallow { get => _world.BgHallow; set => _world.BgHallow = value; }
    public byte BgSnow { get => _world.BgSnow; set => _world.BgSnow = value; }
    public byte BgJungle { get => _world.BgJungle; set => _world.BgJungle = value; }
    public byte BgCorruption { get => _world.BgCorruption; set => _world.BgCorruption = value; }
    public byte BgTree { get => _world.BgTree; set => _world.BgTree = value; }
    public byte BgTree2 { get => _world.BgTree2; set => _world.BgTree2 = value; }
    public byte BgTree3 { get => _world.BgTree3; set => _world.BgTree3 = value; }
    public byte BgTree4 { get => _world.BgTree4; set => _world.BgTree4 = value; }
    public byte UnderworldBg { get => _world.UnderworldBg; set => _world.UnderworldBg = value; }
    public byte MushroomBg { get => _world.MushroomBg; set => _world.MushroomBg = value; }
    public int IceBackStyle { get => _world.IceBackStyle; set => _world.IceBackStyle = value; }
    public int JungleBackStyle { get => _world.JungleBackStyle; set => _world.JungleBackStyle = value; }
    public int HellBackStyle { get => _world.HellBackStyle; set => _world.HellBackStyle = value; }

    // ── Tree/Cave Positions & Styles ────────────────────────────────
    public int TreeX0 { get => _world.TreeX0; set => _world.TreeX0 = value; }
    public int TreeX1 { get => _world.TreeX1; set => _world.TreeX1 = value; }
    public int TreeX2 { get => _world.TreeX2; set => _world.TreeX2 = value; }
    public int TreeStyle0 { get => _world.TreeStyle0; set => _world.TreeStyle0 = value; }
    public int TreeStyle1 { get => _world.TreeStyle1; set => _world.TreeStyle1 = value; }
    public int TreeStyle2 { get => _world.TreeStyle2; set => _world.TreeStyle2 = value; }
    public int TreeStyle3 { get => _world.TreeStyle3; set => _world.TreeStyle3 = value; }
    public int TreeTop1 { get => _world.TreeTop1; set => _world.TreeTop1 = value; }
    public int TreeTop2 { get => _world.TreeTop2; set => _world.TreeTop2 = value; }
    public int TreeTop3 { get => _world.TreeTop3; set => _world.TreeTop3 = value; }
    public int TreeTop4 { get => _world.TreeTop4; set => _world.TreeTop4 = value; }
    public int CaveBackX0 { get => _world.CaveBackX0; set => _world.CaveBackX0 = value; }
    public int CaveBackX1 { get => _world.CaveBackX1; set => _world.CaveBackX1 = value; }
    public int CaveBackX2 { get => _world.CaveBackX2; set => _world.CaveBackX2 = value; }
    public int CaveBackStyle0 { get => _world.CaveBackStyle0; set => _world.CaveBackStyle0 = value; }
    public int CaveBackStyle1 { get => _world.CaveBackStyle1; set => _world.CaveBackStyle1 = value; }
    public int CaveBackStyle2 { get => _world.CaveBackStyle2; set => _world.CaveBackStyle2 = value; }
    public int CaveBackStyle3 { get => _world.CaveBackStyle3; set => _world.CaveBackStyle3 = value; }

    // ── World Bounds ────────────────────────────────────────────────
    public float BottomWorld { get => _world.BottomWorld; set => _world.BottomWorld = value; }
    public float TopWorld { get => _world.TopWorld; set => _world.TopWorld = value; }
    public float RightWorld { get => _world.RightWorld; set => _world.RightWorld = value; }
    public float LeftWorld { get => _world.LeftWorld; set => _world.LeftWorld = value; }

    // ── Other ───────────────────────────────────────────────────────
    public int CultistDelay { get => _world.CultistDelay; set => _world.CultistDelay = value; }
    public bool Apocalypse { get => _world.Apocalypse; set => _world.Apocalypse = value; }
    public bool DownedClown { get => _world.DownedClown; set => _world.DownedClown = value; }
}
