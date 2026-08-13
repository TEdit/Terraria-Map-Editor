# Morph Tool - Remaining Work

Historical issue context: #533 and the reports merged into it. Issue state does not determine the requirements below; user expectations, reversibility, and least-surprising behavior do.

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
- Added Safe conversion and Generate biome modes with independent target lists
- Made Safe conversion the default and kept its JSON non-destructive
- Restored the original broad conversion sets as explicit per-target Generate biome profiles

## Remaining Tasks

### P0 - Determinism, Reversibility, and Conversion Integrity

Safe conversion is a reversible biome-equivalence operation. Generate biome is an explicit material-changing operation that retains TEdit's broad legacy behavior. The safe-mode contract is:

> MORPH converts biome identity without destroying material identity. A tile, wall, or sprite is converted only when an explicit, complete, one-to-one source/target pair exists. Otherwise it remains byte-for-byte unchanged. Applying the original biome must deterministically recover the original member of the conversion family. Normal biome conversion must never delete content or use a lossy many-to-one fallback.

An explicit operation such as **Remove moss** or **Generate biome** may remove or replace content. Safe conversion must never infer destructive intent from a missing biome variant.

#### Authoritative biome data sources

- [ ] **Use the decompiled Terraria game code as the behavioral and classification authority.** Extract conversion targets, membership sets, and contextual rules from `WorldGen.Convert()`, `TileID.Sets.Conversion`, `WallID.Sets.Conversion`, and `BiomeConversionID` under `D:\dev\ai\tedit\_reference_sources\terraria-server-disassembly`. GitHub issues and hand-written catalogs are supporting evidence, not biome-data authorities.
- [ ] **Use TEdit's embedded JSON as the implementation authority.** The runtime inputs are `src/TEdit.Terraria/Data/morphBiomes.json` for morph rules and `src/TEdit.Terraria/Data/tiles.json` for tile, wall, sprite, frame, and solidity metadata. Do not validate against the unrelated root-level `tiles.json`; it is not identical to the embedded resource.
- [ ] **Generate a normalized conversion manifest from the game-code resources.** Record each conversion family, member ID, target biome, contextual predicate, tile/wall distinction, and framed UV/style rule. Keep the game version/source revision with the generated artifact so changes are reviewable.
- [ ] **Compare the normalized game manifest with both profiles in tests.** Compare safe `morphBiomes.json` and legacy generation `morphBiomes.generate.json`. Report missing biome-specific members, invalid target IDs/UVs, stale mappings, cross-family mappings, and TEdit-only extensions.
- [ ] **Do not copy destructive game behavior blindly.** Game code determines biome membership and counterpart identity; the MORPH safety contract still requires preservation when the game deletes a tile or provides no reversible counterpart.

#### Principle of least surprise and reversibility

- [ ] **Define conversion families by semantic role.** Examples are grass variants, stone variants, sand variants, ice variants, hardened-sand variants, sandstone variants, biome walls, vines, thorns, cave decorations, and framed sprite styles. A morph changes only the biome member of a family; it does not silently change the family itself.
- [ ] **Require a deterministic round trip for every supported pair.** For every family member `x` and target biome `B`, if `Morph(x, B) = y`, then morphing `y` back to the biome represented by `x` must return `x`. This must hold across save/reload and must not depend on edit history.
- [ ] **Reject lossy many-to-one mappings.** If dirt, mud, silt, clay, sand, or another distinct material would collapse to the same target, the original cannot be recovered from the world file. Such rules must be replaced with one-to-one family mappings or **Preserve**.
- [ ] **Do not infer origin from depth, neighboring tiles, current biome percentages, or conversion path.** Context may select among explicitly reversible variants within one family, but it must not erase which source family member was present.
- [ ] **Convert biome-specific content.** When an exact counterpart exists, leaving corruption/crimson/hallow/jungle/desert/snow biome-specific tiles or decorations behind is also a defect. Completeness and non-destruction are equal requirements.
- [x] **Separate destructive and material-changing behavior from safe conversion.** The Morph tool now defaults to Safe conversion and exposes the broad legacy rules only through **Generate biome (destructive)**. The UI warns that generation can change terrain and remove decorations.

