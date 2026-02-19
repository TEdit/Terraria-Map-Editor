# Item Texture Rendering in Terraria

How Terraria loads and renders item textures (v1.4.5.4).

## Texture Loading

Every item has an entry in `TextureAssets.Item[]`. During initialization (`AssetInitializer.LoadTextures`), the game loads item textures:

```csharp
for (int i = 0; i < TextureAssets.Item.Length; i++)
{
    int copyFrom = ItemID.Sets.TextureCopyLoad[i];
    TextureAssets.Item[i] = copyFrom == -1
        ? LoadAsset<Texture2D>("Images/Item_" + i, mode)
        : TextureAssets.Item[copyFrom];  // share the Asset reference
}
```

When `TextureCopyLoad[id] != -1`, the item shares the **exact same texture asset** as the source item. No separate `Item_N.xnb` file exists for these items.

## TextureCopyLoad Mapping

~65 items use this system. The primary use case is **trapped chests** (`Fake_` prefix items) that reuse their regular chest counterpart's icon.

### Trapped Chests on Tile 441 (FakeContainers) — Items 3665-3706

These items have **no XNB files** of their own.

| Item ID | Key | Copies Texture From |
|---------|-----|-------------------|
| 3665 | Fake_Chest | 48 (Chest) |
| 3666 | Fake_GoldChest | 306 (Gold Chest) |
| 3667 | Fake_ShadowChest | 328 (Shadow Chest) |
| 3668 | Fake_EbonwoodChest | 625 |
| 3669 | Fake_RichMahoganyChest | 626 |
| 3670 | Fake_PearlwoodChest | 627 |
| 3671 | Fake_IvyChest | 680 |
| 3672 | Fake_IceChest | 681 |
| 3673 | Fake_LivingWoodChest | 831 |
| 3674 | Fake_SkywareChest | 838 |
| 3675 | Fake_ShadewoodChest | 914 |
| 3676 | Fake_WebCoveredChest | 952 |
| 3677 | Fake_LihzahrdChest | 1142 |
| 3678 | Fake_WaterChest | 1298 |
| 3679 | Fake_JungleChest | 1528 |
| 3680 | Fake_CorruptionChest | 1529 |
| 3681 | Fake_CrimsonChest | 1530 |
| 3682 | Fake_HallowedChest | 1531 |
| 3683 | Fake_FrozenChest | 1532 |
| 3684 | Fake_DynastyChest | 2230 |
| 3685 | Fake_HoneyChest | 2249 |
| 3686 | Fake_SteampunkChest | 2250 |
| 3687 | Fake_PalmWoodChest | 2526 |
| 3688 | Fake_MushroomChest | 2544 |
| 3689 | Fake_BorealWoodChest | 2559 |
| 3690 | Fake_SlimeChest | 2574 |
| 3691 | Fake_GreenDungeonChest | 2612 |
| 3692 | Fake_PinkDungeonChest | 2613 |
| 3693 | Fake_BlueDungeonChest | 2614 |
| 3694 | Fake_BoneChest | 2615 |
| 3695 | Fake_CactusChest | 2616 |
| 3696 | Fake_FleshChest | 2617 |
| 3697 | Fake_ObsidianChest | 2618 |
| 3698 | Fake_PumpkinChest | 2619 |
| 3699 | Fake_SpookyChest | 2620 |
| 3700 | Fake_GlassChest | 2748 |
| 3701 | Fake_MartianChest | 2814 |
| 3702 | Fake_MeteoriteChest | 3180 |
| 3703 | Fake_GraniteChest | 3125 |
| 3704 | Fake_MarbleChest | 3181 |
| 3705 | Fake_newchest1 | 3665 → 48 (chained) |
| 3706 | Fake_newchest2 | 3665 → 48 (chained) |

### Trapped Chests on Tile 468 (FakeContainers2) — With Own XNBs

These items have their **own unique XNB files** and are NOT in TextureCopyLoad:

| Item ID | Key | Has Own Texture |
|---------|-----|----------------|
| 3886 | Fake_CrystalChest | Yes (28×30) |
| 3887 | Fake_GoldenChest | Yes (32×30) |
| 3950 | Fake_SpiderChest | Yes (32×28) |
| 3976 | Fake_LesionChest | Yes (32×30) |
| 4164 | Fake_SolarChest | Yes (32×28) |
| 4185 | Fake_VortexChest | Yes (32×28) |
| 4206 | Fake_NebulaChest | Yes (32×28) |
| 4227 | Fake_StardustChest | Yes (32×28) |
| 4266 | Fake_GolfChest | Yes (32×30) |
| 4268 | Fake_DesertChest | Yes (32×30) |
| 4585 | Fake_BambooChest | Yes (32×28) |

