# Terraria Dye Rendering System — Armor & Equipment

This document describes how Terraria's dye system works end-to-end, from item
definitions through shader application, based on analysis of the
Terraria source (v1.4.4+).

## Overview

Dyes in Terraria are items that, when placed in a dye equipment slot, cause the
corresponding armor/accessory/mount sprite to be drawn with an HLSL pixel shader
effect. The pipeline is:

```
Dye Item → Shader ID (byte) → Player.c* field → DrawData.shader → GPU HLSL pass
```

---

## 1. Dye Item Definitions

### Item Fields

```csharp
// Item.cs
public byte dye;       // armor shader ID (0 = not a dye)
public short hairDye;  // hair shader ID (-1 = not a hair dye)
```

When `Item.SetDefaults()` runs, the shader ID is resolved:

```csharp
this.dye = (byte)GameShaders.Armor.GetShaderIdFromItemId(this.type);
```

The `dye` byte is a **1-based index** into `ArmorShaderDataSet._shaderData`.
Zero means "no dye / default rendering."

### Registration at Startup

All dye→shader mappings are created in `DyeInitializer.Load()`:

```csharp
// DyeInitializer.cs — examples
GameShaders.Armor.BindShader<ArmorShaderData>(1007,          // Red Dye item ID
    new ArmorShaderData(pixelShaderRef, "ArmorColored"))
    .UseColor(1f, 0f, 0f).UseSaturation(1.2f);

GameShaders.Armor.BindShader<ArmorShaderData>(2869,          // Living Flame Dye
    new ArmorShaderData(pixelShaderRef, "ArmorLivingFlame"))
    .UseColor(1f, 0.9f, 0f).UseSecondaryColor(1f, 0.2f, 0f);

GameShaders.Armor.BindShader<TeamArmorShaderData>(1969,      // Team Dye
    new TeamArmorShaderData(pixelShaderRef, "ArmorColored"));
```

`BindShader` in `ArmorShaderDataSet` assigns a sequential 1-based ID:

```csharp
public T BindShader<T>(int itemId, T shaderData) where T : ArmorShaderData
{
    _shaderLookupDictionary[itemId] = ++_shaderDataCount;
    _shaderData.Add(shaderData);
    return shaderData;
}
```

---

## 2. Equipment Slots & Dye Slot Layout

### Player Arrays

```csharp
// Player.cs
public Item[] armor      = new Item[20]; // 0-9 equipped, 10-19 vanity
public Item[] dye        = new Item[10]; // dye for armor slots
public Item[] miscEquips = new Item[5];  // pet/light/cart/mount/grapple
public Item[] miscDyes   = new Item[5];  // dyes for misc equips
public byte   hairDye;                   // hair dye shader ID
```

### Slot Mapping

| Dye Slot | Equipment Slots | Shader Field |
|----------|----------------|--------------|
| `dye[0]` | `armor[0]` (head) / `armor[10]` (vanity head) | `cHead` |
| `dye[1]` | `armor[1]` (body) / `armor[11]` (vanity body) | `cBody` |
| `dye[2]` | `armor[2]` (legs) / `armor[12]` (vanity legs) | `cLegs` |
| `dye[3]`–`dye[9]` | `armor[3-9]` / `armor[13-19]` (accessories) | varies by accessory type |
| `miscDyes[0]` | `miscEquips[0]` (pet) | `cPet` / `cYorai` |
| `miscDyes[1]` | `miscEquips[1]` (light pet) | `cLight` |
| `miscDyes[2]` | `miscEquips[2]` (minecart) | `cMinecart` |
| `miscDyes[3]` | `miscEquips[3]` (mount) | `cMount` |
| `miscDyes[4]` | `miscEquips[4]` (grapple) | `cGrapple` |

Vanity items (slots 10-19) share the same dye slot as their equipped
counterpart via `slot % 10`. The dye applies regardless of whether the
equipped or vanity item is visually active.

### Accessory Sub-Slot Dispatch

Accessories are dispatched to specific shader fields based on their equip type:

```csharp
// Player.UpdateItemDye() — simplified
if (armorItem.wingSlot > 0)   cWings   = dyeItem.dye;
if (armorItem.shieldSlot > 0) cShield  = dyeItem.dye;
if (armorItem.backSlot > 0)   cBack    = dyeItem.dye;  // or cBackpack/cTail
if (armorItem.shoeSlot > 0)   cShoe    = dyeItem.dye;
if (armorItem.faceSlot > 0)   cFace    = dyeItem.dye;
if (armorItem.neckSlot > 0)   cNeck    = dyeItem.dye;
if (armorItem.waistSlot > 0)  cWaist   = dyeItem.dye;
if (armorItem.balloonSlot > 0) cBalloon = dyeItem.dye;
if (armorItem.handOnSlot > 0) cHandOn  = dyeItem.dye;
if (armorItem.handOffSlot > 0) cHandOff = dyeItem.dye;
// ... plus special cases for cCoat (item 5587)
```

---

## 3. Per-Tick Update: `Player.UpdateDyes()`

Called once per `Player.Update()` frame. Resets all `c*` fields to 0, then
repopulates them from the current dye items:

```csharp
public void UpdateDyes()
{
    cHead = cBody = cLegs = /* all c* fields */ = 0;

    cHead = (int)dye[0].dye;
    cBody = (int)dye[1].dye;
    cLegs = (int)dye[2].dye;
    if (wearsRobe) cLegs = cBody;  // robes override leg dye

    cPet      = (int)miscDyes[0].dye;
    cLight    = (int)miscDyes[1].dye;
    cMinecart = (int)miscDyes[2].dye;
    cMount    = (int)miscDyes[3].dye;
    cGrapple  = (int)miscDyes[4].dye;

    // Dispatch accessories (slots 0-19)
    for (int slot = 0; slot < 20; ++slot)
        UpdateItemDye(slot < 10, hideVisibleAccessory[slot % 10],
                      armor[slot], dye[slot % 10]);

    cYorai = cPet;
}
```

---

## 4. Rendering Pipeline

### Step 1: PlayerDrawSet Setup

`LegacyPlayerRenderer.DrawPlayer()` creates a `PlayerDrawSet` and calls
`BoringSetup()`, which copies all `c*` fields from the player:

```csharp
// PlayerDrawSet.BoringSetup()
this.cHead = drawPlayer.cHead;
this.cBody = drawPlayer.cBody;
this.cLegs = drawPlayer.cLegs;
// ... all c* fields

// Hair dye is "packed" to distinguish from armor shaders
this.hairDyePacked = PlayerDrawHelper.PackShader(
    (int)drawPlayer.hairDye, ShaderConfiguration.HairShader);
```

### Step 2: Layer Drawing

Over 30 static `DrawPlayer_XX_*()` methods in `PlayerDrawLayers` create
`DrawData` structs for each visual element (head, torso, legs, accessories,
wings, etc.), setting the shader ID:

```csharp
// Example: head armor
drawData = new DrawData(TextureAssets.ArmorHead[player.head].Value,
    position, sourceRect, color, rotation, origin, scale, effects, layer);
drawData.shader = drawinfo.cHead;
drawinfo.DrawDataCache.Add(drawData);
```

### Step 3: Batch Rendering

`DrawPlayer_RenderAllLayers()` iterates the `DrawDataCache` and applies
shaders before each sprite draw:

```csharp
for (int i = 0; i < drawDataCache.Count; ++i)
{
    cdd = drawDataCache[i];
    PlayerDrawHelper.SetShaderForData(player, cHead, ref cdd);
    spriteBuffer.DrawSingle(i);
}
Main.pixelShader.CurrentTechnique.Passes[0].Apply(); // reset
```

### Step 4: Shader Dispatch

`PlayerDrawHelper.SetShaderForData()` unpacks the shader type and routes to
the correct shader system:

```csharp
public static void SetShaderForData(Player player, int cHead, ref DrawData cdd)
{
    UnpackShader(cdd.shader, out int shaderIndex, out ShaderConfiguration type);

    switch (type)
    {
        case ShaderConfiguration.ArmorShader:
            GameShaders.Armor.Apply(shaderIndex, player, cdd);
            break;
        case ShaderConfiguration.HairShader:
            if (player.head == 0)  // bareheaded: use head dye on hair
                GameShaders.Armor.Apply(cHead, player, cdd);
            else
                GameShaders.Hair.Apply((short)shaderIndex, player, cdd);
            break;
    }
}
```

**Packing scheme:**
- Armor shaders: stored directly (0–999)
- Hair shaders: `shaderID + 1000`
- Tile shaders: `shaderID + 2000`

