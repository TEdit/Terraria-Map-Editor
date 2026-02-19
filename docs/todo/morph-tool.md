# Morph Tool - Remaining Work

Related: #533

## Recently Completed
- Moved MorphBiomeDataApplier to TEdit.Editor (shared library, no WPF dependency)
- Fixed EnableBaseTiles checkbox (was ignored, now guards tile type conversion)
- Fixed CleanseWorldPlugin depth layers (all tiles were morphing at Sky level)
- Added torch style conversion (tile 4) to all biomes
- Added MossBrick tile conversion to all biomes
- Added ComputeMorphLevel static helper for consistent depth calculation

## Remaining Tasks

### High Priority
- [ ] Jungle biome morph (#815) - Add Jungle as a target biome in morphBiomes.json with jungle grass (60), mud walls, jungle torch (style 21, U=462), jungle vines, jungle thorn
- [ ] Masking system integration (#1824) - Morph tool should respect the mask system to restrict which tiles/biomes are affected. Prevents mushroom→jungle when purifying
- [ ] Cleanse World progress bar (#913) - CleanseWorldPlugin processes entire world with no progress feedback. Add progress callback

### Medium Priority
- [ ] Tree morphing - Different tree tile IDs per biome. Use root location + biome type for conversion
- [ ] Dungeon biome conversion - Dungeon-specific walls and tiles
- [ ] Desert morph deco bug (#1819) - Some underground desert rock decos incorrectly convert to ice/mushroom variants when morphing to corruption/crimson
- [ ] Confirmation dialog for Cleanse World - Show affected tile count preview before processing

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
