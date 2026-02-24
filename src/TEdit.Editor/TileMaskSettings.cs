using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TEdit.Terraria;

namespace TEdit.Editor;

public partial class TileMaskSettings : ReactiveObject
{
    [Reactive] private MaskPreset _maskPreset = MaskPreset.Off;

    // Tile Type
    [Reactive] private MaskMode _tileMaskMode = MaskMode.Off;
    [Reactive] private int _tileMaskValue = -1; // -1 = empty/inactive

    // Wall Type
    [Reactive] private MaskMode _wallMaskMode = MaskMode.Off;
    [Reactive] private int _wallMaskValue = 0; // 0 = no wall

    // Brick Style
    [Reactive] private MaskMode _brickStyleMaskMode = MaskMode.Off;
    [Reactive] private BrickStyle _brickStyleMaskValue = BrickStyle.Full;

    // Actuator
    [Reactive] private MaskMode _actuatorMaskMode = MaskMode.Off;

    // Actuator Inactive
    [Reactive] private MaskMode _actuatorInActiveMaskMode = MaskMode.Off;

    // Tile Paint
    [Reactive] private MaskMode _tilePaintMaskMode = MaskMode.Off;
    [Reactive] private int _tilePaintMaskValue = 0;

    // Wall Paint
    [Reactive] private MaskMode _wallPaintMaskMode = MaskMode.Off;
    [Reactive] private int _wallPaintMaskValue = 0;

    // Tile Coatings
    [Reactive] private MaskMode _tileEchoMaskMode = MaskMode.Off;
    [Reactive] private MaskMode _tileIlluminantMaskMode = MaskMode.Off;

    // Wall Coatings
    [Reactive] private MaskMode _wallEchoMaskMode = MaskMode.Off;
    [Reactive] private MaskMode _wallIlluminantMaskMode = MaskMode.Off;

    // Wires
    [Reactive] private MaskMode _wireRedMaskMode = MaskMode.Off;
    [Reactive] private MaskMode _wireBlueMaskMode = MaskMode.Off;
    [Reactive] private MaskMode _wireGreenMaskMode = MaskMode.Off;
    [Reactive] private MaskMode _wireYellowMaskMode = MaskMode.Off;

    // Liquid Type
    [Reactive] private MaskMode _liquidTypeMaskMode = MaskMode.Off;
    [Reactive] private LiquidType _liquidTypeMaskValue = LiquidType.Water;

    // Liquid Level
    [Reactive] private LiquidLevelMaskMode _liquidLevelMaskMode = LiquidLevelMaskMode.Ignore;
    [Reactive] private byte _liquidLevelMaskValue = 0;

    /// <summary>
    /// Fast-path: true when no masks are active, allowing callers to skip evaluation entirely.
    /// </summary>
    public bool AllMasksOff =>
        TileMaskMode == MaskMode.Off &&
        WallMaskMode == MaskMode.Off &&
        BrickStyleMaskMode == MaskMode.Off &&
        ActuatorMaskMode == MaskMode.Off &&
        ActuatorInActiveMaskMode == MaskMode.Off &&
        TilePaintMaskMode == MaskMode.Off &&
        WallPaintMaskMode == MaskMode.Off &&
        TileEchoMaskMode == MaskMode.Off &&
        TileIlluminantMaskMode == MaskMode.Off &&
        WallEchoMaskMode == MaskMode.Off &&
        WallIlluminantMaskMode == MaskMode.Off &&
        WireRedMaskMode == MaskMode.Off &&
        WireBlueMaskMode == MaskMode.Off &&
        WireGreenMaskMode == MaskMode.Off &&
        WireYellowMaskMode == MaskMode.Off &&
        LiquidTypeMaskMode == MaskMode.Off &&
        LiquidLevelMaskMode == LiquidLevelMaskMode.Ignore;

