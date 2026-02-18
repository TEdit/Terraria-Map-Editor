# Fixing Accessory & Wings Rendering in TEdit

## Problem Summary

Accessory and wings rendering is disabled in two places:

1. **World renderer** (`WorldRenderXna.xaml.cs`) — `DrawDollBody()` and `DrawDollAccessory()` are fully implemented but commented out at the call site (lines 2741-2744, 2826-2833).
2. **Player editor preview** (`PlayerEditorViewModel.cs`) — `RegeneratePreview()` never passes accessory slot parameters to `PlayerPreviewRenderer.RenderPreview()` (line 630).

The helper methods exist and the texture loading pipeline works. The blockers are:
- **Pose-aware frame offsets** — armor overlays use fixed frame row 0, but body parts render at the pose's frame offset, causing misalignment.
- **Accessory texture sizing** — `DrawDollAccessory` uses `GetDollBodyDest()` which assumes a 40×56 body frame, but accessories (especially wings) have different dimensions and need per-type positioning.
- **Wing frame counts are not uniform** — must be looked up per wing ID (4, 6, 7, 8, 11, or 14 frames).

---

## Reference: Terraria's Actual Draw System

Source: `terraria-server-dasm/TerrariaServer/Terraria/`

### Draw Order (back to front)

From `LegacyPlayerRenderer.DrawPlayer_UseNormalLayers`:

```
Layer 08   — Backpacks (player.backpack)
Layer 08_1 — Tails (player.tail)
Layer 09   — Wings                          ← behind everything visible
Layer 10   — Back accessories (capes, etc.)
Layer 11   — Balloons
Layer 12   — Skin (body frame)
Layer 13   — Leggings + Armor legs
Layer 14   — Shoes
Layer 17   — Torso (composite system)
Layer 18   — Offhand accessories
Layer 19   — Waist accessories
Layer 20   — Neck accessories
Layer 21   — Head + Helmet
Layer 22   — Face accessories
Layer 25   — Shield
Layer 29   — Onhand accessories
Layer 32   — Front accessories
```

### Body Frame Dimensions

- Standard body frame: **40×56 pixels** per animation frame
- Frame index = `bodyFrame.Y / 56` (values 0–19 for 20 animation frames)
- Display dolls use skin variants **10** (male) and **11** (female)

### Standard Body Position Formula

Used by most layers for in-game rendering:

```csharp
Vector2 position = new Vector2(
    (int)(Position.X - screenPosition.X - bodyFrame.Width / 2 + player.width / 2),
    (int)(Position.Y - screenPosition.Y + player.height - bodyFrame.Height + 4)
) + player.bodyPosition;
// origin = new Vector2(legFrame.Width * 0.5f, legFrame.Height * 0.5f)
```

### Per-Frame Attachment Offsets

From `Main.cs` — these arrays are indexed by `bodyFrame.Y / 56`:

```csharp
// Headgear Y shift per animation frame (used by some wing types too)
OffsetsPlayerHeadgear[20] = {
    (0,2), (0,2), (0,2), (0,2), (0,2),
    (0,2), (0,2), (0,0), (0,0), (0,0),
    (0,2), (0,2), (0,2), (0,2), (0,0),
    (0,0), (0,0), (0,2), (0,2), (0,2)
};

// Offhand attachment point
OffsetsPlayerOffhand[20] = {
    (14,20), (14,20), (14,20), (14,18), (14,20),
    (16, 4), (16,16), (18,14), (18,14), (18,14),
    (16,16), (16,16), (16,16), (16,16), (14,14),
    (14,14), (12,14), (14,16), (16,16), (16,16)
};

// Onhand attachment point
OffsetsPlayerOnhand[20] = {
    (6,19), (5,10), (12,10), (13,17), (12,19),
    (5,10), (7,17), (6,16), (6,16), (6,16),
    (6,17), (7,17), (7,17), (7,17), (8,17),
    (9,16), (9,12), (8,17), (7,17), (7,17)
};
```

---

## Wings Rendering Details

Source: `DataStructures/PlayerDrawLayers.cs`, method `DrawPlayer_09_Wings`

### Base Position

