# Player Preview Renderer - Technical Reference

This document describes how TEdit renders player character previews, based on analysis of Terraria 1.4.5.4 and the TEdit implementation in `src/TEdit/Render/PlayerPreviewRenderer.cs`.

## Texture Assets

### Body Part Textures

Each skin variant has up to 16 body part textures stored as XNB files:

```
Images/Player_{skinVariant}_{partIndex}.xnb
```

| Part | Name               | Tint Color       | Description                          |
|------|--------------------|------------------|--------------------------------------|
| 0    | Head skin          | skinColor        | Head/face outline and skin           |
| 1    | Eye whites         | white (no tint)  | White part of eyes                   |
| 2    | Eye irises         | eyeColor         | Colored iris portion                 |
| 3    | Torso skin         | skinColor        | Chest/torso skin layer               |
| 4    | Undershirt         | underShirtColor  | Shirt worn under armor               |
| 5    | Hands (skin)       | skinColor        | Front hand/wrist skin                |
| 6    | Shirt overlay      | shirtColor       | Shirt color overlay on torso         |
| 7    | Back arm skin      | skinColor        | Rear arm skin (drawn behind body)    |
| 8    | Undershirt sleeves | underShirtColor  | Sleeves matching undershirt          |
| 9    | Front arm skin     | skinColor        | Front arm/hand skin                  |
| 10   | Legs skin          | skinColor        | Bare leg skin layer                  |
| 11   | Pants              | pantsColor       | Pants overlay on legs                |
| 12   | Shoes              | shoeColor        | Shoe/boot layer                      |
| 13   | Shirt sleeves      | shirtColor       | Sleeves matching shirt               |
| 14   | Long coat/dress    | shirtColor       | Only for variants 3, 7, 8           |
| 15   | Eyelid             | skinColor        | Blinking eyelid overlay              |

### Hair Textures

Hair textures are **1-indexed** in the content files:

```
Images/Player_Hair_{hairIndex + 1}.xnb
```

Hair style 0 loads from `Player_Hair_1.xnb`, style 1 from `Player_Hair_2.xnb`, etc.

There are 228 hair styles (indices 0-227). Hair is tinted with `hairColor`.

## Skin Variants

There are 12 skin variants (0-11), though TEdit's player editor exposes variants 0-9. Variants 10-11 are Display Dolls (mannequins).

### Variant Names

| Index | Name                | Base   | Gender |
|-------|---------------------|--------|--------|
| 0     | Male (Default)      | -      | Male   |
| 1     | Male (Sticker)      | 0      | Male   |
| 2     | Male (Gangster)     | 0      | Male   |
| 3     | Male (Coat)         | 0      | Male   |
| 4     | Female (Default)    | 0      | Female |
| 5     | Female (Sticker)    | 4      | Female |
| 6     | Female (Gangster)   | 4      | Female |
| 7     | Female (Coat)       | 4      | Female |
| 8     | Dress               | 0      | Male   |
| 9     | Female (Dress)      | 4      | Female |
| 10    | Display Doll Male   | 0      | Male   |
| 11    | Display Doll Female | 10     | Female |

Gender is defined by `PlayerVariantID.Sets.Male` in Terraria source:
- **Male**: 0, 1, 2, 3, **8**, 10
- **Female**: 4, 5, 6, 7, **9**, 11

Note: Variant 8 (Dress) is **male**, variant 9 (Female Dress) is **female**.

### Texture Inheritance (CopyVariant)

Terraria's `PlayerDataInitializer` uses a `CopyVariant(to, from)` pattern where a variant first copies ALL 16 texture slots from a base variant, then `LoadVariant` overrides specific parts with variant-specific textures.

**Variant 0 (Starter Male)** - Base, loads from disk:
- Parts loaded: 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 15
- Part 14 = `Asset<Texture2D>.Empty` (no coat)

**Variant 1 (Sticker Male)**:
- `CopyVariant(1, 0)` then override: 4, 6, 8, 11, 12, 13

**Variant 2 (Gangster Male)**:
- `CopyVariant(2, 0)` then override: 4, 6, 8, 11, 12, 13