    /// <summary>
    /// Evaluates all active masks against the given tile. Returns true if the tile passes all masks.
    /// All masks are AND'd: if any enabled mask fails, the tile is rejected.
    /// </summary>
    public bool Passes(Tile tile)
    {
        if (AllMasksOff) return true;

        // Tile Type mask
        if (TileMaskMode != MaskMode.Off && !PassesTileMask(tile)) return false;

        // Wall Type mask
        if (WallMaskMode != MaskMode.Off && !PassesIntMask(WallMaskMode, tile.Wall, WallMaskValue, emptyValue: 0)) return false;

        // Brick Style mask
        if (BrickStyleMaskMode != MaskMode.Off && !PassesBrickStyleMask(tile)) return false;

        // Actuator mask
        if (ActuatorMaskMode != MaskMode.Off && !PassesBoolMask(ActuatorMaskMode, tile.Actuator)) return false;

        // Actuator InActive mask
        if (ActuatorInActiveMaskMode != MaskMode.Off && !PassesBoolMask(ActuatorInActiveMaskMode, tile.InActive)) return false;

        // Tile Paint mask
        if (TilePaintMaskMode != MaskMode.Off && !PassesIntMask(TilePaintMaskMode, tile.TileColor, TilePaintMaskValue, emptyValue: 0)) return false;

        // Wall Paint mask
        if (WallPaintMaskMode != MaskMode.Off && !PassesIntMask(WallPaintMaskMode, tile.WallColor, WallPaintMaskValue, emptyValue: 0)) return false;

        // Tile Echo coating mask
        if (TileEchoMaskMode != MaskMode.Off && !PassesBoolMask(TileEchoMaskMode, tile.InvisibleBlock)) return false;

        // Tile Illuminant coating mask
        if (TileIlluminantMaskMode != MaskMode.Off && !PassesBoolMask(TileIlluminantMaskMode, tile.FullBrightBlock)) return false;

        // Wall Echo coating mask
        if (WallEchoMaskMode != MaskMode.Off && !PassesBoolMask(WallEchoMaskMode, tile.InvisibleWall)) return false;

        // Wall Illuminant coating mask
        if (WallIlluminantMaskMode != MaskMode.Off && !PassesBoolMask(WallIlluminantMaskMode, tile.FullBrightWall)) return false;

        // Wire masks
        if (WireRedMaskMode != MaskMode.Off && !PassesBoolMask(WireRedMaskMode, tile.WireRed)) return false;
        if (WireBlueMaskMode != MaskMode.Off && !PassesBoolMask(WireBlueMaskMode, tile.WireBlue)) return false;
        if (WireGreenMaskMode != MaskMode.Off && !PassesBoolMask(WireGreenMaskMode, tile.WireGreen)) return false;
        if (WireYellowMaskMode != MaskMode.Off && !PassesBoolMask(WireYellowMaskMode, tile.WireYellow)) return false;

        // Liquid Type mask
        if (LiquidTypeMaskMode != MaskMode.Off && !PassesLiquidTypeMask(tile)) return false;

        // Liquid Level mask
        if (LiquidLevelMaskMode != LiquidLevelMaskMode.Ignore && !PassesLiquidLevelMask(tile)) return false;

        return true;
    }

    /// <summary>
    /// Populate all mask values from the given tile and set all modes to Match.
    /// </summary>
    public void SetFromTile(Tile tile)
    {
        TileMaskMode = MaskMode.Match;
        TileMaskValue = tile.IsActive ? tile.Type : -1;

        WallMaskMode = MaskMode.Match;
        WallMaskValue = tile.Wall;

        BrickStyleMaskMode = MaskMode.Match;
        BrickStyleMaskValue = tile.BrickStyle;

        ActuatorMaskMode = tile.Actuator ? MaskMode.Match : MaskMode.Empty;
        ActuatorInActiveMaskMode = tile.InActive ? MaskMode.Match : MaskMode.Empty;

        TilePaintMaskMode = MaskMode.Match;
        TilePaintMaskValue = tile.TileColor;

        WallPaintMaskMode = MaskMode.Match;
        WallPaintMaskValue = tile.WallColor;

        TileEchoMaskMode = tile.InvisibleBlock ? MaskMode.Match : MaskMode.Empty;
        TileIlluminantMaskMode = tile.FullBrightBlock ? MaskMode.Match : MaskMode.Empty;

        WallEchoMaskMode = tile.InvisibleWall ? MaskMode.Match : MaskMode.Empty;
        WallIlluminantMaskMode = tile.FullBrightWall ? MaskMode.Match : MaskMode.Empty;

        WireRedMaskMode = tile.WireRed ? MaskMode.Match : MaskMode.Empty;
        WireBlueMaskMode = tile.WireBlue ? MaskMode.Match : MaskMode.Empty;
        WireGreenMaskMode = tile.WireGreen ? MaskMode.Match : MaskMode.Empty;
        WireYellowMaskMode = tile.WireYellow ? MaskMode.Match : MaskMode.Empty;

        LiquidTypeMaskMode = MaskMode.Match;
        LiquidTypeMaskValue = tile.LiquidType;

        LiquidLevelMaskMode = LiquidLevelMaskMode.Equal;
        LiquidLevelMaskValue = tile.LiquidAmount;

        MaskPreset = MaskPreset.ExactMatch;
    }

