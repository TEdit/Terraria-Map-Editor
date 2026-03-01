using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TEdit.Geometry;
using TEdit.Input;
using TEdit.Terraria;
using TEdit.UI;
using TEdit.ViewModel;
using Wpf.Ui.Controls;

namespace TEdit.Editor.Tools;

public abstract partial class BaseTool : ReactiveObject, ITool
{
    protected WriteableBitmap _preview;
    protected WorldViewModel _wvm;
    private double _previewScale = 1;

    protected BaseTool(WorldViewModel worldViewModel)
    {
        _wvm = worldViewModel;
        _preview = new WriteableBitmap(1, 1, 96, 96, PixelFormats.Bgra32, null);
        _preview.Clear();
        _preview.SetPixel(0, 0, 127, 0, 90, 255);
    }

    #region ITool Members

    public string Name { get; protected set; }

    public string Title => Properties.Language.ResourceManager.GetString($"tool_{Name.ToLower()}_title") ?? Name;

    public string ToolTipText
    {
        get
        {
            var actionId = $"tool.{Name.ToLower()}";
            // SpriteTool2 has Name "Sprite2" but action ID "tool.sprite"
            if (Name == "Sprite2") actionId = "tool.sprite";

            var bindings = App.Input.Registry.GetBindings(actionId);
            if (bindings.Count > 0)
                return $"{Title} ({bindings[0]})";

            return Title;
        }
    }

    public virtual ToolType ToolType { get; protected set; }

    public virtual BitmapImage Icon { get; protected set; }

    public virtual SymbolRegular SymbolIcon { get; protected set; } = SymbolRegular.Empty;

    public virtual ImageSource? VectorIcon { get; protected set; }

    [Reactive]
    private bool _isActive;

    public virtual void MouseDown(TileMouseState e)
    {
    }

    public virtual void MouseMove(TileMouseState e)
    {
    }

    public virtual void MouseUp(TileMouseState e)
    {
    }

    public virtual void MouseWheel(TileMouseState e)
    {
    }

    public double PreviewScale
    {
        get { return _previewScale; }
        protected set { _previewScale = value; }
    }

    public int PreviewOffsetX { get; protected set; } = -1;
    public int PreviewOffsetY { get; protected set; } = -1;

    public virtual WriteableBitmap PreviewTool()
    {
        return _preview;
    }

    public virtual bool PreviewIsTexture
    {
        get { return false; }
    }

    public virtual IReadOnlyList<Vector2Int32> CadPreviewPath => Array.Empty<Vector2Int32>();
    public virtual IReadOnlyList<Vector2Int32> CadPreviewTunnelPath => Array.Empty<Vector2Int32>();
    public virtual bool HasCadPreview => false;
    public virtual Vector2Int32 LinePreviewAnchor => default;
    public virtual bool HasLinePreviewAnchor => false;

    public virtual bool IsFloatingPaste => false;
    public virtual Vector2Int32 FloatingPasteAnchor => default;
    public virtual Vector2Int32 FloatingPasteSize => default;
    public virtual void AcceptPaste() { }
    public virtual void CancelPaste() { }
    public virtual CursorHint GetCursorHint(Vector2Int32 tilePos) => CursorHint.Default;

    #endregion

    #region Input Helpers

    [DllImport("user32.dll")]
    private static extern short GetKeyState(int nVirtKey);

    private const int VK_SHIFT = 0x10;
    private const int VK_CONTROL = 0x11;
    private const int VK_MENU = 0x12; // Alt

    /// <summary>
    /// Gets the current modifier keys using Win32 GetKeyState directly.
    /// WPF's Keyboard.Modifiers and Keyboard.IsKeyDown both query the WPF
    /// InputManager which misses key state when the XNA HwndHost has native focus.
    /// GetKeyState reads the thread message queue state and always works.
    /// </summary>
    public static ModifierKeys GetModifiers()
    {
        var modifiers = ModifierKeys.None;
        if ((GetKeyState(VK_CONTROL) & 0x8000) != 0)
            modifiers |= ModifierKeys.Control;
        if ((GetKeyState(VK_SHIFT) & 0x8000) != 0)
            modifiers |= ModifierKeys.Shift;
        if ((GetKeyState(VK_MENU) & 0x8000) != 0)
            modifiers |= ModifierKeys.Alt;
        return modifiers;
    }