**Variant 3 (Coat Male)**:
- `CopyVariant(3, 0)` then override: 4, 6, 8, 11, 12, 13, **14**

**Variant 4 (Starter Female)**:
- `CopyVariant(4, 0)` then override: 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13
- Inherits head (0), eye whites (1), eye irises (2), eyelid (15) from variant 0

**Variant 5 (Sticker Female)**:
- `CopyVariant(5, 4)` then override: 4, 6, 8, 11, 12, 13

**Variant 6 (Gangster Female)**:
- `CopyVariant(6, 4)` then override: 4, 6, 8, 11, 12, 13

**Variant 7 (Coat Female)**:
- `CopyVariant(7, 4)` then override: 4, 6, 8, 11, 12, 13, **14**

**Variant 8 (Dress)**:
- `CopyVariant(8, 0)` then override: 4, 6, 8, 11, 12, 13, **14**

**Variant 9 (Dress Female)**:
- `CopyVariant(9, 4)` then override: 4, 6, 8, 11, 12, 13

**Variant 10 (Display Doll Male)** - Special:
- `CopyVariant(10, 0)` then load: 0, 2, 3, 5, 7, 9, 10
- Then assign `Player_10_2` texture to parts: 1, 2, 4, 6, 8, 11, 12, 13, 15