    /// <summary>
    /// Reset all masks to Off/Ignore.
    /// </summary>
    public void ClearAll()
    {
        TileMaskMode = MaskMode.Off;
        TileMaskValue = -1;
        WallMaskMode = MaskMode.Off;
        WallMaskValue = 0;
        BrickStyleMaskMode = MaskMode.Off;
        BrickStyleMaskValue = BrickStyle.Full;
        ActuatorMaskMode = MaskMode.Off;
        ActuatorInActiveMaskMode = MaskMode.Off;
        TilePaintMaskMode = MaskMode.Off;
        TilePaintMaskValue = 0;
        WallPaintMaskMode = MaskMode.Off;
        WallPaintMaskValue = 0;
        TileEchoMaskMode = MaskMode.Off;
        TileIlluminantMaskMode = MaskMode.Off;
        WallEchoMaskMode = MaskMode.Off;
        WallIlluminantMaskMode = MaskMode.Off;
        WireRedMaskMode = MaskMode.Off;
        WireBlueMaskMode = MaskMode.Off;
        WireGreenMaskMode = MaskMode.Off;
        WireYellowMaskMode = MaskMode.Off;
        LiquidTypeMaskMode = MaskMode.Off;
        LiquidTypeMaskValue = LiquidType.Water;
        LiquidLevelMaskMode = LiquidLevelMaskMode.Ignore;
        LiquidLevelMaskValue = 0;
        MaskPreset = MaskPreset.Off;
    }

    // ── Private mask evaluators ──────────────────────────────────────

    /// <summary>
    /// Tile type mask: handles the special -1 (empty/inactive) case.
    /// </summary>
    private bool PassesTileMask(Tile tile)
    {
        return TileMaskMode switch
        {
            MaskMode.Match when TileMaskValue == -1 => !tile.IsActive,
            MaskMode.Match when TileMaskValue >= 0 => tile.IsActive && tile.Type == TileMaskValue,
            MaskMode.Empty => !tile.IsActive,
            MaskMode.NotMatching when TileMaskValue == -1 => tile.IsActive,
            MaskMode.NotMatching when TileMaskValue >= 0 => !tile.IsActive || tile.Type != TileMaskValue,
            _ => true,
        };
    }

    /// <summary>
    /// Generic int-valued mask evaluator for wall, paint, etc.
    /// </summary>
    private static bool PassesIntMask(MaskMode mode, int tileValue, int maskValue, int emptyValue)
    {
        return mode switch
        {
            MaskMode.Match => tileValue == maskValue,
            MaskMode.Empty => tileValue == emptyValue,
            MaskMode.NotMatching => tileValue != maskValue,
            _ => true,
        };
    }

    /// <summary>
    /// Boolean-valued mask evaluator: Match = has it, Empty = doesn't have it.
    /// </summary>
    private static bool PassesBoolMask(MaskMode mode, bool tileValue)
    {
        return mode switch
        {
            MaskMode.Match => tileValue,
            MaskMode.Empty => !tileValue,
            MaskMode.NotMatching => !tileValue,
            _ => true,
        };
    }

    /// <summary>
    /// BrickStyle mask evaluator.
    /// </summary>
    private bool PassesBrickStyleMask(Tile tile)
    {
        return BrickStyleMaskMode switch
        {
            MaskMode.Match => tile.BrickStyle == BrickStyleMaskValue,
            MaskMode.Empty => tile.BrickStyle == BrickStyle.Full,
            MaskMode.NotMatching => tile.BrickStyle != BrickStyleMaskValue,
            _ => true,
        };
    }

    /// <summary>
    /// Liquid type mask evaluator.
    /// </summary>
    private bool PassesLiquidTypeMask(Tile tile)
    {
        return LiquidTypeMaskMode switch
        {
            MaskMode.Match => tile.LiquidType == LiquidTypeMaskValue,
            MaskMode.Empty => tile.LiquidAmount == 0 || tile.LiquidType == LiquidType.None,
            MaskMode.NotMatching => tile.LiquidType != LiquidTypeMaskValue,
            _ => true,
        };
    }

    /// <summary>
    /// Liquid level mask evaluator with comparison modes.
    /// </summary>
    private bool PassesLiquidLevelMask(Tile tile)
    {
        return LiquidLevelMaskMode switch
        {
            LiquidLevelMaskMode.Equal => tile.LiquidAmount == LiquidLevelMaskValue,
            LiquidLevelMaskMode.GreaterThan => tile.LiquidAmount > LiquidLevelMaskValue,
            LiquidLevelMaskMode.LessThan => tile.LiquidAmount < LiquidLevelMaskValue,
            _ => true,
        };
    }
}
