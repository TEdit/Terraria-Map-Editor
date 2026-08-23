# World property value ranges

This reference documents every world value exposed by TEdit's **World Properties** panel. It is based on TEdit `main` and the Terraria 1.4.5.8 dedicated-server source.

Three different ranges are relevant:

- **TEdit range** is what the current control accepts.
- **Terraria range** is what Terraria creates or treats as meaningful during normal play.
- **File range** is the capacity of the serialized data type. It is not automatically a safe editing range.

`Correct` means the TEdit control covers Terraria's meaningful values. `Review` means the control excludes normal values, offers values Terraria does not normally use, or does not match the stored property type. `Dynamic` means the safe value depends on the world dimensions or current game state.

## Corrected editor ranges

| Property | TEdit range | Terraria range | Finding |
| --- | --- | --- | --- |
| `SundialCooldown`, `MoondialCooldown` | 0–8 | 0–8 days; byte in file | Corrected. Terraria sets these to 8 and decrements them at dawn or dusk. |
| `SlimeRainTime` | −604,700–54,000 numeric editor | −604,700–54,000 ticks in ordinary state transitions | Corrected. Negative values are the wait until the next event; positive values are the active duration. |
| `CloudBgActive` | −345,599–172,799 numeric editor | about −345,599–172,799 ticks | Corrected. This is a signed transition timer, not a Boolean intensity. |
| `NumClouds` | 0–200 | 0–200 | Corrected. Terraria declares 200 as the maximum and produces 200 during some weather. |
| `WindSpeedSet` | −0.8–0.8 | −0.8–0.8 | Corrected. Terraria clamps the target to 0.8 in ordinary updates and Journey controls. |
| `SandStormIntendedSeverity` | 0–1.4 | 0–less than 1.4 | Corrected. An active sandstorm chooses `0.4 + random [0,1)`. |
| `AltarCount` | 0–Int32 maximum numeric editor | 0 and upward | Corrected. The count increases for every smashed altar. |
| `ShadowOrbCount` | 0–2 | 0–2 persistent | Corrected. The third smash triggers the event and resets the counter to 0. |
| `InvasionSize` | 0–15,420 numeric editor | Formula based on invasion type and up to 255 active players | Corrected. 15,420 is the Pirate Invasion maximum at Terraria's player cap. |
| `LanternNightCooldown` | 0–10 numeric editor | 0–10 days stored as Int32 | Corrected. The property is a day count, not a Boolean. |
| `AnglerQuest` | Quest IDs 0–40 | Quest IDs 0–40 | Corrected. Scarab Fish (39) and Scorpio Fish (40) are included. |

## Numeric input validation

Numeric entry now uses bounded number controls so typed input follows the same range as the adjacent slider:

| Property | Slider | Typed-value behavior |
| --- | --- | --- |
| `MoonPhase` | 0–7 | Typed values are limited to 0–7. |
| `GroundLevel` | 6–`MaxGroundLevel`, six-tile slider ticks | Typed values are limited to the dynamic range; decimal layer values remain supported. |
| `RockLevel` | 12–`MaxCavernLevel`, six-tile slider ticks | Typed values are limited to the dynamic range; decimal layer values remain supported. |
| `TreeX0`, `TreeX1`, `TreeX2` | 0–`TilesWide` | Typed values are bounded and the model preserves threshold ordering. |
| `CaveBackX0`, `CaveBackX1`, `CaveBackX2` | 0–`TilesWide` | Typed values are bounded and the model preserves threshold ordering. |
| `TeamSpawns` | X: 0–`TilesWide − 1`; Y: 0–`TilesHigh − 1` | Typed coordinates are limited to valid tile indexes and therefore also fit Int16 for vanilla world sizes. |

`SandStormSeverity` and `SandStormIntendedSeverity` display one decimal place so their 0.1 steps remain visible.

## Identity and file metadata

| Property | TEdit control | File range or shape | Terraria meaning | Status |
| --- | --- | --- | --- | --- |
| `Title` | Editable text | .NET/BinaryWriter string | World name. Terraria's creation UI applies its own text rules; TEdit does not impose a length range here. | Policy |
| `WorldId` | Editable number text | Int32: −2,147,483,648–2,147,483,647 | Generated world identifier. | File range |
| `WorldGUID` | Editable GUID text | 128-bit GUID | Generated world identifier. | File range |
| `FileRevision` | Read-only text | UInt32: 0–4,294,967,295 | Save revision counter. | Correct |
| `WorldVersion` | Read-only text | UInt32; TEdit support is version-configured | Terraria 1.4.5.8 writes 326. TEdit maps 326 to the unchanged 1.4.5.7 configuration payload. | Correct |
| `Seed` | Read-only text | .NET/BinaryWriter string | World-generation seed text. | Correct |
| `IsChinese` | Checkbox | Boolean | Chinese/mobile-derived world header flag. | Correct |
| `IsConsole` | Checkbox | Boolean | Console-derived world header flag. | Correct |