**Variant 11 (Display Doll Female)** - Special:
- `CopyVariant(11, 10)` then load: 3, 5, 7, 9, 10
- Then assign `Player_10_2` (variant 10's part 2) to parts: 1, 2, 4, 6, 8, 11, 12, 13, 15

### Texture Files on Disk per Variant

Not all parts have dedicated files — missing parts inherit from the base variant via `CopyVariant`:

| Variant | Files on disk (part indices)         |
|---------|--------------------------------------|
| 0       | 0-13, 15 (14 is empty)              |
| 1       | 4, 6, 8, 11, 12, 13                 |
| 2       | 4, 6, 8, 11, 12, 13                 |
| 3       | 4, 6, 8, 11, 12, 13, 14             |
| 4       | 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13|
| 5       | 4, 6, 8, 11, 12, 13                 |
| 6       | 4, 6, 8, 11, 12, 13                 |
| 7       | 4, 6, 8, 11, 12, 13, 14             |
| 8       | 4, 6, 8, 11, 12, 13, 14             |
| 9       | 4, 6, 8, 11, 12, 13                 |
| 10      | 0, 2, 3, 5, 7, 9, 10                |
| 11      | 3, 5, 7, 9, 10                      |

### TEdit Fallback Chain

Since TEdit loads textures individually from disk (not using Terraria's runtime `CopyVariant`), `Textures.GetPlayerBody()` implements a fallback chain:

```
variant → base variant → variant 0
```

Mapping:
- Variants 1, 2, 3, 8 → fall back to variant 0
- Variants 5, 6, 7, 9 → fall back to variant 4 → then variant 0
- Variant 4 → fall back to variant 0
- Variant 11 → fall back to variant 10 → then variant 0

## Sprite Sheet Format — Composite Frame System

### Overview

Terraria 1.4+ uses a **composite frame system** for body rendering. Body part sprite sheets are organized as a **2D grid** of 40×56 frames, not a simple vertical strip. Each frame is at grid position `(X, Y)` mapped to pixel coordinates `(X * 40, Y * 56)`.

For **unarmored players** (`body < 1`), composite rendering is **always** used — see `PlayerDrawSet.CreateCompositeData()`.

### Frame Positions

```csharp
CreateCompositeFrameRect(Point pt) => new Rectangle(pt.X * 40, pt.Y * 56, 40, 56);
```

For the **standing pose** (targetFrameNumber = 0):

| Frame Type       | Grid (Male) | Pixels (Male) | Grid (Female) | Pixels (Female) |
|------------------|-------------|---------------|---------------|-----------------|
| Torso            | (0, 0)      | (0, 0)        | (0, 2)        | (0, 112)        |
| Front Shoulder   | (0, 1)      | (0, 56)       | (0, 3)        | (0, 168)        |
| Back Shoulder    | (1, 1)      | (40, 56)      | (1, 3)        | (40, 168)       |

The gender offset is applied in `CreateCompositeData()`:

```csharp
if (!this.drawPlayer.Male)
{
    pt1.Y += 2;  // back shoulder
    pt2.Y += 2;  // front shoulder
    pt3.Y += 2;  // torso
}
```

### Which Parts Use Composite Frames

| Parts       | Frame Type   | Notes                                              |
|-------------|--------------|----------------------------------------------------|
| 3           | compTorsoFrame | Torso skin — `DrawPlayer_12_Skin_Composite`      |
| 4, 6        | compBackShoulderFrame + compTorsoFrame | Undershirt/shirt clothing drawn **twice** — `DrawPlayer_17_TorsoComposite` |
| 5           | compTorsoFrame | Hands skin (integrated into torso in composite mode) |
| 0, 1, 2, 15 | bodyFrame (traditional) | Head, eyes, eyelid — not composite |
| 10, 11, 12  | legFrame (traditional) | Legs, pants, shoes — not composite |
| 7, 8, 9, 13 | Traditional frame 0 | Arms/sleeves — composite arm frames exist but TEdit uses traditional frame 0 successfully |

### Why Male Characters Worked Without Composite Awareness

For male characters, the composite standing torso frame is at grid **(0, 0)** — the exact same pixel position as the traditional frame 0. So reading from `(0, 0, 40, 56)` happened to be correct.

For female characters, the composite standing torso frame is at grid **(0, 2)** = pixel `(0, 112)`. Reading from `(0, 0)` read the wrong (empty/different) frame data, causing the missing central body.

## Pixel Format

### XNA Texture Format

XNA `Texture2D.GetData<int>()` returns pixels packed as **ABGR**:

```
int pixel = (A << 24) | (B << 16) | (G << 8) | R
```

XNA textures use **premultiplied alpha**. Before tinting, un-premultiply:

```csharp
if (srcA < 255 && srcA > 0)
{
    srcR = Math.Min(255, srcR * 255 / srcA);
    srcG = Math.Min(255, srcG * 255 / srcA);
    srcB = Math.Min(255, srcB * 255 / srcA);
}
```

### WPF Bitmap Format

`WriteableBitmap` with `PixelFormats.Bgra32` stores pixels as **ARGB** (in memory as BGRA byte order):

```
int pixel = (A << 24) | (R << 16) | (G << 8) | B
```

### Color Key Removal

`Textures.LoadTextureImmediate()` performs legacy chroma key removal: magenta pixels (`#FF00FF` / `Color(255, 0, 255)`) are replaced with transparent.

## Draw Order

### Terraria's Full Draw Order

From `LegacyPlayerRenderer.DrawPlayer_UseNormalLayers()`, the complete draw order (back to front):

1. `DrawPlayer_extra_TorsoPlus` - torso offset
2. `DrawPlayer_01_2_JimsCloak` - Jim's cloak
3. `DrawPlayer_02_MountBehindPlayer` - mount (behind)
4. `DrawPlayer_03_Carpet` / `PortableStool`
5. `DrawPlayer_04_ElectrifiedDebuffBack`
6. `DrawPlayer_05_ForbiddenSetRing` / `SafemanSun`
7. `DrawPlayer_06_WebbedDebuffBack`
8. `DrawPlayer_07_LeinforsHairShampoo`
9. `DrawPlayer_08_Backpacks`
10. `DrawPlayer_08_1_Tails`
11. `DrawPlayer_09_Wings`
12. `DrawPlayer_01_BackHair` - back hair
13. `DrawPlayer_10_BackAcc` - back accessory
14. `DrawPlayer_01_3_BackHead` - back of head
15. `DrawPlayer_11_Balloons`
16. `DrawPlayer_27_HeldItem` (if behind back arm)
17. `DrawPlayer_13_ArmorBackCoat` - back coat armor
18. **`DrawPlayer_12_Skin`** - torso skin (part 3) + legs skin (part 10)
19. **`DrawPlayer_13_Leggings`** - pants (part 11) + shoes (part 12) or armor legs
20. **`DrawPlayer_14_Shoes`** - shoe accessory
21. **`DrawPlayer_15_SkinLongCoat`** - long coat skin (part 14), variants 3/7/8 only
22. `DrawPlayer_16_ArmorLongCoat` - armor long coat
23. **`DrawPlayer_17_Torso`** - undershirt (part 4) + shirt (part 6) + hands (part 5) or armor body
24. `DrawPlayer_18_OffhandAcc`
25. `DrawPlayer_19_WaistAcc`
26. `DrawPlayer_20_NeckAcc`
27. **`DrawPlayer_21_Head`** - head skin (part 0) + eyes (parts 1, 2) + eyelid (part 15) + hair + helmet
28. `DrawPlayer_22_FaceAcc`
29. `DrawPlayer_23_MountFront`
30. `DrawPlayer_25_Shield`
31. `DrawPlayer_26_SolarShield`
32. `DrawPlayer_27_HeldItem` (if behind front arm)
33. **`DrawPlayer_28_ArmOverItem`** - back arm (part 7) + undershirt sleeves (part 8) + shirt sleeves (part 13) or armor arm + front arm (part 9)
34. `DrawPlayer_29_OnhandAcc`
35. `DrawPlayer_30_BladedGlove`
36. `DrawPlayer_32_FrontAcc_FrontPart`
37. `DrawPlayer_27_HeldItem` (if over front arm)
38. Various buff effects (frozen, electrified, ice barrier, beetle, etc.)

### TEdit Simplified Draw Order

For the standing preview (no armor, no mount, no accessories), TEdit draws using composite-aware frame positions. Parts marked with `[C]` use composite frame offsets (gender-dependent):

```
1.  Back arm skin       (part 7)  → skinColor           frame (0, 0)
2.  Undershirt sleeves  (part 8)  → underShirtColor     frame (0, 0)
3.  Shirt sleeves       (part 13) → shirtColor          frame (0, 0)
4.  Legs skin           (part 10) → skinColor           frame (0, 0)
5.  Shoes               (part 12) → shoeColor           frame (0, 0)
6.  Pants               (part 11) → pantsColor          frame (0, 0)
7.  Long coat/dress     (part 14) → shirtColor          frame (0, 0) (skipped if missing)
8.  Torso skin          (part 3)  → skinColor       [C] compTorsoFrame
9.  Undershirt shoulder (part 4)  → underShirtColor [C] compBackShoulderFrame
10. Shirt shoulder      (part 6)  → shirtColor      [C] compBackShoulderFrame
11. Undershirt torso    (part 4)  → underShirtColor [C] compTorsoFrame
12. Shirt torso         (part 6)  → shirtColor      [C] compTorsoFrame
13. Hands skin          (part 5)  → skinColor       [C] compTorsoFrame
14. Head skin           (part 0)  → skinColor           frame (0, 0)
15. Eye whites          (part 1)  → no tint (white)     frame (0, 0)
16. Eye irises          (part 2)  → eyeColor            frame (0, 0)
17. Eyelid              (part 15) → skinColor           frame (0, 0)
18. Hair                          → hairColor           frame (0, 0)
19. Front arm skin      (part 9)  → skinColor           frame (0, 0)
```

Undershirt (part 4) and shirt (part 6) are each drawn **twice** — once with the shoulder frame and once with the torso frame — matching Terraria's `DrawPlayer_17_TorsoComposite` behavior. This provides full clothing coverage across both the shoulder and chest regions.

## Terraria Draw Layer Details

### DrawPlayer_12_Skin / DrawPlayer_12_Skin_Composite

When `usesCompositeTorso` is true (always true for unarmored players):
- **Part 3** (torso): drawn with `compTorsoFrame` tinted with `colorBodySkin`
- **Part 10** (legs): drawn with `legFrame` tinted with `colorLegs`

When `usesCompositeTorso` is false (only with older armor that doesn't use composite):
- **Part 3** (torso): drawn with `bodyFrame` tinted with `colorBodySkin`
- **Part 10** (legs): drawn with `legFrame` tinted with `colorLegs`

### DrawPlayer_13_Leggings

When no leg armor is equipped, draws:
- **Part 11** (pants): tinted with `colorPants` (= pantsColor)
- **Part 12** (shoes): tinted with `colorShoes` (= shoeColor)

When leg armor is equipped, draws the armor texture instead.

### DrawPlayer_15_SkinLongCoat

Draws **Part 14** (long coat/dress) for skin variants 3, 7, and 8 only:
- Tinted with `colorShirt` (= shirtColor)
- Only renders when no body armor is equipped

### DrawPlayer_17_Torso / DrawPlayer_17_TorsoComposite

When `usesCompositeTorso` is true and no body armor is equipped, draws **four layers**:
1. **Part 4** (undershirt) with `compBackShoulderFrame` — shoulder area
2. **Part 6** (shirt) with `compBackShoulderFrame` — shoulder area
3. **Part 4** (undershirt) with `compTorsoFrame` — chest area
4. **Part 6** (shirt) with `compTorsoFrame` — chest area

When `usesCompositeTorso` is false and no body armor:
- **Part 4** (undershirt): with `bodyFrame`, tinted with `colorUnderShirt`
- **Part 6** (shirt overlay): with `bodyFrame`, tinted with `colorShirt`
- **Part 5** (hands/skin): with `bodyFrame`, tinted with `colorBodySkin`

### DrawPlayer_21_Head (TheFace)

When no helmet is equipped, draws:
- **Part 0** (head skin): tinted with `colorHead` (= skinColor)
- **Part 1** (eye whites): tinted with `colorEyeWhites` (= white)
- **Part 2** (eye irises): tinted with `colorEyes` (= eyeColor)
- **Part 15** (eyelid): tinted with `colorHead` (= skinColor), drawn via `DrawPlayer_21_Head_TheFace_Eyelid`

Then draws hair texture on top.

### DrawPlayer_28_ArmOverItem / DrawPlayer_28_ArmOverItemComposite

When no body armor is equipped, draws:
- **Part 7** (back arm skin): tinted with `colorBodySkin` (= skinColor)
- **Part 8** (undershirt sleeves): tinted with `colorUnderShirt` (= underShirtColor)
- **Part 13** (shirt sleeves): tinted with `colorShirt` (= shirtColor)

In composite mode, uses `compFrontArmFrame` / `compBackArmFrame` for arm positions. These frame indices are **not** affected by the gender Y offset.

## Tinting Algorithm

Terraria uses simple multiplicative tinting:

```
outputR = (pixelR * tintR) / 255
outputG = (pixelG * tintG) / 255
outputB = (pixelB * tintB) / 255
```

In TEdit, since XNA textures are premultiplied, we un-premultiply first, tint, then alpha-blend onto the composite buffer.

## Alpha Blending

Standard Porter-Duff "over" operation for compositing layers:

```
outA = srcA + dstA * (255 - srcA) / 255
outR = (srcR * srcA + dstR * dstA * (255 - srcA) / 255) / outA
outG = (srcG * srcA + dstG * dstA * (255 - srcA) / 255) / outA
outB = (srcB * srcA + dstB * dstA * (255 - srcA) / 255) / outA
```

## Default Appearance Colors

From `PlayerAppearance.cs`:

| Color          | Default (R,G,B) |
|----------------|-----------------|
| SkinColor      | 255, 125, 90    |
| HairColor      | 151, 100, 69    |
| EyeColor       | 105, 90, 75     |
| ShirtColor     | 175, 165, 140   |
| UnderShirtColor| 160, 180, 215   |
| PantsColor     | 255, 230, 175   |
| ShoeColor      | 160, 105, 60    |

## TEdit Implementation Files

| File | Purpose |
|------|---------|
| `src/TEdit/Render/PlayerPreviewRenderer.cs` | Static compositor - combines body parts into WriteableBitmap |
| `src/TEdit/Render/Textures.cs` | Texture loading with fallback chain (`GetPlayerBody`, `GetPlayerHair`) |
| `src/TEdit/ViewModel/PlayerEditorViewModel.cs` | ViewModel with reactive preview regeneration |
| `src/TEdit/ViewModel/SkinVariantOption.cs` | Model for skin variant ComboBox items |
| `src/TEdit/View/Sidebar/PlayerEditorView.xaml` | UI with preview image and skin variant selector |