### Trapped Chests on Tile 468 — Using TextureCopyLoad

| Item ID | Key | Copies From |
|---------|-----|-------------|
| 4713 | Fake_DungeonDesertChest | 4712 |
| 5167 | Fake_CoralChest | 5156 |
| 5188 | Fake_BalloonChest | 5177 |
| 5209 | Fake_AshWoodChest | 5198 |
| 5567 | Fake_AetheriumChest | 5556 |
| 5620 | Fake_FallenStarChest | 5609 |
| 5708 | Fake_FeywoodChest | 5697 |
| 5731 | Fake_HallowedFurnitureChest | 5720 |
| 5754 | Fake_GothicChest | 5745 |
| 5776 | Fake_DemoniteChest | 5763 |
| 5797 | Fake_CrimtaneChest | 5784 |
| 5818 | Fake_SnowChest | 5805 |
| 5839 | Fake_FlinxFurChest | 5826 |
| 5857 | Fake_PineChest | 5846 |
| 5878 | Fake_EasterChest | 5865 |
| 5897 | Fake_StoneChest | 5886 |
| 5918 | Fake_JellyfishChest | 5905 |
| 5952 | Fake_HarpyChest | 5939 |
| 5974 | Fake_CloudChest | 5962 |
| 5995 | Fake_MoonplateChest | 5982 |
| 6018 | Fake_LibrarianChest | 6005 |
| 6041 | Fake_SpikeChest | 6028 |
| 6064 | Fake_OfficeChest | 6051 |
| 6087 | Fake_ForbiddenChest | 6074 |
| 6131 | Fake_BoulderChest | 6118 |

## Item Animation System

Some items have multi-frame vertical strip textures. Registered in `Main.InitializeItemAnimations()`:

| Item ID(s) | Type | Frames | Notes |
|-----------|------|--------|-------|
| 75 | Fallen Star | 8 | PingPong animation |
| 520, 521, 547-549, 575 | Souls | 4 | 6 ticks/frame |
| 3453-3455 | Fragments | 4 | 6 ticks/frame |
| 3580, 3581 | Luminite/Fragment | 4 | 4-6 ticks/frame |
| 4068-4070 | (unknown) | 4 | NotActuallyAnimating (always frame 0) |
| All IsFood items | Food | 3 | int.MaxValue ticks (always frame 0) |
| 5644 | Scrying Orb | 9 | Custom DrawAnimationScryingOrb |

### Frame Extraction

`DrawAnimationVertical.GetFrame()` divides the texture height by `FrameCount`. Each frame has a **2-pixel gap** subtracted:

```
frameHeight = (texture.Height / frameCount) - 2
```

For static previews (inventory icons), always use **frame 0**.

No trapped chest items have animations.

## GetItemDrawFrame

```csharp
public static void GetItemDrawFrame(int item, out Texture2D itemTexture, out Rectangle itemFrame)
{
    Main.instance.LoadItem(item);
    itemTexture = TextureAssets.Item[item].Value;
    if (Main.itemAnimations[item] != null)
        itemFrame = Main.itemAnimations[item].GetFrame(itemTexture);
    else
        itemFrame = itemTexture.Frame();  // full texture
}
```

## Other Rendering Details

### TrapSigned Indicator

All trapped chest items have `ItemID.Sets.TrapSigned[type] = true`. The game draws a small red wire indicator (8×8 pixels from `Wires` texture at offset 4,58) in the corner of the item slot.

### ItemIconPulse

Items 520, 521, 547-549, 575, 3456-3459, 3580, 3581 pulse between 0.7× and 1.0× scale using `Main.essScale`. For static TEdit previews, render at 1.0×.

### Coins (71-74)

Use `TextureAssets.Coin[index]` (separate `Images/Coin_N` assets), not `TextureAssets.Item[71-74]`. 8-frame animations.

### Glow Masks

If `item.glowMask != -1`, `TextureAssets.GlowMask[glowMask]` is overlaid additively.

## TEdit Implementation

To match Terraria's behavior, TEdit needs:

1. **`textureId` field on items** — When `TextureCopyLoad[id] != -1`, store the source item ID. At render time, load `Item_{textureId}.xnb` instead of `Item_{id}.xnb`.

2. **Animation frame extraction** — For animated items (food, souls, etc.), only render frame 0 with height = `(texture.Height / frameCount) - 2`.

3. **Chained aliases** — Items 3705/3706 point to 3665, which points to 48. Resolve transitively.