## Time, weather, and event values

| Property | TEdit range or options | File type | Terraria range or behavior | Status |
| --- | --- | --- | --- | --- |
| `MoonPhase` | 0–7 | Int32 | 0–7; Terraria wraps at 8. | Correct |
| `MoonType` | 0–8 list | Byte | 0–8 in the current version configuration. | Correct |
| `Time` | 0–86,400 continuous-time scale | Double plus `DayTime` | Day is 0–54,000; night is 0–32,400. TEdit combines both into one full-cycle editor scale. | Correct |
| `DayTime` | Represented by the time slider | Boolean | Day/night selector for `Time`. | Correct |
| `SundialCooldown` | 0–8 | Byte | 0–8 days in normal play. | Correct |
| `MoondialCooldown` | 0–8 | Byte | 0–8 nights in normal play. | Correct |
| `TempRainTime` | 0–1 year, piecewise slider; permanent override | Int32 | Natural rain can reach 221,389 ticks. At 5,184,000 or greater Terraria stops decrementing the timer, so all such values are effectively permanent. The checkbox writes exactly 5,184,000; 1,892,160,000 is the one-year display preset. | Correct by design |
| `TempMaxRain` | 0–1 | Single | 0–1 intensity. | Correct |
| `SlimeRainTime` | −604,700–54,000 | Double | Active timers start at 32,400–54,000 and count down through 0; cooldown reaches as low as −604,700. | Correct |
| `CloudBgActive` | −345,599–172,799 | Int32 on disk; Single in TEdit | Signed cloud-background transition timer, about −345,599–172,799 in normal updates. Terraria 1.4.5.8 reads but discards the saved value and initializes a new negative timer. | Correct |
| `NumClouds` | 0–200 | Int16 | 0–200. | Correct |
| `WindSpeedSet` | −0.8–0.8 | Single | −0.8–0.8 ordinary target range. | Correct |
| `SandStormTimeLeft` | 0–86,400 | Int32 | Starts at 28,800–86,400 and counts down to 0. Values over 86,400 are cleared. | Correct |
| `SandStormSeverity` | 0–1 | Single | Clamped to 0–1. | Correct |
| `SandStormIntendedSeverity` | 0–1.4 | Single | Clear weather: 0–less than 0.3; active storm: 0.4–less than 1.4. | Correct |
| `CultistDelay` | 0–86,400 | Int32 | 0–86,400. A missing/older field defaults to 86,400; destroying the tablet sets 43,200. | Correct |
| `LanternNightCooldown` | 0–10 numeric editor | Int32 | 0–10 nights on cooldown. | Correct |
| `InvasionSize` | 0–15,420 numeric editor | Int32 | Base formula is 80 + 40 per active player; pirates add 40 + 20 per player; Martians use 160 + 40 per player. | Correct |
| `InvasionX` | 0–`TilesWide` | Double | Moves between the world edges and spawn. Terraria intentionally uses `maxTilesX` as an edge value. | Correct |

The rain-duration slider reserves 0–50% for 0–60 minutes and uses a logarithmic 50–100% band from 60 minutes to one year. The **Permanent** checkbox writes exactly 5,184,000 and disables the slider. Terraria considers any value at or above that threshold permanent, including the one-year preset; the checkbox identifies the exact override value rather than every value with permanent game behavior.

## Layers, coordinates, and thresholds

World tile coordinates are dynamic. A usable tile is normally `X = 0..TilesWide − 1` and `Y = 0..TilesHigh − 1`, but Terraria uses a few edge values such as `TilesWide` as sentinels.

