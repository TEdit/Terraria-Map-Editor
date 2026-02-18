# TEdit Rendering TODO - Gap Analysis

Comparing TEdit's current implementation against [custom-rendered-tiles.md](custom-rendered-tiles.md) to identify rendering gaps.

## Comparison Summary

| Section | Reference Doc | TEdit Status | Gap |
|---------|---------------|--------------|-----|
| 2. Basic/Blendable Tiles | Complete spec | IMPLEMENTED | None |
| 3. Tree Rendering | Normal + Gem + Vanity + Ash | IMPLEMENTED | Gem (583-589), Vanity (596, 616), Ash (634) all done |
| 4. Palm Tree Rendering | Complete spec | IMPLEMENTED | None |
| 5. Grass & Plant Rendering | Height overrides, wind, flip | IMPLEMENTED | Sprite flip done; wind animation N/A for map editor |
| 6. Vine Rendering | tileTop=-2, sprite flip, crawl | IMPLEMENTED | -2px offset and flip on alternating X |
| 7. Cactus Rendering | Biome variants | IMPLEMENTED | None |
| 8. Platform Rendering | Edge detection | IMPLEMENTED | None |
| 9. Torch & Flame Rendering | Particle system | PARTIAL | Missing flame particles (low priority for map editor) |
| 10. Christmas Tree Rendering | Multi-layer | IMPLEMENTED | None |
| 11. Minecart Track Rendering | Multi-component | IMPLEMENTED | None |
| 12. Animal Cage Rendering | Animated creatures | N/A | Static display, no animation in map editor |
| 13. Display Tile Rendering | Mannequin, Rack, Frame, Platter | IMPLEMENTED | Body, armor, pose, hat rack, weapon rack, kite, food platter |
| 14. Glow Mask Tiles | 25+ tiles with glow | NOT IMPLEMENTED | Needs glow texture loading + additive blend pass |
| 15. Slope & Half-Brick | Sliced rendering | IMPLEMENTED | None |
| 16. Special Cases | Various | PARTIAL | Some implemented |

---

## Detailed Gaps

### 1. Gem Trees (Section 3)

**Missing Tiles:**

- 583 Topaz, 584 Amethyst, 585 Sapphire, 586 Emerald, 587 Ruby, 588 Diamond, 589 Amber
- 596 Sakura, 616 Willow (Vanity)
- 634 Ash Tree (with glow mask)

**Implementation:** Same pattern as normal trees - detect frameY >= 198, frameX 22/44/66 for top/left/right. Need treeStyle mapping.

---

### 2. Vine Rendering (Section 6)

Key requirements:

```csharp
tileTop = -2;  // Vertical offset for types 52, 62, 115, 205, 382, 528, 636, 638

// Sprite flip for variety
if (x % 2 == 0)
    tileSpriteEffect = SpriteEffects.FlipHorizontally;
```

**Vine Tile IDs:** 52, 62, 115, 205, 382, 528, 636, 638

**ReverseVine:** 549

**MultiTileVine:** 34, 42, 91, 95, 126, 270, 271, 444, 454, 465, 487, 528, 572, 581, 591, 592, 636, 638, 660, 698, 709

**Implementation:**

1. Add HashSet for vine tile IDs
2. Apply -2px Y offset
3. Apply horizontal flip when x % 2 == 0

---

### 3. Display Doll / Hat Rack (Section 13)

#### Display Doll (470) - Current Issue

Uses `GetItem(itemId)` instead of `GetArmorHead/Body/Legs(armorSlot)`

**Fix:** Check ItemProperty.Head/Body/Legs to get armor slot, then use appropriate armor texture.

#### Hat Rack (475) - NOT IMPLEMENTED

TEdit has empty break statement.

**Implementation:**

1. Find TileEntity at anchor position
2. Load Items[0-1] for two hat slots
3. Check ItemProperty.Head for armor slot
4. Position at rack peg locations

---

### 4. Glow Mask Tiles (Section 14)

Glow color constants:

```csharp
_meteorGlow = new Color(100, 100, 100, 0);
_lavaMossGlow = new Color(150, 100, 50, 0);
_kryptonMossGlow = new Color(0, 200, 0, 0);
_xenonMossGlow = new Color(0, 180, 250, 0);
_argonMossGlow = new Color(225, 0, 125, 0);
_violetMossGlow = new Color(150, 0, 250, 0);
```

**Key tiles needing glow:**

| ID | Name | Glow Color |
|----|------|------------|
| 129 | Crystal Ball | Animated rainbow |
| 381, 517, 687 | Lava Moss | Orange-yellow |
| 534, 535, 689 | Krypton Moss | Neon green |
| 536, 537, 690 | Xenon Moss | Cyan |
| 539, 540, 688 | Argon Moss | Purple-magenta |
| 625, 626, 691 | Violet Moss | Violet |
| 627, 628, 692 | Disco Moss | Disco color cycle |
| 634 | Ash Tree | White glow mask |

**Implementation:** Requires new texture loading (Glow_*.xnb), tile-to-glowMask mapping, additive blend rendering pass.

---

### 5. Position Offset Tiles

| Tile ID | Name | Offset | Condition |
|---------|------|--------|-----------|
| 51, 697 | Cobweb/Titanstone | Light * 0.5f | Dimmed rendering |
| 114 | Tiki Torch | Height += 2px | frameY > 0 |
| 136 | Switch | X +/-2px | Based on frameX/18 |
| 442 | Item Pedestal | X += 2px | frameX/22 == 3 |
| 723, 724 | Echo Block | (0,+/-2), (+/-2,0) | Based on frame |
| 751 | Terragrim Pedestal | (+11, -8) | Anchor frame |
| 752 | Seedling | (+8, 0) | Anchor frame |

---

### 6. Grass & Plant Rendering (Section 5)

Height overrides to verify:

| Tile Types | tileHeight | tileTop |
|------------|------------|---------|
| 3, 24, 61, 71, 110, 201, 637, 703 | 20 | 0 |
| 73, 74, 113 | 32 | -12 |
| 82, 83, 84 | 20 | -2 |

Sprite flip (x % 2 == 0) affects: 3, 20, 24, 52, 61, 62, 71, 73, 74, 81, 82, 83, 84, 110, 113, 115, 127, 201, 205, 227, 270, 271, 382, 528, 572, 581, 590, 595, 637, 638, 703

---

### 7. Chest Selection Highlight (Editor Feature)

TEdit has `SelectedChest` property but no visual indication when chest is selected for editing.

**Implementation:** Draw highlight overlay when current chest position matches SelectedChest.

---

## Implementation Priority

| Priority | Feature | Complexity | Status |
|----------|---------|------------|--------|
| 1 | Vine offset & flip | LOW | DONE |
| 2 | Grass sprite flip | LOW | DONE |
| 3 | Position offset tiles | LOW | DONE |
| 4 | Chest selection highlight | LOW | DONE |
| 5 | Gem Trees | MEDIUM | DONE |
| 6 | Display Doll (armor + body + pose) | MEDIUM | DONE |
| 7 | Hat Rack | MEDIUM | DONE |
| 8 | Vanity Trees (596, 616) | MEDIUM | DONE |
| 9 | UV cache reset for all paint modes | LOW | DONE |
| 10 | Glow Mask System | HIGH | NOT STARTED |

---

## Files to Modify

- `src/TEdit/View/WorldRenderXna.xaml.cs` - Main rendering logic
- `src/TEdit/Render/Textures.cs` - Add glow mask loading (for glow feature)

---

## Verification

1. **Vines**: Place vines, verify -2px offset and alternating flip
2. **Grass**: Verify height overrides match reference table
3. **Gem Trees**: Create worlds with gem trees, check rendering
4. **Display Doll**: Place mannequin with armor, verify correct armor textures
5. **Hat Rack**: Place hat rack with hats
6. **Glow Masks**: Check moss tiles, Ash tree for glow effect
