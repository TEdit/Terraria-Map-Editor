# Morph Tool - Remaining Work

Related: #533

## Recently Completed
- Moved MorphBiomeDataApplier to TEdit.Editor (shared library, no WPF dependency)
- Fixed EnableBaseTiles checkbox (was ignored, now guards tile type conversion)
- Fixed CleanseWorldPlugin depth layers (all tiles were morphing at Sky level)
- Added torch style conversion (tile 4) to all biomes
- Added MossBrick tile conversion to all biomes
- Added ComputeMorphLevel static helper for consistent depth calculation
- Grow moss plant sprites (tile 184) on exposed faces when morphing stone to moss
- Correct moss plant V-axis anchoring and skip slopes/half blocks
- Added morph groups system for full-cycle biome equivalence classes
- Groups: Altars, Orbs, Thorns, Vines, CaveSpikes, Torches
- Groups auto-expand into MorphId entries at load time (no applier changes needed)

## Remaining Tasks

### High Priority
- [ ] Jungle biome morph (#815) - Add Jungle as a target biome in morphBiomes.json with jungle grass (60), mud walls, jungle torch (style 21, U=462), jungle vines, jungle thorn
- [ ] Masking system integration (#1824) - Morph tool should respect the mask system to restrict which tiles/biomes are affected. Prevents mushroom→jungle when purifying
- [ ] Cleanse World progress bar (#913) - CleanseWorldPlugin processes entire world with no progress feedback. Add progress callback

### Medium Priority
- [ ] Multi-tile sprite replacement for cross-tile-ID morph groups (altars/orbs need anchor detection + full footprint replacement)
- [ ] Tree morphing - Different tree tile IDs per biome. Use root location + biome type for conversion
- [ ] Dungeon biome conversion - Dungeon-specific walls and tiles
- [ ] Desert morph deco bug (#1819) - Some underground desert rock decos incorrectly convert to ice/mushroom variants when morphing to corruption/crimson
- [ ] Confirmation dialog for Cleanse World - Show affected tile count preview before processing
- [ ] Remove redundant hand-authored morph rules now covered by groups (Vines, Thorns, Torches, CaveSpikes)

### Low Priority / Future
- [ ] Custom from-to tile mappings - Allow users to define arbitrary tile conversion rules beyond biome presets
- [ ] Hive biome conversion - Hive blocks and honey
- [ ] Cave decoration handling (#1053) - Biome-specific cave decorations should morph with the biome

## Architecture Notes
- Morph code is in TEdit.Editor (shared lib, net10.0)
- Data is in TEdit.Terraria/Data/morphBiomes.json (embedded resource)
- MorphBiomeDataApplier accepts World as parameter (decoupled from WPF)
- Each biome has morphTiles and morphWalls arrays
- Torch conversions use spriteOffset system (tile 4, U = style * 22)
- Morph groups (morphGroups array in JSON) define equivalence classes across biomes
- Groups auto-expand into MorphId entries via MorphConfiguration.ExpandGroups()
- Hand-authored rules take precedence over group-generated rules (conflict check by source ID)
- Groups handle 3 cases: tile-ID swap (Case A), frame offset (Case B), mixed (Case C)