| Property | TEdit range or control | File type | Safe or natural range | Status |
| --- | --- | --- | --- | --- |
| `SpawnX`, `SpawnY` | Read-only in this panel | Int32 | Tile coordinate inside the world. | Dynamic |
| `DungeonX`, `DungeonY` | Read-only in this panel | Int32 | Tile coordinate inside the world. | Dynamic |
| `GroundLevel` | 6–`MaxGroundLevel` | Double | World-dependent surface boundary. TEdit keeps at least a six-tile gap from `RockLevel`. | Dynamic safety policy |
| `RockLevel` | 12–`MaxCavernLevel` | Double | World-dependent cavern boundary. TEdit keeps at least a six-tile gap from `GroundLevel`. | Dynamic safety policy |
| `TreeX0`, `TreeX1`, `TreeX2` | 0–`TilesWide` | Int32 | Ordered horizontal background thresholds; `TilesWide` is a valid sentinel. | Correct |
| `CaveBackX0`, `CaveBackX1`, `CaveBackX2` | 0–`TilesWide` | Int32 | Ordered horizontal cave-background thresholds; `TilesWide` is a valid sentinel. | Correct |
| `TeamSpawns` | X: 0–`TilesWide − 1`; Y: 0–`TilesHigh − 1` | Count: Byte; each X/Y: Int16 in TEdit's save path | One spawn per team is expected. Coordinates are limited to valid world tiles. | Correct |

`TilesWide`, `MaxGroundLevel`, and `MaxCavernLevel` are UI support values used to calculate dynamic limits. `SafeGroundLayers` is an editor-only Boolean policy switch. `FixLayerGapCommand` is a command, not a world-file property.

## Counts, modes, and identifiers

| Property | TEdit options or range | File type | Terraria values | Status |
| --- | --- | --- | --- | --- |
| `GameMode` | 0 Classic, 1 Expert, 2 Master, 3 Journey | Int32 | 0–3. Legendary is a seed/difficulty combination, not mode 4. | Correct |
| `InvasionType` | 0 None, 1 Goblin, 2 Frost Legion, 3 Pirates, 4 Martians | Int32 | 0–4. | Correct |
| `AltarCount` | 0–Int32 maximum | Int32 | Nonnegative count with no smaller natural cap. | Correct |
| `ShadowOrbCount` | 0–2 | Byte | Persistent cycle states 0–2. | Correct |
| `AnglerQuest` | 0–40 | Int32 | Terraria 1.4.5.8 uses 0–40, including Scarab Fish and Scorpio Fish. | Correct |
| `SavedOreTiersCopper` | −1, 7, 166 | Int32 | Undetermined, Copper, Tin tile IDs. | Correct |
| `SavedOreTiersIron` | −1, 6, 167 | Int32 | Undetermined, Iron, Lead tile IDs. | Correct |
| `SavedOreTiersSilver` | −1, 9, 168 | Int32 | Undetermined, Silver, Tungsten tile IDs. | Correct |
| `SavedOreTiersGold` | −1, 8, 169 | Int32 | Undetermined, Gold, Platinum tile IDs. | Correct |
| `SavedOreTiersCobalt` | −1, 107, 221 | Int32 | Undetermined, Cobalt, Palladium tile IDs. | Correct |
| `SavedOreTiersMythril` | −1, 108, 222 | Int32 | Undetermined, Mythril, Orichalcum tile IDs. | Correct |
| `SavedOreTiersAdamantite` | −1, 111, 223 | Int32 | Undetermined, Adamantite, Titanium tile IDs. | Correct |

## Background and tree styles

These properties are discrete version-specific identifiers. The style fields use Int32 and the biome-background fields use Byte, but the valid range is the set offered by the current preview collection. Treating either file type's capacity as a continuous range would allow missing textures or invalid styles.

| Properties | TEdit value set | File type | Status |
| --- | --- | --- | --- |
| `TreeStyle0`, `TreeStyle1`, `TreeStyle2`, `TreeStyle3` | 0–5 | Int32 | Version-configured |
| `CaveBackStyle0`, `CaveBackStyle1`, `CaveBackStyle2`, `CaveBackStyle3` | 0–7 | Int32 | Version-configured |
| `IceBackStyle` | 0–3 | Int32 | Version-configured |
| `JungleBackStyle` | 0–1 | Int32 | Version-configured |
| `HellBackStyle` | 0–2 | Int32 | Version-configured |
| `BgTree`, `BgTree2`, `BgTree3`, `BgTree4` | 0–13, 31, 51, 71–73 | Byte | Version-configured |
| `BgCorruption` | 0–4, 51–52 | Byte | Version-configured |
| `BgJungle` | 0–6 | Byte | Version-configured |
| `BgSnow` | 0–8, 21–22, 31–32, 41–42 | Byte | Version-configured |
| `BgHallow` | 0–5 | Byte | Version-configured |
| `BgCrimson` | 0–6 | Byte | Version-configured |
| `BgDesert` | 0–4, 51–53 | Byte | Version-configured |
| `BgOcean` | 0–7 | Byte | Version-configured |
| `MushroomBg` | 0–4 | Byte | Version-configured |
| `UnderworldBg` | 0–2 | Byte | Version-configured |