```csharp
Vector2 commonWingPos =
    drawinfo.Position - Main.screenPosition
    + new Vector2(player.width / 2, player.height - player.bodyFrame.Height / 2)
    + new Vector2(0, 7f);  // +7 px down from center
```

### Default Wing Draw (most wing IDs)

```csharp
// Per-wing offsets (default: num8=0, num7=0, num9=4 frames)
Vector2 position = Floor(commonWingPos + new Vector2(num8 - 9, num7 + 2) * directions);

// Source rectangle from sprite sheet:
Rectangle source = new Rectangle(
    0,
    texture.Height / frameCount * player.wingFrame,
    texture.Width,
    texture.Height / frameCount
);

// Origin = center of one frame
Vector2 origin = new Vector2(texture.Width / 2, texture.Height / frameCount / 2);
```

Where `directions = new Vector2(-player.direction, 1)` and `player.direction` is -1 (left) or 1 (right).

### Per-Wing Overrides

| Wing ID | Name | X offset | Y offset | Frame count | Notes |
|---------|------|----------|----------|-------------|-------|
| Default | (most wings) | 0 | 0 | 4 | — |
| 5 | Butterfly | +4 | -4 | 4 | — |
| 12 | Steampunk/Leaf | -1 | -1 | 4 | — |
| 22 | Hoverboard | 0 | +24 | 7 | `AlwaysAnimated`, fire trail |
| 27 | Mothron | +3 | 0 | 4 | — |
| 28 | LazureBarrier | 0 | +17 | 4 | `AlwaysAnimated` |
| 34 | Jims | special | special | 6 | `AlwaysAnimated`, custom position |
| 39 | Leinfor | -6 | -7 | 6 | `AlwaysAnimated` |
| 40 | Ghostar | -4 | 0 | 14 | `AlwaysAnimated`, grounded/flying split |
| 41 | Safeman | -1 | 0 | 4 | — |
| 43 | GroxTheGreat | -5 | -7 | 7 | — |
| 44 | Rainbow | 0 | 0 | 7 | `AlwaysAnimated` |
| 45 | LongTrailRainbow | 0 | +20 | 6 | `AlwaysAnimated` |
| 47 | ChickenBones | special | special | 11 | uses `OffsetsPlayerHeadgear` |
| 48 | Chippys | special | special | 8 | `AlwaysAnimated` |
| 49 | Heroicis | special | special | 11 | uses `OffsetsPlayerHeadgear` |
| 50 | Kazzymodus | special | special | 11 | custom center position |
| 51 | Lunas | special | special | 8 | — |

### AlwaysAnimated Wings

IDs: 22, 28, 34, 39, 40, 44, 45, 48

These wings animate even when the player is standing still. Static display dolls should show frame 0 for these.

### Wing Frame Count Lookup

Must be a per-ID table. There is no formula — the frame counts are hardcoded in `DrawPlayer_09_Wings`:

```
4 frames:  IDs 1-4, 6-11, 13-21, 23-28, 29-33, 35-38, 41-42, 46
6 frames:  IDs 34, 39, 45
7 frames:  IDs 22, 43, 44
8 frames:  IDs 48, 51
11 frames: IDs 47, 49, 50
14 frames: ID 40
```

---

## Accessory Rendering Details

Each accessory type uses the same body-frame coordinate system but with its own texture atlas:

| Slot Type | Texture Path | Draw Layer | Position Reference |
|-----------|-------------|------------|-------------------|
| Wings | `Images/Wings_{id}` | 09 (behind body) | Special formula (see above) |
| Back | `Images/Acc_Back_{id}` | 10 (behind body) | Body position + bodyVect origin |
| Balloon | `Images/Acc_Balloon_{id}` | 11 (behind body) | Body position + bodyVect origin |
| Shoes | `Images/Acc_Shoes_{id}` | 14 (over legs) | Leg position + legVect origin |
| HandOff | `Images/Acc_HandsOff_{id}` | 18 (mid torso) | Body position + bodyVect origin |
| Waist | `Images/Acc_Waist_{id}` | 19 (mid torso) | Body position + bodyVect origin |
| Neck | `Images/Acc_Neck_{id}` | 20 (mid torso) | Body position + bodyVect origin |
| Face | `Images/Acc_Face_{id}` | 22 (over head) | Head position + headVect origin |
| Shield | `Images/Acc_Shield_{id}` | 25 (front mid) | Body position + bodyVect origin |
| HandOn | `Images/Acc_HandsOn_{id}` | 29 (front) | Body position + bodyVect origin |
| Front | `Images/Acc_Front_{id}` | 32 (frontmost) | Body position + bodyVect origin |