#### Confirmed findings

- [ ] **Remove destructive fallback rules from normal biome morphs.** The current JSON contains 79 tile-level `delete` rules. It also has 28 sprite rules containing 64 deleting UV entries. These rules remove herbs, moss plants, grass decorations, thorns, vines, cacti, trees, altars/orbs for Jungle, and other sprites when no conversion is available.
- [ ] **Fix the path-dependent jungle-grass mappings reported in the video.** Hallow, Snow, and Desert currently map source IDs `60`, `70`, `661`, and `662` to moss `179` with `useMoss`; Forest maps the same sources to grass `2`. This is why Jungle→Desert can produce moss while Jungle→Forest→Desert produces sand. Define a real target equivalence or preserve the jungle grass unchanged; do not route it through moss.
- [ ] **Remove path-dependent substrate conversion.** Audit dirt, mud, clay, silt, sand, slush, snow, ash, and related blocks. A direct conversion and a chained conversion must not select different material families. In particular, do not use Forest/Jungle/Desert as implicit dirt↔mud↔sand material converters.
- [ ] **Reject duplicate or shadowed source rules.** `InitCache()` uses `TryAdd`, so the first rule silently wins. The raw Jungle wall list already claims wall IDs `74` and `80` twice (`wallGrass`/`wallMud` and their natural variants). Generated group rules can also be shadowed by hand-authored source-ID rules.
- [ ] **Use exact sprite variants, not a target sheet's default style.** Cross-tile `ApplySpriteReplacement()` currently stores only `TargetTileId` and places `targetSheet.Default`. Every mapping must identify the exact source anchor UV and exact target anchor UV/style.
- [ ] **Capture undo before any multi-cell mutation.** `ApplySpriteReplacement()` clears and places the full sprite before its returned extra locations are saved to undo. `GrowMossPlants()` likewise creates neighboring tiles before the caller saves them. Undo must snapshot the complete source and destination footprints first.
- [ ] **Verify the torch coordinate axis.** Torch styles in `tiles.json` are represented by frame UVs such as `[0, 396]`, while morph rules filter/offset `U` values such as `396`. Confirm TEdit's frame-axis convention with a fixture; correct the rule generator/data if the morph table is operating on the wrong axis.

#### Required data model and validation

- [ ] Replace implicit `delete` targets in biome equivalence groups with an explicit **Preserve** outcome. A missing target variant also means **Preserve**.
- [ ] Give every conversion-family member a stable semantic slot shared across biome targets. Reject a family when two distinct source slots resolve to the same target slot.
- [ ] Represent framed mappings as exact pairs:
  - `(source tile ID, source anchor U, source anchor V, source footprint)`
  - `(target tile ID, target anchor U, target anchor V, target footprint)`
- [ ] Require every sprite selected for conversion to have a mapping for every valid source style/UV. Partial UV coverage is a configuration error; it must not be completed with deletion or a default style.
- [ ] Validate `morphBiomes.json` during tests and startup:
  - all expected conversion families and members extracted from the supported Terraria game code are accounted for;
  - every configured tile/wall ID and framed UV exists in the embedded `src/TEdit.Terraria/Data/tiles.json`;
  - every intentional difference from `WorldGen.Convert()` is named and policy-tested;
  - no overlapping source IDs unless their UV filters are explicit, disjoint, and exhaustive;
  - every source UV and target UV resolves to a real frame in `tiles.json`;
  - offset ranges do not overlap and cannot produce an unknown UV;
  - every generated group rule is reachable in the final cache;
  - no normal biome rule has `delete: true` or a deleting sprite offset;
  - no normal biome rule changes material families;
  - every supported source/target pair has a unique inverse mapping;
  - no two distinct source members collapse into the same target member;
  - every unsupported source is preserved unchanged.