## Boolean properties

Every property in this section has exactly two values, `false` and `true`, and is represented by a checkbox. Terraria may ignore a flag until its related event or seed is active, but there is no wider value range.

| Category | Properties |
| --- | --- |
| Current world state | `BloodMoon`, `IsEclipse`, `IsRaining`, `SandStormHappening`, `HardMode`, `SpawnMeteor`, `IsCrimson`, `FastForwardTime`, `FastForwardTimeToDusk` |
| Celestial pillars | `CelestialSolarActive`, `CelestialVortexActive`, `CelestialNebulaActive`, `CelestialStardustActive` |
| Early bosses and events | `DownedSlimeKingBoss`, `DownedBoss1EyeofCthulhu`, `DownedBoss2EaterofWorlds`, `DownedBoss3Skeletron`, `DownedQueenBee`, `DownedGoblins`, `DownedFrost`, `DownedPirates` |
| Hardmode bosses | `DownedMechBoss1TheDestroyer`, `DownedMechBoss2TheTwins`, `DownedMechBoss3SkeletronPrime`, `DownedPlantBoss`, `DownedGolemBoss`, `DownedFishron`, `DownedLunaticCultist`, `DownedMoonlord`, `DownedMartians` |
| Celestial progression | `DownedCelestialSolar`, `DownedCelestialVortex`, `DownedCelestialNebula`, `DownedCelestialStardust` |
| Seasonal events | `DownedHalloweenTree`, `DownedHalloweenKing`, `DownedChristmasTree`, `DownedChristmasQueen`, `DownedSanta` |
| Later additions | `DownedEmpressOfLight`, `DownedQueenSlime`, `DownedDeerclops`, `DownedDD2InvasionT1`, `DownedDD2InvasionT2`, `DownedDD2InvasionT3` |
| NPCs rescued | `SavedGoblin`, `SavedMech`, `SavedWizard`, `SavedStylist`, `SavedTaxCollector`, `SavedBartender`, `SavedAngler`, `SavedGolfer` |
| Pets bought | `BoughtCat`, `BoughtDog`, `BoughtBunny` |
| Spawn unlocks | `UnlockedMerchantSpawn`, `UnlockedDemolitionistSpawn`, `UnlockedPartyGirlSpawn`, `UnlockedDyeTraderSpawn`, `UnlockedTruffleSpawn`, `UnlockedArmsDealerSpawn`, `UnlockedNurseSpawn`, `UnlockedPrincessSpawn` |
| Town slime unlocks | `UnlockedSlimeBlueSpawn`, `UnlockedSlimeGreenSpawn`, `UnlockedSlimeOldSpawn`, `UnlockedSlimePurpleSpawn`, `UnlockedSlimeRainbowSpawn`, `UnlockedSlimeRedSpawn`, `UnlockedSlimeYellowSpawn`, `UnlockedSlimeCopperSpawn` |
| Permanent upgrades | `CombatBookUsed`, `CombatBookVolumeTwoWasUsed`, `PeddlersSatchelWasUsed` |
| Lantern night | `LanternNightGenuine`, `LanternNightManual`, `LanternNightNextNightIsGenuine` |
| Celebration and seasonal overrides | `PartyOfDoom`, `ForceHalloweenForToday`, `ForceHalloweenForever`, `ForceXMasForToday`, `ForceXMasForever` |
| Secret seeds | `DrunkWorld`, `GoodWorld`, `TenthAnniversaryWorld`, `DontStarveWorld`, `NotTheBeesWorld`, `RemixWorld`, `NoTrapsWorld`, `ZenithWorld`, `SkyblockWorld`, `InfectedSeed`, `VampireSeed`, `DualDungeonsSeed`, `TeamBasedSpawnsSeed`, `NoLightningSeed`, `MoreLightningSeed`, `Apocalypse` |

## Audit rules for future versions

When Terraria updates, check each property in this order:

1. Confirm the serialized type and version gate in Terraria `IO/WorldFile.cs` and TEdit `World.FileV2.cs`.
2. Search Terraria assignments, clamps, comparisons, and reset logic to find the effective range.
3. Compare those values with the XAML control, discrete option source, and any converter.
4. Keep sentinel values and inactive-state values such as negative cooldowns; they can be meaningful even when they look outside the active range.
5. Do not widen a control to the raw file-type limits unless Terraria safely handles that entire range.