Most accessories (all except wings) use the **standard body frame rectangle**: first frame = `(0, 0, 40, 56)`. The source rect Y is `bodyFrame.Y` (same animation frame as the body). The origin is `bodyVect = (width * 0.5, height * 0.5)`.

---

## Fix Strategy

### Fix 1: World Renderer — Enable `DrawDollBody` with Pose-Aware Armor

**Problem**: Armor overlay rects use `source.Y = 0` (standing frame) while `DrawDollBody` renders at the pose's frame Y offset.

**Solution**: Pass the pose's `bodyFrameY` and `legFrameY` to the armor rendering code.

In `WorldRenderXna.xaml.cs`, the armor drawing block (lines 2746-2823) currently uses:
```csharp
var headSrc = new Rectangle(2, 0, 36, 36);         // always frame 0
var bodySrc = new Rectangle(2, 0, 36, 54);          // always frame 0
var legsSrc = new Rectangle(2, 42, 36, 12);         // always frame 0
```

These need to become:
```csharp
var (bodyFrameY, legFrameY, yPixelOffset) = GetDollPoseFrames(te.Pose);

// Head — uses bodyFrameY (head tracks body animation)
var headSrc = new Rectangle(0, bodyFrameY, DollFrameWidth, DollFrameHeight);

// Body — uses bodyFrameY
var bodySrc = new Rectangle(0, bodyFrameY, DollFrameWidth, DollFrameHeight);

// Legs — uses legFrameY (legs have their own animation)
var legsSrc = new Rectangle(0, legFrameY, DollFrameWidth, DollFrameHeight);
```

Then the destination rects need `yPixelOffset` applied (via `GetDollBodyDest`).

**Steps:**
1. Uncomment `DrawDollBody()` call to render skin behind armor
2. Refactor armor overlay code to use `GetDollPoseFrames()` for source rect Y
3. Use `GetDollBodyDest()` for armor destination rects (replaces manual rect math)
4. Adjust depth values so skin is behind armor which is behind accessories

### Fix 2: World Renderer — Enable Accessory Rendering

**Problem**: `DrawDollAccessory` treats all accessories as 40×56 body-frame overlays, but wings have different dimensions and a different position formula.

**Solution**: Split accessory rendering by type. Non-wing accessories use body-frame positioning. Wings use the Terraria wing formula adapted to tile coordinates.

```csharp
private void DrawDollAccessory(int tileX, int tileY, ItemProperty props,
    byte pose, SpriteEffects effect, int drawOrder)
{
    var (bodyFrameY, legFrameY, yPixelOffset) = GetDollPoseFrames(pose);

    if (props.WingSlot.HasValue)
    {
        DrawDollWings(tileX, tileY, props.WingSlot.Value, effect, drawOrder);
        return;
    }

    // All other accessories: use body-frame source rect at current pose
    Texture2D tex = ResolveAccessoryTexture(props);
    if (tex == null) return;

    int frameY = bodyFrameY;
    // Shoes use leg frame
    if (props.ShoeSlot.HasValue) frameY = legFrameY;

    var source = new Rectangle(0, frameY,
        Math.Min(tex.Width, DollFrameWidth),
        Math.Min(tex.Height - frameY, DollFrameHeight));
    var dest = GetDollBodyDest(tileX, tileY, source.Width, source.Height, yPixelOffset);
    float depth = ComputeAccessoryDepth(props, drawOrder);
    _spriteBatch.Draw(tex, dest, source, Color.White, 0f, default, effect, depth);
}
```

For wings specifically:

```csharp
private void DrawDollWings(int tileX, int tileY, int wingId, SpriteEffects effect, int drawOrder)
{
    var tex = _textureDictionary.GetAccWings(wingId);
    if (tex == null || tex == _textureDictionary.DefaultTexture) return;

    int frameCount = GetWingFrameCount(wingId);
    var (xOff, yOff) = GetWingOffset(wingId);

    // Static display: always frame 0
    int frameHeight = tex.Height / frameCount;
    var source = new Rectangle(0, 0, tex.Width, frameHeight);

    // Wing position: center on doll + Terraria offsets scaled to tile coords
    // The (-9, +2) is the default Terraria offset, converted from pixels to tile fractions
    float wingX = tileX + 1f;  // center of 2-tile-wide doll
    float wingY = tileY + 1.5f; // center-ish vertically

    // Apply Terraria's pixel offsets (scaled by 1/16 to convert to tile coords)
    wingX += (xOff - 9f) / 16f;
    wingY += (yOff + 2f + 7f) / 16f;  // +7 is Terraria's base wing downshift

    var dest = new Rectangle(
        1 + (int)((_scrollPosition.X + wingX - tex.Width / 32f) * _zoom),
        1 + (int)((_scrollPosition.Y + wingY - frameHeight / 32f) * _zoom),
        (int)(_zoom * tex.Width / 16f),
        (int)(_zoom * frameHeight / 16f));

    float depth = LayerDollBody - drawOrder * 0.00001f;
    _spriteBatch.Draw(tex, dest, source, Color.White, 0f, default, effect, depth);
}
```

**Data table needed** — add as a static dictionary or array:

```csharp
private static int GetWingFrameCount(int wingId) => wingId switch
{
    22 or 43 or 44 => 7,
    34 or 39 or 45 => 6,
    48 or 51 => 8,
    47 or 49 or 50 => 11,
    40 => 14,
    _ => 4
};

private static (int x, int y) GetWingOffset(int wingId) => wingId switch
{
    5 => (4, -4),
    12 => (-1, -1),
    22 => (0, 24),
    27 => (3, 0),
    28 => (0, 17),
    39 => (-6, -7),
    40 => (-4, 0),
    41 => (-1, 0),
    43 => (-5, -7),
    45 => (0, 20),
    _ => (0, 0)
};
```

### Fix 3: World Renderer — Accessory Draw Order (Z-Depth)

Accessories must be drawn in the correct z-order relative to the body and armor. Map Terraria's layer numbers to depth values:

```csharp
// Wings (layer 09) — behind body
private const float LayerDollWings = LayerDollBody - 0.001f;
// Back acc (layer 10) — behind body, in front of wings
private const float LayerDollBackAcc = LayerDollBody - 0.0009f;
// Balloon (layer 11) — behind body
private const float LayerDollBalloon = LayerDollBody - 0.0008f;
// Body/armor layers use LayerDollBody
// Shoes (layer 14) — over legs, use LayerDollBody + small offset
// ... etc for each layer
```

Or compute from the Terraria layer number:
```csharp
float depth = LayerDollBody + (layerNumber - 12) * 0.0001f;
```

### Fix 4: Player Editor Preview — Wire Up Accessory Slots

**Problem**: `PlayerEditorViewModel.RegeneratePreview()` resolves head/body/legs slots but never resolves or passes accessory slots.

**Solution**: After the existing armor slot resolution (lines 590-630), add accessory slot resolution:

```csharp
int? wingSlot = null, backSlot = null, balloonSlot = null;
int? shoeSlot = null, waistSlot = null, neckSlot = null;
int? faceSlot = null, shieldSlot = null;
int? handOnSlot = null, handOffSlot = null, frontSlot = null;

// Resolve visible accessories (slots 3-9 for active, 13-19 for vanity)
// Vanity overrides active, matching Terraria's UpdateVisibleAccessories
for (int i = 0; i < 7; i++)
{
    // Check vanity first (slots 13-19 → AccessorySlots index 7+i)
    var vanitySlot = GetAccessorySlotAt(i + 7);
    var activeSlot = GetAccessorySlotAt(i);

    var itemId = vanitySlot?.Item?.Id > 0 ? vanitySlot.Item.Id
               : activeSlot?.Item?.Id > 0 ? activeSlot.Item.Id
               : 0;

    if (itemId <= 0) continue;
    if (!WorldConfiguration.ItemLookupTable.TryGetValue(itemId, out var props)) continue;

    // Check hide flag
    bool hidden = Player.HideVisibleAccessory.Length > i && Player.HideVisibleAccessory[i];
    if (hidden) continue;

    if (props.WingSlot.HasValue)    wingSlot ??= props.WingSlot;
    if (props.BackSlot.HasValue)    backSlot ??= props.BackSlot;
    if (props.BalloonSlot.HasValue) balloonSlot ??= props.BalloonSlot;
    if (props.ShoeSlot.HasValue)    shoeSlot ??= props.ShoeSlot;
    if (props.WaistSlot.HasValue)   waistSlot ??= props.WaistSlot;
    if (props.NeckSlot.HasValue)    neckSlot ??= props.NeckSlot;
    if (props.FaceSlot.HasValue)    faceSlot ??= props.FaceSlot;
    if (props.ShieldSlot.HasValue)  shieldSlot ??= props.ShieldSlot;
    if (props.HandOnSlot.HasValue)  handOnSlot ??= props.HandOnSlot;
    if (props.HandOffSlot.HasValue) handOffSlot ??= props.HandOffSlot;
    if (props.FrontSlot.HasValue)   frontSlot ??= props.FrontSlot;
}
```

Then pass them to `RenderPreview`:
```csharp
PlayerPreview = PlayerPreviewRenderer.RenderPreview(Player, textures, 4,
    headSlot, bodySlot, legsSlot,
    dyeHead, dyeBody, dyeLegs,
    showHairWithHelmet,
    wingSlot, backSlot, balloonSlot,
    shoeSlot, waistSlot, neckSlot,
    faceSlot, shieldSlot,
    handOnSlot, handOffSlot, frontSlot);
```

### Fix 5: `PlayerPreviewRenderer` — Wing Frame Selection

`PlayerPreviewRenderer.BlendArmorPart` currently takes the full texture and reads frame 0 (top-left 40×56 region). Wings have multiple frames stacked vertically with varying frame counts.

The renderer needs to know frame counts for wings to select the correct source rect:

```csharp
if (wingSlot.HasValue)
{
    var wingTex = textures.GetAccWings(wingSlot.Value);
    int frameCount = GetWingFrameCount(wingSlot.Value);
    int frameHeight = wingTex.Height / frameCount;
    // Use frame 0 for static preview
    BlendArmorPart(composite, textures, wingTex,
        sourceRect: new Rectangle(0, 0, wingTex.Width, frameHeight));
}
```

This requires `BlendArmorPart` to accept an optional `sourceRect` parameter (currently it reads the full texture at 40×56).

---

## Implementation Priority

1. **Player preview accessories** (Fix 4 + 5) — lowest risk, highest visibility. The `RenderPreview` method already supports all slots; just need to wire up the ViewModel and handle wing frame counts.

2. **World doll body + pose-aware armor** (Fix 1) — enables correct pose rendering. The methods exist; need to update source rect math.

3. **World doll non-wing accessories** (Fix 2 partial) — straightforward extension of existing armor rendering using body-frame source rects.

4. **World doll wings** (Fix 2 + 3) — requires the wing offset/frame-count data tables and a new position formula.

---

## Key Source Files

### TEdit (map editor)
| File | Role |
|------|------|
| `src/TEdit/View/WorldRenderXna.xaml.cs` | World renderer — `DrawDollBody`, `DrawDollAccessory`, `DrawDollBodyPart` |
| `src/TEdit/Render/PlayerPreviewRenderer.cs` | WPF bitmap preview — `RenderPreview`, `BlendArmorPart`, `BlendBodyPart` |
| `src/TEdit/Render/Textures.cs` | Texture cache — `GetAccWings`, `GetAccBack`, etc. |
| `src/TEdit/ViewModel/PlayerEditorViewModel.cs` | Player editor — `RegeneratePreview()` |
| `src/TEdit.Terraria/Objects/ItemProperty.cs` | Per-item metadata: `WingSlot?`, `BackSlot?`, etc. |
| `src/TEdit.Terraria/TileEntity.cs` | DisplayDoll data model: Items[9], Dyes[9], Misc[1], Pose |