- [ ] Emit a validation report naming the biome, rule, tile ID, and UV for incomplete, overlapping, shadowed, or invalid mappings. Do not silently accept a first match.

#### Safe sprite conversion behavior

- [ ] Resolve a framed sprite from any tile in its footprint back to its anchor, then transform the sprite exactly once.
- [ ] Preflight the entire source and target footprints before mutation: bounds, support/anchor, collisions, containers, signs, and tile entities.
- [ ] Snapshot undo for every affected cell and metadata object before clearing or placing anything.
- [ ] Preserve orientation, on/off state, variety, and equivalent style when the target supplies them. If an equivalent style is absent, preserve the original sprite.
- [ ] If source metadata, target metadata, or a paired UV is missing, leave the entire source sprite unchanged and record a diagnostic; never partially convert or clear it.

#### Moss and exposed-edge behavior

- [ ] Keep moss blocks, moss bricks, and moss plants as separate equivalence groups. Jungle grass is not a moss fallback.
- [ ] For moss plant tile `184`, map every moss column, all four anchors (top, bottom, left, right), and all varieties. Preserve V/anchor/variety while changing only the moss column.
- [ ] Normal biome conversion must preserve existing moss plants when no explicit target exists. Only the explicit **Remove moss** option may delete them.
- [ ] Separate block conversion from optional decoration growth. `GrowMossPlants()` must not create plants outside the selection/mask or on cells not included in the pending undo transaction.
- [ ] Test exposed faces at the brush boundary, selection boundary, world boundary, slopes, half-blocks, occupied neighbors, and adjacent non-solid tiles. Define whether out-of-world neighbors count as air instead of relying on the current skipped-neighbor behavior.
- [ ] Reconcile the moss direction table comments with the actual V bands and test all four anchor directions against `tiles.json`.

#### Acceptance matrix

- [ ] For every target biome, enumerate every source rule against all valid tile IDs and framed UVs from `tiles.json`.
- [ ] Assert each input has exactly one outcome: **Convert to an exact valid target** or **Preserve unchanged**.
- [ ] For every converted tile, wall, and sprite, assert the round trip `source → target biome → source biome` restores the original ID, UV/style, footprint, orientation, state, and metadata.
- [ ] Add pairwise and chained property tests across every biome: the result must depend only on the current family member and requested target, never on the path used to reach it.
- [ ] Add dedicated fixtures proving dirt, mud, clay, silt, sand, slush, snow, ash, and their walls cannot undergo lossy cross-family conversion.
- [ ] Assert no normal biome morph reduces the count of active sprites solely because a target mapping is absent.
- [ ] Exercise direct and chained conversions, including Jungle→Desert and Jungle→Forest→Desert. Equivalent final targets must produce the same result; no difference may be caused by a missing rule, contextual fallback, default-frame fallback, or deletion.
- [ ] Verify save/reload and undo/redo preserve the exact converted or preserved result, including multi-tile metadata.
- [ ] Add focused fixtures for altars, orbs/hearts, herbs at all growth stages, cave spikes, torches, moss plants, vines, thorns, grass decorations, cacti/palms, and tree decorations.

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
- Safe data is in `TEdit.Terraria/Data/morphBiomes.json`; broad generation data is pinned in `morphBiomes.generate.json`
- `MorphMode.SafeConvert` is the default; `MorphMode.GenerateBiome` selects the destructive profile and target list
- MorphBiomeDataApplier accepts World as parameter (decoupled from WPF)
- Each biome has morphTiles and morphWalls arrays
- Torch conversions use spriteOffset system (tile 4, U = style * 22)
- Morph groups (morphGroups array in JSON) define equivalence classes across biomes
- Groups auto-expand into MorphId entries via MorphConfiguration.ExpandGroups()
- Hand-authored rules take precedence over group-generated rules (conflict check by source ID)
- Groups handle 3 cases: tile-ID swap (Case A), frame offset (Case B), mixed (Case C)