### Step 5: GPU Parameter Binding

`ArmorShaderData.Apply()` sends parameters to the HLSL pixel shader:

| Uniform | Source | Purpose |
|---------|--------|---------|
| `uColor` | `ArmorShaderData._uColor` | Primary dye color (RGB) |
| `uSecondaryColor` | `ArmorShaderData._uSecondaryColor` | Second color for gradients |
| `uSaturation` | `ArmorShaderData._uSaturation` | Color saturation multiplier |
| `uOpacity` | `ArmorShaderData._uOpacity` | Transparency control |
| `uTime` | `Main.GlobalTimeWrappedHourly` | Animation driver for animated dyes |
| `uTargetPosition` | World-space draw position | Screen-space UV for noise patterns |
| `uSourceRect` | Sprite source rectangle | For sprite-sheet UV calculation |
| `uImage[1]` | Noise/pattern texture | Complex dyes (twilight, living, etc.) |

Then `ShaderData.Apply()` activates the named HLSL technique pass (e.g.
`"ArmorColored"`, `"ArmorLivingFlame"`, etc.).

---

## 5. HLSL Shader Passes

The following named HLSL passes are used by armor dyes:

| Pass Name | Description | Example Dyes |
|-----------|-------------|--------------|
| `ArmorColored` | Solid color tint | Basic dyes (Red, Blue, etc.), Team Dye |
| `ArmorColoredAndBlack` | Tints color, darkens shadows | Silver/Black compound dyes |
| `ArmorColoredGradient` | Gradient between two colors | Gradient dyes |
| `ArmorBrightnessRainbow` | Animated rainbow based on brightness | Hades Dye |
| `ArmorColoredRainbow` | Animated rainbow | Rainbow Dye |
| `ArmorLivingFlame` | Animated fire pattern | Living Flame Dye |
| `ArmorLivingRainbow` | Animated rainbow flowing | Living Rainbow Dye |
| `ArmorLivingOcean` | Animated water/ocean | Living Ocean Dye |
| `ArmorReflectiveColor` | Environment light reflection | Reflective Dyes |
| `ArmorMartian` | Martian-themed effect | Martian Dye |
| `ArmorInvert` | Inverts colors | Negative Dye |
| `ArmorWisp` | Wispy/ghostly effect | Wisp Dye |
| `ArmorTwilight` | Noise-mapped twilight pattern | Twilight Dye |
| `ArmorAcid` | Acid/psychedelic effect | Acid Dye |
| `ArmorMushroom` | Mushroom glow effect | Mushroom Dye |
| `ArmorPhase` | Phase/pulse effect | Phase Dye |
| `ArmorSolar` / `ArmorNebula` / `ArmorVortex` / `ArmorStardust` | Celestial pillar themes | Lunar dyes |

---

## 6. Special Dye Types

### Team Dye (`TeamArmorShaderData`)

**Item 1969.** Overrides `Apply()` to dynamically set `uColor` based on
`player.team` before invoking the standard `ArmorColored` pass:

```csharp
public override void Apply(Entity entity, DrawData? drawData)
{
    UseColor(Main.teamColor[((Player)entity).team]);
    base.Apply(entity, drawData);
}
```

Also provides per-team secondary shaders for particle/dust effects via
`GetSecondaryShader()`.

### Reflective Dyes (`ReflectiveArmorShaderData`)

**Items 3026, 3027, 3190, 3553-3555.** Overrides `Apply()` to sample tile
lighting at several positions around the entity, computing a `uLightSource`
direction vector for environment reflection in the shader.

### Twilight Dye (`TwilightDyeShaderData`)

**Item 3039.** Passes `Main.screenPosition + drawData.position` as
`uTargetPosition` so the noise texture maps to world-space coordinates,
creating a consistent pattern regardless of camera movement.

### Hair Dyes

Two categories:

**Legacy hair dyes** (`LegacyHairShaderData`, items 1977-1986, 2863) — no GPU
shader. They override `GetColor(Player, Color)` to compute a tint color from
game state:

| Item | Name | Color Source |
|------|------|-------------|
| 1977 | Life Dye | Player health % |
| 1978 | Mana Dye | Inverse mana % |
| 1979 | Depth Dye | Player Y position (sky → underworld) |
| 1980 | Money Dye | Total coins in inventory |
| 1981 | Time Dye | In-game time of day |
| 1982 | Team Dye | `Main.teamColor[player.team]` |
| 1983 | Biome Dye | Current water style / shimmer zone |
| 1985 | Party Dye | `Main.DiscoR/G/B` (cycling rainbow) |
| 1986 | Speed Dye | Player velocity magnitude |
| 2863 | Martian Dye | Ambient tile light at player position |

**GPU shader hair dye** (item 3259, Twilight Hair Dye) — uses the same
`ArmorTwilight` HLSL pass with noise texture, applied through the hair shader
pipeline.

### Non-Colorful Dyes

`ItemID.Sets.NonColorfulDyeItems` lists dyes that don't contribute meaningful
color (Loki's Dye 3599, Void Dye 3530, Mirage Dye 3534). These are excluded
from color sampling operations.

### ColorOnly Shader (Item 3978)

A special shader used internally for rendering flat-colored player silhouettes
(e.g., outline effects). Its shader ID is stored in
`ContentSamples.DyeShaderIDs.ColorOnlyShaderIndex`.

---

## 7. Robe Special Case

When a player wears a robe-type body armor (`Player.wearsRobe = true`), the
leg dye is automatically overridden to match the body dye:

```csharp
if (this.wearsRobe) this.cLegs = this.cBody;
```

This ensures the robe renders as a single cohesive piece.

---

## 8. Equipment Loadouts

The `EquipmentLoadout` class stores complete snapshots of all equipment and dye
arrays for the loadout preset system. All 10 `dye[]` items and 5 `miscDyes[]`
items are saved/restored atomically when switching loadouts.

---

## 9. Persistence (Player Save File)

Dye items are saved as **item type IDs** (not shader IDs):

```csharp
// Saving
for (int i = 0; i < player.dye.Length; ++i)
{
    fileIO.Write(player.dye[i].type);
    fileIO.Write(player.dye[i].prefix);
}

// Loading — SetDefaults re-resolves the shader ID
item.netDefaults(savedType);  // → SetDefaults() → dye = GetShaderIdFromItemId(type)
```

This means shader IDs are ephemeral and re-derived at load time from the
current registration order in `DyeInitializer`.

---

## 10. End-to-End Example

**Scenario:** Player equips Living Flame Dye (item 2869) in the body dye slot.

1. **Item creation:** `Item.SetDefaults(2869)` → `Item.dye = (byte)GameShaders.Armor.GetShaderIdFromItemId(2869)` → e.g., shader ID `47`

2. **Dye slot:** Player places item in `Player.dye[1]` (body dye slot)

3. **Per-tick update:** `Player.UpdateDyes()` → `Player.cBody = (int)dye[1].dye` → `47`

4. **Draw setup:** `PlayerDrawSet.BoringSetup()` → `drawinfo.cBody = 47`

5. **Layer draw:** `DrawPlayer_17_Torso()` creates `DrawData` with `shader = 47`, adds to `DrawDataCache`

6. **Render:** `DrawPlayer_RenderAllLayers()` calls `SetShaderForData()` → unpacks as `ArmorShader` type → `GameShaders.Armor.Apply(47, player, drawData)`

7. **Shader lookup:** `ArmorShaderDataSet._shaderData[46]` → the `ArmorShaderData` instance registered for item 2869

8. **GPU bind:** `ArmorShaderData.Apply()` sends:
   - `uColor = (1.0, 0.9, 0.0)` (yellow-orange)
   - `uSecondaryColor = (1.0, 0.2, 0.0)` (red-orange)
   - `uTime = Main.GlobalTimeWrappedHourly` (drives animation)
   - Sprite geometry uniforms

9. **HLSL:** The `"ArmorLivingFlame"` pixel shader pass transforms every pixel of the body armor sprite into an animated flame pattern using the color parameters and time

10. **Reset:** After drawing, `Passes[0].Apply()` returns the GPU to default state

---

## Key Source Files

| File | Role |
|------|------|
| `Terraria/Player.cs` | Equipment arrays, `c*` shader fields, `UpdateDyes()`, `UpdateItemDye()` |
| `Terraria/Item.cs` | `dye` byte field, `hairDye` field, `SetDefaults()` |
| `Terraria/Initializers/DyeInitializer.cs` | Registers all dye→shader mappings at startup |
| `Terraria/Graphics/Shaders/GameShaders.cs` | Static `Armor` and `Hair` shader set instances |
| `Terraria/Graphics/Shaders/ArmorShaderData.cs` | Per-dye shader data, GPU parameter binding |
| `Terraria/Graphics/Shaders/ArmorShaderDataSet.cs` | Registry of all armor shaders, ID lookup |
| `Terraria/Graphics/Shaders/HairShaderData.cs` | Hair dye shader data |
| `Terraria/Graphics/Shaders/ShaderData.cs` | Base class, applies HLSL effect pass |
| `Terraria/GameContent/Dyes/TeamArmorShaderData.cs` | Team-color dynamic dye |
| `Terraria/GameContent/Dyes/ReflectiveArmorShaderData.cs` | Environment-reflecting dye |
| `Terraria/GameContent/Dyes/TwilightDyeShaderData.cs` | World-space noise-mapped dye |
| `Terraria/GameContent/Dyes/LegacyHairShaderData.cs` | CPU-computed hair dye colors |
| `Terraria/DataStructures/PlayerDrawSet.cs` | Draw context with copied `c*` fields |
| `Terraria/DataStructures/PlayerDrawLayers.cs` | Per-layer draw methods, final render loop |
| `Terraria/DataStructures/PlayerDrawHelper.cs` | Shader packing/unpacking, dispatch routing |
| `Terraria/DataStructures/DrawData.cs` | Per-sprite draw data including shader ID |
| `Terraria/Graphics/Renderers/LegacyPlayerRenderer.cs` | Entry point for player rendering |
| `Terraria/EquipmentLoadout.cs` | Loadout preset save/restore |

---

## Appendix A: Complete Dye Item ID → Static Color Lookup Table

This table provides a **static RGB approximation** for every dye, suitable for
CPU-based tinting in a map editor. Colors are derived from `DyeInitializer.cs`
`UseColor()` values, clamped to 0-255.

For animated/complex dyes that have no single static color, a representative
"frozen frame" color is provided with notes.

### How to use this table

Apply as a **multiplicative tint**: for each pixel, `outR = srcR * dyeR / 255`.
This matches Terraria's `ArmorColored` pass behavior for basic dyes.

For `AndBlack` variants, dark areas of the sprite are pushed toward black.
A simple approximation: tint with the color, then darken pixels below a
brightness threshold.

For `Bright` variants, the color is pre-lightened: `bright = base * 0.5 + 0.5`.

### Basic Color Dyes

Each base dye spawns 4 variants via `LoadBasicColorDye(base, r, g, b)`:
- **Base** (item ID): `ArmorColored` — multiply tint
- **Black** (base+12): `ArmorColoredAndBlack` — tint + darken shadows
- **Bright** (base+31): `ArmorColored` — lightened tint (`r*0.5+0.5`)
- **Silver** (base+44): `ArmorColoredAndSilverTrim` — tint + silver trim

| Base ID | Name | R | G | B | Black ID | Bright ID | Silver ID |
|---------|------|---|---|---|----------|-----------|-----------|
| 1007 | Red Dye | 255 | 0 | 0 | 1019 | 1038 | 1051 |
| 1008 | Orange Dye | 255 | 128 | 0 | 1020 | 1039 | 1052 |
| 1009 | Yellow Dye | 255 | 255 | 0 | 1021 | 1040 | 1053 |
| 1010 | Lime Dye | 128 | 255 | 0 | 1022 | 1041 | 1054 |
| 1011 | Green Dye | 0 | 255 | 0 | 1023 | 1042 | 1055 |
| 1012 | Teal Dye | 0 | 255 | 128 | 1024 | 1043 | 1056 |
| 1013 | Cyan Dye | 0 | 255 | 255 | 1025 | 1044 | 1057 |
| 1014 | Sky Blue Dye | 51 | 128 | 255 | 1026 | 1045 | 1058 |
| 1015 | Blue Dye | 0 | 0 | 255 | 1027 | 1046 | 1059 |
| 1016 | Purple Dye | 128 | 0 | 255 | 1028 | 1047 | 1060 |
| 1017 | Violet Dye | 255 | 0 | 255 | 1029 | 1048 | 1061 |
| 1018 | Pink Dye | 255 | 26 | 128 | 1030 | 1049 | 1062 |
| 2874 | Brown Dye | 102 | 51 | 0 | 2875 | 2876 | 2877 |

**Bright variant RGB** (computed as `base * 0.5 + 0.5`, scaled to 255):

| Bright ID | Name | R | G | B |
|-----------|------|---|---|---|
| 1038 | Bright Red Dye | 255 | 128 | 128 |
| 1039 | Bright Orange Dye | 255 | 191 | 128 |
| 1040 | Bright Yellow Dye | 255 | 255 | 128 |
| 1041 | Bright Lime Dye | 191 | 255 | 128 |
| 1042 | Bright Green Dye | 128 | 255 | 128 |
| 1043 | Bright Teal Dye | 128 | 255 | 191 |
| 1044 | Bright Cyan Dye | 128 | 255 | 255 |
| 1045 | Bright Sky Blue Dye | 153 | 191 | 255 |
| 1046 | Bright Blue Dye | 128 | 128 | 255 |
| 1047 | Bright Purple Dye | 191 | 128 | 255 |
| 1048 | Bright Violet Dye | 255 | 128 | 255 |
| 1049 | Bright Pink Dye | 255 | 141 | 191 |
| 2876 | Bright Brown Dye | 179 | 153 | 128 |

### Brightness / Grayscale Dyes

| Item ID | Name | Shader | R | G | B | Notes |
|---------|------|--------|---|---|---|-------|
| 1050 | Black Dye | ArmorBrightnessColored | 153 | 153 | 153 | Darkens toward gray |
| 1037 | Silver Dye | ArmorBrightnessColored | 255 | 255 | 255 | Neutral brightness |
| 3558 | Bright Silver Dye | ArmorBrightnessColored | 255 | 255 | 255 | Brightens (1.5x, clamped) |
| 2871 | Shadow Dye | ArmorBrightnessColored | 13 | 13 | 13 | Near-black |
| 3559 | Silver and Black Dye | ArmorColoredAndBlack | 255 | 255 | 255 | White tint + black shadows |

### Gradient Dyes (two-color)

Static approximation: use the **average** of primary and secondary color.

| Item ID | Name | Shader | Color1 (R,G,B) | Color2 (R,G,B) | Static Avg (R,G,B) |
|---------|------|--------|-----------------|-----------------|---------------------|
| 1031 | Flame Dye | ColoredGradient | 255,0,0 | 255,255,0 | 255,128,0 |
| 1032 | Flame and Black Dye | ColoredAndBlackGradient | 255,0,0 | 255,255,0 | 255,128,0 |
| 3550 | Flame and Silver Dye | ColoredAndSilverTrimGradient | 255,0,0 | 255,255,0 | 255,128,0 |
| 1063 | Intense Flame Dye | BrightnessGradient | 255,0,0 | 255,255,0 | 255,128,0 |
| 1035 | Blue Flame Dye | ColoredGradient | 0,0,255 | 0,255,255 | 0,128,255 |
| 1036 | Blue Flame and Black Dye | ColoredAndBlackGradient | 0,0,255 | 0,255,255 | 0,128,255 |
| 3552 | Blue Flame and Silver Dye | ColoredAndSilverTrimGradient | 0,0,255 | 0,255,255 | 0,128,255 |
| 1065 | Intense Blue Flame Dye | BrightnessGradient | 0,0,255 | 0,255,255 | 0,128,255 |
| 1033 | Green Flame Dye | ColoredGradient | 0,255,0 | 255,255,0 | 128,255,0 |
| 1034 | Green Flame and Black Dye | ColoredAndBlackGradient | 0,255,0 | 255,255,0 | 128,255,0 |
| 3551 | Green Flame and Silver Dye | ColoredAndSilverTrimGradient | 0,255,0 | 255,255,0 | 128,255,0 |
| 1064 | Intense Green Flame Dye | BrightnessGradient | 0,255,0 | 255,255,0 | 128,255,0 |
| 1068 | Yellow Gradient Dye | ColoredGradient | 128,255,0 | 255,128,0 | 191,191,0 |
| 1069 | Cyan Gradient Dye | ColoredGradient | 0,255,128 | 0,128,255 | 0,191,191 |
| 1070 | Violet Gradient Dye | ColoredGradient | 255,0,128 | 128,0,255 | 191,0,191 |

### Rainbow / Animated Dyes

These cycle through colors over time. Static fallback: a mid-cycle representative.

| Item ID | Name | Shader | Static Fallback (R,G,B) | Notes |
|---------|------|--------|------------------------|-------|
| 1066 | Rainbow Dye | ArmorColoredRainbow | 255,128,0 | Cycling rainbow; use orange as mid-point |
| 1067 | Intense Rainbow Dye | ArmorBrightnessRainbow | 255,128,0 | Same cycle, brightness-mapped |
| 3556 | Midnight Rainbow Dye | ArmorMidnightRainbow | 128,0,128 | Dark rainbow; use deep purple |
| 2870 | Living Rainbow Dye | ArmorLivingRainbow | 255,128,0 | Animated flowing rainbow |

### Living / Animated Effect Dyes

| Item ID | Name | Shader | Primary (R,G,B) | Secondary (R,G,B) | Static Tint (R,G,B) | Notes |
|---------|------|--------|-----------------|-------------------|---------------------|-------|
| 2869 | Living Flame Dye | ArmorLivingFlame | 255,230,0 | 255,51,0 | 255,140,0 | Avg of flame colors |
| 2873 | Living Ocean Dye | ArmorLivingOcean | — | — | 0,128,255 | Animated ocean blue |

### Reflective Dyes

Static approximation: use the `UseColor()` as a tint (the reflection effect is lost).

| Item ID | Name | Color (R,G,B) | Notes |
|---------|------|--------------|-------|
| 3026 | Reflective Silver Dye | 255,255,255 | Neutral reflective |
| 3027 | Reflective Gold Dye | 255,255,128 | Warm gold (1.5x clamped) |
| 3553 | Reflective Copper Dye | 255,179,102 | Copper tone (1.35x clamped) |
| 3554 | Reflective Obsidian Dye | 64,0,179 | Deep purple |
| 3555 | Reflective Metal Dye | 102,102,102 | Neutral gray |
| 3190 | Reflective Dye | 255,255,255 | Pure reflective (no color) |

### Special Effect Dyes

| Item ID | Name | Shader | Primary (R,G,B) | Secondary (R,G,B) | Static Tint (R,G,B) | Notes |
|---------|------|--------|-----------------|-------------------|---------------------|-------|
| 1969 | Team Dye | ArmorColored | — | — | 255,255,255 | Dynamic team color; fallback white |
| 2864 | Martian Dye | ArmorMartian | 0,255,255 | — | 0,255,255 | Clamped from (0,2,3) |
| 2872 | Negative Dye | ArmorInvert | — | — | 128,128,128 | Inverts colors; use mid-gray |
| 2878 | Wisp Dye | ArmorWisp | 179,255,230 | 89,217,204 | 134,236,217 | Avg of wisp colors |
| 2879 | Pixie Dye | ArmorWisp | 255,255,0 | 255,153,77 | 255,204,38 | Yellow wisp |
| 2885 | Infernal Wisp Dye | ArmorWisp | 255,204,0 | 204,51,0 | 230,128,0 | Fire wisp |
| 2884 | Unicorn Wisp Dye | ArmorWisp | 255,0,255 | 255,77,153 | 255,38,204 | Pink-purple wisp |
| 2883 | Chlorophyte Dye | ArmorHighContrastGlow | 0,255,0 | — | 0,255,0 | Bright green glow |
| 3025 | Purple Ooze Dye | ArmorFlow | 255,128,255 | 153,26,255 | 204,77,255 | Flowing purple |
| 3039 | Twilight Dye | ArmorTwilight | 128,26,255 | — | 128,26,255 | Noise-mapped twilight |
| 3040 | Acid Dye | ArmorAcid | 128,255,77 | — | 128,255,77 | Green acid |
| 3560 | Red Acid Dye | ArmorAcid | 230,51,51 | — | 230,51,51 | Red acid |
| 3028 | Blue Acid Dye | ArmorAcid | 128,179,255 | — | 128,179,255 | Blue acid |
| 3041 | Glowing Mushroom Dye | ArmorMushroom | 13,51,255 | — | 13,51,255 | Blue-purple mushroom glow |
| 3042 | Phase Dye | ArmorPhase | 102,51,255 | — | 102,51,255 | Pulsing purple (clamped) |
| 3561 | Gel Dye | ArmorGel | 102,179,255 | 0,0,26 | 51,89,140 | Blue gel (avg primary+secondary) |
| 3562 | Pink Gel Dye | ArmorGel | 255,191,255 | 115,26,77 | 185,108,166 | Pink gel |
| 3024 | Skiphs' Blood | ArmorGel | 128,0,0 | 255,255,255 | 191,128,128 | Blood red gel (neg vals clamped) |
| 4663 | Bloodbath Dye | ArmorGel | 255,153,153 | 51,0,0 | 153,77,77 | Dark blood gel |
| 4662 | Fogbound Dye | ArmorFog | 242,242,242 | 77,77,77 | 160,160,160 | Gray fog |
| 4778 | Prismatic Dye | ArmorHallowBoss | — | — | 255,200,255 | Empress shimmer; pink-white |
| 3534 | Mirage Dye | ArmorMirage | — | — | 200,200,255 | Heat-shimmer; use pale blue |
| 3557 | Black and White Dye | ArmorPolarized | — | — | 128,128,128 | Desaturation; use gray |
| 3978 | ColorOnly | ColorOnly | — | — | 255,255,255 | Internal flat-color shader |

### Hades / Shadow Dyes

| Item ID | Name | Primary (R,G,B) | Static Tint (R,G,B) | Notes |
|---------|------|-----------------|---------------------|-------|
| 3038 | Hades Dye | 128,179,255 | 128,179,255 | Blue hades glow (clamped) |
| 3600 | Shadowflame Hades Dye | 179,102,255 | 179,102,255 | Purple hades (clamped) |
| 3597 | Burning Hades Dye | 255,153,102 | 255,153,102 | Orange-red hades (clamped) |
| 3598 | Grim Dye | 26,26,26 | 26,26,26 | Near-black with faint red glow |
| 3599 | Loki's Dye | ArmorLoki | 26,26,26 | 26,26,26 | Pure dark shadow |

### Shifting / Celestial Dyes

| Item ID | Name | Shader | Primary (R,G,B) | Secondary (R,G,B) | Static Tint (R,G,B) |
|---------|------|--------|-----------------|-------------------|---------------------|
| 3533 | Shifting Sands Dye | ArmorShiftingSands | 255,255,128 | 179,128,77 | 217,191,102 |
| 3535 | Shifting Pearlsands Dye | ArmorShiftingPearlsands | 255,204,230 | 89,64,112 | 172,134,171 |
| 3526 | Solar Dye | ArmorSolar | 255,0,0 | 255,255,0 | 255,128,0 |
| 3527 | Nebula Dye | ArmorNebula | 255,0,255 | 255,255,255 | 255,128,255 |
| 3528 | Vortex Dye | ArmorVortex | 26,128,89 | 255,255,255 | 140,191,172 |
| 3529 | Stardust Dye | ArmorStardust | 102,153,255 | 255,255,255 | 179,204,255 |
| 3530 | Void Dye | ArmorVoid | — | — | 0,0,0 | Pure black void |

### Hair Dyes — Static Fallback Colors

Hair dyes are CPU-computed in Terraria based on game state. These fallbacks
represent a "default/neutral" state for static rendering.

| Item ID | Name | Static Fallback (R,G,B) | Notes |
|---------|------|------------------------|-------|
| 1977 | Life Hair Dye | 255,20,20 | Full health = bright red |
| 1978 | Mana Hair Dye | 50,255,75 | Full mana = blue-tinted |
| 1979 | Depth Hair Dye | 28,216,94 | Surface level = green |
| 1980 | Money Hair Dye | 226,118,76 | Copper (no money) |
| 1981 | Time Hair Dye | 255,255,0 | Midday = yellow |
| 1982 | Team Hair Dye | 255,255,255 | No team = white |
| 1983 | Biome Hair Dye | 28,216,94 | Default biome = green |
| 1984 | Party Hair Dye | 244,22,175 | Fixed hot pink |
| 1985 | Rainbow Hair Dye | 255,128,0 | Cycling; use orange mid |
| 1986 | Speed Hair Dye | 75,255,200 | Max speed color |
| 2863 | Martian Hair Dye | 128,128,128 | Ambient light; use mid-gray |
| 3259 | Twilight Hair Dye | 128,26,255 | Same as Twilight Dye |
