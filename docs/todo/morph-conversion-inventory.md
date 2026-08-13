# Morph conversion inventory

This inventory defines the safe conversion contract implemented by
`src/TEdit.Terraria/Data/morphBiomes.json`. The original broad conversion sets
remain available through the explicit Generate biome mode and the
`src/TEdit.Terraria/Data/morphBiomes.generate.json` legacy profile.

Terraria's `WorldGen.Convert` implementation and its `TileID`/`WallID` conversion
sets are the authority for identifying biome counterpart families. TEdit's JSON
is the authority for editor behavior. TEdit intentionally narrows Terraria's
behavior where the game deletes an object, chooses a result from position or
randomness, or collapses multiple source families into one result.

## Safe conversion policy

- Convert a tile, wall, or framed sprite only when the target biome has a known
  counterpart in the same family.
- Preserve the source when no exact counterpart exists.
- Never delete content as a side effect of a normal biome morph.
- Keep the JSON and runtime delete capabilities available for explicit future
  operations; normal morph conversion sets simply don't enable them.
- A source ID can resolve to only one rule in a biome. Rule order must not decide
  the result.
- A supported conversion must have a deterministic path back through the source
  biome. Depth, adjacency, randomness, and current moss selection can't be
  required to recover the source family.

## Generate biome policy

- Generate biome is an explicit destructive mode. It can change material
  families and remove decorations.
- One canonical embedded JSON profile contains every generation target and the
  original broad TEdit conversion sets.
- A pinned canonical JSON SHA-256 regression test verifies every value in the
  reviewed legacy profile while ignoring formatting and line-ending changes.
- Undo is the supported way to recover the original terrain. Generation doesn't
  promise a deterministic reverse conversion after the world is saved.

## Tile counterpart families

| Family | Canonical IDs |
| --- | --- |
| Grass | Forest 2, Corruption 23, Crimson 199, Hallow 109 |
| Golf grass | Forest 477, Hallow 492 |
| Jungle grass | Jungle 60, Mushroom 70, Corruption 661, Crimson 662 |
| Stone | Pure 1, Corruption 25, Hallow 117, Crimson 203 |
| Ice | Pure 161, Corruption 163, Hallow 164, Crimson 200 |
| Sand | Pure 53, Corruption 112, Hallow 116, Crimson 234 |
| Hardened sand | Pure 397, Corruption 398, Crimson 399, Hallow 402 |
| Sandstone | Pure 396, Corruption 400, Crimson 401, Hallow 403 |

Dirt, mud, silt, snow, slush, moss, and moss brick are separate families. They
aren't interchangeable fallback materials. In particular, morphing no longer
uses mud/dirt/silt, moss, depth, adjacency, or the selected moss type to make a
many-to-one substrate conversion.

## Wall counterpart families

Covered wall families include natural and safe grass, biome stone, four natural
and four safe biome-rock variants, hardened sand, and sandstone. Natural/safe
variants remain distinct. Dirt, mud, snow, ice, old stone, craggy stone, flower,
and mushroom walls are preserved when an exact counterpart isn't available.

Mushroom walls 74 and 80 are deliberately not used as generic targets. Terraria
can produce them from several source wall families, so the resulting wall can't
deterministically identify which family should be restored.

## Framed sprite counterparts

The shared JSON morph groups are the complete counterpart sets for altars, orbs,
thorns, vines, cave spikes, and torches. Altars no longer map to forges, furnaces,
or campfires; orbs no longer map to crystal hearts, crystal balls, or disco balls.
Flower vines and Plantera thorns are also excluded from the vines and biome-thorn
families.

Any other framed sprite is preserved unless its JSON rule has an exact tile and
frame/UV counterpart. This avoids fabricating a sprite or deleting it when an
appropriate frame mapping is missing.

## Executable coverage and resolved gaps

`MorphConfigurationTests` inventories and enforces the contract. The tests were
first run against the previous JSON and exposed these gaps:

- 119 delete-enabled normal morph entries.
- 171 cross-family tile mappings.
- 60 missing canonical wall conversions after unsafe wall mappings were removed.
- 18 asymmetric Jungle wall conversions with no Forest/Hallow return path.
- Ambiguous duplicate Jungle wall sources 74 and 80.
- Incomplete or unrelated altar, orb, vine, and thorn sprite sets.
- 69 missing exact sprite replacements/UV transforms in shared counterpart groups.
- Eight one-way grassy-stone sprite transforms without a complete counterpart family.
- 14 missing Forest tile counterparts.

The safe-profile gates require non-destructive rules, unique source resolution,
same-family tile and wall mappings, complete canonical counterpart coverage,
complete vetted sprite groups, exact cave-spike UV families, and exhaustive
return paths for every configured canonical tile and wall mapping. Separate
tests require independent mode configurations and target lists, confirm that
generation retains broad Snow and Desert terrain conversion, and verify that
the generation profile retains deletion-capable rules.

## Preserved follow-up areas

- Tree-branch horizontal mirroring still needs verified paired UV coordinates.
- Tree-branch vertical mirroring needs a separate growth-direction design.
- Newly added Terraria or modded IDs remain unchanged until they are represented
  by an explicit JSON counterpart family. This is the safe behavior for modded
  worlds and prevents vanilla assumptions from destroying unknown content.
