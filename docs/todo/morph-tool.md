# Morph Tool Improvements

Related issues: #533 (primary), merged: #1824, #1819, #1053, #815, #913, #1312, #1325, #1498

## Overview

Overhaul the morph tool to support customizable biome conversions, masking, and comprehensive tile coverage.

## Implementation Approach

The morph tool should use the **in-game Clentaminator algorithm** as the reference implementation for biome conversion logic. This ensures conversions match what players expect from Terraria's own purification/corruption mechanics. See: https://terraria.wiki.gg/wiki/Clentaminator

## Requirements

### Customizable From-To Tiles
- Allow users to define custom tile conversion mappings (source tile → target tile)
- Built-in presets for standard biome morphs
- User-editable conversion tables

### Biome Morph Improvements
- Preserve jungle tiles when purifying corrupt/crimson (don't convert jungle → forest)
- Isolate morph to target biome only — don't convert non-corrupt biomes (#1325)
- Ice blocks and special cave walls
- Hardened sand conversions
- Fix corrupted plants remaining after purify (#1498)
- Prevent morph purify from deleting items on mannequins/item displays
- Golf grass handling (#1312)

### Desert Morph Fix (from #1819)
When morphing underground desert to corruption/crimson, certain rock decos are turned into ice or mushroom cave decos. This doesn't occur when morphing to hallow. Unchecking base tiles, moss, evil, or decorations doesn't help.

![Desert morph bug 1](https://user-images.githubusercontent.com/74626960/208173461-f88f028b-3c9c-433d-b9bd-71e969c1702d.png)
![Desert morph bug 2](https://user-images.githubusercontent.com/74626960/208173479-b3203b90-5be2-440b-aca3-3ce9c98c8d6d.png)

### Morph Masking (from #1824)
- Morph tool must respect the decoupled mask system (see masking-system.md)
- Example problem: purifying evil biomes also changes mushroom biomes to jungle
- Mask allows user to restrict which tiles/biomes are affected

### Additional Biome Support
- Custom biomes: jungle, snow, hive, dungeon
- Tree morphing: use root location + biome type to morph trees to appropriate random presets
- More morph biome combinations (#815)

### Cleanse World (#913)
- "Cleanse World" button that applies purify morph to the entire world
- Should respect current mask settings
- Confirmation dialog with preview of affected tile count

### Cave Decoration Handling (#1053)
- Converting crimson/corruption/hallow back to natural form must also convert cave decorations
- Underground biome-specific decorations should morph with the biome