    /// <summary>
    /// Gets active editor actions based on current mouse state and keyboard modifiers.
    /// </summary>
    protected List<string> GetActiveActions(TileMouseState e)
    {
        var actions = new List<string>();
        var modifiers = GetModifiers();

        // Check left button
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            actions.AddRange(App.Input.HandleMouseButton(TEditMouseButton.Left, modifiers, TEdit.Input.InputScope.Editor));
        }

        // Check right button
        if (e.RightButton == MouseButtonState.Pressed)
        {
            actions.AddRange(App.Input.HandleMouseButton(TEditMouseButton.Right, modifiers, TEdit.Input.InputScope.Editor));
        }

        // Check middle button
        if (e.MiddleButton == MouseButtonState.Pressed)
        {
            actions.AddRange(App.Input.HandleMouseButton(TEditMouseButton.Middle, modifiers, TEdit.Input.InputScope.Editor));
        }

        return actions;
    }

    /// <summary>
    /// Checks if a specific action is active based on current input.
    /// </summary>
    protected bool IsActionActive(TileMouseState e, string actionId)
    {
        return GetActiveActions(e).Contains(actionId);
    }

    /// <summary>
    /// Performs a BFS wire trace from the given location, highlighting the connected network.
    /// </summary>
    protected void PerformWireTrace(Vector2Int32 location)
    {
        var world = _wvm.CurrentWorld;
        if (world == null) { _wvm.ClearWireTrace(); return; }

        if (location.X < 0 || location.Y < 0 ||
            location.X >= world.TilesWide || location.Y >= world.TilesHigh)
        {
            _wvm.ClearWireTrace();
            return;
        }

        var tile = world.Tiles[location.X, location.Y];
        if (!tile.HasWire) { _wvm.ClearWireTrace(); return; }

        // Determine which wire color to trace
        int color = 0;
        var picker = _wvm.TilePicker;

        // Prefer active TilePicker color if it exists on this tile
        if (picker.RedWireActive && tile.WireRed) color = 1;
        else if (picker.BlueWireActive && tile.WireBlue) color = 2;
        else if (picker.GreenWireActive && tile.WireGreen) color = 3;
        else if (picker.YellowWireActive && tile.WireYellow) color = 4;
        // Fallback: first wire found on tile
        else if (tile.WireRed) color = 1;
        else if (tile.WireBlue) color = 2;
        else if (tile.WireGreen) color = 3;
        else if (tile.WireYellow) color = 4;

        if (color == 0) { _wvm.ClearWireTrace(); return; }

        Func<int, int, bool> hasWireAt = color switch
        {
            1 => (x, y) => world.Tiles[x, y].WireRed,
            2 => (x, y) => world.Tiles[x, y].WireBlue,
            3 => (x, y) => world.Tiles[x, y].WireGreen,
            4 => (x, y) => world.Tiles[x, y].WireYellow,
            _ => (_, _) => false
        };

        // Junction box detector: returns junction type (0=Normal, 1=Left, 2=Right)
        // or -1 if the tile is not a junction box.
        WireTracer.JunctionDetector getJunction = (x, y) =>
        {
            var t = world.Tiles[x, y];
            if (t.Type != (int)TileType.JunctionBox) return -1;
            return t.U / 18; // frameX 0→Normal(0), 18→Left(1), 36→Right(2)
        };

        var network = WireTracer.Trace(hasWireAt, world.TilesWide, world.TilesHigh,
            location.X, location.Y, getJunction);

        _wvm.WireTraceHighlight = network;
        _wvm.WireTraceColor = color;
    }

    #endregion
}
